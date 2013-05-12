using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Identificator for MemoryEntry in a MemoryContext
    /// </summary>
    class MemoryEntryId
    {
        internal readonly int Slot;

        internal MemoryEntryId(int slot)
        {
            Slot = slot;
        }
    }
}
