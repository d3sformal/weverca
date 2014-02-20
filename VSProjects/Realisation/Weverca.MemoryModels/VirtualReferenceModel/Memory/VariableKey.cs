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

        /// <summary>
        /// Variables that are extended between calls
        /// </summary>
        CallExtends = Global | GlobalControl | Meta,

        /// <summary>
        /// All Variable kinds
        /// </summary>
        AllExtends = CallExtends | Local | LocalControl
    }

    /// <summary>
    /// Key of stored variable within snapshot
    /// </summary>
    class VariableKey : VariableKeyBase
    {
        /// <summary>
        /// Kind of current key
        /// </summary>
        internal readonly VariableKind Kind;

        /// <summary>
        /// Name of current key
        /// </summary>
        internal readonly VariableName Name;

        /// <summary>
        /// Context stamp of current key
        /// </summary>
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

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0}-{1}|{2}", Name, ContextStamp, Kind);
        }

        /// <inheritdoc />
        internal override VariableInfo GetOrCreateVariable(Snapshot snapshot)
        {
            return snapshot.GetOrCreateInfo(Name, Kind);
        }

        /// <inheritdoc />
        internal override VariableInfo GetVariable(Snapshot snapshot)
        {
            return snapshot.GetInfo(Name, Kind);
        }

        /// <inheritdoc />
        internal override VirtualReference CreateImplicitReference(Snapshot snapshot)
        {
            return new VirtualReference(Name, Kind, ContextStamp);
        }
    }
}
