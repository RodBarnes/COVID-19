using System.Windows;
using System.Windows.Input;
using Common;
using DataClasses;
using WpfViewer.Views;

namespace WpfViewer.ViewModels
{
    public class MainVM
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

        public ICommand MergeDataCommand { get; set; }
        public ICommand WriteDataCommand { get; set; }
        public ICommand ShowChartCommand { get; set; }

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
            var chartWindow = new ChartWindow();
            chartWindow.Show();

        }
    }
}
