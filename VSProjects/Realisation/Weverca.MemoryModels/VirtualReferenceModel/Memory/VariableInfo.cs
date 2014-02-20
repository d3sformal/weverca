using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Information about variable stored within snapshot
    /// </summary>
    class VariableInfo
    {
        /// <summary>
        /// List of current variable references
        /// </summary>
        internal List<VirtualReference> References { get; private set; }

        /// <summary>
        /// Name of variable
        /// </summary>
        internal readonly VariableName Name;

        /// <summary>
        /// Kind of current variable
        /// </summary>
        internal VariableKind Kind;

        /// <summary>
        /// Determine that variable is global
        /// </summary>
        internal bool IsGlobal { get { return Kind == VariableKind.Global; } }

        internal VariableInfo(VariableName name, VariableKind kind)
        {
            References = new List<VirtualReference>();
            Kind = kind;
            Name = name;
        }

        /// <summary>
        /// Clone variable info - to be possible use variable info in another snapshot
        /// </summary>
        /// <returns>Current variable clone</returns>
        internal VariableInfo Clone()
        {
            var result = new VariableInfo(Name,Kind);

            result.References.AddRange(References);
            return result;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int sum = 0;
            foreach (var reference in References)
            {
                sum += reference.GetHashCode();
            }
            return sum;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var o = obj as VariableInfo;
            if (o == null)
            {
                return false;   
            }

            var differInCount = References.Count != o.References.Count;
            if (differInCount)
            {
                return false;   
            }

            var hasDifferentReferences = References.Except(o.References).Any();
            return !hasDifferentReferences;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0}|{1}", Name, Kind);
        }
    }
}
