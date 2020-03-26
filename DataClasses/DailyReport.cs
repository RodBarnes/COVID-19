using System;

namespace DataClasses
{
    public class DailyReport
    {
        public DailyReport() { }

        public DailyReport(string country, string state, string county, DateTime recordDate,
            int totalConfirmed, int totalRecoverd, int totalDeaths,
            int newConfirmed, int newRecovered, int newDeaths,
            int totalActive = 0, double latitude = 0, double longitude = 0)
        {
            Country = country;
            State = state;
            County = county;
            RecordDate = recordDate;
            TotalConfirmed = totalConfirmed;
            NewConfirmed = newConfirmed;
            TotalDeaths = totalDeaths;
            NewDeaths = newDeaths;
            TotalRecovered = totalRecoverd;
            NewRecovered = newRecovered;
            TotalActive = totalActive;
            Latitude = latitude;
            Longitude = longitude;
        }

        public DailyReport Clone()
        {
            var report = new DailyReport(
                Country = Country,
                State = State,
                County = County,
                RecordDate = RecordDate,
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

        #region Properties

        public string Country { get; set; }
        public string State { get; set; }
        public string County { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TotalConfirmed { get; set; }
        public int NewConfirmed { get; set; }
        public int TotalDeaths { get; set; }
        public int NewDeaths { get; set; }
        public int TotalRecovered { get; set; }
        public int NewRecovered { get; set; }
        public int TotalActive { get; set; }

        private DateTime recordDate;
        public DateTime RecordDate
        {
            get => recordDate;
            set => recordDate = new DateTime(value.Year, value.Month, value.Day);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"\"{Country}\",\"{State}\",{RecordDate},{TotalConfirmed},{TotalDeaths},{TotalRecovered}";
        }

        #endregion

    }
}
