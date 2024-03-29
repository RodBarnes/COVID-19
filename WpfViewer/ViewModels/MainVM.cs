﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using Common;
using DataClasses;

namespace Viewer.ViewModels
{
    enum ViewType
    {
        LineSeriesChart,
        AreaSeriesChart,
        StackedColumnChart,
        DataGrid
    }

    public partial class MainVM : INotifyPropertyChanged
    {
        #region Constants

        private const string BASE_PATH = @"D:\Source\BitBucket\3rd Party\COVID-19";
        private const string GIT_PULL_COMMAND = @"""C:\Users\rodba\AppData\Local\Atlassian\SourceTree\git_local\bin\git.exe"" pull";
        private const string GIT_CLONE_COMMAND = @"""C:\Users\rodba\AppData\Local\Atlassian\SourceTree\git_local\bin\git.exe"" clone https://github.com/libgit2/libgit2";
        private const string CLEAR_SCRIPT_PATH = @"D:\Source\BitBucket\COVID-19\Clear all data.sql";

        private const string BASE_DATE = "10/1/2019";
        private const string GLOBAL_NAME = "(GLOBAL)";
        private const string TRUE_LITERAL = "True";

        private const string CONFIRMED_TITLE = "Confirmed";
        private const string ACTIVE_TITLE = "Active";
        private const string RECOVERED_TITLE = "Recovered";
        private const string DEATHS_TITLE = "Deaths";

        private const string TOTAL_LINE_SELECTOR = "Confirmed Total";
        private const string ACTIVE_AREA_SELECTOR = "Active Total";
        private const string NEW_BAR_SELECTOR = "New Active";
        private const string DAILY_DATAGRID_SELECTOR = "Daily Data";

        private const string VISIBILITY_COLLAPSED = "Collapsed";
        private const string VISIBILITY_VISIBLE = "Visible";

        #endregion

        private BackgroundWorker worker;
        private readonly MainWindow AssociatedWindow;

        public MainVM(MainWindow wdw)
        {
            AssociatedWindow = wdw;

            LoadSettings();

            InitBusyPanel();
            InitMainPanel();

            ImportData();   // Normal
            //ImportData(true);   // For testing with always doing a refresh
            //ReadData(); // For testing with no changes in DB
        }

