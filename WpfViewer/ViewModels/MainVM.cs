using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using DataClasses;
using LiveCharts;
using LiveCharts.Wpf;

namespace WpfViewer.ViewModels
{
    public partial class MainVM : INotifyPropertyChanged
    {
        private readonly string readPath = @"D:\Source\BitBucket\3rd Party\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";
        private BackgroundWorker bw;

        public MainVM()
        {
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

        private ObservableCollection<TotalReport> areas;
        public ObservableCollection<TotalReport> TotalReport
        {
            get => areas;
            set
            {
                areas = value;
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

        private TotalReport selectedArea;
        public TotalReport SelectedArea
        {
            get => selectedArea;
            set
            {
                selectedArea = value;
                NotifyPropertyChanged();
                if (selectedArea != null)
                {
                    var deathsCnt = ((double)SelectedArea.TotalDeaths);
                    var recoveredCnt = ((double)SelectedArea.TotalRecovered);
                    var confirmedCnt = ((double)SelectedArea.TotalConfirmed);
                    var activeCnt = confirmedCnt - deathsCnt - recoveredCnt;

                    DeathsPct = Math.Round(deathsCnt / confirmedCnt * 100, 2);
                    RecoveredPct = Math.Round(recoveredCnt / confirmedCnt * 100, 2);
                    ActivePct = Math.Round(activeCnt / confirmedCnt * 100, 2);
                    ShowChart(SelectedArea);
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

        private void ShowChart(TotalReport area)
        {
            List<DailyReport> list = DailyReports.ToList();

            if (!string.IsNullOrEmpty(area.Region))
            {
                list = DailyReports.Where(r => r.Region == area.Region).ToList();
                if (!string.IsNullOrEmpty(area.State))
                {
                    list = list.Where(r => r.State == area.State).ToList();
                }
            }

            var confirmed = list.Select(r => r.TotalConfirmed);
            var recovered = list.Select(r => r.TotalRecovered);
            var deaths = list.Select(r => r.TotalDeaths);
            var dates = list.Select(r => r.RecordDate.ToString("MMM-dd"));

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Confirmed",
                    Stroke = Brushes.Yellow,
                    Fill = Brushes.LightYellow,
                    Values = new ChartValues<int>(confirmed)
                },
                new LineSeries
                {
                    Title = "Recovered",
                    Stroke = Brushes.Green,
                    Fill = Brushes.LightGreen,
                    Values = new ChartValues<int>(recovered)
                },
                new LineSeries
                {
                    Title = "Deaths",
                    Stroke = Brushes.Red,
                    Fill = Brushes.LightCyan,
                    Values = new ChartValues<int>(deaths)
                }
            };
            
            Labels = dates.ToArray();
            //YFormatter = value => value.ToString();
        }

        #endregion

        #region Background Workers

        private void bw_LoadDataDoWork(object sender, DoWorkEventArgs e)
        {
            ShowBusyPanel("Importing data...");

            DailyReports = new DailyReports(readPath);

            // Get the list of areas for the combo box
            // by Region, State without regard to date
            var areas = DailyReports
                .GroupBy(r => new
                {
                    r.Region,
                    r.State
                })
                .Select(g => new TotalReport()
                {
                    Region = g.Key.Region,
                    State = g.Key.State,
                    TotalConfirmed = g.Sum(s => s.TotalConfirmed),
                    TotalRecovered = g.Sum(s => s.TotalRecovered),
                    TotalDeaths = g.Sum(s => s.TotalDeaths)

                })
                .OrderBy(a => a.RegionState);

            TotalReport = new ObservableCollection<TotalReport>(areas);
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
                SelectedArea = TotalReport.Where(a => a.Region == "(All)").FirstOrDefault();

                HideBusyPanel();
            }
        }

        #endregion
    }
}
