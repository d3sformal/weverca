using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    class VariableInfo
    {
        internal List<VirtualReference> References { get; private set; }
        internal readonly bool IsGlobal;
        internal readonly VariableName Name;

        public VariableInfo(VariableName name, bool isGlobal)
        {
            References = new List<VirtualReference>();
            IsGlobal = isGlobal;
            Name = name;
        }

        internal VariableInfo Clone()
        {
            var result = new VariableInfo(Name,IsGlobal);

            result.References.AddRange(References);
            return result;
        }

        public override int GetHashCode()
        {
            int sum = 0;
            foreach (var reference in References)
            {
                sum += reference.GetHashCode();
            }
            return sum;
        }

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
    }
}
