using System;

namespace DataClasses
{
    public class DailyFileDataFips : DailyFileData
    {
        /// <summary>
        /// This structure become into use with Mar 23 data.
        /// </summary>
        public DailyFileDataFips() : base() { }

        public DailyFileDataFips(int fips, string admin2, string provinceState, string countryRegion, DateTime lastUpdate,
            double latitude, double longitude, int confirmed, int deaths, int recovered, int active, string combinedKey)
            : base(provinceState, countryRegion, lastUpdate, confirmed, deaths, recovered, latitude, longitude)
        {
            FIPS = fips;
            Admin2 = admin2;
            Active = active;
            CombinedKey = combinedKey;
        }

        public int FIPS { get; set; }
        public string Admin2 { get; set; }
        public int Active { get; set; }
        public string CombinedKey { get; set; }
    }
}
