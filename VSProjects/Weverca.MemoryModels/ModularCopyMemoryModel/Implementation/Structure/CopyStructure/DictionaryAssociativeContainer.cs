﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    class CopyDictionaryAssociativeContainerFactory : IAssociativeContainerFactory
    {
        public IWriteableAssociativeContainer<TKey, TValue> CreateWriteableAssociativeContainer<TKey, TValue>()
        {
            return new CopyDictionaryAssociativeContainer<TKey, TValue>();
        }
    }

    class CopyDictionaryAssociativeContainer<TKey, TValue> : IWriteableAssociativeContainer<TKey, TValue>
    {
        private Dictionary<TKey, TValue> associativeContainer;

        public CopyDictionaryAssociativeContainer()
        {
            associativeContainer = new Dictionary<TKey, TValue>();
        }

        public CopyDictionaryAssociativeContainer(CopyDictionaryAssociativeContainer<TKey, TValue> cloned)
        {
            associativeContainer = new Dictionary<TKey, TValue>(cloned.associativeContainer);
        }

        public TValue this[TKey key]
        {
            get
            {
                return associativeContainer[key];
            }
            set
            {
                associativeContainer[key] = value;
            }
        }

        public void Add(TKey key, TValue value)
        {
            associativeContainer.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return associativeContainer.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return associativeContainer.TryGetValue(key, out value);
        }

        public void Clear()
        {
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
            return new CopyDictionaryAssociativeContainer<TKey, TValue>(this);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return associativeContainer.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return associativeContainer.GetEnumerator();
        }
    }
}
