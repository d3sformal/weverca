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
    /// NOTE:
    ///     * Uses worklist algorithm for serving statements.
    ///     * Dispatch can be caused because of call, include,...
    /// </summary>    
  /*  class AnalysisDispatchContext
    {
        #region Worklist algorithm output API

        /// <summary>
        /// Type of dispatch (describes why dispatch was made)
        /// </summary>
        internal readonly DispatchType DispatchType;

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
        internal ProgramPointBase CurrentProgramPoint { get { return _currentPartialContext.Source; } }

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
        Queue<ProgramPointBase> _worklist = new Queue<ProgramPointBase>();

        /// <summary>
        /// Partial context of current partial
        /// </summary>
        PartialContext _currentPartialContext;

        /// <summary>
        /// Services available from analysis
        /// </summary>
        AnalysisServices _services;

        internal AnalysisDispatchContext(ProgramPointGraph methodGraph, AnalysisServices services, DispatchType dispatchType, params FlowInputSet[] entryInSets)
        {
            _services = services;
            DispatchType = dispatchType;

            CurrentWalker = services.CreateWalker();

            initializeProgramPointGraph(methodGraph, entryInSets);
            _methodGraph = methodGraph;

            enqueueWorkDependencies(_methodGraph.Start);
            dequeueNextWorkItem();
        }

        #region Worklist algorithm processing
        /// <summary>
        /// Move current partial to next partial. Uses Work list algorithm.
        /// </summary>
        internal void NextPartial()
        {
            _currentPartialContext.MoveNextPartial();
            if (_currentPartialContext.IsComplete)
            {
                CurrentWalker.OnComplete();
                var completedProgramPoint = _currentPartialContext.Source;

                if (completedProgramPoint.IsCondition)
                {
                    conditionCompleted(CurrentWalker.CurrentFlow);
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

        /// <summary>
        /// Dequeue (with initialization) next work item from worklist
        /// </summary>
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

        /// <summary>
        /// Enqueue depencencies of given point into worklist
        /// </summary>
        /// <param name="point">Resolved program point</param>
        private void enqueueWorkDependencies(ProgramPointBase point)
        {
            if (point.OutSet.HasChanges)
            {
                foreach (var child in point.FlowChildren)
                {
                    enqueueWork(child);
                }
                point.ResetChanges();
            }
        }

        /// <summary>
        /// Enqueue given work as program point to worklist (if is not present yet)
        /// </summary>
        /// <param name="work">Enqueued program point</param>
        private void enqueueWork(ProgramPointBase work)
        {
            if (_worklist.Contains(work))
            {
                return;
            }
            _worklist.Enqueue(work);
        }

        #endregion

        #region Completition handlers

        /// <summary>
        /// Handler called when analysis of all partials in statement is completed
        /// </summary>
        /// <param name="statement">Program point of completed statement</param>
        private void statementCompleted(ProgramPointBase statement)
        {
            if (!statement.OutSet.HasChanges)
            {
                //nothing changed - dependencies won't be affected
                return;
            }
            //enqueue dependencies, which input has changed
            enqueueWorkDependencies(statement);
        }

        /// <summary>
        /// Handler called when analysis of all partials in condition is completed
        /// </summary>       
        private void conditionCompleted(FlowController flow)
        {
            if (_services.ConfirmAssumption(flow, flow.ProgramPoint.Condition))
            {
                //assumption is made
                enqueueWorkDependencies(flow.ProgramPoint);
            }
        }
        #endregion

        #region Initialization routines

        /// <summary>
        /// Initialize program point after dequeing from worklist
        /// </summary>
        /// <param name="point">Program point that will be initialized</param>
        private void initWork(ProgramPointBase point)
        {
            var inputs = collectInputs(point);

            setInputs(point, inputs);
            point.OutSet.StartTransaction();
            //all outsets has to be extended by its in sets
            point.OutSet.Extend(point.InSet);
            _currentPartialContext = new PartialContext(point);
            CurrentWalker.Reset();

            _services.FlowThrough(point);
        }

        /// <summary>
        /// Set input of given program point
        /// </summary>
        /// <param name="point">Program point, which input will be set</param>
        /// <param name="inputs">Inputs available for program point</param>
        private void setInputs(ProgramPointBase point, IEnumerable<FlowInputSet> inputs)
        {
            ensureInitialized(point);

            var workInput = (point.InSet as FlowOutputSet);

            //TODO performance improvement
            workInput.StartTransaction();
            workInput.Extend(inputs.ToArray());
            workInput.CommitTransaction();
        }

        /// <summary>
        /// Initialize program point if needed
        /// </summary>
        /// <param name="point">Program point to be initialized</param>
        private void ensureInitialized(ProgramPointBase point)
        {
            if (!point.IsInitialized)
            {
                point.Initialize(_services.CreateEmptySet(), _services.CreateEmptySet());
            }
        }

        /// <summary>
        /// Collect inputs from given program point
        /// </summary>
        /// <param name="point">Program point which inputs are returned</param>
        /// <returns>Collected inputs</returns>
        private IEnumerable<FlowInputSet> collectInputs(ProgramPointBase point)
        {
            return from parent in point.FlowParents where parent.OutSet != null select parent.OutSet;
        }
               
        /// <summary>
        /// Initialize program point graph. Input is set for start program point.
        /// </summary>
        /// <param name="ppGraph">Initialized program point graph</param>
        /// <param name="startInputs">Inputs that will be merged as input for ppGraph.Start</param>
        private void initializeProgramPointGraph(ProgramPointGraph ppGraph, FlowInputSet[] startInputs)
        {
            if (!ppGraph.Start.IsInitialized)
            {
                var startInput = _services.CreateEmptySet();
                var startOutput = _services.CreateEmptySet();

                ppGraph.Start.Initialize(startInput, startOutput);
            }

            var inputSet = (ppGraph.Start.InSet as FlowOutputSet);
            inputSet.StartTransaction();
            inputSet.Extend(startInputs);
            inputSet.CommitTransaction();

            ppGraph.Start.OutSet.StartTransaction();
            ppGraph.Start.OutSet.Extend(inputSet);
            ppGraph.Start.OutSet.CommitTransaction();
        }
        #endregion
    }*/
}
