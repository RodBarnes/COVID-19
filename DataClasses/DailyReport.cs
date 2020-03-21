using System;

namespace DataClasses
{
    public class DailyReport
    {
        public DailyReport() { }

        public DailyReport(string region, string state, string district, DateTime recordDate, int totalConfirmed, int newConfirmed, int totalDeaths, int newDeaths, int totalRecoverd, int newRecovered)
        {
            Region = region;
            State = state;
            District = district;
            RecordDate = recordDate;
            TotalConfirmed = totalConfirmed;
            NewConfirmed = newConfirmed;
            TotalDeaths = totalDeaths;
            NewDeaths = newDeaths;
            TotalRecovered = totalRecoverd;
            NewRecovered = newRecovered;
        }

        #region Properties

        public string Region { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public int TotalConfirmed { get; set; }
        public int NewConfirmed { get; set; }
        public int TotalDeaths { get; set; }
        public int NewDeaths { get; set; }
        public int TotalRecovered { get; set; }
        public int NewRecovered { get; set; }

        private DateTime recordDate;
        public DateTime RecordDate
        {
            get => recordDate;
            set => recordDate = new DateTime(value.Year, value.Month, value.Day);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"\"{Region}\",\"{State}\",{RecordDate},{TotalConfirmed},{TotalDeaths},{TotalRecovered}";
        }

        #endregion

    }
}
