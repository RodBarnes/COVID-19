using System;
using System.Collections.Generic;

namespace DataClasses
{
    public class TotalReport
    {
        public TotalReport() { }

        public TotalReport(string region, string state)
        {
            Region = region;
            State = state;
        }
        public string Region { get; set; }
        public string State { get; set; }
        public IEnumerable<int> TotalConfirmed { get; set; }
        public IEnumerable<int> NewConfirmed { get; set; }
        public IEnumerable<int> TotalDeaths { get; set; }
        public IEnumerable<int> NewDeaths { get; set; }
        public IEnumerable<int> TotalRecovered { get; set; }
        public IEnumerable<int> NewRecovered { get; set; }
        public IEnumerable<string> RecordDates { get; set; }

        public string RegionState
        {
            get
            {
                var result = Region;
                if (!string.IsNullOrEmpty(State))
                {
                    result += $",{State}";
                }

                return result;
            }
        }
    }
}
