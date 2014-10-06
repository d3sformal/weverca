/*
Copyright (c) 2012-2014 Matyas Brenner and David Hauzar

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

using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.FlowResolver
{
    static class ValueHelper
    {
        /// <summary>
        /// Tries the get minimum value if some of provided values are <see cref="ConcreteValue"/> or <see cref="IntervalValue"/>.
        /// </summary>
        /// <param name="values">The list of values to evaluate.</param>
        /// <param name="minInt">The minimum found int.</param>
        /// <param name="minLong">The minimum found long.</param>
        /// <param name="minDouble">The minimum found double.</param>
        public static void TryGetMinimumValue(IEnumerable<Value> values, out int? minInt, out long? minLong, out double? minDouble)
        {
            minInt = null;
            minLong = null;
            minDouble = null;

            foreach (Value value in values)
            {
                if (value is IntegerValue)
                {
                    var currentValue = (IntegerValue)value;
                    minInt = GetMin(minInt, currentValue.Value);
                }
                else if (value is LongintValue)
                {
                    var currentValue = (LongintValue)value;
                    minLong = GetMin(minLong, currentValue.Value);
                }
                else if (value is FloatValue)
                {
                    var currentValue = (FloatValue)value;
                    minDouble = GetMin(minDouble, currentValue.Value);
                }
                else if (value is IntegerIntervalValue)
                {
                    var currentValue = (IntegerIntervalValue)value;
                    minInt = GetMin(minInt, currentValue.Start);
                }
                else if (value is LongintIntervalValue)
                {
                    var currentValue = (LongintIntervalValue)value;
                    minLong = GetMin(minLong, currentValue.Start);
                }
                else if (value is FloatValue)
                {
                    var currentValue = (FloatIntervalValue)value;
                    minDouble = GetMin(minDouble, currentValue.Start);
                }
            }

            if (minInt >= minLong || minInt >= minDouble)
            {
                minInt = null;
            }

            if (minLong > minInt || minLong >= minDouble)
            {
                minLong = null;
            }

            if (minDouble > minInt || minDouble > minLong)
            {
                minDouble = null;
            }
        }

        /// <summary>
        /// Tries the get maximum value if some of provided values are <see cref="ConcreteValue"/> or <see cref="IntervalValue"/>.
        /// </summary>
        /// <param name="values">The list of values to evaluate.</param>
        /// <param name="maxInt">The maximum found int.</param>
        /// <param name="maxLong">The maximum found long.</param>
        /// <param name="maxDouble">The maximum found double.</param>
        public static void TryGetMaximumValue(IEnumerable<Value> values, out int? maxInt, out long? maxLong, out double? maxDouble)
        {
            maxInt = null;
            maxLong = null;
            maxDouble = null;

            foreach (Value value in values)
            {
                if (value is IntegerValue)
                {
                    var currentValue = (IntegerValue)value;
                    maxInt = GetMax(maxInt, currentValue.Value);
                }
                else if (value is LongintValue)
                {
                    var currentValue = (LongintValue)value;
                    maxLong = GetMax(maxLong, currentValue.Value);
                }
                else if (value is FloatValue)
                {
                    var currentValue = (FloatValue)value;
                    maxDouble = GetMax(maxDouble, currentValue.Value);
                }
                else if (value is IntegerIntervalValue)
                {
                    var currentValue = (IntegerIntervalValue)value;
                    maxInt = GetMax(maxInt, currentValue.Start);
                }
                else if (value is LongintIntervalValue)
                {
                    var currentValue = (LongintIntervalValue)value;
                    maxLong = GetMax(maxLong, currentValue.Start);
                }
                else if (value is FloatValue)
                {
                    var currentValue = (FloatIntervalValue)value;
                    maxDouble = GetMax(maxDouble, currentValue.Start);
                }
            }

            if (maxInt <= maxLong || maxInt <= maxDouble)
            {
                maxInt = null;
            }

            if (maxLong < maxInt || maxLong <= maxDouble)
            {
                maxLong = null;
            }

            if (maxDouble < maxInt || maxDouble < maxLong)
            {
                maxDouble = null;
            }
        }

        static T GetMin<T>(T? a, T b)
            where T : struct, IComparable<T>
        {
            if (a.HasValue)
            {
                return a.Value.CompareTo(b) <= 0 ? a.Value : b;
            }
            else
            {
                return b;
            }
        }

        static T GetMax<T>(T? a, T b)
            where T : struct, IComparable<T>
        {
            if (a.HasValue)
            {
                return a.Value.CompareTo(b) >= 0 ? a.Value : b;
            }
            else
            {
                return b;
            }
        }
    }
}