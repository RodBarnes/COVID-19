using System;

namespace DataClasses
{
    public class DailyReport
    {
        public DailyReport() { }

        public DailyReport(string region, string state, string district, DateTime recordDate, int confirmed, int deaths, int recovered)
        {
            Region = region;
            State = state;
            District = district;
            RecordDate = recordDate;
            Confirmed = confirmed;
            Deaths = deaths;
            Recovered = recovered;
        }

        #region Properties

        public string Region { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public int Confirmed { get; set; }
        public int Deaths { get; set; }
        public int Recovered { get; set; }

        private DateTime recordDate;
        public DateTime RecordDate
        {
            get => DateTime.Parse(recordDate.ToString("yyyy-MM-dd"));
            set => recordDate = value;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"\"{Region}\",\"{State}\",{RecordDate},{Confirmed},{Deaths},{Recovered}";
        }

        #endregion

    }
}
