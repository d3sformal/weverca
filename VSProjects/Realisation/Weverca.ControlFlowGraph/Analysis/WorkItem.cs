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
        internal bool AtBlockEnd { get { return _position >= Block.Statements.Count - 1; } }
        internal readonly AssumptionCondition AssumptionCondition;
        internal bool NeedAssumptionConfirmation { get { return AssumptionCondition != null; } }

        private int _position;


        private WorkItem(BasicBlock block, AssumptionCondition condition)
        {
            if (block.Statements.Count == 0)
            {
                throw new NotSupportedException("Cannot add empty block as workItem");
            }

            AssumptionCondition = condition;
            Block = block;            
        }



        /// <summary>
        /// Creates work item from default branch of block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        internal static WorkItem FromDefaultBranch(BasicBlock block)
        {
            var conditions = from edge in block.OutgoingEdges select edge.Condition;
            var condition = new AssumptionCondition(ConditionForm.SomeNot, conditions.ToArray());

            return new WorkItem(block.DefaultBranch.To, condition);
        }

        internal static WorkItem FromEdge(ConditionalEdge edge)
        {
            AssumptionCondition condition = null;
            if (edge.Condition != null)
            {
                condition = new AssumptionCondition(ConditionForm.All, edge.Condition);
            }

            return new WorkItem(edge.To, condition);
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
