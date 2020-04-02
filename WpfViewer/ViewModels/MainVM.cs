using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private const string BASE_PATH = @"D:\Source\BitBucket\3rd Party\COVID-19";
        private const string GIT_COMMAND = @"""D:\Program Files\Git\cmd\git.exe"" pull";
        private const string CLEAR_SCRIPT_PATH = @"D:\Source\BitBucket\COVID-19\Clear all data.sql";

        private const string BASE_DATE = "10/1/2019";
        private const string GLOBAL_NAME = "(GLOBAL)";
        private const string LONG_DATE_FORMAT = "MM/dd/yyyy";
        private const string SHORT_DATE_FORMAT = "MMM-dd";
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

        private BackgroundWorker worker;

        public MainVM()
        {
            LoadSettings();

            InitBusyPanel();
            InitMessagePanel();
            InitMainPanel();

            //DailyReports = new DailyReports();

            //ImportData();
            ReadData();
        }

        ~MainVM()
        {
            SaveSettings();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Commands

        public ICommand RefreshDataCommand { get; set; }

        #endregion

        #region Actions

        private void RefreshDataAction(object obj)
        {
            LastImportDateTime = DateTime.Parse(BASE_DATE);
            ImportData(true);
        }

        #endregion

        #region General Properties

        public string GitCommand { get; set; }
        public string RepositoryPath { get; set; }
        public string DataPath { get; set; }
        public string PullData { get; set; }
        public string ReplacementsPath { get; set; }
        public DateTime LastImportDateTime { get; set; }
        public DateTime LastReplacementDateTime { get; set; }

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

        //private DailyReports dailyReports;
        //public DailyReports DailyReports
        //{
        //    get => dailyReports;
        //    set
        //    {
        //        dailyReports = value;
        //        NotifyPropertyChanged();
        //    }
        //}

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
                    //GenerateDailyCounts(selectedTotalReport);
                    switch (selectedView.DisplayName)
                    {
                        case TOTAL_LINE_SELECTOR:
                            ShowLineChart(SelectedTotalReport);
                            break;
                        case ACTIVE_AREA_SELECTOR:
                            ShowStackedAreaSeriesChart(SelectedTotalReport);
                            break;
                        case NEW_BAR_SELECTOR:
                            ShowStackedColumnChart(SelectedTotalReport);
                            break;
                        case DAILY_DATAGRID_SELECTOR:
                            ShowDailyDataGrid(SelectedTotalReport);
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
        }

        private void ReadData()
        {
            //DailyReports.ReadData();
            var list = DatabaseManager.ReadTotalReports();
            TotalReports = new ObservableCollection<TotalReport>(list);

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

        #region ShowChart Methods

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

        private void ShowDailyDataGrid(TotalReport report)
        {
            var list = DatabaseManager.ReadDailyTotalsForReport(report);
            DailyTotalReports = new ObservableCollection<DailyReport>(list);
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

        private void PullLastestData()
        {
            var result = Utility.RunCommand(GitCommand, RepositoryPath);
            if (!result.Contains("Already up to date."))
            {
                ShowMessagePanel("Result", result);
            }
        }

        private List<string> GetFileList(string path, DateTime? dateTime)
        {
            List<string> fileList = new List<string>();
            if (dateTime == null)
            {
                dateTime = DateTime.Parse(BASE_DATE);
            }

            var dir = new DirectoryInfo(path);
            var files = dir.GetFiles("*.csv");
            foreach (var file in files)
            {
                var fileDateTime = file.LastWriteTime;
                if (fileDateTime >= dateTime)
                {
                    fileList.Add(file.FullName);
                }
            }

            return fileList;
        }

        #endregion

        #region Background Workers

        private void Background_LoadDataDoWork(object sender, DoWorkEventArgs e)
        {
            var clearAllData = (bool)e.Argument;

            if (PullData == "True")
            {
                ShowBusyPanel("Pulling latest data...");
                PullLastestData();
            }

            ShowBusyPanel("Checking for new data...");
            if (DatabaseManager.ImportSwaps(ReplacementsPath, LastReplacementDateTime))
            {
                // Replacements are new; tell the user a full refresh may be needed
                ShowMessagePanel("New replacement data", "New replacement data was found and read. " +
                    "This will require a full refresh to ensure that the swaps are applied to older data.");
            }

            // Create a list of files to import
            List<string> fileList = GetFileList(DataPath, LastImportDateTime);

            if (fileList.Count > 0)
            {
                if (clearAllData)
                {
                    DatabaseManager.ClearAll(CLEAR_SCRIPT_PATH);
                }
                else
                {
                    var file = fileList.Min();
                    var clearDate = DateTime.Parse(Path.GetFileNameWithoutExtension(file));
                    DatabaseManager.Clear(clearDate);
                }

                if (fileList.Count > 0)
                {
                    LastImportDateTime = DateTime.Now;

                    for (int i = 0; i < fileList.Count; i++)
                    {
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        var filePath = fileList[i];
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        BusyPanelTitle = $"Reading {fileName}";
                        DatabaseManager.ImportData(filePath, worker, BusyProgressMaximum);
                    }
                }
            }
        }

        private void Background_LoadDataProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BusyProgressValue = e.ProgressPercentage;
            BusyProgressText = e.ProgressPercentage.ToString();
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
                ShowMessagePanel("Error!!", $"{e.Error.Message}");
            }
            else
            {
                LastImportDateTime = DateTime.Now;
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
                new Setting(nameof(GitCommand), GIT_COMMAND),
                new Setting(nameof(RepositoryPath), BASE_PATH),
                new Setting(nameof(PullData), TRUE_LITERAL),
                new Setting(nameof(DataPath), $@"{BASE_PATH}\csse_covid_19_data\csse_covid_19_daily_reports"),
                new Setting(nameof(ReplacementsPath), $@"{dir}\Replacements.csv"),
                new Setting(nameof(LastImportDateTime), BASE_DATE),
                new Setting(nameof(LastReplacementDateTime), BASE_DATE)
            };
            Utility.LoadSettings(list);
            GitCommand = list[nameof(GitCommand)].Value;
            RepositoryPath = list[nameof(RepositoryPath)].Value;
            PullData = list[nameof(PullData)].Value;
            DataPath = list[nameof(DataPath)].Value;
            ReplacementsPath = list[nameof(ReplacementsPath)].Value;
            LastImportDateTime = DateTime.Parse(list[nameof(LastImportDateTime)].Value);
            LastReplacementDateTime = DateTime.Parse(list[nameof(LastReplacementDateTime)].Value);
        }

        private void SaveSettings()
        {
            var list = new Settings
            {
                new Setting(nameof(GitCommand), GitCommand),
                new Setting(nameof(RepositoryPath), RepositoryPath),
                new Setting(nameof(PullData), PullData),
                new Setting(nameof(DataPath), DataPath),
                new Setting(nameof(ReplacementsPath), ReplacementsPath),
                new Setting(nameof(LastImportDateTime), LastImportDateTime.ToString()),
                new Setting(nameof(LastReplacementDateTime), LastReplacementDateTime.ToString())
            };
            Utility.SaveSettings(list);
        }

        #endregion
    }

    public class Selection
    {
        public Selection(string displayName, string chartname)
        {
            DisplayName = displayName;
            ChartName = chartname;
        }

        public string DisplayName { get; set; }
        public string ChartName { get; set; }
    }
}
