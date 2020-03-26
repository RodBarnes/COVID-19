using System.Collections;
using System.Collections.Generic;
using Common;

namespace DataClasses
{
    public class Replacements : IList<Replacement>
    {
        private static List<Replacement> list = new List<Replacement>();

        public Replacements() { }

        #region Methods

        public void ReadReplacements(string path)
        {
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

        public void Apply(ref string country, ref string state, ref string county)
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

        public Replacement this[int index] { get => ((IList<Replacement>)list)[index]; set => ((IList<Replacement>)list)[index] = value; }

        public int Count => ((IList<Replacement>)list).Count;

        public bool IsReadOnly => ((IList<Replacement>)list).IsReadOnly;

        public void Add(Replacement item) => ((IList<Replacement>)list).Add(item);

        public void Clear() => ((IList<Replacement>)list).Clear();

        public bool Contains(Replacement item) => ((IList<Replacement>)list).Contains(item);

        public void CopyTo(Replacement[] array, int arrayIndex) => ((IList<Replacement>)list).CopyTo(array, arrayIndex);

        public IEnumerator<Replacement> GetEnumerator() => ((IList<Replacement>)list).GetEnumerator();

        public int IndexOf(Replacement item) => ((IList<Replacement>)list).IndexOf(item);

        public void Insert(int index, Replacement item) => ((IList<Replacement>)list).Insert(index, item);

        public bool Remove(Replacement item) => ((IList<Replacement>)list).Remove(item);

        public void RemoveAt(int index) => ((IList<Replacement>)list).RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => ((IList<Replacement>)list).GetEnumerator();
    }

    #endregion
}
