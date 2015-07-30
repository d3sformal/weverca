using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure
{
    /// <summary>
    /// Lazy implementation of standard dictionary. Copying will provide deep copy of all elements 
    /// stored within the container - call Copy method to obtain copied instances which should 
    /// be stored in the copied instance.
    /// 
    /// This is a lazy implementation. Copy method creates new instance with readonly 
    /// referece to the inner container. Container is copied when the first update operation 
    /// is performed. Otherwise is shared with previous instances.
    /// </summary>
    /// <typeparam name="K">The type of the key.</typeparam>
    /// <typeparam name="V">The type of the value.</typeparam>
    class LazyDeepCopyDictionary<K, V> : IEnumerable<KeyValuePair<K, V>> where V : IGenericCloneable<V>
    {
        private Dictionary<K, V> dictionary;

        private bool copied;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyDeepCopyDictionary{K, V}"/> class.
        /// </summary>
        public LazyDeepCopyDictionary()
        {
            dictionary = new Dictionary<K, V>();
            copied = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyDeepCopyDictionary{K, V}"/> class.
        /// </summary>
        /// <param name="dictionaryToCopy">The dictionary to copy.</param>
        public LazyDeepCopyDictionary(LazyDeepCopyDictionary<K, V> dictionaryToCopy)
        {
            dictionary = dictionaryToCopy.dictionary;
            copied = false;
        }

        /// <summary>
        /// Gets the list keys stored within the distionary.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<K> Keys { get { return dictionary.Keys; } }

        /// <summary>
        /// Gets the values stored within the dictionary.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IEnumerable<V> Values { get { return dictionary.Values; } }

        /// <summary>
        /// Determines whether the this instance contais specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(K key)
        {
            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(K key, out V value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the number of elements stored within this instance.
        /// </summary>
        /// <returns>The number of elements stored within this instance.</returns>
        public int Count()
        {
            return dictionary.Count;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public V this[K key]
        {
            get { return dictionary[key]; }
            set { Copy(); dictionary[key] = value; }
        }

        /// <summary>
        /// Adds the value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(K key, V value)
        {
            Copy();
            dictionary.Add(key, value);
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(K key)
        {
            Copy();
            dictionary.Remove(key);
        }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        public void Copy()
        {
            if (!copied)
            {
                Dictionary<K, V> oldDictionary = dictionary;
                dictionary = new Dictionary<K, V>();

                foreach (var item in oldDictionary)
                {
                    dictionary.Add(item.Key, item.Value.Clone());
                }

                copied = true;
            }
        }
    }
}
