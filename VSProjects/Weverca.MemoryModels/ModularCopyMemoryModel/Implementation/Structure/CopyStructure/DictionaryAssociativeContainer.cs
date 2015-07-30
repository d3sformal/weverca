using System;
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

    /// <summary>
    /// Copy version of a associative container. Associative container is implemented by inner dictionary. 
    /// Copy method will create new instance and copy the content of inner dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    class CopyDictionaryAssociativeContainer<TKey, TValue> : IWriteableAssociativeContainer<TKey, TValue>
    {
        private Dictionary<TKey, TValue> associativeContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyDictionaryAssociativeContainer{TKey, TValue}"/> class.
        /// </summary>
        public CopyDictionaryAssociativeContainer()
        {
            associativeContainer = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyDictionaryAssociativeContainer{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="cloned">The cloned.</param>
        public CopyDictionaryAssociativeContainer(CopyDictionaryAssociativeContainer<TKey, TValue> cloned)
        {
            associativeContainer = new Dictionary<TKey, TValue>(cloned.associativeContainer);
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
                associativeContainer[key] = value;
            }
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            associativeContainer.Add(key, value);
        }

        /// <inheritdoc />
        public void Remove(TKey key)
        {
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
            return new CopyDictionaryAssociativeContainer<TKey, TValue>(this);
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
    }
}
