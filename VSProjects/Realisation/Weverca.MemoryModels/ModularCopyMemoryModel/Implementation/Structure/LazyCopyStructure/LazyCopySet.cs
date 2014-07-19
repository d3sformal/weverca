using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopySet<T> : IReadonlySet<T>, IWriteableSet<T>, IGenericCloneable<LazyCopySet<T>>
    {
        private HashSet<T> valueSet;
        private bool copied;

        /// <inheritdoc />
        public LazyCopySet()
        {
            valueSet = new HashSet<T>();
            copied = true;
        }

        /// <inheritdoc />
        public LazyCopySet(LazyCopySet<T> set)
        {
            valueSet = set.valueSet;
            copied = false;
        }

        /// <inheritdoc />
        public IEnumerable<T> Values
        {
            get { return valueSet; }
        }

        /// <inheritdoc />
        public int Count
        {
            get { return valueSet.Count; }
        }

        /// <inheritdoc />
        public bool Contains(T value)
        {
            return valueSet.Contains(value);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return valueSet.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return valueSet.GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(T value)
        {
            valueSet.Add(value);
        }

        /// <inheritdoc />
        public void AddAll(IEnumerable<T> values)
        {
            CollectionTools.AddAll(valueSet, values);
        }

        /// <inheritdoc />
        public void Remove(T value)
        {
            valueSet.Remove(value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            valueSet.Clear();
        }

        /// <inheritdoc />
        public LazyCopySet<T> Clone()
        {
            return new LazyCopySet<T>(this);
        }

        private void copy()
        {
            if (!copied)
            {
                valueSet = new HashSet<T>(valueSet);
                copied = true;
            }
        }
    }
}
