using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DataClasses
{
    /// <summary>
    /// TotalReport is a class that contains the list of daily dates and totals for a Country, State for a given date
    /// Each TotalReport has the Country, State, and totals for all dates for each dimension
    /// List<TotalReport> is built from the database to be used to display the County,State in the list and the data for the charts
    /// A TotalReport (SelectedTotalReport) is used when displaying data in a chart
    /// </summary>
    public class TotalReport : INotifyPropertyChanged
    {
        public TotalReport()
        {
            ItemForeColor = new SolidColorBrush(Colors.Black);
            ItemBackColor = new SolidColorBrush(Colors.Transparent);
        }

        public TotalReport(string region, string state) : this()
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

        private SolidColorBrush itemForeColor;
        public SolidColorBrush ItemForeColor
        {
            get => itemForeColor;
            set
            {
                itemForeColor = value;
                NotifyPropertyChanged();
            }
        }

        private SolidColorBrush itemBackColor;
        public SolidColorBrush ItemBackColor
        {
            get => itemBackColor;
            set
            {
                itemBackColor = value;
                NotifyPropertyChanged();
            }
        }

        public decimal PctDeathPopulation
        {
            get
            {
                decimal denominator = Population;
                int numerator = TotalDeaths[TotalDeaths.Count - 1];
                int precision = 4;
                return (denominator > 0) ? Math.Round(numerator / denominator * 100, precision) : 0;
            }
        }
        public decimal PctConfirmedPopulation
        {
            get
            {
                decimal denominator = Population;
                int numerator = TotalConfirmeds[TotalConfirmeds.Count - 1];
                int precision = 4;
                return (denominator > 0) ? Math.Round(numerator / denominator * 100, precision) : 0;
            }
        }
        public decimal PctDeathConfirmed
        {
            get
            {
                decimal denominator = TotalConfirmeds[TotalConfirmeds.Count - 1];
                int numerator = TotalDeaths[TotalDeaths.Count - 1];
                int precision = 4;
                return (denominator > 0) ? Math.Round(numerator / denominator * 100, precision) : 0;
            }
        }
        public string Country { get; set; } = "";
        private string state = "";
        public string State
        {
            get => state;
            set
            {
                state = value;
                ItemForeColor = new SolidColorBrush(state != "" ? Colors.Gray : Colors.Black);
            }
        }
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
