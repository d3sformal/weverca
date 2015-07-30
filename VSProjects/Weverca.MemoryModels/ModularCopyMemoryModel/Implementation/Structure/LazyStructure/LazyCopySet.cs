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
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure
{
    /// <summary>
    /// Represents cloneable and enumerable set of values which can be used in memory collections.
    /// 
    /// This is a lazy implementation. Copy method creates new instance with readonly 
    /// referece to the inner container. Container is copied when the first update operation 
    /// is performed. Otherwise is shared with previous instances.
    /// </summary>
    /// <typeparam name="T">Element stored within this set.</typeparam>
    class LazyCopySet<T> : IReadonlySet<T>, IWriteableSet<T>, IGenericCloneable<LazyCopySet<T>>
    {
        private HashSet<T> valueSet;
        private bool copied;

        /// <inheritdoc />
        public LazyCopySet()
        {
            valueSet = new HashSet<T>();
            copied = true;
        }

        /// <inheritdoc />
        public LazyCopySet(LazyCopySet<T> set)
        {
            valueSet = set.valueSet;
            copied = false;
        }

        /// <inheritdoc />
        public IEnumerable<T> Values
        {
            get { return valueSet; }
        }

        /// <inheritdoc />
        public int Count
        {
            get { return valueSet.Count; }
        }

        /// <inheritdoc />
        public bool Contains(T value)
        {
            return valueSet.Contains(value);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return valueSet.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return valueSet.GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(T value)
        {
            copy();
            valueSet.Add(value);
        }

        /// <inheritdoc />
        public void AddAll(IEnumerable<T> values)
        {
            copy();
            CollectionMemoryUtils.AddAll(valueSet, values);
        }

        /// <inheritdoc />
        public void Remove(T value)
        {
            copy();
            valueSet.Remove(value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            copy();
            valueSet.Clear();
        }

        /// <inheritdoc />
        public LazyCopySet<T> Clone()
        {
            return new LazyCopySet<T>(this);
        }

        private void copy()
        {
            if (!copied)
            {
                valueSet = new HashSet<T>(valueSet);
                copied = true;
            }
        }
    }
}