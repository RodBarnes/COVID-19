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

        public string Country { get; set; }
        public string State { get; set; }
        public IEnumerable<int> TotalConfirmed { get; set; }
        public IEnumerable<int> TotalActive { get; set; }
        public IEnumerable<int> TotalRecovered { get; set; }
        public IEnumerable<int> TotalDeaths { get; set; }
        public IEnumerable<int> NewConfirmed { get; set; }
        public IEnumerable<int> NewActive { get; set; }
        public IEnumerable<int> NewRecovered { get; set; }
        public IEnumerable<int> NewDeaths { get; set; }
        public IEnumerable<string> FileDates { get; set; }

        #endregion
    }
}
