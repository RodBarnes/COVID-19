using System.Collections;
using System.Collections.Generic;
using Common;

namespace DataClasses
{
    public class Replacements : IList<Replacement>
    {
        private static readonly List<Replacement> list = new List<Replacement>();

        public Replacements() { }

        #region Methods

        public void Refresh(string path)
        {
            list.Clear();

            using (var parser = new TextFieldParser(path))
            {
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                do
                {
                    var fields = parser.ReadFields();
                    list.Add(new Replacement(fields));
                }
                while (!parser.EndOfData);
            }
        }

        public void Swap(ref string country, ref string state, ref string county)
        {
            foreach (var rep in list)
            {
                switch (rep.ReplacementType)
                {
                    case 1:
                        if (country == rep.FromRegion)
                        {
                            country = rep.ToRegion;
                        }
                        break;
                    case 2:
                        if (country == rep.FromRegion && state == rep.FromState)
                        {
                            country = rep.ToRegion;
                            state = rep.ToState;
                        }
                        break;
                    case 3:
                        if (country == rep.FromRegion && state == rep.FromState && county == rep.FromCounty)
                        {
                            country = rep.ToRegion;
                            state = rep.ToState;
                            county = rep.ToCounty;
                        }
                        break;
                    default:
                        break;
                }
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
