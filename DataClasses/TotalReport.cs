namespace DataClasses
{
    public class TotalReport
    {
        public TotalReport() { }

        public TotalReport(string region, string state, string district)
        {
            Region = region;
            State = state;
            District = district;
        }
        public string Region { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public int TotalConfirmed { get; set; }
        public int NewConfirmed { get; set; }
        public int TotalDeaths { get; set; }
        public int NewDeaths { get; set; }
        public int TotalRecovered { get; set; }
        public int NewRecovered { get; set; }

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

        public string RegionStateDistrict
        {
            get
            {
                var result = Region;
                if (!string.IsNullOrEmpty(State))
                {
                    result += $",{State}";
                }
                if (!string.IsNullOrEmpty(District))
                {
                    result += $",{District}";
                }

                return result;
            }
        }
    }
}
