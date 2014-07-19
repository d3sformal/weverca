using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        private Dictionary<K, V> dictionary;

        private bool copied;

        public LazyCopyDictionary()
        {
            dictionary = new Dictionary<K, V>();
            copied = true;
        }

        public LazyCopyDictionary(LazyCopyDictionary<K, V> dictionaryToCopy)
        {
            dictionary = dictionaryToCopy.dictionary;
            copied = false;
        }

        public IEnumerable<K> Keys { get { return dictionary.Keys; } }

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
            set { copy(); dictionary[key] = value; }
        }

        public void Add(K key, V value)
        {
            copy();
            dictionary.Add(key, value);
        }

        public void Remove(K key)
        {
            copy();
            dictionary.Remove(key);
        }

        private void copy()
        {
            if (!copied)
            {
                dictionary = new Dictionary<K, V>(dictionary);
                copied = true;
            }
        }
    }
}