        ~MainVM()
        {
            SaveSettings();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Commands

        public Command RefreshDataCommand { get; set; }

        #endregion

        #region Actions

        private void RefreshDataAction(object obj)
        {
            LastImportDateTime = DateTime.Parse(BASE_DATE);
            ImportData(true);
        }

        #endregion

        #region Main Properties

        public AboutProperties AboutProperties { get; set; }
        public string GitCommand { get; set; }
        public string RepositoryPath { get; set; }
        public string DataPath { get; set; }
        public string PullData { get; set; }
        public DateTime LastImportDateTime { get; set; }
        public string ReplacementsPath { get; set; }
        public DateTime LastReplacementDateTime { get; set; }
        public string CountryStatsPath { get; set; }
        public DateTime LastCountryStatsDateTime { get; set; }
        public List<CountryStats> CountryStats { get; set; }

        private ObservableCollection<Selection> viewSelections;
        public ObservableCollection<Selection> ViewSelections
        {
            get => viewSelections;
            set
            {
                viewSelections = value;
                NotifyPropertyChanged();
            }
        }

        private Selection selectedView;
        public Selection SelectedView
        {
            get => selectedView;
            set
            {
                selectedView = value;
                NotifyPropertyChanged();
                if (selectedView != null && SelectedTotalReport != null)
                {
                    switch (selectedView.DisplayName)
                    {
                        case TOTAL_LINE_SELECTOR:
                            SetDisplayView(ViewType.LineSeriesChart);
                            ShowLineChart(SelectedTotalReport);
                            break;
                        case ACTIVE_AREA_SELECTOR:
                            SetDisplayView(ViewType.AreaSeriesChart);
                            ShowStackedAreaSeriesChart(SelectedTotalReport);
                            break;
                        case NEW_BAR_SELECTOR:
                            SetDisplayView(ViewType.StackedColumnChart);
                            ShowStackedColumnChart(SelectedTotalReport);
                            break;
                        case DAILY_DATAGRID_SELECTOR:
                            SetDisplayView(ViewType.DataGrid);
                            ShowDailyDataGrid(SelectedTotalReport);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private CollectionViewSource dailyTotalsView = new CollectionViewSource();
        public CollectionViewSource DailyTotalsView
        {
            get => dailyTotalsView;
            set
            {
                dailyTotalsView = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<DailyReport> regionDailyReports;
        public ObservableCollection<DailyReport> DailyTotalReports
        {
            get => regionDailyReports;
            set
            {
                regionDailyReports = value;
                NotifyPropertyChanged();
            }
        }

        private DailyReport selectedDailyReport;
        public DailyReport SelectedDailyReport
        {
            get => selectedDailyReport;
            set
            {
                selectedDailyReport = value;
                NotifyPropertyChanged();
            }
        }

        private CollectionViewSource totalReportsView = new CollectionViewSource();
        public CollectionViewSource TotalReportsView
        {
            get => totalReportsView;
            set
            {
                totalReportsView = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<TotalReport> totalReports;
        public ObservableCollection<TotalReport> TotalReports
        {
            get => totalReports;
            set
            {
                totalReports = value;
                NotifyPropertyChanged();
            }
        }

        private TotalReport selectedTotalReport;
        public TotalReport SelectedTotalReport
        {
            get => selectedTotalReport;
            set
            {
                selectedTotalReport = value;
                NotifyPropertyChanged();
                if (selectedTotalReport != null)
                {
                    switch (selectedView.DisplayName)
                    {
                        case TOTAL_LINE_SELECTOR:
                            ShowLineChart(selectedTotalReport);
                            break;
                        case ACTIVE_AREA_SELECTOR:
                            ShowStackedAreaSeriesChart(selectedTotalReport);
                            break;
                        case NEW_BAR_SELECTOR:
                            ShowStackedColumnChart(selectedTotalReport);
                            break;
                        case DAILY_DATAGRID_SELECTOR:
                            ShowDailyDataGrid(selectedTotalReport);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        #endregion

        #region Main Methods

        private void InitMainPanel()
        {
            RefreshDataCommand = new Command(RefreshDataAction);

            ViewSelections = new ObservableCollection<Selection>
            {
                new Selection(TOTAL_LINE_SELECTOR, "Total Cases: Confirmed, Recovered, Deaths"),
                new Selection(ACTIVE_AREA_SELECTOR, "Total Cases: Active, Recovered, Deaths"),
                new Selection(NEW_BAR_SELECTOR, "New Cases: Active, Recovered, Deaths"),
                new Selection(DAILY_DATAGRID_SELECTOR, "Daily Case Counts")
            };
            SelectedView = viewSelections[0];

            var assyInfo = new AssemblyInfo(Assembly.GetExecutingAssembly());
            AboutProperties = new AboutProperties
            {
                ApplicationName = assyInfo.Product,
                ApplicationVersion = assyInfo.AssemblyVersionString,
                Copyright = $"{assyInfo.Copyright} {assyInfo.Company}",
                Description = assyInfo.Description,
                Background = nameof(Colors.LightSteelBlue),
                ImagePath = @"D:\Source\BitBucket\COVID-19\WpfViewer\Images\coronavirus_96x96.png"
            };
        }

        private void ReadData()
        {
            CountryStats = DatabaseManager.ReadCountryStats();

            var reports = DatabaseManager.ReadTotalReports();
            foreach (var report in reports)
            {
                var stat = CountryStats.Where(i => i.Country == report.Country).FirstOrDefault();
                report.Population = (stat != null) ? stat.Population : 0;
            }
            var global = reports.Where(i => i.Country == GLOBAL_NAME).FirstOrDefault();
            global.Population = CountryStats.Sum(i => i.Population);
            TotalReports = new ObservableCollection<TotalReport>(reports);

            TotalReportsView.Source = TotalReports;
            TotalReportsView.SortDescriptions.Add(new SortDescription("Country", ListSortDirection.Ascending));
            TotalReportsView.SortDescriptions.Add(new SortDescription("State", ListSortDirection.Ascending));

            SelectedTotalReport = TotalReports.Where(a => a.Country == GLOBAL_NAME).FirstOrDefault();
        }

        #endregion

        #region Chart Properties

        private string lineChartVisibility = VISIBILITY_VISIBLE;
        public string SeriesChartVisibility
        {
            get => lineChartVisibility;
            set
            {
                lineChartVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string barChartVisibility = VISIBILITY_COLLAPSED;
        public string ColumnChartVisibility
        {
            get => barChartVisibility;
            set
            {
                barChartVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string dataGridVisibility = VISIBILITY_COLLAPSED;
        public string DataGridVisibility
        {
            get => dataGridVisibility;
            set
            {
                dataGridVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private SeriesCollection lineSeriesCollection;
        public SeriesCollection LineSeriesCollection
        {
            get => lineSeriesCollection;
            set
            {
                lineSeriesCollection = value;
                NotifyPropertyChanged();
            }
        }

        private string[] lineLabels;
        public string[] LineLabels
        {
            get => lineLabels;
            set
            {
                lineLabels = value;
                NotifyPropertyChanged();
            }
        }

        private Func<double, string> lineFormatter;
        public Func<double, string> LineFormatter
        {
            get => lineFormatter;
            set
            {
                lineFormatter = value;
                NotifyPropertyChanged();
            }
        }

        private SeriesCollection barSeriesCollection;
        public SeriesCollection BarSeriesCollection
        {
            get => barSeriesCollection;
            set
            {
                barSeriesCollection = value;
                NotifyPropertyChanged();
            }
        }

        private string[] barLabels;
        public string[] BarLabels
        {
            get => barLabels;
            set
            {
                barLabels = value;
                NotifyPropertyChanged();
            }
        }

        private Func<double, string> barFormatter;
        public Func<double, string> BarFormatter
        {
            get => barFormatter;
            set
            {
                barFormatter = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Chart Methods

        private void ShowDailyDataGrid(TotalReport report)
        {
            var list = DatabaseManager.ReadDailyTotalsForReport(report);
            DailyTotalReports = new ObservableCollection<DailyReport>(list);

            DailyTotalsView.Source = DailyTotalReports;
            DailyTotalsView.SortDescriptions.Add(new SortDescription("State", ListSortDirection.Ascending));
            DailyTotalsView.SortDescriptions.Add(new SortDescription("FileDate", ListSortDirection.Descending));
        }

        private void SetDisplayView(ViewType type)
        {
            switch (type)
            {
                case ViewType.LineSeriesChart:
                    SeriesChartVisibility = VISIBILITY_VISIBLE;
                    ColumnChartVisibility = VISIBILITY_COLLAPSED;
                    DataGridVisibility = VISIBILITY_COLLAPSED;
                    break;
                case ViewType.AreaSeriesChart:
                    SeriesChartVisibility = VISIBILITY_VISIBLE;
                    ColumnChartVisibility = VISIBILITY_COLLAPSED;
                    DataGridVisibility = VISIBILITY_COLLAPSED;
                    break;
                case ViewType.StackedColumnChart:
                    SeriesChartVisibility = VISIBILITY_COLLAPSED;
                    ColumnChartVisibility = VISIBILITY_VISIBLE;
                    DataGridVisibility = VISIBILITY_COLLAPSED;
                    break;
                case ViewType.DataGrid:
                    SeriesChartVisibility = VISIBILITY_COLLAPSED;
                    ColumnChartVisibility = VISIBILITY_COLLAPSED;
                    DataGridVisibility = VISIBILITY_VISIBLE;
                    break;
                default:
                    break;
            }
        }

        private void ShowLineChart(TotalReport report)
        {
            LineSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = CONFIRMED_TITLE,
                    Stroke = Brushes.Yellow,
                    Fill = Brushes.LightYellow,
                    Values = new ChartValues<int>(report.TotalConfirmeds)
                },
                new LineSeries
                {
                    Title = RECOVERED_TITLE,
                    Stroke = Brushes.Green,
                    Fill = Brushes.LightGreen,
                    Values = new ChartValues<int>(report.TotalRecovereds)
                },
                new LineSeries
                {
                    Title = DEATHS_TITLE,
                    Stroke = Brushes.Red,
                    Fill = Brushes.LightCoral,
                    Values = new ChartValues<int>(report.TotalDeaths)
                }
            };

            LineLabels = report.FileDates.ToArray();
            LineFormatter = value => value.ToString();
        }

        private void ShowStackedColumnChart(TotalReport report)
        {
            BarSeriesCollection = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Title = ACTIVE_TITLE,
                    Stroke = Brushes.Yellow,
                    Fill = Brushes.Yellow,
                    Values = new ChartValues<int>(report.NewActives)
                },
                new StackedColumnSeries
                {
                    Title = RECOVERED_TITLE,
                    Stroke = Brushes.Green,
                    Fill = Brushes.Green,
                    Values = new ChartValues<int>(report.NewRecovereds)
                },
                new StackedColumnSeries
                {
                    Title = DEATHS_TITLE,
                    Stroke = Brushes.Red,
                    Fill = Brushes.Red,
                    Values = new ChartValues<int>(report.NewDeaths)
                }
            };

            BarLabels = report.FileDates.ToArray();
            BarFormatter = value => value.ToString();
        }

        private void ShowStackedAreaSeriesChart(TotalReport report)
        {
            LineSeriesCollection = new SeriesCollection
            {
                new StackedAreaSeries
                {
                    Title = ACTIVE_TITLE,
                    Stroke = Brushes.Yellow,
                    Fill = Brushes.LightYellow,
                    Values = new ChartValues<int>(report.TotalActives)
                },
                new StackedAreaSeries
                {
                    Title = RECOVERED_TITLE,
                    Stroke = Brushes.Green,
                    Fill = Brushes.LightGreen,
                    Values = new ChartValues<int>(report.TotalRecovereds)
                },
                new StackedAreaSeries
                {
                    Title = DEATHS_TITLE,
                    Stroke = Brushes.Red,
                    Fill = Brushes.LightCoral,
                    Values = new ChartValues<int>(report.TotalDeaths)
                }
            };

            LineLabels = report.FileDates.ToArray();
            LineFormatter = value => value.ToString();
        }

        #endregion

        #region Import Methods

        private void ImportData(bool clearAllData = false)
        {
            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.DoWork += Background_LoadDataDoWork;
            worker.ProgressChanged += Background_LoadDataProgressChanged;
            worker.RunWorkerCompleted += Background_LoadDataRunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync(clearAllData);
            }
        }

        private string PullLastestData()
        {
            string result;

            result = ProcessManagement.RunCommand(GitCommand, RepositoryPath);

            return result;
        }

        #endregion

        #region Background Workers

        private void Background_LoadDataDoWork(object sender, DoWorkEventArgs e)
        {
            DateTime? lastWriteTime = null;

            var clearAllData = (bool)e.Argument;
            if (PullData == "True")
            {
                ShowBusyPanel("Pulling latest data...");
                var result = PullLastestData();
                //if (!result.Contains("Already up to date."))
                //{
                //    AssociatedWindow.MessagePanel.Show("Result", result);
                //}
            }

            // If the user requested it, just reimport everything
            if (clearAllData)
            {
                var dateTime = DateTime.Parse(BASE_DATE);
                LastImportDateTime = dateTime;
                LastReplacementDateTime = dateTime;
                LastCountryStatsDateTime = dateTime;
            }

            ShowBusyPanel("Checking for new data...");

            lastWriteTime = DatabaseManager.ImportCountryStats(CountryStatsPath, LastCountryStatsDateTime);
            if (lastWriteTime != null)
            {
                LastCountryStatsDateTime = (DateTime)lastWriteTime;
            }

            lastWriteTime = DatabaseManager.ImportSwaps(ReplacementsPath, LastReplacementDateTime);
            if (lastWriteTime != null)
            {
                LastReplacementDateTime = (DateTime)lastWriteTime;
                // Reimport all the COVID-19 data since the replacements have been changed
                var dateTime = DateTime.Parse(BASE_DATE);
                LastImportDateTime = dateTime;
            }

            // Get a list of the files in LastWriteTime order
            var dir = new DirectoryInfo(DataPath);
            var files = dir.GetFiles("*.csv")
                .Where(f => f.LastWriteTime.TrimMilliseconds() > LastImportDateTime)
                .OrderBy(f => f.LastWriteTime);

            // Go through the list
            foreach (var file in files)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                // Clear the existing data for this file
                var clearDate = DateTime.Parse(Path.GetFileNameWithoutExtension(file.FullName));
                DatabaseManager.Clear(clearDate);

                var fileName = Path.GetFileNameWithoutExtension(file.FullName);
                BusyPanelTitle = $"Importing data for {fileName}";

                // Import the new data
                DatabaseManager.ImportData(file.FullName, worker, BusyProgressMaximum);
            }
            // Since the files were processed in LastWriteTime order, the last file processed
            // will have the latest date and it should be used for the start date on the next data check
            if (files.Count() > 0)
            {
                LastImportDateTime = files.Max(f => f.LastWriteTime.TrimMilliseconds());
            }
        }

        private void Background_LoadDataProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BusyProgressValue = e.ProgressPercentage;
            BusyProgressText = $"{e.ProgressPercentage}%";
        }

        private void Background_LoadDataRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                HideBusyPanel();
            }
            else if (!(e.Error == null))
            {
                HideBusyPanel();
                AssociatedWindow.MessagePanel.Show("Error!!", $"{e.Error.Message}");
            }
            else
            {
                ReadData();
                HideBusyPanel();
            }
        }

        #endregion

        #region Settings Management

        private void LoadSettings()
        {
            var dir = Directory.GetCurrentDirectory();
            var list = new Settings
            {
                new Setting(nameof(GitCommand), GIT_PULL_COMMAND),
                new Setting(nameof(RepositoryPath), BASE_PATH),
                new Setting(nameof(PullData), TRUE_LITERAL),
                new Setting(nameof(DataPath), $@"{BASE_PATH}\csse_covid_19_data\csse_covid_19_daily_reports"),
                new Setting(nameof(LastImportDateTime), BASE_DATE),
                new Setting(nameof(ReplacementsPath), $@"{dir}\{nameof(Replacements)}.csv"),
                new Setting(nameof(LastReplacementDateTime), BASE_DATE),
                new Setting(nameof(CountryStatsPath), $@"{dir}\{nameof(CountryStats)}.csv"),
                new Setting(nameof(LastCountryStatsDateTime), BASE_DATE)
            };
            Utility.LoadSettings(list);
            GitCommand = list[nameof(GitCommand)].Value;
            RepositoryPath = list[nameof(RepositoryPath)].Value;
            PullData = list[nameof(PullData)].Value;
            DataPath = list[nameof(DataPath)].Value;
            LastImportDateTime = DateTime.Parse(list[nameof(LastImportDateTime)].Value);
            ReplacementsPath = list[nameof(ReplacementsPath)].Value;
            LastReplacementDateTime = DateTime.Parse(list[nameof(LastReplacementDateTime)].Value);
            CountryStatsPath = list[nameof(CountryStatsPath)].Value;
            LastCountryStatsDateTime = DateTime.Parse(list[nameof(LastCountryStatsDateTime)].Value);
        }

        private void SaveSettings()
        {
            var list = new Settings
            {
                new Setting(nameof(GitCommand), GitCommand),
                new Setting(nameof(RepositoryPath), RepositoryPath),
                new Setting(nameof(PullData), PullData),
                new Setting(nameof(DataPath), DataPath),
                new Setting(nameof(LastImportDateTime), LastImportDateTime.ToString()),
                new Setting(nameof(ReplacementsPath), ReplacementsPath),
                new Setting(nameof(LastReplacementDateTime), LastReplacementDateTime.ToString()),
                new Setting(nameof(CountryStatsPath), CountryStatsPath),
                new Setting(nameof(LastCountryStatsDateTime), LastCountryStatsDateTime.ToString())
            };
            Utility.SaveSettings(list);
        }

        #endregion
    }
}
