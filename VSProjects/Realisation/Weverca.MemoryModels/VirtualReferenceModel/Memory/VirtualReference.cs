using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Hash and equals according to originatedVariable - needed for merging two branches with same variable names
    /// </summary>
    class VirtualReference
    {
        /// <summary>
        /// Variable that cause creating this reference
        /// </summary>
        internal readonly VariableName OriginatedVariable;

        internal readonly VariableKind Kind;
        /// <summary>
        /// Create virtual reference according to originatedVariable
        /// </summary>
        /// <param name="originatedVariable">Variable determining reference target</param>
        internal VirtualReference(VariableName originatedVariable, VariableKind kind)
        {
            OriginatedVariable = originatedVariable;
            Kind = kind;
        }

        internal VirtualReference(VariableInfo info)
            : this(info.Name, info.Kind)
        {
        }

        public override int GetHashCode()
        {
            return OriginatedVariable.GetHashCode() + (int)Kind;
        }


        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            var o = obj as VirtualReference;

            if (o == null)
            {
                return false;
            }


            return o.OriginatedVariable == this.OriginatedVariable && o.Kind == this.Kind;
        }
    }
}
