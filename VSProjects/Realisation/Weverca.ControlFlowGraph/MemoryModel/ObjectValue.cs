using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.ControlFlowGraph.Analysis.Memory;
using Weverca.ControlFlowGraph.Analysis;

namespace Weverca.ControlFlowGraph.MemoryModel
{
    class ObjectValue : Weverca.ControlFlowGraph.Analysis.Memory.ObjectValue
    {

        protected override bool equals(Analysis.Memory.ObjectValue value)
        {
            throw new NotImplementedException();
        }

        protected override int getHashCode()
        {
            throw new NotImplementedException();
        }

        protected override Analysis.Memory.ObjectValue setField(ContainerIndex field, MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        protected override Analysis.Memory.ObjectValue setFieldAlias(ContainerIndex field, Analysis.Memory.AliasValue alias)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry GetField(ContainerIndex field)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.AliasValue CreateAlias(ContainerIndex field)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<PHP.Core.AST.FunctionDecl> GetFunction(string functionName)
        {
            throw new NotImplementedException();
        }

        public override Analysis.Memory.ObjectValue DefineFunction(string functionName, PHP.Core.AST.FunctionDecl function)
        {
            throw new NotImplementedException();
        }
    }
}
