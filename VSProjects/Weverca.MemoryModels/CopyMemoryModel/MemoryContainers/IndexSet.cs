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

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Represents cloneable and enumerable set of indexes which can be used in memory stack object.
    /// </summary>
    /// <typeparam name="T">Type of index object</typeparam>
    public class IndexSet<T> : IGenericCloneable<IndexSet<T>>, IEnumerable<T>
    {
        /// <summary>
        /// The set of used indexes
        /// </summary>
        private HashSet<T> indexes;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexSet{T}"/> class.
        /// </summary>
        public IndexSet()
        {
            indexes = new HashSet<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexSet{T}"/> class and fills set by given values.
        /// </summary>
        /// <param name="values">The values.</param>
        public IndexSet(IEnumerable<T> values)
        {
            indexes = new HashSet<T>(values);
        }

        /// <summary>
        /// Adds the specified index into set.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Add(T index)
        {
            indexes.Add(index);
        }
        /// <summary>
        /// Removes the specified index from set.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Remove(T index)
        {
            indexes.Remove(index);
        }
        /// <summary>
        /// Determines whether set contains specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>True whether set contains specified index.</returns>
        public bool Contains(T index)
        {
            return indexes.Contains(index);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>New instance which contains copy of this instance.</returns>
        public IndexSet<T> Clone()
        {
            return new IndexSet<T>(this);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return indexes.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return indexes.GetEnumerator();
        }
    }
}