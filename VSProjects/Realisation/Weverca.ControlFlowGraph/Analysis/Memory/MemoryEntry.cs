using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    /// <summary>
    /// Memory entry represents multiple possiblities that for example single variable can have
    /// NOTE:
    ///     * Is immutable    
    /// </summary>
    public class MemoryEntry
    {
        public readonly IEnumerable<Value> PossibleValues;

        internal MemoryEntry(params Value[] values)
        {
            PossibleValues = new ReadOnlyCollection<Value>((Value[])values.Clone());
        }
    }
}
