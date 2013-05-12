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
        /// Get values that are present on stored address in given context.
        /// </summary>
        /// <param name="context">Context where values are searched</param>
        /// <returns>Values referenced by this reference</returns>
        public IEnumerable<AbstractValue> GetValues(MemoryContext context)
        {
            throw new NotImplementedException();
        }
    }
}
