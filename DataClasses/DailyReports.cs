using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataClasses
{
    public class DailyReports : IList<DailyReport>
    {
        //private const string DateFormat = "yyyy-MM-dd";
        private readonly List<DailyReport> reports = new List<DailyReport>();
        private TextFieldParser parser;
        private bool readHeaders = true;

        private readonly List<Replacement> replacements = new List<Replacement>();

        public DailyReports() { }

        //public DailyReports(string path)
        //{
        //    ReadReplacements();
        //    MergeData(path);
        //}

        #region Properties

        public string ColumnHeader01 { get; set; }
        public string ColumnHeader02 { get; set; }
        public string ColumnHeader03 { get; set; }
        public string ColumnHeader04 { get; set; }
        public string ColumnHeader05 { get; set; }
        public string ColumnHeader06 { get; set; }
        public string ColumnHeader07 { get; set; }
        public string ColumnHeader08 { get; set; }
        public string ColumnHeader09 { get; set; }
        public string ColumnHeader10 { get; set; }
        public string ColumnHeader11 { get; set; }
        public string ColumnHeader12 { get; set; }

        #endregion

        #region Methods

        public void ReadDailyFiles(string filePath)
        {
            var firstLine = true;
            var fileDate = Path.GetFileNameWithoutExtension(filePath);

            parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            do
            {
                var fields = parser.ReadFields();
                if (!firstLine)
                {
                    // New data structure

                    //var sb = new StringBuilder();
                    //for (int i = 0; i<fields.Length; i++)
                    //{
                    //    if (i == 0)
                    //        sb.Append(fields[i]);
                    //    else
                    //        sb.Append($",{fields[i]}");
                    //}
                    //System.Diagnostics.Debug.WriteLine($"LoadData  FIELDS:{sb}");

                    bool isValid = false;
                    string region;
                    string state;
                    DateTime recordDate;
                    int totalConfirmed;
                    int totalRecovered;
                    int totalDeaths;
                    double latitude = 0;
                    double longitude = 0;
                    int fips = 0;
                    string county = "";
                    int totalActive = 0;
                    string combinedKey = "";

                    if (fields.Length > 8)
                    {
                        isValid = int.TryParse(fields[0], out int nbr);
                        fips = isValid ? nbr : 0;
                        county = fields[1].Trim();
                        state = fields[2].Trim();
                        region = fields[3].Trim();
                        isValid = DateTime.TryParse(Path.GetFileNameWithoutExtension(filePath).ToString(), out DateTime dateTime);
                        recordDate = isValid ? new DateTime(dateTime.Year, dateTime.Month, dateTime.Day) : new DateTime();
                        isValid = double.TryParse(fields[5], out double lat);
                        latitude = isValid ? lat : 0;
                        isValid = double.TryParse(fields[6], out double lng);
                        longitude = isValid ? lng : 0;
                        isValid = int.TryParse(fields[7], out int confirmed);
                        totalConfirmed = isValid ? confirmed : 0;
                        isValid = int.TryParse(fields[8], out int deaths);
                        totalDeaths = isValid ? deaths : 0;
                        isValid = int.TryParse(fields[9], out int recovered);
                        totalRecovered = isValid ? recovered : 0;
                        isValid = int.TryParse(fields[10], out int active);
                        totalActive = isValid ? active : 0;
                        combinedKey = fields[11].Trim();
                    }
                    else
                    {
                        if (fields[0].Contains(','))
                        {
                            var split = fields[0].Split(',');
                            county = split[0].Trim();
                            state = split[1].Trim();
                            region = fields[1].Trim();
                        }
                        else
                        {
                            county = "";
                            state = fields[0].Trim();
                            region = fields[1].Trim();
                        }

                        isValid = DateTime.TryParse(Path.GetFileNameWithoutExtension(filePath).ToString(), out DateTime dateTime);
                        recordDate = isValid ? dateTime : new DateTime();
                        isValid = int.TryParse(fields[3], out int confirmed);
                        totalConfirmed = isValid ? confirmed : 0;
                        isValid = int.TryParse(fields[4], out int deaths);
                        totalDeaths = isValid ? deaths : 0;
                        isValid = int.TryParse(fields[5], out int recovered);
                        totalRecovered = isValid ? recovered : 0;
                        if (fields.Length > 6)
                        {
                            isValid = double.TryParse(fields[6], out double lat);
                            latitude = isValid ? lat : 0;
                            isValid = double.TryParse(fields[7], out double lng);
                            longitude = isValid ? lng : 0;
                        }
                    }
                    //System.Diagnostics.Debug.WriteLine($"LoadData BEFORE:{region},{state},{district},{recordDate},{totalConfirmed},{totalRecovered},{totalDeaths}");
                    //if (region == "France" && recordDate > new DateTime(2020, 3, 10))
                    //{
                    //    System.Diagnostics.Debugger.Break();
                    //}

                    foreach (var rep in replacements)
                    {
                        switch (rep.ReplacementType)
                        {
                            case 1:
                                if (region == rep.FromRegion)
                                {
                                    region = rep.ToRegion;
                                }
                                break;
                            case 2:
                                if (region == rep.FromRegion && state == rep.FromState)
                                {
                                    region = rep.ToRegion;
                                    state = rep.ToState;
                                }
                                break;
                            case 3:
                                if (region == rep.FromRegion && state == rep.FromState && county == rep.FromCounty)
                                {
                                    region = rep.ToRegion;
                                    state = rep.ToState;
                                    county = rep.ToCounty;
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    // Calculate the daily change
                    var newConfirmed = 0;
                    var newDeaths = 0;
                    var newRecovered = 0;
                    var prevDateObj = from rep in reports
                                        group rep by new { rep.Country, rep.State, rep.County } into g
                                        where g.Key.Country == region && g.Key.State == state && g.Key.County == county
                                        select g.OrderByDescending(t => t.RecordDate).FirstOrDefault();
                    if (prevDateObj.Count() > 0)
                    {
                        var prevReport = prevDateObj.First();
                        newConfirmed = totalConfirmed - prevReport.TotalConfirmed;
                        newDeaths = totalDeaths - prevReport.TotalDeaths;
                        newRecovered = totalRecovered - prevReport.TotalRecovered;
                    }

                    //System.Diagnostics.Debug.WriteLine($"DAILY: {region},{state},{district},{recordDate},{totalConfirmed},{totalRecovered},{totalDeaths},{newConfirmed},{newRecovered},{newDeaths}");
                    //if (region == "France" && recordDate > new DateTime(2020, 3, 10))
                    //{
                    //    System.Diagnostics.Debugger.Break();
                    //}

                    var report = reports.Where(i => i.Country == region && i.State == state && i.RecordDate == recordDate).FirstOrDefault();
                    if (report != null)
                    {
                        // Update the existing report
                        UpdateReport(report, totalConfirmed, totalDeaths, totalRecovered, newConfirmed, newDeaths, newRecovered);
                    }
                    else
                    {
                        // Add the report to the collection
                        report = new DailyReport(region, state, county, recordDate, totalConfirmed, newConfirmed, totalDeaths, newDeaths, totalRecovered, newRecovered, totalActive, latitude, longitude);
                        reports.Add(report);
                    }

                    //System.Diagnostics.Debug.WriteLine($"DAILY: {report.RecordDate.ToString(DateFormat)},{report.Region},{report.State},{report.County},{report.TotalConfirmed},{report.TotalRecovered},{report.TotalDeaths}");
                }
                else
                {
                    if (readHeaders)
                    {
                        ExtractHeadersFromFields(fields);
                        readHeaders = false;
                    }
                    firstLine = false;
                }
            }
            while (!parser.EndOfData);
        }

        private static void UpdateReport(DailyReport report, int totalConfirmed, int totalDeaths, int totalRecovered, int newConfirmed, int newDeaths, int newRecovered)
        {
            report.TotalConfirmed = totalConfirmed;
            report.TotalRecovered = totalRecovered;
            report.TotalDeaths = totalDeaths;
            report.NewConfirmed = newConfirmed;
            report.NewRecovered = newRecovered;
            report.NewDeaths = newDeaths;
        }

        private void ExtractHeadersFromFields(string[] fields)
        {
            if (fields.Length > 8)
            {
                ColumnHeader01 = fields[1].Trim();
                ColumnHeader02 = fields[0].Trim();
                ColumnHeader03 = fields[2].Trim();
                ColumnHeader04 = fields[3].Trim();
                ColumnHeader05 = fields[4].Trim();
                ColumnHeader06 = fields[5].Trim();
                ColumnHeader07 = fields[6].Trim();
                ColumnHeader08 = fields[7].Trim();
                ColumnHeader09 = fields[8].Trim();
                ColumnHeader10 = fields[9].Trim();
                ColumnHeader11 = fields[10].Trim();
                ColumnHeader12 = fields[12].Trim();
            }
            else
            {
                ColumnHeader01 = fields[1].Trim();
                ColumnHeader02 = fields[0].Trim();
                ColumnHeader03 = fields[2].Trim();
                ColumnHeader04 = fields[3].Trim();
                ColumnHeader05 = fields[4].Trim();
                ColumnHeader06 = fields[5].Trim();
                if (fields.Length > 6)
                {
                    ColumnHeader07 = fields[6].Trim();
                    ColumnHeader08 = fields[7].Trim();
                }
            }
        }

        //public void MergeData(string path)
        //{
        //    readHeaders = true;

        //    var filePaths = Directory.GetFiles(path, "*.csv");

        //    if (filePaths.Length > 0)
        //    {
        //        foreach (var filePath in filePaths)
        //        {
        //            ReadDailyFiles(filePath);
        //        }
        //        //AddMissingRecords();
        //        //AddGlobalSums();
        //    }
        //    else
        //    {
        //        throw new FileNotFoundException($"No files found at path '{path}'.");
        //    }
        //}

        //public void AddMissingRecords()
        //{
        //    var minDate = reports.Min(r => r.RecordDate);
        //    var maxDate = reports.Max(r => r.RecordDate);
        //    var days = maxDate.Subtract(minDate).Days;
        //    var list = reports
        //        .Select(r => new { r.Country, r.State, r.County, r.RecordDate })
        //        .Distinct()
        //        .OrderBy(r => r.Country)
        //        .ThenBy(r => r.State)
        //        .ThenBy(r => r.County)
        //        .ThenBy(r => r.RecordDate);

        //    foreach (var item in list)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"{item.RecordDate},{item.Country},{item.State},{item.County}");
        //    }

        //    // Examine each dimension...
        //    foreach (var item in list)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"{item.RecordDate},{item.Country},{item.State},{item.County}");

        //        var curDate = minDate;
        //        // Checking for each date in the full range
        //        do
        //        {
        //            var report = reports
        //                .Where(r => r.RecordDate <= curDate && r.Country == item.Country && r.State == item.State && r.County == item.County)
        //                .OrderByDescending(r => r.RecordDate)
        //                .FirstOrDefault();
        //            if (report == null)
        //            {
        //                // We found no report older or the same age as the date being checked
        //                // so we need to fill in the older dates
        //                report = reports
        //                .Where(r => r.Country == item.Country && r.State == item.State && r.County == item.County)
        //                .OrderBy(r => r.RecordDate)
        //                .FirstOrDefault();

        //                var chkDate = minDate;
        //                do
        //                {
        //                    // Make a copy of the existing report, give it the new RecordDate, then add it to the reports
        //                    var newReport = report.Clone();
        //                    newReport.RecordDate = chkDate;
        //                    newReport.TotalConfirmed = newReport.TotalRecovered = newReport.TotalRecovered = newReport.TotalActive = 0;
        //                    reports.Add(newReport);

        //                    chkDate = chkDate.AddDays(1);
        //                }
        //                while (chkDate < report.RecordDate);
        //            }
        //            else if (report.RecordDate < curDate)
        //            {
        //                // We found a report for the same dimension but with a date older then the current date being checked
        //                // so we need to fill in the missing dates
        //                var chkDate = report.RecordDate.AddDays(1);
        //                do
        //                {
        //                    // Make a copy of the existing report, give it the new RecordDate, then add it to the reports
        //                    var newReport = report.Clone();
        //                    newReport.RecordDate = chkDate;
        //                    reports.Add(newReport);

        //                    chkDate = chkDate.AddDays(1);
        //                }
        //                while (chkDate <= curDate);
        //            }
        //            curDate = curDate.AddDays(1);
        //        }
        //        while (curDate <= maxDate);
        //    }
        //}

        public void AddGlobalSums()
        {
            // Get a list by date rolled up across all regions
            var sums = reports
                .GroupBy(i => i.RecordDate)
                .Select(g => new DailyReport
                {
                    Country = "(GLOBAL)",
                    RecordDate = g.Key,
                    TotalConfirmed = g.Sum(s => s.TotalConfirmed),
                    TotalRecovered = g.Sum(s => s.TotalRecovered),
                    TotalDeaths = g.Sum(s => s.TotalDeaths),
                    NewConfirmed = g.Sum(s => s.NewConfirmed),
                    NewRecovered = g.Sum(s => s.NewRecovered),
                    NewDeaths = g.Sum(s => s.NewDeaths)
                }).ToList();

            foreach (DailyReport sum in sums)
            {
                reports.Add(sum);
            }
        }

        public void ReadReplacements()
        {
            var dir = Directory.GetCurrentDirectory();
            var filePath = $@"{dir}\Replacements.csv";
            using (parser = new TextFieldParser(filePath))
            {
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                do
                {
                    var fields = parser.ReadFields();
                    replacements.Add(new Replacement(fields));
                }
                while (!parser.EndOfData);
            }
        }

        #endregion

        #region Standard List Operations

        public DailyReport this[int index] { get => reports[index]; set => reports[index] = value; }

        public int Count => reports.Count;

        public bool IsReadOnly => ((IList<DailyReport>)reports).IsReadOnly;

        public void Add(DailyReport item) => reports.Add(item);

        public void Clear() => reports.Clear();

        public bool Contains(DailyReport item) => reports.Contains(item);

        public void CopyTo(DailyReport[] array, int arrayIndex) => reports.CopyTo(array, arrayIndex);

        public IEnumerator<DailyReport> GetEnumerator() => ((IList<DailyReport>)reports).GetEnumerator();
        
        public int IndexOf(DailyReport item) => reports.IndexOf(item);

        public void Insert(int index, DailyReport item) => reports.Insert(index, item);

        public bool Remove(DailyReport item) => reports.Remove(item);

        public void RemoveAt(int index) => reports.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IList<DailyReport>)reports).GetEnumerator();

        #endregion
    }
}
