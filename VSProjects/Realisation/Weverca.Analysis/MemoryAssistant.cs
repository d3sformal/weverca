using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis
{
    class MemoryAssistant : MemoryAssistantBase
    {
        public override MemoryEntry ReadIndex(AnyValue value, MemberIdentifier index)
        {
            //throw new NotImplementedException();
            return new MemoryEntry(Context.AnyValue);
        }

        public override MemoryEntry ReadField(AnyValue value, AnalysisFramework.VariableIdentifier field)
        {
            //throw new NotImplementedException();
            return new MemoryEntry(Context.AnyValue);
        }
    }
}
