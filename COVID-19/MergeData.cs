using System;
using Common;
using DataClasses;

namespace COVID_19
{
    class MergeData
    {
        private static string readPath;
        private static string writePath;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                readPath = args[0];
            }
            else
            {
                readPath = @"D:\Source\BitBucket\3rd Party\COVID-19\csse_covid_19_data\csse_covid_19_daily_reports";
            }
            if (args.Length > 1)
            {
                writePath = args[1];
            }
            else
            {
                writePath = @"D:\Source\BitBucket\3rd Party\COVID-19";
            }

            try
            {
                var list = new DailyReports(readPath);
                list.WriteData($@"{writePath}\DailyReport.csv");
            }
            catch (Exception ex)
            {
                var msg = Utility.ParseException(ex);
                Console.WriteLine(msg);
            }
        }
    }
}
