using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Common;

namespace DataClasses
{
    public static class DatabaseManager
    {
        private const string GLOBAL_NAME = "(GLOBAL)";

        private static readonly List<DailyReport> reports = new List<DailyReport>();
        private static bool readHeaders = true;
        private static readonly Replacements Replacements = new Replacements();

        #region Properties

        public static string ColumnHeader01 { get; set; }
        public static string ColumnHeader02 { get; set; }
        public static string ColumnHeader03 { get; set; }
        public static string ColumnHeader04 { get; set; }
        public static string ColumnHeader05 { get; set; }
        public static string ColumnHeader06 { get; set; }
        public static string ColumnHeader07 { get; set; }
        public static string ColumnHeader08 { get; set; }
        public static string ColumnHeader09 { get; set; }
        public static string ColumnHeader10 { get; set; }
        public static string ColumnHeader11 { get; set; }
        public static string ColumnHeader12 { get; set; }

        #endregion

        #region Methods

        public static void Clear(DateTime lastImportDate)
        {
            using (var db = new DatabaseConnection())
            {
                db.ClearDataFromDate(lastImportDate);
            }
            reports.Clear();
        }

        public static void ClearAll(string scriptPath)
        {
            using (var db = new DatabaseConnection())
            {
                db.ClearDataAll(scriptPath);
            }
            reports.Clear();
        }

        public static bool ImportSwaps(string path, DateTime datetime) => Replacements.Refresh(path, datetime);

        public static List<DailyReport> ReadDailyTotalsForReport(TotalReport report)
        {
            List<DailyReport> list = new List<DailyReport>();
            using (var db = new DatabaseConnection())
            {
                if (string.IsNullOrEmpty(report.Country) || report.Country == GLOBAL_NAME)
                {
                    list = db.GlobalDailiesRead(report);
                }
                else if (string.IsNullOrEmpty(report.State))
                {
                    list = db.CountryRegionDailiesRead(report);
                }
                else
                {
                    list = db.CountryRegionStateProvinceDailiesRead(report);
                }
            }

            return list;
        }

        public static List<TotalReport> ReadTotalReports()
        {
            var list = new List<TotalReport>();
            using (var db = new DatabaseConnection())
            {
                db.CountryRegionStateProvinceTotalsRead(list);
                db.CountryRegionTotalsRead(list);
                db.GlobalTotalsRead(list);
            }

            return list;
        }

        public static void ImportData(string filePath, BackgroundWorker worker = null, double maxProgressValue = 0)
        {
            using (var db = new DatabaseConnection())
            {
                // Used to provide progress bar values
                long byteCount = 0;
                var sb = new StringBuilder();

                // Used when checking for missing country-only entries
                var countries = new List<string>();
                DateTime fileDate = DateTime.Now;

                // Used to parse each file
                using (var parser = new TextFieldParser(filePath))
                {
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;

                    var firstLine = true;
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

                            Replacements.Swap(ref country, ref state, ref county);

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
                            else if (!string.IsNullOrEmpty(country))
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

                        if (worker != null)
                        {
                            // Calculate bytes to move progress bar
                            sb.Clear();
                            foreach (var field in fields)
                            {
                                sb.Append(field);
                            }
                            byteCount += (sb.Length + fields.Length + 1);
                            worker.ReportProgress((int)(byteCount * maxProgressValue / parser.Length));
                        }
                    }
                    while (!parser.EndOfData);
                }

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

        private static void ExtractHeadersFromFields(string[] fields)
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

        #endregion
    }
}
