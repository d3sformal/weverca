using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    class ReferenceAliasEntry : AliasEntry
    {
        internal readonly VariableKeyBase Key;

        public ReferenceAliasEntry(VariableKeyBase key)
        {
            Key = key;
        }
    }
}
