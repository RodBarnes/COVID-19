using System;

namespace DataClasses
{
    public class DailyReport
    {
        public DailyReport() { }

        public DailyReport(DateTime fileDate, string country, string state, string county, DateTime lastUpdate,
            int totalConfirmed, int totalRecoverd, int totalDeaths,
            int newConfirmed, int newRecovered, int newDeaths,
            int totalActive = 0, double latitude = 0, double longitude = 0)
        {
            FileDate = fileDate;
            Country = country;
            State = state;
            County = county;
            LastUpdate = lastUpdate;
            TotalConfirmed = totalConfirmed;
            TotalRecovered = totalRecoverd;
            TotalDeaths = totalDeaths;
            NewConfirmed = newConfirmed;
            NewRecovered = newRecovered;
            NewDeaths = newDeaths;
            TotalActive = totalActive;
            Latitude = latitude;
            Longitude = longitude;
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
        public int NewConfirmed { get; set; }
        public int NewRecovered { get; set; }
        public int NewDeaths { get; set; }
        public int TotalActive { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

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
                NewConfirmed = NewConfirmed,
                NewRecovered = NewRecovered,
                NewDeaths = NewDeaths,
                TotalActive = TotalActive,
                Latitude = Latitude,
                Longitude = Longitude
            );

            return report;
        }

        public override string ToString()
        {
            return $"\"{Country}\",\"{State}\",{FileDate},{TotalConfirmed},{TotalDeaths},{TotalRecovered}";
        }

        #endregion

    }
}
