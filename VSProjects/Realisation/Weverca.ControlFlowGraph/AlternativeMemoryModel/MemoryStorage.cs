using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Real storage for MemoryContext abstraction
    /// </summary>
    class MemoryStorage
    {
        Dictionary<MemoryEntryId, MemoryEntry> _entries = new Dictionary<MemoryEntryId, MemoryEntry>();


        internal IEnumerable<AbstractValue> GetPossibleValues(MemoryContext context, VirtualReference reference)
        {
            MemoryEntry entry;
            if (!_entries.TryGetValue(reference.MemoryEntryId, out entry))
            {
                throw new NotSupportedException("Reference contains invalid MemoryEntryId - probably belongs to another MemoryStorage");
            }
            
            Debug.Assert(entry.CreationVersion == reference.CreationVersion,"Versions has to match, possible invalid implementation of reference creating");
            

            return entry.GetPossibleValues(context.Version);
        }



        internal void OpenWriting(MemoryContextVersion buildedVersion)
        {
            throw new NotImplementedException();
        }

        internal void CloseWriting()
        {
            throw new NotImplementedException();
        }

        internal void Write(IEnumerable<VirtualReference> enumerable1, IEnumerable<AbstractValue> enumerable2)
        {
            throw new NotImplementedException();
        }

        internal VirtualReference ReferenceForVariable(PHP.Core.VariableName variableName)
        {
            throw new NotImplementedException();
        }
    }
}
