using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    public class SnapshotStatistics
    {
        public int CreatedIntValues;
        public int CreatedBooleanValues;
        public int CreatedFloatValues;
        public int CreatedObjectValues;
        public int CreatedAliasValues;
        public int AliasAssigns;
        public int MemoryEntryAssigns;
        public int SnapshotExtendings;
        public int WithCallMerges;
        public int ValueReads;
        public int CreatedCallSnapshots;
        public int CreatedStringValues;

        internal SnapshotStatistics Clone()
        {
            throw new NotImplementedException();
        }

       
    }
}
