using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.AnalysisFramework.Memory
{
    public enum Statistic
    {
        CreatedIntValues,
        CreatedBooleanValues,
        CreatedFloatValues,
        CreatedObjectValues,
        CreatedArrayValues,
        CreatedAliasValues,
        AliasAssigns,
        MemoryEntryAssigns,
        SnapshotExtendings,
        CallLevelMerges,
        ValueReads,
        AsCallExtendings,
        CreatedStringValues,
        SimpleHashSearches,
        SimpleHashAssigns,
        CreatedFunctionValues,
        MemoryEntryMerges,
        MemoryEntryComparisons,
        MemoryEntryCreation,
        IndexAssigns,
        FieldAssigns,
        IndexReads,
        FieldReads,
        IndexAliasAssigns,
        FieldAliasAssigns,
        GlobalVariableFetches,
        CreatedLongValues,
        CreatedIndexes,
        DeclaredFunctions,
        FunctionResolvings,
        DeclaredTypes,
        TypeResolvings,
        MethodResolvings,
        CreatedIntIntervalValues,
        CreatedLongIntervalValues,
        CreatedFloatIntervalValues,
        VariableInfoSettings,
        ValueInfoSettings,
        ValueInfoReads,
        VariableInfoReads,
        ObjectIterations,
        ArrayIterations,
        CreatedSourceTypeValues,
        CreatedNativeTypeValues,
        VariableExistSearches,
        ObjectFieldExistsSearches,
        ArrayIndexExistsSearches,
        ValueReadAttempts,
        FieldReadAttempts,
        IndexReadAttempts,
        ModeSwitch,
        ObjectTypeSearches,

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

        internal void Report(Statistic statistic, int value = 1)
        {
            _statistics[(int)statistic] += value;
        }

        internal SnapshotStatistics Clone()
        {
            var statistics = (int[])_statistics.Clone();
            return new SnapshotStatistics(statistics);
        }

        public int[] GetStatisticsValues()
        {
            return (int[])_statistics.Clone();
        }

        public void MergeWith(SnapshotStatistics statistics)
        {
            for (int i = 0; i < _statistics.Length; ++i)
            {
                _statistics[i] += statistics._statistics[i];
            }
        }
    }
}
