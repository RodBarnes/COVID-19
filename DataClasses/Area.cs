namespace DataClasses
{
    public class Area
    {
        public Area() { }

        public Area(string region, string state, string district, int confirmed, int recovered, int deaths)
        {
            Region = region;
            State = state;
            District = district;
            Confirmed = confirmed;
            Recovered = recovered;
            Deaths = deaths;
        }
        public string Region { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public int Confirmed { get; set; }
        public int Recovered { get; set; }
        public int Deaths { get; set; }

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
