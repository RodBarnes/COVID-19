using System;
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
    public class MainVM : INotifyPropertyChanged
    {
        private readonly string readPath = @"D:\Source\BitBucket\3rd Party\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";

        public MainVM()
        {
            using (new WaitCursor())
            {
                DailyReports = new DailyReports(readPath);

                // Get the list of areas for the combo box
                var areas = DailyReports
                    .GroupBy(r => new 
                    {
                        r.Region,
                        r.State
                    })
                    .Select(g => new Area()
                    {
                        Region = g.Key.Region,
                        State = g.Key.State,
                        Confirmed = g.Sum(s => s.TotalConfirmed),
                        Recovered = g.Sum(s => s.TotalRecovered),
                        Deaths = g.Sum(s => s.TotalDeaths)

                    });

                Areas = new ObservableCollection<Area>(areas.Distinct().OrderBy(a => a.RegionState));
                SelectedArea = Areas.Where(a => a.Region == "(All)").FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Properties

        private ObservableCollection<Area> areas;
        public ObservableCollection<Area> Areas
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

        private Area selectedArea;
        public Area SelectedArea
        {
            get => selectedArea;
            set
            {
                selectedArea = value;
                NotifyPropertyChanged();
                if (selectedArea != null)
                {
                    var deathsCnt = ((double)SelectedArea.Deaths);
                    var recoveredCnt = ((double)SelectedArea.Recovered);
                    var confirmedCnt = ((double)SelectedArea.Confirmed);
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

        private void ShowChart(Area area)
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
    }

    public class Area
    {
        public Area() { }

        public Area(string region, string state, string district, int confirmed, int recovered, int deaths)
        {
            Region = region;
            State = state;
            District = district;
            Confirmed = confirmed;
            Recovered = recovered;
            Deaths = deaths;
        }
        public string Region { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public int Confirmed { get; set; }
        public int Recovered { get; set; }
        public int Deaths { get; set; }

        public string RegionState
        {
            get
            {
                var result = Region;
                if (!string.IsNullOrEmpty(State))
                {
                    result += $",{State}";
                }

                return result;
            }
        }

        public string RegionStateDistrict
        {
            get
            {
                var result = Region;
                if (!string.IsNullOrEmpty(State))
                {
                    result += $",{State}";
                }
                if (!string.IsNullOrEmpty(District))
                {
                    result += $",{District}";
                }

                return result;
            }
        }
    }
}
