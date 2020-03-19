using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataClasses
{
    public class DailyReports : IList<DailyReport>
    {
        private readonly List<DailyReport> reports = new List<DailyReport>();
        private TextFieldParser parser;
        private bool readHeaders = true;

        private readonly List<Replacement> replacements = new List<Replacement>();

        public DailyReports(string path = "")
        {
            ReadReplacements();
            MergeData(path);
        }

        public void MergeData(string path)
        {
            readHeaders = true;

            var filePaths = Directory.GetFiles(path, "*.csv");

            if (filePaths.Length > 0)
            {
                foreach (var filePath in filePaths)
                {
                    ParseData(filePath);
                }
                var sums = SumData();
                foreach (DailyReport report in sums)
                {
                    reports.Add(report);
                }
            }
            else
            {
                throw new FileNotFoundException($"No files found at path '{path}'.");
            }
        }

        private void ReadReplacements()
        {
            var dir = Directory.GetCurrentDirectory();
            var filePath = $@"{dir}\Replacements.csv";
            using (parser = new TextFieldParser(filePath))
            {
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                do
                {
                    var fields = parser.ReadFields();
                    replacements.Add(new Replacement(fields[0], fields[1]));
                }
                while (!parser.EndOfData);
            }
        }

        private List<DailyReport> SumData()
        {
            return reports
                .GroupBy(r => new { r.CountryRegion, r.RecordDate })
                .Select(cr => new DailyReport
                {
                    CountryRegion = cr.Key.CountryRegion,
                    ProvinceState = "(All)",
                    RecordDate = cr.Key.RecordDate,
                    Confirmed = cr.Sum(c => c.Confirmed),
                    Recovered = cr.Sum(c => c.Recovered),
                    Deaths = cr.Sum(c => c.Deaths)
                }).ToList();
        }

        private void ParseData(string filePath)
        {
            var firstLine = true;

            parser = new TextFieldParser(filePath);
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            do
            {
                var fields = parser.ReadFields();
                if (!firstLine)
                {
                    string provinceState = fields[0].Trim();
                    string countryRegion = fields[1].Trim();
                    var lastUpdate = DateTime.Parse(fields[2]);
                    int.TryParse(fields[3], out int confirmed);
                    int.TryParse(fields[4], out int deaths);
                    int.TryParse(fields[5], out int recovered);

                    foreach (var rep in replacements)
                    {
                        if (countryRegion == rep.From)
                        {
                            countryRegion = rep.To;
                        }
                        if (provinceState == rep.From)
                        {
                            provinceState = rep.To;
                        }
                    }

                    var report = new DailyReport(countryRegion, provinceState, lastUpdate, confirmed, deaths, recovered);
                    reports.Add(report);
                }
                else
                {
                    if (readHeaders)
                    {
                        ColumnHeader1 = fields[1].Trim();
                        ColumnHeader2 = fields[0].Trim();
                        ColumnHeader3 = fields[2].Trim();
                        ColumnHeader4 = fields[3].Trim();
                        ColumnHeader5 = fields[4].Trim();
                        readHeaders = false;
                    }
                    firstLine = false;
                }
            }
            while (!parser.EndOfData);
        }

        public string ColumnHeader1 { get; set; }   
        public string ColumnHeader2 { get; set; }
        public string ColumnHeader3 { get; set; }
        public string ColumnHeader4 { get; set; }
        public string ColumnHeader5 { get; set; }

        #region Standard List Operations

        public DailyReport this[int index] { get => reports[index]; set => reports[index] = value; }

        public int Count => reports.Count;

        public bool IsReadOnly => ((IList<DailyReport>)reports).IsReadOnly;

        public void Add(DailyReport item) => reports.Add(item);

        public void Clear() => reports.Clear();

        public bool Contains(DailyReport item) => reports.Contains(item);

        public void CopyTo(DailyReport[] array, int arrayIndex) => reports.CopyTo(array, arrayIndex);

        public IEnumerator<DailyReport> GetEnumerator() => ((IList<DailyReport>)reports).GetEnumerator();
        
        public int IndexOf(DailyReport item) => reports.IndexOf(item);

        public void Insert(int index, DailyReport item) => reports.Insert(index, item);

        public bool Remove(DailyReport item) => reports.Remove(item);

        public void RemoveAt(int index) => reports.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IList<DailyReport>)reports).GetEnumerator();

        #endregion
    }
}
