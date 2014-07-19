using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyList<T> : IEnumerable<T> where T : IGenericCloneable<T>
    {
        private List<T> list;
        private bool copied;

        public LazyCopyList()
        {
            list = new List<T>();
            copied = true;
        }

        public LazyCopyList(LazyCopyList<T> listToCopy)
        {
            list = listToCopy.list;
            copied = false;
        }

        public void Copy()
        {
            if (!copied)
            {
                List<T> oldList = list;
                list = new List<T>();

                foreach (T value in oldList)
                {
                    list.Add(value.Clone());
                }

                copied = true;
            }
        }

        public int Count { get { return list.Count; } }

        public void Add(T value)
        {
            Copy();
            list.Add(value);
        }

        public T this[int index]
        {
            get { return list[index]; }
            set { Copy(); list[index] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
