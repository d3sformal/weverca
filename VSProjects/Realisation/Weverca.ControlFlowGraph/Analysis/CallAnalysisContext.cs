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
        internal FlowInputSet<FlowInfo> CurrentInputSet { get { return _currentProgramPoint.InSet; } }
        /// <summary>
        /// Set which belongs to program point after current statement.
        /// WARNING: sets are copied because of avoiding unwanted changes.
        /// </summary>
        internal FlowOutputSet<FlowInfo> CurrentOutputSet { get { return _currentProgramPoint.OutSet; } }

        internal FlowOutputSet<FlowInfo> CurrentOutputSetUpdate { get {  
            var result=_currentProgramPoint.OutSetUpdate; 
            if(result==null)
                result = _services.CreateEmptySet();

            return result;
        } }

        /// <summary>
        /// Current statement to analyze according to worklist algorithm.
        /// </summary>
        internal LangElement CurrentStatement { get { return _currentProgramPoint.Statement; } }

        public ProgramPointGraph<FlowInfo> ProgramPointGraph { get { return _ppGraph; } }

        #endregion

        /// <summary>
        /// Program point graph for entry method CFG
        /// </summary>
        ProgramPointGraph<FlowInfo> _ppGraph;
        /// <summary>
        /// Currently proceeded workitem
        /// </summary>
        ProgramPoint<FlowInfo> _currentProgramPoint;
        /// <summary>
        /// Items that has to be processed.
        /// </summary>
        Queue<ProgramPoint<FlowInfo>> _worklist = new Queue<ProgramPoint<FlowInfo>>();
 
      
        AnalysisServices<FlowInfo> _services;
        BasicBlock _entryPoint;
        FlowInputSet<FlowInfo> _entryInSet;

        internal AnalysisCallContext(BasicBlock entryPoint,FlowInputSet<FlowInfo> entryInSet, AnalysisServices<FlowInfo> services)
        {
            _services = services;
            _entryPoint = entryPoint;
            _entryInSet = entryInSet;
            
           _ppGraph=initProgramPoints(entryInSet);
                      
           enqueueDependencies(_ppGraph.Start);
           dequeuNextItem();
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

       
            var hasUpdate = _currentProgramPoint.HasUpdate;
            var hasChanged = hasUpdate && _currentProgramPoint.CommitUpdate();

            if (hasChanged)
            {
                //notify other basic blocks and add them into work list
                enqueueDependencies(_currentProgramPoint);            
            }
                              
            dequeuNextItem();            
        }

        /// <summary>
        /// Updates output set for current statement. Can cause adding new items into worklist algorithm queue.
        /// </summary>
        /// <param name="outSet">Output set for current statement.</param>
        internal void UpdateOutputSet(FlowOutputSet<FlowInfo> outSet)
        {          
            _currentProgramPoint.UpdateOutSet(outSet);
        }

        #endregion

        #region Private utils
        
        private void dequeuNextItem()
        {
            var started = false;
            do
            {
                if (_worklist.Count == 0)
                {
                    //work is done
                    IsComplete = true;
                    _currentProgramPoint = null;
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
        private bool startWork(ProgramPoint<FlowInfo> work)
        {
            var inputSet = computeInput(work);
            work.InSet = inputSet;
            if (work.IsCondition)
            {
                //Conditional program point cannot be set as current work item

                var assumptedOutSet = _services.CreateEmptySet();
                if (!_services.ConfirmAssumption(work.InSet, work.Condition, assumptedOutSet))
                {
                    //assumption cannot be confirmed. => Flow is not reachable under assumption condition.
                    return false;
                }
                work.UpdateOutSet(assumptedOutSet);
                if (work.CommitUpdate())
                {
                    enqueueDependencies(work);
                }
                
                return false;
            }

            if (work.IsEmpty)
            {
                //Empty program point cannot be set as current work item
                work.UpdateOutSet(inputSet);//empty block doesn't change flow
                if (work.CommitUpdate())
                {
                    enqueueDependencies(work);
                }
                return false;
            }

            _currentProgramPoint = work;
            return true;
        }

        private FlowOutputSet<FlowInfo> computeInput(ProgramPoint<FlowInfo> point)
        {
            if (!point.Parents.Any())
            {
                //there is no preceding program point.
                return point.InSet as FlowOutputSet<FlowInfo>;
            }

            FlowOutputSet<FlowInfo> result=null;

            foreach (var parent in point.Parents)
            {
                if (result == null)
                {
                    result = parent.OutSet;
                }
                else
                {
                    var mergeOutput=_services.CreateEmptySet();
                    _services.Merge(result, parent.OutSet, mergeOutput);
                    result = mergeOutput;
                }
            }
            return result;
        }

        private void enqueueDependencies(ProgramPoint<FlowInfo> point)
        {
            foreach (var child in point.Children)
            {
                addWork(child);
            }
        }
        
        /// <summary>
        /// Add edge's block to worklist, if isn't present yet
        /// </summary>
        /// <param name="edge"></param>
        private void addWork(ProgramPoint<FlowInfo> work)
        {
            if (_worklist.Contains(work))
            {
                return;
            }
            _worklist.Enqueue(work);
        }


        private ProgramPointGraph<FlowInfo> initProgramPoints(FlowInputSet<FlowInfo> startInfo)
        {
            var ppGraph= new ProgramPointGraph<FlowInfo>(_entryPoint);

            foreach (var point in ppGraph.Points)
            {
                point.InSet = _services.CreateEmptySet();
                point.UpdateOutSet(_services.CreateEmptySet());
                point.CommitUpdate();
            }

            var startInfoCopy = (startInfo as FlowOutputSet<FlowInfo>).Copy();
            ppGraph.Start.InSet = startInfo;
            ppGraph.Start.UpdateOutSet(startInfoCopy);
            ppGraph.Start.CommitUpdate();
            return ppGraph;
        }

        #endregion


    }
}
