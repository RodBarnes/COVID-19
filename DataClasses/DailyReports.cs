using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataClasses
{
    public class DailyReports : IList<DailyReport>
    {
        private readonly List<DailyReport> list;
        private TextFieldParser parser;

        public DailyReports(string path = "")
        {
            list = new List<DailyReport>();
            if (!string.IsNullOrEmpty(path))
            {
                ReadData(path);
            }
        }

        public void ReadData(string path)
        {
            var filePaths = Directory.GetFiles(path, "*.csv");

            if (filePaths.Length > 0)
            {
                foreach (var filePath in filePaths)
                {
                    parser = new TextFieldParser(filePath);
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;
                    var firstLine = true;
                    do
                    {
                        var fields = parser.ReadFields();
                        if (!firstLine)
                        {
                            var provinceState = fields[0];
                            var countryRegion = fields[1];
                            var lastUpdate = DateTime.Parse(fields[2]);
                            int.TryParse(fields[3], out int confirmed);
                            int.TryParse(fields[4], out int deaths);
                            int.TryParse(fields[5], out int recovered);

                            var report = new DailyReport(provinceState, countryRegion, lastUpdate, confirmed, deaths, recovered);
                            list.Add(report);
                        }
                        else
                        {
                            firstLine = false;
                        }
                    }
                    while (!parser.EndOfData);
                }
            }
            else
            {
                throw new FileNotFoundException($"No files found at path '{path}'.");
            }
        }

        public void WriteData(string filepath)
        {
            File.WriteAllText(filepath, ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var report in list)
            {
                sb.Append($"{report.ToString()}\n");
            }

            return sb.ToString();
        }

        public DailyReport this[int index] { get => ((IList<DailyReport>)list)[index]; set => ((IList<DailyReport>)list)[index] = value; }

        public int Count => ((IList<DailyReport>)list).Count;

        public bool IsReadOnly => ((IList<DailyReport>)list).IsReadOnly;

        public void Add(DailyReport item) => ((IList<DailyReport>)list).Add(item);

        public void Clear() => ((IList<DailyReport>)list).Clear();

        public bool Contains(DailyReport item) => ((IList<DailyReport>)list).Contains(item);

        public void CopyTo(DailyReport[] array, int arrayIndex) => ((IList<DailyReport>)list).CopyTo(array, arrayIndex);

        public IEnumerator<DailyReport> GetEnumerator() => ((IList<DailyReport>)list).GetEnumerator();
        
        public int IndexOf(DailyReport item) => ((IList<DailyReport>)list).IndexOf(item);

        public void Insert(int index, DailyReport item) => ((IList<DailyReport>)list).Insert(index, item);

        public bool Remove(DailyReport item) => ((IList<DailyReport>)list).Remove(item);

        public void RemoveAt(int index) => ((IList<DailyReport>)list).RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IList<DailyReport>)list).GetEnumerator();
    }
}
