namespace DataClasses
{
    public class Selection
    {
        public Selection(string displayName, string chartname)
        {
            DisplayName = displayName;
            ChartName = chartname;
        }

        public string DisplayName { get; set; }
        public string ChartName { get; set; }
    }
}
