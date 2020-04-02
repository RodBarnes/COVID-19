using System;

namespace DataClasses
{
    /// <summary>
    /// DailyReport is a class that contains the counts for a Country, State for a given date
    /// Each DailyReport has the Country,State,and totals for that one date
    /// A DailyReport is built during import for each record and written to the database
    /// List<DailyReport> is built from a selected TotalReport's Country,State and is used during DataGrid display
    /// </summary>
    public class DailyReport
    {
        public DailyReport() { }

        public DailyReport(DateTime fileDate, string country, string state, string county, DateTime lastUpdate,
            int totalConfirmed, int totalRecoverd, int totalDeaths, int totalActive,
            int newConfirmed, int newRecovered, int newDeaths, int newActive,
            double latitude = 0, double longitude = 0, int fips = 0)
        {
            FileDate = fileDate;
            Country = country;
            State = state;
            County = county;
            LastUpdate = lastUpdate;
            TotalConfirmed = totalConfirmed;
            TotalRecovered = totalRecoverd;
            TotalDeaths = totalDeaths;
            TotalActive = totalActive;
            NewConfirmed = newConfirmed;
            NewRecovered = newRecovered;
            NewDeaths = newDeaths;
            NewActive = newActive;
            Latitude = latitude;
            Longitude = longitude;
            FIPS = fips;
        }

        #region Properties

        private DateTime fileDate;
        public DateTime FileDate
        {
            get => fileDate;
            set => fileDate = new DateTime(value.Year, value.Month, value.Day);
        }

        public string Country { get; set; }
        public string State { get; set; }
        public string County { get; set; }
        public DateTime LastUpdate { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalRecovered { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalActive { get; set; }
        public int NewConfirmed { get; set; }
        public int NewRecovered { get; set; }
        public int NewDeaths { get; set; }
        public int NewActive { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int FIPS { get; set; }

        #endregion

        #region Methods

        public DailyReport Clone()
        {
            var report = new DailyReport(
                FileDate = FileDate,
                Country = Country,
                State = State,
                County = County,
                LastUpdate = LastUpdate,
                TotalConfirmed = TotalConfirmed,
                TotalRecovered = TotalRecovered,
                TotalDeaths = TotalDeaths,
                TotalActive = TotalActive,
                NewConfirmed = NewConfirmed,
                NewRecovered = NewRecovered,
                NewDeaths = NewDeaths,
                NewActive = NewActive,
                Latitude = Latitude,
                Longitude = Longitude
            );

            return report;
        }

        #endregion
    }
}
