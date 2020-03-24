using System;

namespace DataClasses
{
    public class DailyFileData
    {
        /// <summary>
        /// This structure was used from the start of Jan 22.
        /// The Lat/Long were added starting on Mar 1.
        /// </summary>

        public DailyFileData() { }

        public DailyFileData(string provinceState, string countryRegion, DateTime lastUpdate, int confirmed, int deaths, int recovered, double latitude = 0, double longitude = 0)
        {
            ProvinceState = provinceState;
            CountryRegion = countryRegion;
            LastUpdate = lastUpdate;
            Confirmed = confirmed;
            Deaths = deaths;
            Recovered = recovered;
            Latitude = latitude;
            Longitude = longitude;
        }


        public string ProvinceState { get; set; }
        public string CountryRegion { get; set; }
        public DateTime LastUpdate { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
