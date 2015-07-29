using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure
{
    class LazyDictionaryAssociativeContainerFactory : IAssociativeContainerFactory
    {
        public IWriteableAssociativeContainer<TKey, TValue> CreateWriteableAssociativeContainer<TKey, TValue>()
        {
            return new LazyDictionaryAssociativeContainer<TKey, TValue>();
        }
    }

    class LazyDictionaryAssociativeContainer<TKey, TValue> : IWriteableAssociativeContainer<TKey, TValue>
    {
        private Dictionary<TKey, TValue> associativeContainer;
        private bool copied;

        public LazyDictionaryAssociativeContainer()
        {
            associativeContainer = new Dictionary<TKey, TValue>();
            copied = true;
        }

        public LazyDictionaryAssociativeContainer(LazyDictionaryAssociativeContainer<TKey, TValue> cloned)
        {
            associativeContainer = cloned.associativeContainer;
            copied = false;
        }

        public TValue this[TKey key]
        {
            get
            {
                return associativeContainer[key];
            }
            set
            {
                copy();
                associativeContainer[key] = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            copy();
            associativeContainer.Add(key, value);
        }

        public void Remove(TKey key)
        {
            copy();
            associativeContainer.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return associativeContainer.TryGetValue(key, out value);
        }

        public void Clear()
        {
            copy();
            associativeContainer.Clear();
        }

        public IEnumerable<TKey> Keys
        {
            get { return associativeContainer.Keys; }
        }

        public IEnumerable<TValue> Values
        {
            get { return associativeContainer.Values; }
        }

        public int Count
        {
            get { return associativeContainer.Count; }
        }

        public bool ContainsKey(TKey key)
        {
            return associativeContainer.ContainsKey(key);
        }

        public IWriteableAssociativeContainer<TKey, TValue> Copy()
        {
            return new LazyDictionaryAssociativeContainer<TKey, TValue>(this);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return associativeContainer.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return associativeContainer.GetEnumerator();
        }

        private void copy()
        {
            if (!copied)
            {
                associativeContainer = new Dictionary<TKey, TValue>(associativeContainer);
                copied = true;
            }
        }
    }
}
