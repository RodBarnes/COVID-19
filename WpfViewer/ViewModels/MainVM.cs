using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Common;
using DataClasses;
using LiveCharts;
using LiveCharts.Wpf;
using WpfViewer.Views;

namespace WpfViewer.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        private readonly DailyReports reports;
        private readonly string readPath = @"D:\Source\BitBucket\3rd Party\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";
        private readonly string writePath = @"D:\Source\BitBucket\3rd Party\COVID-19";
        private readonly string writeFilename = "DailyReport.csv";

        public MainVM()
        {
            MergeDataCommand = new Command(MergeDataAction);
            WriteDataCommand = new Command(WriteDataAction);
            ShowChartCommand = new Command(ShowChartAction);

            reports = new DailyReports();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public ICommand MergeDataCommand { get; set; }
        public ICommand WriteDataCommand { get; set; }
        public ICommand ShowChartCommand { get; set; }

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

        public void MergeDataAction(object obj)
        {
            reports.ReadData(readPath);
            MessageBox.Show($"Read is complete.");
        }

        public void WriteDataAction(object obj)
        {
            reports.WriteData($@"{writePath}\{writeFilename}");
            MessageBox.Show($"Write is complete.");
        }

        public void ShowChartAction(object obj)
        {
            UpdateChart();
        }

        private void UpdateChart()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
            SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            //modifying any series values will also animate and update the chart
            SeriesCollection[3].Values.Add(5d);
        }
    }
}
