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

        public void Clear(DateTime? lastImportDate)
        {
            using (var db = new DatabaseConnection())
            {
                if (lastImportDate is null)
                {
                    db.ClearDataAll();
                }
                else
                {
                    db.ClearDataFromDate(lastImportDate);
                }
            }
            Clear();
        }

        public void ReadData()
        {
            using (var db = new DatabaseConnection())
            {
                db.ReportsRead(this);
            }
        }

        public void ImportSwaps(string path)
        {
            Replacements.Refresh(path);
        }

        public void ImportData(string filePath)
        {
            using (var db = new DatabaseConnection())
            {
                var firstLine = true;
                var countries = new List<string>();
                DateTime fileDate = DateTime.Now;

                var parser = new TextFieldParser(filePath);
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                do
                {
                    var fields = parser.ReadFields();
                    if (!firstLine)
                    {
                        DateTime lastUpdate;
                        string country = "";
                        string state = "";
                        string county = "";
                        string combinedKey = "";
                        bool isValid = false;
                        int totalConfirmed;
                        int totalRecovered;
                        int totalDeaths;
                        int totalActive = 0;
                        int newConfirmed = 0;
                        int newDeaths = 0;
                        int newRecovered = 0;
                        int newActive = 0;
                        double latitude = 0;
                        double longitude = 0;
                        int fips = 0;

                        isValid = DateTime.TryParse(Path.GetFileNameWithoutExtension(filePath).ToString(), out DateTime dateTimeChk);
                        fileDate = isValid ? dateTimeChk : new DateTime();

                        if (fields.Length > 8)
                        {
                            // New structure effective 3/22/2020
                            isValid = int.TryParse(fields[0], out int nbr);
                            fips = isValid ? nbr : 0;
                            county = fields[1].Trim();
                            state = fields[2].Trim();
                            country = fields[3].Trim();
                            isValid = DateTime.TryParse(fields[4], out DateTime dateTime);
                            lastUpdate = isValid ? dateTime : fileDate;
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
                            // Old structure
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

                            isValid = DateTime.TryParse(fields[2], out DateTime dateTime);
                            lastUpdate = isValid ? dateTime : fileDate;
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

                        // Calculate the total active
                        if (totalActive == 0)
                        {
                            totalActive = totalConfirmed - totalRecovered - totalDeaths;
                        }

                        // Calculate the daily change
                        var prevReport = db.ReportReadPrevious(country, state, county, fileDate);
                        if (prevReport != null)
                        {
                            newConfirmed = totalConfirmed - prevReport.TotalConfirmed;
                            newDeaths = totalDeaths - prevReport.TotalDeaths;
                            newRecovered = totalRecovered - prevReport.TotalRecovered;
                            newActive = totalActive - prevReport.TotalActive;
}
                        else
                        {
                            newConfirmed = totalConfirmed;
                            newDeaths = totalDeaths;
                            newRecovered = totalRecovered;
                        }

                        var report = db.ReportRead(country, state, county, fileDate);
                        if (report != null)
                        {
                            if (report.Country != country || report.State != state || report.County != county || report.FileDate != fileDate)
                            {
                                // Should never get here
                                throw new Exception($"Read a report matching {country},{state},{county},{fileDate:MM-dd-yyyy} but failed to match!");
                            }
                        }
                        else
                        {
                            // Add the report to the collection
                            report = new DailyReport(fileDate, country, state, county, lastUpdate, totalConfirmed, totalRecovered, totalDeaths,
                                totalActive, newConfirmed, newRecovered, newDeaths, newActive, latitude, longitude, fips);
                            db.ReportInsert(report);

                            // Add the country to list used to check for country-only entries
                            if (!countries.Exists(i => i == country))
                            {
                                countries.Add(country);
                            }
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

                // Add missing country-only entries
                foreach (var country in countries)
                {
                    var found = db.ReportExists(country, "", "", fileDate);
                    if (!found)
                    {
                        db.CountryInsert(country, fileDate);
                    }
                }
            }
        }

        private void ExtractHeadersFromFields(string[] fields)
        {
            if (fields.Length > 8)
            {
                ColumnHeader04 = fields[0].Trim();  // FIPS
                ColumnHeader03 = fields[1].Trim();  // Admin2
                ColumnHeader02 = fields[2].Trim();  // ProvinceState
                ColumnHeader01 = fields[3].Trim();  // CountryRegion
                ColumnHeader05 = fields[4].Trim();  // LastUpdate
                ColumnHeader06 = fields[5].Trim();  // Latitude
                ColumnHeader07 = fields[6].Trim();  // Longitude
                ColumnHeader08 = fields[7].Trim();  // Confirmed
                ColumnHeader09 = fields[8].Trim();  // Deaths
                ColumnHeader10 = fields[9].Trim();  // Recovered
                ColumnHeader11 = fields[10].Trim(); // Active
                ColumnHeader12 = fields[11].Trim(); // CombinedKey
            }
            else
            {
                ColumnHeader02 = fields[0].Trim();  // ProvinceState
                ColumnHeader01 = fields[1].Trim();  // CountryRegion
                ColumnHeader03 = fields[2].Trim();  // LastUpdate
                ColumnHeader04 = fields[3].Trim();  // Confirmed
                ColumnHeader05 = fields[4].Trim();  // Deaths
                ColumnHeader06 = fields[5].Trim();  // Recovered
                if (fields.Length > 6)
                {
                    ColumnHeader07 = fields[6].Trim();  // Latitude
                    ColumnHeader08 = fields[7].Trim();  // Longitude
                }
            }
        }

        public void AddGlobalSums()
        {
            // Get a list by date rolled up across all regions
            var sums = reports
                .GroupBy(i => i.FileDate)
                .Select(g => new DailyReport
                {
                    Country = "(GLOBAL)",
                    FileDate = g.Key,
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

        public void Clear() => reports.Clear();

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
