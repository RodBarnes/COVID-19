using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Common;
using DataClasses;
using LiveCharts;
using LiveCharts.Wpf;

namespace WpfViewer.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        private readonly DailyReports reports;
        private readonly string readPath = @"D:\Source\BitBucket\3rd Party\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";
        private readonly string writePath = @"D:\Source\BitBucket\COVID-19\Data";
        private readonly string writeFilename = "DailyReport.csv";
        private readonly string writeFilepath;

        public MainVM()
        {
            MergeDataCommand = new Command(MergeDataAction);
            WriteDataCommand = new Command(WriteDataAction);

            writeFilepath = $@"{writePath}\{writeFilename}";

            reports = new DailyReports();
            reports.MergeData(readPath);

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
                    Province = g.Key.ProvinceState
                });
            Areas = new ObservableCollection<Area>(areas.Distinct().OrderBy(a => a.RegionProvince));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        #region Properties

        public ICommand MergeDataCommand { get; set; }
        public ICommand WriteDataCommand { get; set; }

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
                    ShowChart(SelectedArea);
                }
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

        #region Actions

        public void MergeDataAction(object obj)
        {
            reports.MergeData(readPath);
            MessageBox.Show($@"Merge is complete from '{readPath}'");
        }

        public void WriteDataAction(object obj)
        {
            reports.WriteData(writeFilepath);
            MessageBox.Show($@"Write is complete to '{writeFilepath}'");
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

        public Area(string region, string province)
        {
            Region = region;
            Province = province;
        }
        public string Region { get; set; }
        public string Province { get; set; }
        public string RegionProvince
        {
            get => $"{Region},{Province}";
        }
    }
}
