using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    class VariableInfo
    {
        internal List<VirtualReference> References { get; private set; }
        internal readonly VariableName Name;
        public VariableKind Kind;

        public bool IsGlobal { get { return Kind == VariableKind.Global; } }

        public VariableInfo(VariableName name, VariableKind kind)
        {
            References = new List<VirtualReference>();
            Kind = kind;
            Name = name;
        }

        internal VariableInfo Clone()
        {
            var result = new VariableInfo(Name,Kind);

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

        public override string ToString()
        {
            return string.Format("{0}|{1}", Name, Kind);
        }
    }
}
