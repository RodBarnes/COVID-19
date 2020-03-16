using System;

namespace DataClasses
{
    public class DailyReport
    {
        public DailyReport(string countryRegion, string provinceState, DateTime recordDate, int confirmed, int deaths, int recovered)
        {
            CountryRegion = countryRegion;
            ProvinceState = provinceState;
            RecordDate = recordDate;
            Confirmed = confirmed;
            Deaths = deaths;
            Recovered = recovered;
        }

        public override string ToString()
        {
            return $"\"{CountryRegion}\",\"{ProvinceState}\",{RecordDate},{Confirmed},{Deaths},{Recovered}";
        }

        public string CountryRegion { get; set; }
        public string ProvinceState { get; set; }
        public DateTime RecordDate { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }
    }
}
