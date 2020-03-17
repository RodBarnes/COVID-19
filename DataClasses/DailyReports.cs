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
        private bool readHeaders = true;

        public DailyReports(string filepath = "")
        {
            list = new List<DailyReport>();
            if (!string.IsNullOrEmpty(filepath))
            {
                ReadData(filepath);
            }
        }

        public void ReadData(string filePath)
        {
           ParseData(filePath, false);
        }

        public void MergeData(string path)
        {
            readHeaders = true;

            var filePaths = Directory.GetFiles(path, "*.csv");

            if (filePaths.Length > 0)
            {
                foreach (var filePath in filePaths)
                {
                    ParseData(filePath, true);
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
            sb.Append($"\"{ColumnHeader1}\",\"{ColumnHeader2}\",\"{ColumnHeader3}\",\"{ColumnHeader4}\",\"{ColumnHeader5}\"\n");
            foreach (var report in list)
            {
                sb.Append($"{report.ToString()}\n");
            }

            return sb.ToString();
        }

        private void ParseData(string filePath, bool merging)
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
                    string provinceState;
                    string countryRegion;

                    if (merging)
                    {
                        provinceState = fields[0].Trim();
                        countryRegion = fields[1].Trim();
                    }
                    else
                    {
                        countryRegion = fields[0].Trim();
                        provinceState = fields[1].Trim();
                    }
                    var lastUpdate = DateTime.Parse(fields[2]);
                    int.TryParse(fields[3], out int confirmed);
                    int.TryParse(fields[4], out int deaths);
                    int.TryParse(fields[5], out int recovered);

                    var report = new DailyReport(countryRegion, provinceState, lastUpdate, confirmed, deaths, recovered);
                    list.Add(report);
                }
                else
                {
                    if (readHeaders)
                    {
                        if (merging)
                        {
                            ColumnHeader1 = fields[1].Trim();
                            ColumnHeader2 = fields[0].Trim();
                        }
                        else
                        {
                            ColumnHeader1 = fields[0].Trim();
                            ColumnHeader2 = fields[1].Trim();
                        }

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

        #endregion
    }
}
