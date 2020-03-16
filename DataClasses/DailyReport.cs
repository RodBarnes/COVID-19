using System;

namespace DataClasses
{
    public class DailyReport
    {
        public DailyReport(string provinceState, string countryRegion, DateTime recordDate, int confirmed, int deaths, int recovered)
        {
            ProvinceState = provinceState;
            CountryRegion = countryRegion;
            RecordDate = recordDate;
            Confirmed = confirmed;
            Deaths = deaths;
            Recovered = recovered;
        }

        public override string ToString()
        {
            return $"\"{CountryRegion}\",\"{ProvinceState}\",{RecordDate},{Confirmed},{Deaths},{Recovered}";
        }

        public string ProvinceState { get; set; }
        public string CountryRegion { get; set; }
        public DateTime RecordDate { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }
    }
}
