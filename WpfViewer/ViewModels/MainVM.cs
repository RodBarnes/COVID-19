using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Common;
using DataClasses;
using LiveCharts;
using LiveCharts.Wpf;

namespace WpfViewer.ViewModels
{
    enum ViewType
    {
        LineChart,
        BarChart,
        DataGrid
    }

    public partial class MainVM : INotifyPropertyChanged
    {
        private BackgroundWorker bw;

        public MainVM()
        {
            LoadSettings();

            InitBusyPanel();
            InitMessagePanel();
            InitMainPanel();

            bw = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            bw.DoWork += bw_LoadDataDoWork;
            bw.ProgressChanged += bw_LoadDataProgressChanged;
            bw.RunWorkerCompleted += bw_LoadDataRunWorkerCompleted;
            if (!bw.IsBusy)
            {
                bw.RunWorkerAsync();
            }
        }

        ~MainVM()
        {
            SaveSettings();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Properties

        public string GitCommand { get; set; }
        public string RepositoryPath { get; set; }
        public string DataPath { get; set; }
        public string PullData { get; set; }

        private ObservableCollection<string> viewSelections;
        public ObservableCollection<string> ViewSelections
        {
            get => viewSelections;
            set
            {
                viewSelections = value;
                NotifyPropertyChanged();
            }
        }

        private int viewIndex = 0;
        public int ViewIndex
        {
            get => viewIndex;
            set
            {
                viewIndex = value;
                NotifyPropertyChanged();
                if (viewIndex >= 0)
                {
                    switch (viewIndex)
                    {
                        case 0:
                            SetDisplayView(ViewType.LineChart);
                            ShowLineChart(SelectedTotalReport);
                            break;
                        case 1:
                            SetDisplayView(ViewType.BarChart);
                            ShowBarChart(SelectedTotalReport);
                            break;
                        case 2:
                            SetDisplayView(ViewType.DataGrid);
                            ShowDataGrid(SelectedTotalReport);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private ObservableCollection<DailyReport> regionDailyReports;
        public ObservableCollection<DailyReport> CountryDailyReports
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

        private string lineChartVisibility = "Visible";
        public string LineChartVisibility
        {
            get => lineChartVisibility;
            set
            {
                lineChartVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string barChartVisibility = "Collapsed";
        public string BarChartVisibility
        {
            get => barChartVisibility;
            set
            {
                barChartVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private string dataGridVisibility = "Collapsed";
        public string DataGridVisibility
        {
            get => dataGridVisibility;
            set
            {
                dataGridVisibility = value;
                NotifyPropertyChanged();
            }
        }

        private DailyReports dailyReports;
        public DailyReports DailyReports
        {
            get => dailyReports;
            set
            {
                dailyReports = value;
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
                    switch(ViewIndex)
                    {
                        case 0:
                            ShowLineChart(selectedTotalReport);
                            break;
                        case 1:
                            ShowBarChart(selectedTotalReport);
                            break;
                        case 2:
                            ShowDataGrid(SelectedTotalReport);
                            break;
                    }
                }
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

        #region Methods

        private void InitMainPanel()
        {
            var col = new ObservableCollection<string>();
            col.Add("Daily Total");
            col.Add("Daily New");
            col.Add("Daily Data");
            ViewSelections = col;
        }

        private void SetDisplayView(ViewType type)
        {
            switch(type)
            {
                case ViewType.LineChart:
                    LineChartVisibility = "Visible";
                    BarChartVisibility = "Collapsed";
                    DataGridVisibility = "Collapsed";
                    break;
                case ViewType.BarChart:
                    LineChartVisibility = "Collapsed";
                    BarChartVisibility = "Visible";
                    DataGridVisibility = "Collapsed";
                    break;
                case ViewType.DataGrid:
                    LineChartVisibility = "Collapsed";
                    BarChartVisibility = "Collapsed";
                    DataGridVisibility = "Visible";
                    break;
                default:
                    break;
            }
        }

        private void PullLastestData()
        {
            var result = Utility.RunCommand(GitCommand, RepositoryPath);
            if (!result.Contains("Already up to date."))
            {
                ShowMessagePanel("Error!", result);
            }
        }

        private void ShowLineChart(TotalReport report)
        {
            List<DailyReport> list = GetFilteredList(report);

            if (report.TotalConfirmed == null)
            {
                report.TotalConfirmed = list
                    .GroupBy(r => r.RecordDate)
                    .Select(g => g.Sum(i => i.TotalConfirmed));
            }
            if (report.TotalRecovered == null)
            {
                report.TotalRecovered = list
                    .GroupBy(r => r.RecordDate)
                    .Select(g => g.Sum(i => i.TotalRecovered));
            }
            if (report.TotalDeaths == null)
            {
                report.TotalDeaths = list
                    .GroupBy(r => r.RecordDate)
                    .Select(g => g.Sum(i => i.TotalDeaths));
            }
            if (report.RecordDates == null)
            {
                report.RecordDates = list
                    .GroupBy(r => r.RecordDate)
                    .Select(g => g.Key.ToString("MMM-dd"));
            }

            //foreach (var item in report.TotalConfirmed)
            //{
            //    System.Diagnostics.Debug.WriteLine($"{item}");
            //}

            LineSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Confirmed",
                    Stroke = Brushes.Yellow,
                    Fill = Brushes.LightYellow,
                    Values = new ChartValues<int>(report.TotalConfirmed)
                },
                new LineSeries
                {
                    Title = "Recovered",
                    Stroke = Brushes.Green,
                    Fill = Brushes.LightGreen,
                    Values = new ChartValues<int>(report.TotalRecovered)
                },
                new LineSeries
                {
                    Title = "Deaths",
                    Stroke = Brushes.Red,
                    Fill = Brushes.LightCoral,
                    Values = new ChartValues<int>(report.TotalDeaths)
                }
            };

            LineLabels = report.RecordDates.ToArray();
            LineFormatter = value => value.ToString();
        }

        private void ShowBarChart(TotalReport report)
        {
            List<DailyReport> list = GetFilteredList(report);

            if (report.NewConfirmed == null)
            {
                report.NewConfirmed = list
                    .GroupBy(r => r.RecordDate)
                    .Select(g => g.Sum(i => i.NewConfirmed));
            }
            if (report.NewRecovered == null)
            {
                report.NewRecovered = list
                    .GroupBy(r => r.RecordDate)
                    .Select(g => g.Sum(i => i.NewRecovered));
            }
            if (report.NewDeaths == null)
            {
                report.NewDeaths = list
                    .GroupBy(r => r.RecordDate)
                    .Select(g => g.Sum(i => i.NewDeaths));
            }
            if (report.RecordDates == null)
            {
                report.RecordDates = list
                    .GroupBy(r => r.RecordDate)
                    .Select(g => g.Key.ToString("MMM-dd"));
            }

            BarSeriesCollection = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Title = "Confirmed",
                    Stroke = Brushes.Yellow,
                    Fill = Brushes.Yellow,
                    Values = new ChartValues<int>(report.NewConfirmed)
                },
                new StackedColumnSeries
                {
                    Title = "Recovered",
                    Stroke = Brushes.Green,
                    Fill = Brushes.Green,
                    Values = new ChartValues<int>(report.NewRecovered)
                },
                new StackedColumnSeries
                {
                    Title = "Deaths",
                    Stroke = Brushes.Red,
                    Fill = Brushes.Red,
                    Values = new ChartValues<int>(report.NewDeaths)
                }
            };

            BarLabels = report.RecordDates.ToArray();
            BarFormatter = value => value.ToString();
        }

        private void ShowDataGrid(TotalReport report)
        {
            var list = GetFilteredList(report);

            CountryDailyReports = new ObservableCollection<DailyReport>(list);
        }

        private List<DailyReport> GetFilteredList(TotalReport report)
        {
            List<DailyReport> list;

            if (!string.IsNullOrEmpty(report.Country))
            {
                list = DailyReports.Where(r => r.Country == report.Country).ToList();
                if (!string.IsNullOrEmpty(report.State))
                {
                    list = list.Where(r => r.State == report.State).ToList();
                }
            }
            else
            {
                list = DailyReports.ToList();
            }

            //foreach (var item in list)
            //{
            //    System.Diagnostics.Debug.WriteLine($"{item.RecordDate},{item.Country},{item.State},{item.County},{item.TotalConfirmed}");
            //}

            return list;
        }

        #endregion

        #region Background Workers

        private void bw_LoadDataDoWork(object sender, DoWorkEventArgs e)
        {
            if (PullData == "True")
            {
                ShowBusyPanel("Pulling latest data...");
                PullLastestData();
            }

            var filePaths = Directory.GetFiles(DataPath, "*.csv");

            if (filePaths.Length > 0)
            {
                DailyReports = new DailyReports();

                ShowBusyPanel("Reading replacements...");
                DailyReports.ReadReplacements();

                ShowBusyPanel("Importing data...");
                foreach (var filePath in filePaths)
                {
                    DailyReports.ReadDailyFiles(filePath);
                }

                //ShowBusyPanel("Filling in missing dates...");
                //DailyReports.AddMissingRecords();

                ShowBusyPanel("Calculating global values...");
                DailyReports.AddGlobalSums();
            }
            else
            {
                throw new FileNotFoundException($"No files found at path '{DataPath}'.");
            }

            // Get the list of reports for the combo box
            // by Country, State without regard to date
            var displayNames = DailyReports
                .GroupBy(r => new
                {
                    r.Country,
                    r.State
                })
                .Select(g => new TotalReport()
                {
                    Country = g.Key.Country,
                    State = g.Key.State
                })
                .OrderBy(a => a.DisplayName);

            TotalReports = new ObservableCollection<TotalReport>(displayNames);
        }

        private void bw_LoadDataProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BusyProgressText = e.ProgressPercentage.ToString();
        }

        private void bw_LoadDataRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                HideBusyPanel();
                ShowMessagePanel("Cancelled", "User cancelled the operation!");
            }
            else if (!(e.Error == null))
            {
                HideBusyPanel();
                ShowMessagePanel("Error!!", $"{e.Error.Message}");
            }
            else
            {
                SelectedTotalReport = TotalReports.Where(a => a.Country == "(GLOBAL)").FirstOrDefault();
                HideBusyPanel();
            }
        }

        #endregion

        #region Settings Management

        private void LoadSettings()
        {
            var path = @"D:\Source\BitBucket\3rd Party\COVID-19";
            var list = new Settings
            {
                new Setting(nameof(GitCommand), @"""D:\Program Files\Git\cmd\git.exe"" pull"),
                new Setting(nameof(RepositoryPath), path),
                new Setting(nameof(PullData),"True"),
                new Setting(nameof(DataPath), $@"{path}\csse_covid_19_data\csse_covid_19_daily_reports")
            };
            Utility.LoadSettings(list);
            GitCommand = list[nameof(GitCommand)].Value;
            RepositoryPath = list[nameof(RepositoryPath)].Value;
            PullData = list[nameof(PullData)].Value;
            DataPath = list[nameof(DataPath)].Value;
        }

        private void SaveSettings()
        {
            var list = new Settings
            {
                new Setting(nameof(GitCommand), GitCommand),
                new Setting(nameof(RepositoryPath), RepositoryPath),
                new Setting(nameof(PullData), PullData),
                new Setting(nameof(DataPath), DataPath)
            };
            Utility.SaveSettings(list);
        }

        #endregion
    }
}
