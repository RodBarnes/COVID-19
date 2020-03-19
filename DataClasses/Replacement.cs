namespace DataClasses
{
    class Replacement
    {
        public Replacement(string from, string to)
        {
            From = from;
            To = to;
        }

        public string From { get; set; }
        public string To { get; set; }
    }
}
