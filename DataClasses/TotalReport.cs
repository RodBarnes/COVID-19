using System.Collections.Generic;

namespace DataClasses
{
    public class TotalReport
    {
        public TotalReport() { }

        public TotalReport(string region, string state)
        {
            Country = region;
            State = state;
        }

        public TotalReport(string region, string state, double latitude, double longitude, int fips) : this(region, state)
        {
            Latitude = latitude;
            Longitude = longitude;
            FIPS = fips;
        }

        #region Properties

        public string DisplayName
        {
            get
            {
                var result = Country;
                if (!string.IsNullOrEmpty(State))
                {
                    result += $", {State}";
                }

                return result;
            }
        }

        public string Country { get; set; } = "";
        public string State { get; set; } = "";
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
        public int FIPS { get; set; } = 0;
        public List<string> FileDates { get; set; } = new List<string>();
        public List<int> TotalConfirmeds { get; set; } = new List<int>();
        public List<int> TotalActives { get; set; } = new List<int>();
        public List<int> TotalRecovereds { get; set; } = new List<int>();
        public List<int> TotalDeaths { get; set; } = new List<int>();
        public List<int> NewConfirmeds { get; set; } = new List<int>();
        public List<int> NewActives { get; set; } = new List<int>();
        public List<int> NewRecovereds { get; set; } = new List<int>();
        public List<int> NewDeaths { get; set; } = new List<int>();

        #endregion
    }
}
