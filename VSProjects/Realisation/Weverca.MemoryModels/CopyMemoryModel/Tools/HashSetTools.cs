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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class HashSetTools
    {
        public static void AddAll<T>(ICollection<T> targetSet, IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                targetSet.Add(value);
            }
        }

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

            return true;
        }
    }
}