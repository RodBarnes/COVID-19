using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common;

namespace DataClasses
{
    public class Replacements : IList<Replacement>
    {
        private static readonly List<Replacement> list = new List<Replacement>();

        public Replacements() { }

        #region Methods

        public bool Refresh(string path, DateTime datetime)
        {
            var refresh = false;

            if (File.Exists(path))
            {
                var fileWriteTime = File.GetLastWriteTime(path);
                if (fileWriteTime > datetime)
                {
                    refresh = true;
                    list.Clear();

                    using (var parser = new TextFieldParser(path))
                    {
                        parser.SetDelimiters(",");
                        parser.HasFieldsEnclosedInQuotes = true;
                        do
                        {
                            var fields = parser.ReadFields();
                            var replacement = new Replacement(fields);
                            list.Add(replacement);
                        }
                        while (!parser.EndOfData);
                    }
                }
            }
            else
            {
                throw new FileNotFoundException($"No file found at '{path}'");
            }

            return refresh;
        }

        public void Swap(ref string country, ref string state, ref string county)
        {
            bool matched = false;

            foreach (var rep in list)
            {

                switch (rep.ReplacementType)
                {
                    case 1:
                        if (country == rep.FromCountry)
                        {
                            country = rep.ToCountry;
                            matched = true;
                        }
                        break;
                    case 2:
                        if (country == rep.FromCountry && state == rep.FromState)
                        {
                            country = rep.ToCountry;
                            state = rep.ToState;
                            matched = true;
                        }
                        break;
                    case 3:
                        if (country == rep.FromCountry && state == rep.FromState && county == rep.FromDistrict)
                        {
                            country = rep.ToCountry;
                            state = rep.ToState;
                            county = rep.ToDistrict;
                            matched = true;
                        }
                        break;
                    case 4:
                        if (country == rep.FromCountry && state == rep.FromState)
                        {
                            country = rep.ToCountry;
                            state = rep.ToState;
                            county = rep.ToDistrict;
                            matched = true;
                        }
                        break;
                    default:
                        break;
                }
                if (matched)
                    break;
            }
        }

        #endregion

        #region Standard Methods

        public Replacement this[int index] { get => list[index]; set => list[index] = value; }

        public int Count => list.Count;

        public bool IsReadOnly => ((IList<Replacement>)list).IsReadOnly;

        public void Add(Replacement item) => list.Add(item);

        public void Clear() => list.Clear();

        public bool Contains(Replacement item) => list.Contains(item);

        public void CopyTo(Replacement[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

        public IEnumerator<Replacement> GetEnumerator() => ((IList<Replacement>)list).GetEnumerator();

        public int IndexOf(Replacement item) => list.IndexOf(item);

        public void Insert(int index, Replacement item) => list.Insert(index, item);

        public bool Remove(Replacement item) => list.Remove(item);

        public void RemoveAt(int index) => list.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IList<Replacement>)list).GetEnumerator();
    }

    #endregion
}
