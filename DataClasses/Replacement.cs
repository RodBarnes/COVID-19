namespace DataClasses
{
    class Replacement
    {
        public Replacement(string[] fields)
        {
            ReplacementType = int.Parse(fields[0]);

            switch (ReplacementType)
            {
                case 1:
                    FromRegion = fields[1];
                    ToRegion = fields[2];
                    break;
                case 2:
                    FromState = fields[1];
                    FromRegion = fields[2];
                    ToState = fields[3];
                    ToRegion = fields[4];
                    break;
                case 3:
                    FromDistrict = fields[1];
                    FromState = fields[2];
                    FromRegion = fields[3];
                    ToState = fields[4];
                    ToRegion = fields[5];
                    break;
                default:
                    break;
            }
        }

        public int ReplacementType { get; set; } = 0;
        public string FromRegion { get; set; } = null;
        public string ToRegion { get; set; } = null;
        public string FromState { get; set; } = null;
        public string ToState { get; set; } = null;
        public string FromDistrict { get; set; } = null;
        public string ToDistrict { get; set; } = null;
    }
}
