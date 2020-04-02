namespace DataClasses
{
    public class Replacement
    {
        public Replacement(string[] fields)
        {
            ReplacementType = int.Parse(fields[0]);

            switch (ReplacementType)
            {
                case 1:
                    FromCountry = fields[1];
                    ToCountry = fields[2];
                    break;
                case 2:
                    FromState = fields[1];
                    FromCountry = fields[2];
                    ToState = fields[3];
                    ToCountry = fields[4];
                    break;
                case 3:
                    FromDistrict = fields[1];
                    FromState = fields[2];
                    FromCountry = fields[3];
                    ToState = fields[4];
                    ToCountry = fields[5];
                    break;
                default:
                    break;
            }
        }

        #region Properties

        public int ReplacementType { get; set; } = 0;
        public string FromCountry { get; set; } = "";
        public string ToCountry { get; set; } = "";
        public string FromState { get; set; } = "";
        public string ToState { get; set; } = "";
        public string FromDistrict { get; set; } = "";
        public string ToDistrict { get; set; } = "";

        #endregion
    }
}
