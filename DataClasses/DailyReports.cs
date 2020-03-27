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
        private List<DailyReport> reports = new List<DailyReport>();
        private bool readHeaders = true;
        private readonly Replacements Replacements = new Replacements();

        public DailyReports() { }

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

        public void Clear()
        {
            using (var db = new DatabaseConnection())
            {
                db.ClearData();
            }
            reports.Clear();
            Replacements.Clear();
        }


        public void ReadData()
        {
            using (var db = new DatabaseConnection())
            {
                db.ReportsRead(this);
            }
        }

        public void ReplacementsRefresh(string path)
        {
            Replacements.Refresh(path);
        }

        public void DataRefresh(string filePath)
        {
            var firstLine = true;
            var fileDate = Path.GetFileNameWithoutExtension(filePath);

            using (var db = new DatabaseConnection())
            {
                var parser = new TextFieldParser(filePath);
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                do
                {
                    var fields = parser.ReadFields();
                    if (!firstLine)
                    {
                        bool isValid = false;
                        string country;
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
                            country = fields[3].Trim();
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
                                country = fields[1].Trim();
                            }
                            else
                            {
                                county = "";
                                state = fields[0].Trim();
                                country = fields[1].Trim();
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

                        Replacements.Apply(ref country, ref state, ref county);

                        // Calculate the daily change
                        var newConfirmed = 0;
                        var newDeaths = 0;
                        var newRecovered = 0;
                        var prevReport = db.ReportReadPrevious(country, state, county, recordDate);
                        if (prevReport != null)
                        {
                            newConfirmed = totalConfirmed - prevReport.TotalConfirmed;
                            newDeaths = totalDeaths - prevReport.TotalDeaths;
                            newRecovered = totalRecovered - prevReport.TotalRecovered;
                        }

                        var report = db.ReportRead(country, state, county, recordDate);
                        if (report != null)
                        {
                            if (report.Country != country || report.State != state || report.County != county || report.RecordDate != recordDate)
                            {
                                // Should never get here
                                throw new Exception($"Read a report matching {country},{state},{county},{recordDate:MM-dd-yyyy} but failed to match!");
                            }
                        }
                        else
                        {
                            // Add the report to the collection
                            report = new DailyReport(country, state, county, recordDate, totalConfirmed, totalRecovered, totalDeaths,
                                newConfirmed, newRecovered, newDeaths, totalActive, latitude, longitude);
                            db.ReportInsert(report);
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

        #endregion

        #region Standard List Operations

        public DailyReport this[int index] { get => reports[index]; set => reports[index] = value; }

        public int Count => reports.Count;

        public bool IsReadOnly => ((IList<DailyReport>)reports).IsReadOnly;

        public void Add(DailyReport item) => reports.Add(item);

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
