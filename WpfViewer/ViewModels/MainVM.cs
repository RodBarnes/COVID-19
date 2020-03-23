﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly string readPath = @"D:\Source\BitBucket\3rd Party\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";
        private BackgroundWorker bw;

        public MainVM()
        {
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Properties

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
        public ObservableCollection<DailyReport> RegionDailyReports
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
                            break;
                        case 2:
                            ShowDataGrid(SelectedTotalReport);
                            break;
                    }
                }
            }
        }

        private double deathsPct = 0;
        public double DeathsPct
        {
            get => deathsPct;
            set
            {
                deathsPct = value;
                NotifyPropertyChanged();
            }
        }

        private double recoveredPct = 0;
        public double RecoveredPct
        {
            get => recoveredPct;
            set
            {
                recoveredPct = value;
                NotifyPropertyChanged();
            }
        }

        private double activePct = 0;
        public double ActivePct
        {
            get => activePct;
            set
            {
                activePct = value;
                NotifyPropertyChanged();
            }
        }

        private SeriesCollection seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => seriesCollection;
            set
            {
                seriesCollection = value;
                NotifyPropertyChanged();
            }
        }

        private string[] labels;
        public string[] Labels
        {
            get => labels;
            set
            {
                labels = value;
                NotifyPropertyChanged();
            }
        }

        private Func<double, string> yFormatter;
        public Func<double, string> YFormatter
        {
            get => yFormatter;
            set
            {
                yFormatter = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void InitMainPanel()
        {
            var col = new ObservableCollection<string>();
            col.Add("Line Chart");
            col.Add("Bar Chart");
            col.Add("Data Grid");
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
            var gitCmd = @"""D:\Program Files\Git\cmd\git.exe"" pull";
            var repositoryDir = @"D:\Source\BitBucket\3rd Party\COVID-19";

            var result = Utility.RunCommand(gitCmd, repositoryDir);
            if (!result.Contains("Already up to date."))
            {
                ShowMessagePanel("Error!", result);
            }
        }

        private void ShowLineChart(TotalReport report)
        {
            List<DailyReport> list = GetFilteredList(report);

            var confirmedValues = list.Select(r => r.TotalConfirmed);
            var recoveredValues = list.Select(r => r.TotalRecovered);
            var deathsValues = list.Select(r => r.TotalDeaths);
            var dateValues = list.Select(r => r.RecordDate.ToString("MMM-dd"));

            var deathsCnt = ((double)deathsValues.Sum());
            var recoveredCnt = ((double)recoveredValues.Sum());
            var confirmedCnt = ((double)confirmedValues.Sum());
            var activeCnt = confirmedCnt - deathsCnt - recoveredCnt;

            DeathsPct = Math.Round(deathsCnt / confirmedCnt * 100, 2);
            RecoveredPct = Math.Round(recoveredCnt / confirmedCnt * 100, 2);
            ActivePct = Math.Round(activeCnt / confirmedCnt * 100, 2);

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Confirmed",
                    Stroke = Brushes.Yellow,
                    Fill = Brushes.LightYellow,
                    Values = new ChartValues<int>(confirmedValues)
                },
                new LineSeries
                {
                    Title = "Recovered",
                    Stroke = Brushes.Green,
                    Fill = Brushes.LightGreen,
                    Values = new ChartValues<int>(recoveredValues)
                },
                new LineSeries
                {
                    Title = "Deaths",
                    Stroke = Brushes.Red,
                    Fill = Brushes.LightCyan,
                    Values = new ChartValues<int>(deathsValues)
                }
            };

            Labels = dateValues.ToArray();
            //YFormatter = value => value.ToString();
        }

        private void ShowBarChart(TotalReport report)
        {
            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "2015",
                    Values = new ChartValues<double> { 10, 50, 39, 50 }
                }
            };

            //adding series will update and animate the chart automatically
            SeriesCollection.Add(new ColumnSeries
            {
                Title = "2016",
                Values = new ChartValues<double> { 11, 56, 42 }
            });

            //also adding values updates and animates the chart automatically
            SeriesCollection[1].Values.Add(48d);

            Labels = new[] { "Maria", "Susan", "Charles", "Frida" };
            //Formatter = value => value.ToString("N");

        }

        private void ShowDataGrid(TotalReport report)
        {
            var list = GetFilteredList(report);

            RegionDailyReports = new ObservableCollection<DailyReport>(list);
        }

        private List<DailyReport> GetFilteredList(TotalReport report)
        {
            List<DailyReport> list;

            if (!string.IsNullOrEmpty(report.Region))
            {
                list = DailyReports.Where(r => r.Region == report.Region).ToList();
                if (!string.IsNullOrEmpty(report.State))
                {
                    list = list.Where(r => r.State == report.State).ToList();
                }
            }
            else
            {
                list = DailyReports.ToList();
            }

            return list;
        }

        #endregion

        #region Background Workers

        private void bw_LoadDataDoWork(object sender, DoWorkEventArgs e)
        {
            ShowBusyPanel("Pulling latest data...");

            PullLastestData();

            ShowBusyPanel("Importing data...");

            DailyReports = new DailyReports(readPath);

            // Get the list of reports for the combo box
            // by Region, State without regard to date
            var displayNames = DailyReports
                .GroupBy(r => new
                {
                    r.Region,
                    r.State
                })
                .Select(g => new TotalReport()
                {
                    Region = g.Key.Region,
                    State = g.Key.State
                })
                .OrderBy(a => a.RegionState);

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
                SelectedTotalReport = TotalReports.Where(a => a.Region == "(All)").FirstOrDefault();
                HideBusyPanel();
            }
        }

        #endregion
    }
}
