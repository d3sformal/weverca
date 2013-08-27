using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel
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
