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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Utils
{
    /// <summary>
    /// Static util class. Contains generic static helper methods for sets and collections.
    /// </summary>
    public class CollectionMemoryUtils
    {
        /// <summary>
        /// Adds all values to target collection.
        /// </summary>
        /// <typeparam name="T">Type of collection.</typeparam>
        /// <param name="targetSet">The target set.</param>
        /// <param name="values">The values.</param>
        public static void AddAll<T>(ICollection<T> targetSet, IEnumerable<T> values)
        {
            if (values == null) return;
            foreach (T value in values)
            {
                targetSet.Add(value);
            }
        }

        /// <summary>
        /// Adds all values to target collection. Do nothing when the values enumeration is null.
        /// </summary>
        /// <typeparam name="T">Type of collection.</typeparam>
        /// <param name="targetSet">The target set.</param>
        /// <param name="values">The values.</param>
        public static void AddAllIfNotNull<T>(ICollection<T> targetSet, IEnumerable<T> values)
        {
            if (values != null)
            {
                AddAll(targetSet, values);
            }
        }

        public static void RemoveAll<T>(ICollection<T> targetSet, IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                targetSet.Remove(value);
            }
        }

        /// <summary>
        /// Determines whether given collections contains the same values or not.
        /// </summary>
        /// <typeparam name="T">Type of collection.</typeparam>
        /// <param name="setA">The set a.</param>
        /// <param name="setB">The set b.</param>
        /// <returns>True whethe given collections contains the same values.</returns>
        public static bool EqualsSet<T>(ICollection<T> setA, ICollection<T> setB)
        {
            if (setA == setB)
            {
                return true;
            }

            if (setA.Count != setB.Count)
            {
                return false;
            }

            foreach (T value in setA)
            {
                if (!setB.Contains(value))
                {
                    return false;
                }
            }

            foreach (T value in setB)
            {
                if (!setA.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}