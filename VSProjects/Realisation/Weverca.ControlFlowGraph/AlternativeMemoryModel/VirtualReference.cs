using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Representation of reference into MemoryContext
    /// 
    /// WARNING: Reference has to be independent on concrete MemoryContex     
    /// NOTES: 
    /// * Is immutable    
    /// </summary>
    public class VirtualReference
    {
        /// <summary>
        /// Id describing slot in memory context, where data are stored
        /// </summary>
        internal readonly MemoryEntryId MemoryEntryId;
        /// <summary>
        /// Version of context where reference has been allocated
        /// </summary>
        internal readonly int CreationVersion;

        /// <summary>
        /// Creating references is highly internall stuff and shouldn't be accessible from public
        /// </summary>
        internal VirtualReference(MemoryEntryId memoryEntryId, int versionId)
        {
            MemoryEntryId = memoryEntryId;
            CreationVersion = versionId;
        }       
    }
}
