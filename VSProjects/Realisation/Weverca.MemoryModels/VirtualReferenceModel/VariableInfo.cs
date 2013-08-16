using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.VirtualReferenceModel
{
    class VariableInfo
    {
        internal List<VirtualReference> References { get; private set; }

        public VariableInfo()
        {
            References = new List<VirtualReference>();
        }

        internal VariableInfo Clone()
        {
            var result = new VariableInfo();

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
