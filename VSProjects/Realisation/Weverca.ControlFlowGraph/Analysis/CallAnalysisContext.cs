using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Handle context for inter procedural analysis (Program points, dependencies,...)
    /// NOTE: Uses worklist algorithm for serving statements.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    class AnalysisCallContext<FlowInfo>
    {
        #region Worklist algorithm output API

        /// <summary>
        /// Determine that call context has empty worklist queue => analysis of call context is complete.
        /// </summary>
        internal bool IsComplete { get; private set; }

        /// <summary>
        /// Set which belongs to program point beffore current statement.
        /// WARNING: sets are copied because of avoiding unwanted changes.
        /// </summary>
        internal FlowInputSet<FlowInfo> CurrentInputSet { get; private set; }
        /// <summary>
        /// Set which belongs to program point after current statement.
        /// WARNING: sets are copied because of avoiding unwanted changes.
        /// </summary>
        internal FlowOutputSet<FlowInfo> CurrentOutputSet { get; private set; }

        /// <summary>
        /// Current statement to analyze according to worklist algorithm.
        /// </summary>
        internal LangElement CurrentStatement { get { return _currentWorkItem.CurrentStatement; } }
        
        /// <summary>
        /// Program point for CurrentStatement
        /// </summary>
        private ProgramPoint<FlowInfo> CurrentProgramPoint { get { return getProgramPoint(CurrentStatement); } }

        #endregion

        /// <summary>
        /// Currently proceeded workitem
        /// </summary>
        WorkItem _currentWorkItem;
        /// <summary>
        /// Items that has to be processed.
        /// </summary>
        Queue<ConditionalEdge> _worklist = new Queue<ConditionalEdge>();
        /// <summary>
        /// Program points for each statement.
        /// </summary>
        Dictionary<LangElement, ProgramPoint<FlowInfo>> _statementPoints = new Dictionary<LangElement, ProgramPoint<FlowInfo>>();

        internal AnalysisCallContext(ControlFlowGraph methodGraph)
        {            
            throw new NotImplementedException();
        }

        #region Worklist algorithm input API

        /// <summary>
        /// Skips to next statement if there is some in worklist queue. If there isn't, analysis is completed.
        /// </summary>
        internal void SkipToNextStatement()
        {
            var itemComplete = _currentWorkItem.AtBlockEnd;
            var hasUpdate = CurrentProgramPoint.HasUpdate;
            var hasChanged = hasUpdate && CurrentProgramPoint.CommitUpdate();

            if (hasChanged && itemComplete)
            {
                //notify other basic blocks and add them into work list
                updateDependencies(_currentWorkItem.BasicBlock);
            }
            else if (hasChanged)
            {
                //we are not at end of basic block - shift to next statement
                _currentWorkItem.NextStatement();
            }

            if (itemComplete)
            {
                //item is removed from worklist
                dequeueCurrentItem();
            }
        }

        /// <summary>
        /// Updates output set for current statement. Can cause adding new items into worklist algorithm queue.
        /// </summary>
        /// <param name="outSet">Output set for current statement.</param>
        internal void UpdateOutputSet(FlowOutputSet<FlowInfo> outSet)
        {
            var currentPoint = getProgramPoint(CurrentStatement);

            var oldOutSet = currentPoint.OutSet;
            if (oldOutSet == outSet)
            {
                //nothing to update
                return;
            }

            currentPoint.UpdateOutSet(outSet);
        }

        #endregion

        #region Private utils


        /// <summary>
        /// Get program point for given statement.         
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        private ProgramPoint<FlowInfo> getProgramPoint(LangElement statement)
        {
            ProgramPoint<FlowInfo> result;
            if (!_statementPoints.TryGetValue(statement, out result))
            {
                result = new ProgramPoint<FlowInfo>();
                _statementPoints.Add(statement, result);
            }

            return result;
        }


        private void dequeueCurrentItem()
        {
            if (_worklist.Count == 0)
            {
                //work is done
                IsComplete = true;
                _currentWorkItem = null;
                return;
            }

            var toProcess = _worklist.Dequeue();
            _currentWorkItem = new WorkItem(toProcess);
        }

        private void updateDependencies(BasicBlock block)
        {            
            foreach (var dependency in block.OutgoingEdges)
            {
                addWork(dependency);
            }
        }


        private void addWork(ConditionalEdge blockEdge)
        {
            if (_worklist.Contains(blockEdge))
            {
                return;
            }

            _worklist.Enqueue(blockEdge);
        }

        #endregion
    }
}
