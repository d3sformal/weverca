using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Work item for worklist alogrithm.
    /// </summary>
    class WorkItem
    {
        /// <summary>
        /// Block which statements are proceeded during this work item.
        /// </summary>
        internal readonly BasicBlock Block;
        /// <summary>
        /// Currently proceeded statement.
        /// </summary>
        internal LangElement CurrentStatement { get { return Block.Statements[_position]; } }
        /// <summary>
        /// Start statement of block.
        /// </summary>
        internal LangElement BlockStart { get { return Block.Statements[0]; } }
        /// <summary>
        /// Determine that last block's statement has been reached.
        /// </summary>
        internal bool AtBlockEnd { get { return _position >= Block.Statements.Count - 1; } }
        /// <summary>
        /// Assumption condition for work item. Null, if there isn't condition.
        /// </summary>
        internal readonly AssumptionCondition AssumptionCondition;
        /// <summary>
        /// Determine that work item needs assumption confirmation.
        /// </summary>
        internal bool NeedAssumptionConfirmation { get { return AssumptionCondition != null; } }

        /// <summary>
        /// Current position in block.
        /// </summary>
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

        /// <summary>
        /// Creates workitem from edge.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        internal static WorkItem FromEdge(ConditionalEdge edge)
        {
            AssumptionCondition condition = null;
            if (edge.Condition != null)
            {
                condition = new AssumptionCondition(ConditionForm.All, edge.Condition);
            }

            return new WorkItem(edge.To, condition);
        }

        /// <summary>
        /// Skip to next statement in workitem.
        /// </summary>
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
