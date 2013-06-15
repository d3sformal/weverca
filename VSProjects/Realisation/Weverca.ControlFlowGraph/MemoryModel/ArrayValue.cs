using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.ControlFlowGraph.Analysis.Memory;
using Weverca.ControlFlowGraph.Analysis;

namespace Weverca.ControlFlowGraph.MemoryModel
{
    class ArrayValue : Weverca.ControlFlowGraph.Analysis.Memory.AssociativeArray
    {
        Dictionary<ContainerIndex, VariableInfo> data = new Dictionary<ContainerIndex, VariableInfo>();

        protected override bool equals(AssociativeArray value)
        {
            throw new NotImplementedException();
        }

        protected override int getHashCode()
        {
            throw new NotImplementedException();
        }

        protected override AssociativeArray setIndex(ContainerIndex index, MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        protected override AssociativeArray setIndexAlias(ContainerIndex index, Analysis.Memory.AliasValue alias)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry GetIndex(ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.AliasValue CreateAlias(ContainerIndex index)
        {
            throw new NotImplementedException();
        }
    }
}
