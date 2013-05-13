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
    /// * Values of variable are resolved only according to concrete MemoryContext
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Name of represented variable
        /// </summary>
        public readonly VariableName Name;
     
        /// <summary>
        /// References that variable can have 
        ///     * no reference means, that variable is undefined
        ///     * reference on no possible values is uninitialized
        /// </summary>
        public IEnumerable<VirtualReference> PossibleReferences { get; private set; }

        public Variable(VariableName name, IEnumerable<VirtualReference> possibleReferences)
        {
            Name = name;
            PossibleReferences = new List<VirtualReference>(possibleReferences);
        }


        /// <summary>
        /// Get values that can be possibly stored in given memory context.
        /// NOTE:
        ///     Values for all possible references will be merged
        /// </summary>
        public IEnumerable<AbstractValue> GetPossibleValues(MemoryContext context)
        {
            var outputSet = new HashSet<AbstractValue>();
            foreach (var reference in PossibleReferences)
            {
                foreach (var possibleValue in context.GetPossibleValues(reference))
                {
                    outputSet.Add(possibleValue);
                }
            }

            return outputSet;
        }

    }
}
