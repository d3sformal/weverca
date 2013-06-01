using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    public class MemoryEntry
    {
        public readonly IEnumerable<Value> PossibleValues;

        internal MemoryEntry(ReadOnlyCollection<Value> possibleValues)
        {
            PossibleValues = possibleValues;
        }
    }
}
