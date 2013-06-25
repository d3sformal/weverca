﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.Analysis.Memory
{
    public class SnapshotStatistics
    {
        public int CreatedIntValues;
        public int CreatedBooleanValues;
        public int CreatedFloatValues;
        public int CreatedObjectValues;
        public int CreatedArrayValues;
        public int CreatedAliasValues;
        public int AliasAssigns;
        public int MemoryEntryAssigns;
        public int SnapshotExtendings;
        public int WithCallMerges;
        public int ValueReads;
        public int CreatedCallSnapshots;
        public int CreatedStringValues;
        public int SimpleHashSearches;
        public int SimpleHashAssigns;
        public int CreatedFunctionValues;
        public int MemoryEntryMerges;
        public int MemoryEntryComparisons;
        public int MemoryEntryCreation;
        public int IndexAssigns;
        public int FieldAssigns;
        public int IndexReads;
        public int FieldReads;
        public int IndexAliasAssigns;
        public int FieldAliasAssigns;
        public int GlobalVariableFetches;

        internal SnapshotStatistics Clone()
        {
            throw new NotImplementedException();
        }        
    }
}