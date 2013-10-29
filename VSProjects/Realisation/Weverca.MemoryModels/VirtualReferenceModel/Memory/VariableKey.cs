using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Determine kind of variable
    /// </summary>
    public enum VariableKind { 
        /// <summary>
        /// Variable is available in global scope
        /// </summary>
        Global, 
        /// <summary>
        /// Variable is available in local scope
        /// </summary>
        Local, 
        /// <summary>
        /// Variable is used for control information propaggated in global context
        /// </summary>
        GlobalControl,
        /// <summary>
        /// Variable is used for control information propaggated in local context
        /// </summary>
        LocalControl,
        /// <summary>
        /// Variable is used for snapshot internal's meta information
        /// </summary>
        Meta 
    }

    class VariableKey
    {
        internal readonly VariableKind Kind;

        internal readonly VariableName Name;

        internal VariableKey(VariableKind kind, VariableName name)
        {
            Kind = kind;
            Name = name;
        }
    }
}
