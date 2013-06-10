using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.ControlFlowGraph.Analysis.Memory;

namespace Weverca.ControlFlowGraph.VirtualReferenceModel
{
    class ReferenceAlias: AliasValue
    {
        internal readonly List<VirtualReference> References;


        public ReferenceAlias(IEnumerable<VirtualReference> references)
        {
            References = new List<VirtualReference>(references);
        }
    }
}
