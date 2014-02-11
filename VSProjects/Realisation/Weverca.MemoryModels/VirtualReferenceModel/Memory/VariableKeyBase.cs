using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    abstract class VariableKeyBase
    {
        internal abstract VariableInfo GetOrCreateVariable(Snapshot snapshot);

        internal abstract VariableInfo GetVariable(Snapshot snapshot);

        internal abstract VirtualReference CreateImplicitReference(Snapshot snapshot);

    }
}
