using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyDeepCopyDictionary<K, V> : IEnumerable<KeyValuePair<K, V>> where V : IGenericCloneable<V>
    {
        private Dictionary<K, V> dictionary;

        private bool copied;

        public LazyDeepCopyDictionary()
        {
            dictionary = new Dictionary<K, V>();
            copied = true;
        }

        public LazyDeepCopyDictionary(LazyDeepCopyDictionary<K, V> dictionaryToCopy)
        {
            dictionary = dictionaryToCopy.dictionary;
            copied = false;
        }

        public IEnumerable<K> Keys { get { return dictionary.Keys; } }

        public IEnumerable<V> Values { get { return dictionary.Values; } }

        public bool ContainsKey(K key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool TryGetValue(K key, out V value)
        {
            return dictionary.TryGetValue(key, out value);
        }

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

        public V this[K key]
        {
            get { return dictionary[key]; }
            set { Copy(); dictionary[key] = value; }
        }

        public void Add(K key, V value)
        {
            Copy();
            dictionary.Add(key, value);
        }

        public void Remove(K key)
        {
            Copy();
            dictionary.Remove(key);
        }

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
