using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{
    class WorkItem
    {
        public WorkItem(ConditionalEdge edge)
        {
            throw new NotImplementedException();
        }
        internal LangElement CurrentStatement { get; private set; }

        public bool AtBlockEnd { get; set; }

        public BasicBlock BasicBlock { get; set; }

        internal void NextStatement()
        {
            throw new NotImplementedException();
        }
    }
}
