using System;
using System.Collections.Generic;

namespace DataClasses
{
    /// <summary>
    /// TotalReport is a class that contains the list of daily dates and totals for a Country, State for a given date
    /// Each TotalReport has the Country, State, and totals for all dates for each dimension
    /// List<TotalReport> is built from the database to be used to display the County,State in the list and the data for the charts
    /// A TotalReport (SelectedTotalReport) is used when displaying data in a chart
    /// </summary>
    public class TotalReport
    {
        public TotalReport() { }

        public TotalReport(string region, string state)
        {
            Country = region;
            State = state;
        }

        public TotalReport(string region, string state, decimal latitude, decimal longitude, int fips) : this(region, state)
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

        public decimal PctDeathPopulation => Math.Round(TotalDeaths[TotalDeaths.Count-1] / (decimal)Population * 100, 4);
        public decimal PctConfirmedPopulation => Math.Round(TotalConfirmeds[TotalConfirmeds.Count - 1] / (decimal)Population * 100, 4);
        public decimal PctDeathConfirmed => Math.Round(TotalDeaths[TotalDeaths.Count - 1] / (decimal)TotalConfirmeds[TotalConfirmeds.Count - 1] * 100, 4);
        public string Country { get; set; } = "";
        public string State { get; set; } = "";
        public decimal Latitude { get; set; } = 0;
        public decimal Longitude { get; set; } = 0;
        public int FIPS { get; set; } = 0;
        public long Population { get; set; }
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
