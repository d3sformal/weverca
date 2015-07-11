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
using Weverca.MemoryModels.CopyMemoryModel;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common
{
    public interface ISetFactory
    {
        IWriteableSet<T> CreateWriteableSet<T>();
    }

    /// <summary>
    /// Represents readonly version of set to store collection ofdistinct values.
    /// </summary>
    /// <typeparam name="T">Tye of value to store in set.</typeparam>
    public interface IReadonlySet<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets the collection of values stored in the set.
        /// </summary>
        IEnumerable<T> Values { get; }

        /// <summary>
        /// Gets the number of values in set.
        /// </summary>
        /// <value>
        /// The number of values in set.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Determines whether the set contains specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True whether the set contains specified value.</returns>
        bool Contains(T value);
    }

    /// <summary>
    /// Represents writeable version of set to store collection ofdistinct values.
    /// </summary>
    /// <typeparam name="T">Tye of value to store in set.</typeparam>
    public interface IWriteableSet<T> : IReadonlySet<T>
    {
        /// <summary>
        /// Adds the value into set.
        /// </summary>
        /// <param name="value">The value.</param>
        void Add(T value);

        /// <summary>
        /// Adds all values into set.
        /// </summary>
        /// <param name="values">Collection of values.</param>
        void AddAll(IEnumerable<T> values);

        /// <summary>
        /// Removes the value from set.
        /// </summary>
        /// <param name="value">The value.</param>
        void Remove(T value);

        /// <summary>
        /// Removes all values from set.
        /// </summary>
        void Clear();
    }
}