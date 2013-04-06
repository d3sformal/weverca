using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{
    class WorkItem
    {        
        internal readonly BasicBlock Block;
        internal LangElement CurrentStatement { get { return Block.Statements[_position]; } }
        internal LangElement BlockStart { get { return Block.Statements[0]; } }
        internal bool AtBlockEnd { get { return _position >= Block.Statements.Count-1; } }

        private int _position;
        
        public WorkItem(ConditionalEdge edge)
        {
            if (edge.Condition != null)
            {
                throw new NotImplementedException("Condition assumption");
            }

            Block = edge.To;
        }

        internal void NextStatement()
        {
            if (AtBlockEnd)
            {
                throw new NotSupportedException("Cannot go to next statement at block and");
            }
            ++_position;            
        }
    }
}
