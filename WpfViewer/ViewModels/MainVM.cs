using System;
using System.Collections.Generic;
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
            ShowChartCommand = new Command(ShowChartAction);

            writeFilepath = $@"{writePath}\{writeFilename}";

            if (File.Exists(writeFilepath))
            {
                reports = new DailyReports(writeFilepath);
            }
            else
            {
                reports = new DailyReports();
                reports.MergeData(readPath);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        #region Properties

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

        private string chartButtonContent = "China Chart";
        public string ChartButtonContent
        {
            get => chartButtonContent;
            set
            {
                chartButtonContent = value;
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
            var writeFilepath = $@"{writePath}\{writeFilename}";
            reports.WriteData(writeFilepath);
            MessageBox.Show($@"Write is complete to '{writeFilepath}'");
        }

        public void ShowChartAction(object obj)
        {
            ShowChart("Mainland China", "Hubei");
        }

        #endregion

        #region Methods

        private void ShowChart(string region = "", string province = "")
        {
            List<DailyReport> list = reports.ToList();

            if (!string.IsNullOrEmpty(region))
            {
                list = reports.Where(r => r.CountryRegion == region).ToList();
                if (!string.IsNullOrEmpty(province))
                {
                    list = list.Where(r => r.ProvinceState == province).ToList();
                }

            }

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Confirmed",
                    Stroke = Brushes.Yellow,
                    Fill = Brushes.LightYellow,
                    Values = new ChartValues<int>(list.Select(r => r.Confirmed))
                },
                new LineSeries
                {
                    Title = "Recovered",
                    Stroke = Brushes.Green,
                    Fill = Brushes.LightGreen,
                    Values = new ChartValues<int>(list.Select(r => r.Recovered))
                },
                new LineSeries
                {
                    Title = "Deaths",
                    Stroke = Brushes.Red,
                    Fill = Brushes.LightCyan,
                    Values = new ChartValues<int>(list.Select(r => r.Deaths))
                }
            };

            Labels = list.Select(r => r.RecordDate.ToString("MMM-dd")).ToArray();
            YFormatter = value => value.ToString();
        }

        #endregion
    }
}
