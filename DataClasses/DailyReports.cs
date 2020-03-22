using Common;
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
        private readonly List<DailyReport> list = new List<DailyReport>();
        private TextFieldParser parser;
        private bool readHeaders = true;

        private readonly List<Replacement> replacements = new List<Replacement>();

        public DailyReports(string path = "")
        {
            ReadReplacements();
            MergeData(path);
        }

        #region Properties

        public string ColumnHeader1 { get; set; }
        public string ColumnHeader2 { get; set; }
        public string ColumnHeader3 { get; set; }
        public string ColumnHeader4 { get; set; }
        public string ColumnHeader5 { get; set; }

        #endregion

        #region Methods

        private void ReadDataFromFile(string filePath)
        {
            var firstLine = true;

            parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            do
            {
                string state;
                string region;
                string district;

                var fields = parser.ReadFields();

                //var sb = new StringBuilder();
                //for (int i = 0; i<fields.Length; i++)
                //{
                //    if (i == 0)
                //        sb.Append(fields[i]);
                //    else
                //        sb.Append($",{fields[i]}");
                //}
                //System.Diagnostics.Debug.WriteLine($"LoadData  FIELDS:{sb}");

                if (!firstLine)
                {
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

                    //System.Diagnostics.Debug.WriteLine($"LoadData BEFORE:{region},{state},{district},{recordDate},{totalConfirmed},{totalRecovered},{totalDeaths}");
                    //if (region == "Hong Kong SAR")
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
                    var prevDateObj = from rep in list
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

                    //System.Diagnostics.Debug.WriteLine($"LoadData  AFTER:{region},{state},{district},{recordDate},{totalConfirmed},{totalRecovered},{totalDeaths},{newConfirmed},{newRecovered},{newDeaths}");
                    //if (region == "Hong Kong SAR")
                    //{
                    //    System.Diagnostics.Debugger.Break();
                    //}

                    var existingReport = list.Where(i => i.Region == region && i.State == state && i.RecordDate == recordDate).FirstOrDefault();
                    if (existingReport != null)
                    {
                        // Update the existing report
                        existingReport.TotalConfirmed = totalConfirmed;
                        existingReport.TotalRecovered = totalRecovered;
                        existingReport.TotalDeaths = totalDeaths;
                        existingReport.NewConfirmed = newConfirmed;
                        existingReport.NewRecovered = newRecovered;
                        existingReport.NewDeaths = newDeaths;
                    }
                    else
                    {
                        // Add the report to the collection
                        var report = new DailyReport(region, state, district, recordDate, totalConfirmed, newConfirmed, totalDeaths, newDeaths, totalRecovered, newRecovered);
                        list.Add(report);
                    }
                }
                else
                {
                    if (readHeaders)
                    {
                        ColumnHeader1 = fields[1].Trim();
                        ColumnHeader2 = fields[0].Trim();
                        ColumnHeader3 = fields[2].Trim();
                        ColumnHeader4 = fields[3].Trim();
                        ColumnHeader5 = fields[4].Trim();
                        readHeaders = false;
                    }
                    firstLine = false;
                }
            }
            while (!parser.EndOfData);
        }

        private void AddSumsForRegion()
        {
            var regionSums = list
                .GroupBy(r => new { r.Region, r.RecordDate })
                .Select(cr => new DailyReport
                {
                    Region = cr.Key.Region,
                    State = "(All)",
                    District = "(All)",
                    RecordDate = cr.Key.RecordDate,
                    TotalConfirmed = cr.Sum(c => c.TotalConfirmed),
                    TotalRecovered = cr.Sum(c => c.TotalRecovered),
                    TotalDeaths = cr.Sum(c => c.TotalDeaths)
                }).ToList();

            var allSums = list
                .GroupBy(r => new { r.Region, r.State })
                .Select(r => new { r.Key.Region, r.Key.State })
                .Where(r => !string.IsNullOrEmpty(r.Region) && string.IsNullOrEmpty(r.State)).ToList();

            foreach (DailyReport regionSum in regionSums)
            {
                //System.Diagnostics.Debug.WriteLine($"TEST:{regionSum.Region},{regionSum.State},{regionSum.District}");

                //var tests = regionSums.Where(r => r.Region == "Taiwan").ToList();
                //foreach (var test in tests)
                //{
                //    System.Diagnostics.Debug.WriteLine($"TEST:{test.Region},{test.State},{test.District}");
                //}

                // If there is no existing (All) record, add one
                var all = allSums.Where(a => a.Region == regionSum.Region);
                var chk = allSums.Where(a => a.Region == regionSum.Region).Count();
                if (chk == 0)
                {
                    // Add the (All) for the region
                    list.Add(regionSum);
                }
                else
                {
                    //Else, update the existing (All) record
                    var fixes = list.Where(r => r.Region == regionSum.Region).ToList();
                    foreach (var fix in fixes)
                    {
                        fix.State = "(All)";
                    }
                }
            }
        }

        private void AddSumsForGlobal()
        {
            var sums = list
                .GroupBy(r => r.RecordDate)
                .Select(cr => new DailyReport
                {
                    Region = "(All)",
                    State = "(All)",
                    RecordDate = cr.Key,
                    TotalConfirmed = cr.Sum(c => c.TotalConfirmed),
                    TotalRecovered = cr.Sum(c => c.TotalRecovered),
                    TotalDeaths = cr.Sum(c => c.TotalDeaths)
                }).ToList();

            foreach (DailyReport report in sums)
            {
                list.Add(report);
            }
        }

        public void MergeData(string path)
        {
            readHeaders = true;

            var filePaths = Directory.GetFiles(path, "*.csv");

            if (filePaths.Length > 0)
            {
                foreach (var filePath in filePaths)
                {
                    ReadDataFromFile(filePath);
                }
                AddSumsForRegion();
                AddSumsForGlobal();
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

        public DailyReport this[int index] { get => list[index]; set => list[index] = value; }

        public int Count => list.Count;

        public bool IsReadOnly => ((IList<DailyReport>)list).IsReadOnly;

        public void Add(DailyReport item) => list.Add(item);

        public void Clear() => list.Clear();

        public bool Contains(DailyReport item) => list.Contains(item);

        public void CopyTo(DailyReport[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

        public IEnumerator<DailyReport> GetEnumerator() => ((IList<DailyReport>)list).GetEnumerator();
        
        public int IndexOf(DailyReport item) => list.IndexOf(item);

        public void Insert(int index, DailyReport item) => list.Insert(index, item);

        public bool Remove(DailyReport item) => list.Remove(item);

        public void RemoveAt(int index) => list.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IList<DailyReport>)list).GetEnumerator();

        #endregion
    }
}
