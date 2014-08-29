/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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