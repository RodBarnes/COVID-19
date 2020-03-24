﻿using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataClasses
{
    public class DailyReports : IList<DailyReport>
    {
        //private const string DateFormat = "yyyy-MM-dd";
        private readonly List<DailyReport> reports = new List<DailyReport>();
        private TextFieldParser parser;
        private bool readHeaders = true;

        private readonly List<Replacement> replacements = new List<Replacement>();

        public DailyReports(string path = "")
        {
            ReadReplacements();
            MergeData(path);
        }

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

        private void ReadDailyFiles(string filePath)
        {
            var firstLine = true;
            var fileDate = Path.GetFileNameWithoutExtension(filePath);

            parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            do
            {
                string state;
                string region;
                string district;

                var fields = parser.ReadFields();
                if (!firstLine)
                {
                    if (fields.Length > 8)
                    {
                    }
                    else
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

                        if (fields[0].Contains(','))
                        {
                            var split = fields[0].Split(',');
                            district = split[0].Trim();
                            state = split[1].Trim();
                            region = fields[1].Trim();
                        }
                        else
                        {
                            district = "";
                            state = fields[0].Trim();
                            region = fields[1].Trim();
                        }

                        var dateTime = DateTime.Parse(fields[2]);
                        var recordDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
                        int.TryParse(fields[3], out int totalConfirmed);
                        int.TryParse(fields[4], out int totalDeaths);
                        int.TryParse(fields[5], out int totalRecovered);
                        double latitude = 0;
                        double longitude = 0;
                        if (fields.Length > 6)
                        {
                            double.TryParse(fields[6], out double lat);
                            double.TryParse(fields[7], out double lng);
                            latitude = lat;
                            longitude = lng;
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
                                    if (region == rep.FromRegion && state == rep.FromState && district == rep.FromDistrict)
                                    {
                                        region = rep.ToRegion;
                                        state = rep.ToState;
                                        district = rep.ToDistrict;
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
                                            group rep by new { rep.Region, rep.State, rep.District } into g
                                            where g.Key.Region == region && g.Key.State == state && g.Key.District == district
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

                        var report = reports.Where(i => i.Region == region && i.State == state && i.RecordDate == recordDate).FirstOrDefault();
                        if (report != null)
                        {
                            // Update the existing report
                            UpdateReport(report, totalConfirmed, totalDeaths, totalRecovered, newConfirmed, newDeaths, newRecovered);
                        }
                        else
                        {
                            // Add the report to the collection
                            report = new DailyReport(region, state, district, recordDate, totalConfirmed, newConfirmed, totalDeaths, newDeaths, totalRecovered, newRecovered, latitude, longitude);
                            reports.Add(report);
                        }

                        //System.Diagnostics.Debug.WriteLine($"DAILY: {report.RecordDate.ToString(DateFormat)},{report.Region},{report.State},{report.District},{report.TotalConfirmed},{report.TotalRecovered},{report.TotalDeaths}");
                    }
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

        private void AddGlobalSums(List<DailyReport> sums)
        {
            foreach (DailyReport sum in sums)
            {
                //System.Diagnostics.Debug.WriteLine($"GLOBAL:{sum.RecordDate.ToString(DateFormat)},{sum.Region},{sum.State},{sum.District},{sum.TotalConfirmed},{sum.TotalRecovered},{sum.TotalDeaths}");

                //var tests = sums.Where(r => r.Region == "Taiwan").ToList();
                //foreach (var test in tests)
                //{
                //    System.Diagnostics.Debug.WriteLine($"GLOBAL:{test.Region},{test.State},{test.District}");

                reports.Add(sum);
            }
        }

        private List<DailyReport> CalculateGlobalSums()
        {
            // Get a list by date rolled up across all regions
            return reports
                .GroupBy(i => i.RecordDate)
                .Select(g => new DailyReport
                {
                    Region = "(GLOBAL)",
                    //State = "(All)",
                    //District = "(All)",
                    RecordDate = g.Key,
                    TotalConfirmed = g.Sum(s => s.TotalConfirmed),
                    TotalRecovered = g.Sum(s => s.TotalRecovered),
                    TotalDeaths = g.Sum(s => s.TotalDeaths),
                    NewConfirmed = g.Sum(s => s.NewConfirmed),
                    NewRecovered = g.Sum(s => s.NewRecovered),
                    NewDeaths = g.Sum(s => s.NewDeaths)
                }).ToList();
        }

        public void MergeData(string path)
        {
            readHeaders = true;

            var filePaths = Directory.GetFiles(path, "*.csv");

            if (filePaths.Length > 0)
            {
                foreach (var filePath in filePaths)
                {
                    ReadDailyFiles(filePath);
                }
                var globalSums = CalculateGlobalSums();
                AddGlobalSums(globalSums);
            }
            else
            {
                throw new FileNotFoundException($"No files found at path '{path}'.");
            }
        }

        private void ReadReplacements()
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
