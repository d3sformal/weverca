using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    /// <summary>
    /// Representation of pure variable in php source code    
    /// 
    /// NOTES:
    /// * Is immutable
    /// * Values of variable are resolved only according to concrete MemoryContext    /// 
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Name of represented variable
        /// </summary>
        public readonly VariableName Name;

        /// <summary>
        /// References that variable can have  (no reference means, that variable is uninitialized)
        /// </summary>
        public IEnumerable<Reference> PossibleReferences { get; private set; }

        /// <summary>
        /// Get values that can be possibly stored in given memory context.
        /// NOTE:
        ///     Values for all possible references will be merged
        /// </summary>
        public IEnumerable<AbstractValue> GetPossibleValues(MemoryContext context)
        {
            throw new NotImplementedException();
        }

    }
}
