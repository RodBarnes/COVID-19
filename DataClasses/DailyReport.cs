using System;

namespace DataClasses
{
    public class DailyReport
    {
        public DailyReport() { }

        public DailyReport(string countryRegion, string provinceState, DateTime recordDate, int confirmed, int deaths, int recovered)
        {
            CountryRegion = countryRegion;
            ProvinceState = provinceState;
            RecordDate = recordDate;
            Confirmed = confirmed;
            Deaths = deaths;
            Recovered = recovered;
        }

        #region Properties

        public string CountryRegion { get; set; }
        public string ProvinceState { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }

        private DateTime recordDate;
        public DateTime RecordDate
        {
            get => DateTime.Parse(recordDate.ToString("yyyy-MM-dd"));
            set => recordDate = value;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"\"{CountryRegion}\",\"{ProvinceState}\",{RecordDate},{Confirmed},{Deaths},{Recovered}";
        }

        #endregion

    }
}
