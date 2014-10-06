/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Statistics colleted by snapshots during FixPoint computation
    /// </summary>
    public enum Statistic
    {
        /// <summary>
        /// Number of created integer values
        /// </summary>
        CreatedIntValues,

        /// <summary>
        /// Number of created boolean values
        /// </summary>
        CreatedBooleanValues,

        /// <summary>
        /// Number of created float values
        /// </summary>
        CreatedFloatValues,

        /// <summary>
        /// Number of created object values
        /// </summary>
        CreatedObjectValues,

        /// <summary>
        /// Number of created array values
        /// </summary>
        CreatedArrayValues,

        /// <summary>
        /// Number of alias assigns
        /// </summary>
        AliasAssigns,

        /// <summary>
        /// Number of memory entry assigns
        /// </summary>
        MemoryEntryAssigns,

        /// <summary>
        /// Number of snapshot extend calls
        /// </summary>
        SnapshotExtendings,

        /// <summary>
        /// Number of snapshot mergings with call level
        /// </summary>
        CallLevelMerges,

        /// <summary>
        /// Number of value readings
        /// </summary>
        ValueReads,

        /// <summary>
        /// Number of snapshot as call extend calls
        /// </summary>
        AsCallExtendings,

        /// <summary>
        /// Number of created string values
        /// </summary>
        CreatedStringValues,

        /// <summary>
        /// Number of search operations within hash containers on non-structured values
        /// </summary>
        SimpleHashSearches,

        /// <summary>
        /// Number of assign operations within hash containers on non-structured values
        /// </summary>
        SimpleHashAssigns,

        /// <summary>
        /// Number of created function values
        /// </summary>
        CreatedFunctionValues,

        /// <summary>
        /// Number of merged nemory entries
        /// </summary>
        MemoryEntryMerges,

        /// <summary>
        /// Number of memory entries comparison
        /// </summary>
        MemoryEntryComparisons,

        /// <summary>
        /// Number of created memory entries
        /// </summary>
        MemoryEntryCreation,


        /// <summary>
        /// Number of reads by index
        /// </summary>
        IndexReads,

        /// <summary>
        /// Number of reads by field
        /// </summary>
        FieldReads,

        /// <summary>
        /// Number of fetched global variables
        /// </summary>
        GlobalVariableFetches,

        /// <summary>
        /// Number of created long values
        /// </summary>
        CreatedLongValues,

        /// <summary>
        /// Number of declared functions
        /// </summary>
        DeclaredFunctions,

        /// <summary>
        /// Number of resolve function calls
        /// </summary>
        FunctionResolvings,

        /// <summary>
        /// Number of declared types
        /// </summary>
        DeclaredTypes,

        /// <summary>
        /// Number of resolve type calls
        /// </summary>
        TypeResolvings,

        /// <summary>
        /// Number of resolve method calls
        /// </summary>
        MethodResolvings,

        /// <summary>
        /// Number of created integer intervals
        /// </summary>
        CreatedIntIntervalValues,

        /// <summary>
        /// Number of created long integer intervals
        /// </summary>
        CreatedLongIntervalValues,

        /// <summary>
        /// Number of created float intervals
        /// </summary>
        CreatedFloatIntervalValues,

        /// <summary>
        /// Number of info values set for variable
        /// </summary>
        VariableInfoSettings,

        /// <summary>
        /// Number of info values set for value
        /// </summary>
        ValueInfoSettings,

        /// <summary>
        /// Number of info values read from value
        /// </summary>
        ValueInfoReads,

        /// <summary>
        /// Number of info values read from variable
        /// </summary>
        VariableInfoReads,

        /// <summary>
        /// Number of iterate operations on objects
        /// </summary>
        ObjectIterations,

        /// <summary>
        /// Number of iterate operations on array
        /// </summary>
        ArrayIterations,

        /// <summary>
        /// Number of snapshot changes of <see cref="SnapshotMode"/>
        /// </summary>
        ModeSwitch,

        /// <summary>
        /// Number of created <see cref="TypeValue"/>
        /// </summary>
        CreatedTypeValues,

        /// <summary>
        /// Number of searches on object type
        /// </summary>
        ObjectTypeSearches,

        /// <summary>
        /// Numericaly last member of current enumeration
        /// </summary>
        Last = ObjectTypeSearches,

    }

    /// <summary>
    /// Storage used for snapshot statistic measuring
    /// </summary>
    public class SnapshotStatistics
    {
        private readonly int[] _statistics = new int[(int)Statistic.Last + 1];

        private SnapshotStatistics(int[] statistics)
        {
            _statistics = statistics;
        }

        internal SnapshotStatistics()
        {
        }

        /// <summary>
        /// Gets statistics data stored in this object.
        /// </summary>
        /// <returns>statistics data stored in this object</returns>
        public int[] GetStatisticsValues()
        {
            return (int[])_statistics.Clone();
        }

        internal void Report(Statistic statistic, int value = 1)
        {
            _statistics[(int)statistic] += value;
        }

        internal SnapshotStatistics Clone()
        {
            var statistics = (int[])_statistics.Clone();
            return new SnapshotStatistics(statistics);
        }

        internal void MergeWith(SnapshotStatistics statistics)
        {
            for (int i = 0; i < _statistics.Length; ++i)
            {
                _statistics[i] += statistics._statistics[i];
            }
        }
    }
}