using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;
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
        /// Return partial from current statement that should be processed
        /// </summary>
        internal LangElement CurrentPartial { get { return _currentPartialContext.CurrentPartial; } }

        /// <summary>
        /// Program point graph for entry method CFG
        /// </summary>
        internal ProgramPointGraph ProgramPointGraph { get { return _ppGraph; } }

        #endregion

        /// <summary>
        /// Program point graph for entry method CFG
        /// </summary>
        ProgramPointGraph _ppGraph;      
        /// <summary>
        /// Items that has to be processed.
        /// </summary>
        Queue<ProgramPoint> _worklist = new Queue<ProgramPoint>();

        PartialContext _currentPartialContext;

        BasicBlock _entryPoint;
        FlowInputSet _entryInSet;
        AnalysisServices _services;

        internal AnalysisCallContext(BasicBlock entryPoint, FlowInputSet entryInSet,AnalysisServices services)
        {
            _entryPoint = entryPoint;
            _entryInSet = entryInSet;
            _services = services;

            _ppGraph = initProgramPoints(entryInSet);

            enqueueWorkDependencies(_ppGraph.Start);
            dequeueNextWorkItem();
        }
        
        internal void MoveNextPartial()
        {
            _currentPartialContext.MoveNextPartial();
            if (_currentPartialContext.IsComplete)
            {
                if (_currentPartialContext.OutSet.HasChanges)
                {
                    //enqueue dependencies, which input has changed
                    enqueueWorkDependencies(_currentPartialContext.Source);
                }
                
                //creates new partial context if possible
                dequeueNextWorkItem();
            }
        }

        private void dequeueNextWorkItem()
        {
            while (_worklist.Count > 0)
            {
                var work = _worklist.Dequeue();

                if (!work.IsEmpty)
                {
                    //we have program point that has to be processed                    
                    initWork(work);
                    return;
                }
            }
            _currentPartialContext = null;
        }

        private void initWork(ProgramPoint work)
        {
            var inputs = collectInputs(work);

            setInputs(work, inputs);            
            _currentPartialContext = new PartialContext(work);
        }

        private void setInputs(ProgramPoint work, IEnumerable<FlowInputSet> inputs)
        {
            var inputSnapshots = from input in inputs select input.Input;
            var workInput=(work.InSet as FlowOutputSet);

            //TODO performance improvement
            workInput.StartTransaction();
            workInput.Output.Extend(inputSnapshots.ToArray());
            workInput.Commit();
        }
        
        private IEnumerable<FlowInputSet> collectInputs(ProgramPoint point)
        {
            return from parent in point.Parents select parent.OutSet;
        }

        private ProgramPointGraph initProgramPoints(FlowInputSet startInput)
        {
            var ppGraph = new ProgramPointGraph(_entryPoint);
            
            var startOutput=_services.CreateEmptySet();
            startOutput.StartTransaction();
            startOutput.Output.Extend(startInput.Input);
            startOutput.Commit();

            ppGraph.Start.Initialize(startInput, startOutput);
            
            return ppGraph;
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

    }
}
