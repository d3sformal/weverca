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
        internal FlowInputSet<FlowInfo> CurrentInputSet { get { return CurrentProgramPoint.InSet; } }
        /// <summary>
        /// Set which belongs to program point after current statement.
        /// WARNING: sets are copied because of avoiding unwanted changes.
        /// </summary>
        internal FlowOutputSet<FlowInfo> CurrentOutputSet { get { return CurrentProgramPoint.OutSet; } }

        /// <summary>
        /// Current statement to analyze according to worklist algorithm.
        /// </summary>
        internal LangElement CurrentStatement { get { return _currentWorkItem.CurrentStatement; } }

        /// <summary>
        /// Program point for CurrentStatement
        /// </summary>
        private ProgramPoint<FlowInfo> CurrentProgramPoint { get { return getProgramPoint(CurrentStatement); } }

        /// <summary>
        /// Ending program point of call.
        /// </summary>
        public ProgramPoint<FlowInfo> EndProgramPoint { get { return getEndProgramPoint(); } }

        #endregion

        /// <summary>
        /// Currently proceeded workitem
        /// </summary>
        WorkItem _currentWorkItem;
        /// <summary>
        /// Items that has to be processed.
        /// </summary>
        Queue<WorkItem> _worklist = new Queue<WorkItem>();
        /// <summary>
        /// Here are stored proccessed blocks which leads to method end.
        /// </summary>
        HashSet<BasicBlock> _endBlocks = new HashSet<BasicBlock>();
        /// <summary>
        /// Program points for each statement.
        /// </summary>
        Dictionary<LangElement, ProgramPoint<FlowInfo>> _statementPoints = new Dictionary<LangElement, ProgramPoint<FlowInfo>>();

        AnalysisServices<FlowInfo> _services;
        ControlFlowGraph _entryMethodCFG;

        internal AnalysisCallContext(ControlFlowGraph entryMethodCFG, AnalysisServices<FlowInfo> services)
        {
            _services = services;
            _entryMethodCFG = entryMethodCFG;
            var entryEdge = new ConditionalEdge(null, _entryMethodCFG.start, null);

            startWork(WorkItem.FromEdge(entryEdge));
            
            initProgramPoints();
        }

        #region Worklist algorithm input API

        /// <summary>
        /// Skips to next statement if there is some in worklist queue. If there isn't, analysis is completed.
        /// </summary>
        internal void SkipToNextStatement()
        {
            if (IsComplete)
            {
                throw new NotSupportedException("Cannot skip to next statement, when analysis is complete");
            }

            var itemComplete = _currentWorkItem.AtBlockEnd;
            var hasUpdate = CurrentProgramPoint.HasUpdate;
            var hasChanged = hasUpdate && CurrentProgramPoint.CommitUpdate();

            if (hasChanged && itemComplete)
            {
                //notify other basic blocks and add them into work list
                updateDependencies(_currentWorkItem.Block);
            }
            else if (hasChanged)
            {
                //we are not at end of basic block - shift to next statement
                _currentWorkItem.NextStatement();
            }

            if (itemComplete)
            {
                //item is removed from worklist
                if (_currentWorkItem.Block.OutgoingEdges.Count == 0)
                {
                    //ending block
                    _endBlocks.Add(_currentWorkItem.Block);
                }
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
                result.UpdateOutSet(_services.CreateEmptySet());
                result.CommitUpdate();
                _statementPoints.Add(statement, result);
            }

            return result;
        }


        private void dequeueCurrentItem()
        {
            var started = false;
            do
            {
                if (_worklist.Count == 0)
                {
                    //work is done
                    IsComplete = true;
                    _currentWorkItem = null;
                    return;
                }

                started = startWork(_worklist.Dequeue());
            } while (!started);
        }

        /// <summary>
        /// Starts given work, if assumption is valid.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool startWork(WorkItem item)
        {
            _currentWorkItem = item;

            var point = getProgramPoint(_currentWorkItem.BlockStart);

            var inSet = getBlockInput(_currentWorkItem.Block);

            if (_currentWorkItem.NeedAssumptionConfirmation)
            {
                var assumptedOutSet = _services.CreateEmptySet();
                if (!_services.ConfirmAssumption(inSet, _currentWorkItem.AssumptionCondition, assumptedOutSet))
                {
                    //assumption cannot be confirmed. => Flow is not reachable under assumption condition.
                    return false;
                }
                //else we use info from assumption
                inSet = assumptedOutSet;
            }

            point.InSet = inSet;
            return true;
        }

        /// <summary>
        /// Get input from all block points which this block depends on.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private FlowInputSet<FlowInfo> getBlockInput(BasicBlock block)
        {
            FlowInputSet<FlowInfo> result = null;
            foreach (var dependencyEdge in block.IncommingEdges)
            {
                var dependency = dependencyEdge.From;
                var depOutput = getBlockOutput(dependency);

                if (result == null)
                {
                    result = depOutput;
                }
                else
                {
                    var outSet = _services.CreateEmptySet();
                    _services.Merge(result, depOutput, outSet);
                    result = outSet;
                }
            }

            if (result == null)
            {
                result = _services.CreateEmptySet();
            }
            return result;
        }

        /// <summary>
        /// Get output from last statement in block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private FlowOutputSet<FlowInfo> getBlockOutput(BasicBlock block)
        {
            var lastStmt = block.Statements.Last();
            return getProgramPoint(lastStmt).OutSet;
        }

        private void updateDependencies(BasicBlock block)
        {
            foreach (var dependency in block.OutgoingEdges)
            {
                addWork(dependency);
            }

            if (block.DefaultBranch != null)
            {
                addWorkDefault(block);
            }
        }

        /// <summary>
        /// Add edge's block to worklist, if isn't present yet
        /// </summary>
        /// <param name="edge"></param>
        private void addWork(ConditionalEdge edge)
        {
            var work = WorkItem.FromEdge(edge);

            if (edge.To.Statements.Count == 0 || _worklist.Contains(work))
            {
                return;
            }

            _worklist.Enqueue(work);
        }

        /// <summary>
        /// Add default branch of block to worklist, if isn't present yet
        /// </summary>
        /// <param name="block"></param>
        private void addWorkDefault(BasicBlock block)
        {
            var work = WorkItem.FromDefaultBranch(block);

            if (block.DefaultBranch.To.Statements.Count == 0 || _worklist.Contains(work))
            {
                return;
            }

            _worklist.Enqueue(work);
        }


        private ProgramPoint<FlowInfo> getEndProgramPoint()
        {
            FlowOutputSet<FlowInfo> mergedOut = null;
            foreach (var endBlock in _endBlocks)
            {
                var depOutput = getBlockOutput(endBlock);

                if (mergedOut == null)
                {
                    mergedOut = depOutput;
                }
                else
                {
                    var outSet = _services.CreateEmptySet();
                    _services.Merge(mergedOut, depOutput, outSet);
                    mergedOut = outSet;
                }
            }

            if (mergedOut == null)
            {
                mergedOut = _services.CreateEmptySet();
            }
            var result = new ProgramPoint<FlowInfo>();
            result.UpdateOutSet(mergedOut);
            result.CommitUpdate();

            //TODO endpoint should be created properly
            return result;
        }



        private void initProgramPoints()
        {
            //TODO throw new NotImplementedException();
        }

        #endregion


    }
}
