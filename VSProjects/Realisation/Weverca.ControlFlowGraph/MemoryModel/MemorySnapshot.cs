using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using Weverca.ControlFlowGraph.Analysis.Memory;

namespace Weverca.ControlFlowGraph.MemoryModel
{
    class MemorySnapshot : AbstractSnapshot
    {
        protected override void startTransaction()
        {
            throw new NotImplementedException();
        }

        protected override bool commitTransaction()
        {
            throw new NotImplementedException();
        }

        protected override ObjectValue createObject()
        {
            throw new NotImplementedException();
        }

        protected override AssociativeArray createArray()
        {
            throw new NotImplementedException();
        }

        protected override AliasValue createAlias(PHP.Core.VariableName sourceVar)
        {
            throw new NotImplementedException();
        }

        protected override AbstractSnapshot createCall(Analysis.CallInfo callInfo)
        {
            throw new NotImplementedException();
        }

        protected override void assign(PHP.Core.VariableName targetVar, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void assignAlias(PHP.Core.VariableName targetVar, AliasValue alias)
        {
            throw new NotImplementedException();
        }

        protected override void extend(ISnapshotReadonly[] inputs)
        {
            throw new NotImplementedException();
        }

        protected override void mergeWithCall(Analysis.CallResult result, ISnapshotReadonly callOutput)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry readValue(PHP.Core.VariableName sourceVar)
        {
            throw new NotImplementedException();
        }
    }
}
