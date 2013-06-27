using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;
using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
    /// <summary>
    /// Handle context for inter procedural analysis (Program points, dependencies,...)
    /// NOTE: Uses worklist algorithm for serving statements.
    /// </summary>    
    class AnalysisCallContext
    {
        #region Worklist algorithm output API

        /// <summary>
        /// Determine that call context has empty worklist queue => analysis of call context is complete.
        /// </summary>
        internal bool IsComplete { get { return _currentPartialContext == null; } }

        /// <summary>
        /// Set which belongs to program point beffore current statement.
        /// WARNING: sets are copied because of avoiding unwanted changes.
        /// </summary>
        internal FlowInputSet CurrentInputSet { get { return _currentPartialContext.InSet; } }
        /// <summary>
        /// Set which belongs to program point after current statement.
        /// WARNING: sets are copied because of avoiding unwanted changes.
        /// </summary>
        internal FlowOutputSet CurrentOutputSet { get { return _currentPartialContext.OutSet; } }

        /// <summary>
        /// Get partial from current statement that should be processed
        /// </summary>
        internal LangElement CurrentPartial { get { return _currentPartialContext.CurrentPartial; } }
        /// <summary>
        /// Get currently processed program point
        /// </summary>
        internal ProgramPoint CurrentProgramPoint { get { return _currentPartialContext.Source; } }
        /// <summary>
        /// Current partial walker including partial stack - is recycled between statements
        /// </summary>
        internal PartialWalker CurrentWalker { get; private set; }

        /// <summary>
        /// Program point graph for entry method CFG
        /// </summary>
        internal ProgramPointGraph ProgramPointGraph { get { return _methodGraph; } }

        #endregion

        /// <summary>
        /// Program point graph for entry method CFG
        /// </summary>
        ProgramPointGraph _methodGraph;
        /// <summary>
        /// Items that has to be processed.
        /// </summary>
        Queue<ProgramPoint> _worklist = new Queue<ProgramPoint>();

        PartialContext _currentPartialContext;

    
        FlowInputSet _entryInSet;
        AnalysisServices _services;

        internal AnalysisCallContext(FlowInputSet entryInSet,ProgramPointGraph methodGraph, AnalysisServices services)
        {
            _entryInSet = entryInSet;
            _services = services;

            CurrentWalker = services.CreateWalker();

            initializeProgramPointGraph(methodGraph, entryInSet);
            _methodGraph = methodGraph;

            enqueueWorkDependencies(_methodGraph.Start);
            dequeueNextWorkItem();
        }

        internal void NextPartial()
        {
            _currentPartialContext.MoveNextPartial();
            if (_currentPartialContext.IsComplete)
            {
                var completedProgramPoint = _currentPartialContext.Source;

                if (completedProgramPoint.IsCondition)
                {                    
                    conditionCompleted(completedProgramPoint, CurrentWalker.PopAllValues());
                }
                else
                {
                    statementCompleted(completedProgramPoint);
                }

                completedProgramPoint.OutSet.CommitTransaction();
                
                //creates new partial context if possible
                dequeueNextWorkItem();
            }
        }

        private void statementCompleted(ProgramPoint statement)
        {
            if (!statement.OutSet.HasChanges)
            {
                //nothing changed - dependencies won't be affected
                return;
            }
            //enqueue dependencies, which input has changed
            enqueueWorkDependencies(statement);
        }

        private void conditionCompleted(ProgramPoint conditionPoint, MemoryEntry[] expressionParts)
        {
            var flow = new FlowControler(conditionPoint.InSet, conditionPoint.OutSet);
            if (_services.ConfirmAssumption(flow,conditionPoint.Condition, expressionParts))
            {
                //assumption is made
                enqueueWorkDependencies(conditionPoint);
            }
        }

        private void dequeueNextWorkItem()
        {
            while (_worklist.Count > 0)
            {
                var work = _worklist.Dequeue();
                
                //we have program point that has to be processed                    
                initWork(work);
                return;

            }
            _currentPartialContext = null;
        }

        private void initWork(ProgramPoint work)
        {
            var inputs = collectInputs(work);

            setInputs(work, inputs);
            work.OutSet.StartTransaction();
            _currentPartialContext = new PartialContext(work);
            CurrentWalker.Reset();

            removeAllInvokedGraphs(work);
            _services.FlowThrough(new FlowControler(),work);
        }

        private void removeAllInvokedGraphs(ProgramPoint programPoint)
        {
            //TODO collect info for back invoking shared graphs
            foreach (var ppGraph in programPoint.InvokedGraphs)
            {
                ppGraph.RemoveInvocationPoint(programPoint);
            }
        }

        private void setInputs(ProgramPoint work, IEnumerable<FlowInputSet> inputs)
        {
            ensureInitialized(work);

            var workInput = (work.InSet as FlowOutputSet);

            //TODO performance improvement
            workInput.StartTransaction();
            workInput.Extend(inputs.ToArray());
            workInput.CommitTransaction();
        }

        private void ensureInitialized(ProgramPoint work)
        {
            work.Initialize(_services.CreateEmptySet(), _services.CreateEmptySet());
        }

        private IEnumerable<FlowInputSet> collectInputs(ProgramPoint point)
        {
            return from parent in point.Parents where parent.OutSet!=null select parent.OutSet;
        }

     

        private void enqueueWorkDependencies(ProgramPoint point)
        {
            foreach (var child in point.Children)
            {
                enqueueWork(child);
            }
            point.ResetChanges();
        }

        /// <summary>
        /// Add edge's block to worklist, if isn't present yet
        /// </summary>
        /// <param name="edge"></param>
        private void enqueueWork(ProgramPoint work)
        {
            if (_worklist.Contains(work))
            {
                return;
            }
            _worklist.Enqueue(work);
        }

        private void initializeProgramPointGraph(ProgramPointGraph ppGraph,FlowInputSet startInput)
        {
            var startOutput = _services.CreateEmptySet();
            startOutput.StartTransaction();
            startOutput.Extend(startInput);
            startOutput.CommitTransaction();

            ppGraph.Start.Initialize(startInput, startOutput);
        }

        
    }
}
