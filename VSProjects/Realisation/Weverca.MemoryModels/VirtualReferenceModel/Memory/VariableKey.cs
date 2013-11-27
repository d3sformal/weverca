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
    [Flags]
    public enum VariableKind
    {
        /// <summary>
        /// Variable is available in global scope
        /// </summary>
        Global = 1,
        /// <summary>
        /// Variable is available in local scope
        /// </summary>
        Local = 2,
        /// <summary>
        /// Variable is used for control information propaggated in global context
        /// </summary>
        GlobalControl = 4,
        /// <summary>
        /// Variable is used for control information propaggated in local context
        /// </summary>
        LocalControl = 8,
        /// <summary>
        /// Variable is used for snapshot internal's meta information
        /// </summary>
        Meta = 16,

        CallExtends = Global | GlobalControl | Meta,

        AllExtends = CallExtends | Local | LocalControl
    }

    class VariableKey
    {
        internal readonly VariableKind Kind;

        internal readonly VariableName Name;

        internal readonly int ContextStamp;

        internal VariableKey(VariableKind kind, VariableName name, int contextStamp)
        {
            Kind = kind;
            Name = name;
            if (
                kind.HasFlag(VariableKind.Global) |
                kind.HasFlag(VariableKind.GlobalControl)
                ) contextStamp = 0;

            ContextStamp = contextStamp;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}|{2}", Name, ContextStamp, Kind);
        }
    }
}
