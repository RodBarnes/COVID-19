namespace DataClasses
{
    public class CountryStats
    {
        public CountryStats() { }

        public CountryStats(string[] fields)
        {
            Rank = StringToInt(fields[0]);
            Country = fields[1];
            Population = StringToLong(fields[2]);
            PctChange = StringToDecimal(fields[3]);
            NetChange = StringToInt(fields[4]);
            Density = StringToLong(fields[5]);
            Area = StringToLong(fields[6]);
            Migrants = StringToLong(fields[7]);
            FertilityRate = StringToDecimal(fields[8]);
            MedianAge = StringToInt(fields[9]);
            PctUrban = StringToDecimal(fields[10]);
            PctWorld = StringToDecimal(fields[11]);
        }

        #region Properties

        public int Rank { get; set; }
        public string Country { get; set; }
        public long Population { get; set; }
        public decimal PctChange { get; set; }
        public int NetChange { get; set; }
        public long Density { get; set; }
        public long Area { get; set; }
        public long Migrants { get; set; }
        public decimal FertilityRate { get; set; }
        public int MedianAge { get; set; }
        public decimal PctUrban { get; set; }
        public decimal PctWorld { get; set; }

        #endregion

        #region Helpers

        private long StringToLong(string inValue)
        {
            var temp = inValue.Replace(",", "");
            var isValid = long.TryParse(temp, out long outValue);

            return isValid ? outValue : 0;
        }

        private int StringToInt(string inValue)
        {
            var temp = inValue.Replace(",", "");
            var isValid = int.TryParse(temp, out int outValue);

            return isValid ? outValue : 0;
        }

        private decimal StringToDecimal(string inValue)
        {
            var temp = inValue.Replace("%", "");
            var isValid = decimal.TryParse(temp, out decimal outValue);

            return isValid ? outValue : 0;
        }

        #endregion
    }
}
