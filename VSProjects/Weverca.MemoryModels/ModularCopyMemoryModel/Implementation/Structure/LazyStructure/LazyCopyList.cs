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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure
{
    /// <summary>
    /// Adds lazy behavior to the standard list container.
    /// 
    /// This is a lazy implementation. Copy method creates new instance with readonly 
    /// referece to the inner container. Container is copied when the first update operation 
    /// is performed. Otherwise is shared with previous instances.
    /// </summary>
    /// <typeparam name="T">Type of the elements stored within this instance</typeparam>
    class LazyCopyList<T> : IEnumerable<T> where T : IGenericCloneable<T>
    {
        private List<T> list;
        private bool copied;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyList{T}"/> class.
        /// </summary>
        public LazyCopyList()
        {
            list = new List<T>();
            copied = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCopyList{T}"/> class. New instance 
        /// will contain readonly reference to the inner list of the given instance.
        /// </summary>
        /// <param name="listToCopy">The list to copy.</param>
        public LazyCopyList(LazyCopyList<T> listToCopy)
        {
            list = listToCopy.list;
            copied = false;
        }

        /// <summary>
        /// Creates new copy of this instance which contains all elements. This method creates 
        /// a lazy copy which has readonly reference to the inner list.
        /// </summary>
        public void Copy()
        {
            if (!copied)
            {
                List<T> oldList = list;
                list = new List<T>();

                foreach (T value in oldList)
                {
                    list.Add(value.Clone());
                }

                copied = true;
            }
        }

        /// <summary>
        /// Gets the number of items in the list.
        /// </summary>
        /// <value>
        /// The number of items.
        /// </value>
        public int Count { get { return list.Count; } }

        /// <summary>
        /// Adds the specified item at the end of the list.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(T value)
        {
            Copy();
            list.Add(value);
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>Item at specified index.</returns>
        public T this[int index]
        {
            get { return list[index]; }
            set { Copy(); list[index] = value; }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}