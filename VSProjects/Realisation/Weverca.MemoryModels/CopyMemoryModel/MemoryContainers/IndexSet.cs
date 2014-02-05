using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{

    class IndexSet<T> : IGenericCloneable<IndexSet<T>>, IEnumerable<T>
    {
        private HashSet<T> indexes;

        public IndexSet()
        {
            indexes = new HashSet<T>();
        }

        public IndexSet(IEnumerable<T> values)
        {
            indexes = new HashSet<T>(values);
        }

        public void Add(T index)
        {
            indexes.Add(index);
        }
        public void Remove(T index)
        {
            indexes.Remove(index);
        }
        public bool Contains(T index)
        {
            return indexes.Contains(index);
        }

        public IndexSet<T> Clone()
        {
            return new IndexSet<T>(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return indexes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return indexes.GetEnumerator();
        }
    }
}
