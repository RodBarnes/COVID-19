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
        private const string DateFormat = "yyyy-MM-dd";
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

        public string ColumnHeader1 { get; set; }
        public string ColumnHeader2 { get; set; }
        public string ColumnHeader3 { get; set; }
        public string ColumnHeader4 { get; set; }
        public string ColumnHeader5 { get; set; }

        #endregion

        #region Methods

        private void GetDailyFromFile(string filePath)
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
                        report.TotalConfirmed = totalConfirmed;
                        report.TotalRecovered = totalRecovered;
                        report.TotalDeaths = totalDeaths;
                        report.NewConfirmed = newConfirmed;
                        report.NewRecovered = newRecovered;
                        report.NewDeaths = newDeaths;
                    }
                    else
                    {
                        // Add the report to the collection
                        report = new DailyReport(region, state, district, recordDate, totalConfirmed, newConfirmed, totalDeaths, newDeaths, totalRecovered, newRecovered);
                        reports.Add(report);
                    }

                    //System.Diagnostics.Debug.WriteLine($"DAILY: {report.RecordDate.ToString(DateFormat)},{report.Region},{report.State},{report.District},{report.TotalConfirmed},{report.TotalRecovered},{report.TotalDeaths}");
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

        private void AddStateSums(List<DailyReport> sums)
        {
            // Get a list of Region, State, District where District is blank or missing
            var allSums = reports
                .GroupBy(i => new { i.Region, i.State, i.District })
                .Select(i => new { i.Key.Region, i.Key.State, i.Key.District })
                .Where(i => !string.IsNullOrEmpty(i.Region) && !string.IsNullOrEmpty(i.State) && string.IsNullOrEmpty(i.District)).ToList();

            //foreach (var all in allSums)
            //{
            //    System.Diagnostics.Debug.WriteLine($"REGION:{all.Region},{all.State},{all.District}");
            //}

            foreach (DailyReport sum in sums)
            {
                //System.Diagnostics.Debug.WriteLine($"STATE: {sum.RecordDate.ToString(DateFormat)},{sum.Region},{sum.State},{sum.District},{sum.TotalConfirmed},{sum.TotalRecovered},{sum.TotalDeaths}");

                //var tests = sums.Where(r => r.Region == "Taiwan").ToList();
                //foreach (var test in tests)
                //{
                //    System.Diagnostics.Debug.WriteLine($"STATE: {test.Region},{test.State},{test.District}");
                //}

                // If there is no existing District = (All) record, add one
                var chk = allSums.Where(a => a.Region == sum.Region && a.State == sum.State).Count();
                if (chk == 0)
                {
                    // Add the (All) for the region
                    reports.Add(sum);
                }
                else
                {
                    //Else, update the existing District = (All) record
                    var fixes = reports.Where(r => r.Region == sum.Region && r.State == sum.State).ToList();
                    foreach (var fix in fixes)
                    {
                        fix.District = "(All)";
                    }
                }
            }
        }

        private void AddRegionSums(List<DailyReport> sums)
        {
            // Get a list of Region, State where State is blank or missing
            var allSums = reports
                .GroupBy(r => new { r.Region, r.State })
                .Select(r => new { r.Key.Region, r.Key.State })
                .Where(r => !string.IsNullOrEmpty(r.Region) && string.IsNullOrEmpty(r.State)).ToList();

            //foreach (var all in allSums)
            //{
            //    System.Diagnostics.Debug.WriteLine($"REGION:{all.Region},{all.State}");
            //}

            foreach (DailyReport sum in sums)
            {
                //System.Diagnostics.Debug.WriteLine($"REGION:{sum.RecordDate.ToString(DateFormat)},{sum.Region},{sum.State},{sum.District},{sum.TotalConfirmed},{sum.TotalRecovered},{sum.TotalDeaths}");

                //var tests = sums.Where(r => r.Region == "Taiwan").ToList();
                //foreach (var test in tests)
                //{
                //    System.Diagnostics.Debug.WriteLine($"REGION:{test.Region},{test.State},{test.District}");
                //}

                // If there is no existing State = (All) record, add one
                var chk = allSums.Where(a => a.Region == sum.Region).Count();
                if (chk == 0)
                {
                    // Add the (All) for the region
                    reports.Add(sum);
                }
                else
                {
                    //Else, update the existing State = (All) record
                    var fixes = reports.Where(r => r.Region == sum.Region).ToList();
                    foreach (var fix in fixes)
                    {
                        fix.State = "(All)";
                    }
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

        private List<DailyReport> CalculateStateSums()
        {
            // Get a list of counts by date rolled up to Region, State
            return reports
                .GroupBy(r => new { r.Region, r.State, r.RecordDate })
                .Select(g => new DailyReport
                {
                    Region = g.Key.Region,
                    State = g.Key.State,
                    District = "(All)",
                    RecordDate = g.Key.RecordDate,
                    TotalConfirmed = g.Sum(s => s.TotalConfirmed),
                    TotalRecovered = g.Sum(s => s.TotalRecovered),
                    TotalDeaths = g.Sum(s => s.TotalDeaths),
                    NewConfirmed = g.Sum(s => s.NewConfirmed),
                    NewRecovered = g.Sum(s => s.NewRecovered),
                    NewDeaths = g.Sum(s => s.NewDeaths)
                }).ToList();
        }

        private List<DailyReport> CalculateRegionSums()
        {
            // Get a list of counts by date rolled up to region
            return reports
                .GroupBy(r => new { r.Region, r.RecordDate })
                .Select(g => new DailyReport
                {
                    Region = g.Key.Region,
                    State = "(All)",
                    District = "(All)",
                    RecordDate = g.Key.RecordDate,
                    TotalConfirmed = g.Sum(s => s.TotalConfirmed),
                    TotalRecovered = g.Sum(s => s.TotalRecovered),
                    TotalDeaths = g.Sum(s => s.TotalDeaths),
                    NewConfirmed = g.Sum(s => s.NewConfirmed),
                    NewRecovered = g.Sum(s => s.NewRecovered),
                    NewDeaths = g.Sum(s => s.NewDeaths)
                }).ToList();
        }

        private List<DailyReport> CalculateGlobalSums()
        {
            // Get a list by date rolled up across all regions
            return reports
                .GroupBy(i => i.RecordDate)
                .Select(g => new DailyReport
                {
                    Region = "(All)",
                    State = "(All)",
                    District = "(All)",
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
                    GetDailyFromFile(filePath);
                }
                //var stateSums = CalculateStateSums();
                var regionSums = CalculateRegionSums();
                var globalSums = CalculateGlobalSums();
                AddGlobalSums(globalSums);
                AddRegionSums(regionSums);
                //AddStateSums(stateSums);
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
