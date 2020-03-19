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
    public class MainVM : INotifyPropertyChanged
    {
        private readonly DailyReports reports;
        private readonly string readPath = @"D:\Source\BitBucket\3rd Party\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";

        public MainVM()
        {
            reports = new DailyReports(readPath);

            // Get the list of areas for the combo box
            var areas =
                reports
                .GroupBy(r => new 
                {
                    r.CountryRegion,
                    r.ProvinceState
                })
                .Select(g => new Area()
                {
                    Region = g.Key.CountryRegion,
                    Province = g.Key.ProvinceState,
                    Confirmed = g.Sum(s => s.Confirmed),
                    Recovered = g.Sum(s => s.Recovered),
                    Deaths = g.Sum(s => s.Deaths)

                });
            Areas = new ObservableCollection<Area>(areas.Distinct().OrderBy(a => a.RegionProvince));
            SelectedArea = Areas.Where(a => a.Region == "(All)").FirstOrDefault();
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
            List<DailyReport> list = reports.ToList();

            if (!string.IsNullOrEmpty(area.Region))
            {
                list = reports.Where(r => r.CountryRegion == area.Region).ToList();
                if (!string.IsNullOrEmpty(area.Province))
                {
                    list = list.Where(r => r.ProvinceState == area.Province).ToList();
                }
            }

            var confirmed = list.Select(r => r.Confirmed);
            var recovered = list.Select(r => r.Recovered);
            var deaths = list.Select(r => r.Deaths);
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

        public Area(string region, string province, int confirmed, int recovered, int deaths)
        {
            Region = region;
            Province = province;
            Confirmed = confirmed;
            Recovered = recovered;
            Deaths = deaths;
        }
        public string Region { get; set; }
        public string Province { get; set; }
        public int Confirmed { get; set; }
        public int Recovered { get; set; }
        public int Deaths { get; set; }
        public string RegionProvince
        {
            get => $"{Region},{Province}";
        }
    }
}
