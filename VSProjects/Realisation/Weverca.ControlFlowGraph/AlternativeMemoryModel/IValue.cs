using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Representation of value that is stored in some MemoryContext
    /// 
    /// WARNING: All implementations has to be immutable 
    /// </summary>
    public abstract class AbstractValue: IDeepComparable
    {
        /// <summary>
        /// Get reference under which is given value stored in MemoryContext. 
        /// Note that reference isn't resolved according to concrete MemoryContext.
        /// </summary>
        /// <returns>Reference to this value.</returns>
        public VirtualReference Reference { get; private set; }

        public AbstractValue(VirtualReference reference)
        {
            Reference = reference;
        }

        public abstract int DeepGetHashCode();
        public abstract bool DeepEquals(object other);
        
    }
}
