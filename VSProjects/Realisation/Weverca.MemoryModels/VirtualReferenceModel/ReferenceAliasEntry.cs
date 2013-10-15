using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel
{
    class ReferenceAliasEntry : AliasEntry
    {
        internal readonly VariableInfo Variable;

        public ReferenceAliasEntry(VariableInfo variable)
        {
            Variable = variable.Clone();
        }
    }
}
