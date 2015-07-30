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

    /// <summary>
    /// Lazy version of associative container. Contains inner dictionary instance to store all values.
    /// 
    /// This is a lazy implementation. Copy method creates new instance with readonly 
    /// referece to the inner container. Container is copied when the first update operation 
    /// is performed. Otherwise is shared with previous instances.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    class LazyDictionaryAssociativeContainer<TKey, TValue> : IWriteableAssociativeContainer<TKey, TValue>
    {
        private Dictionary<TKey, TValue> associativeContainer;
        private bool copied;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyDictionaryAssociativeContainer{TKey, TValue}"/> class.
        /// </summary>
        public LazyDictionaryAssociativeContainer()
        {
            associativeContainer = new Dictionary<TKey, TValue>();
            copied = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyDictionaryAssociativeContainer{TKey, TValue}"/> class.
        /// 
        /// </summary>
        /// <param name="cloned">The cloned.</param>
        public LazyDictionaryAssociativeContainer(LazyDictionaryAssociativeContainer<TKey, TValue> cloned)
        {
            associativeContainer = cloned.associativeContainer;
            copied = false;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            copy();
            associativeContainer.Add(key, value);
        }

        /// <inheritdoc />
        public void Remove(TKey key)
        {
            copy();
            associativeContainer.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            return associativeContainer.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            copy();
            associativeContainer.Clear();
        }

        /// <inheritdoc />
        public IEnumerable<TKey> Keys
        {
            get { return associativeContainer.Keys; }
        }

        /// <inheritdoc />
        public IEnumerable<TValue> Values
        {
            get { return associativeContainer.Values; }
        }

        /// <inheritdoc />
        public int Count
        {
            get { return associativeContainer.Count; }
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            return associativeContainer.ContainsKey(key);
        }

        /// <inheritdoc />
        public IWriteableAssociativeContainer<TKey, TValue> Copy()
        {
            return new LazyDictionaryAssociativeContainer<TKey, TValue>(this);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return associativeContainer.GetEnumerator();
        }

        /// <inheritdoc />
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
