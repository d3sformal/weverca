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
    /// <typeparam name="FlowInfo"></typeparam>
    class AnalysisCallContext
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
        internal FlowInputSet CurrentInputSet { get { return _currentProgramPoint.InSet; } }
        /// <summary>
        /// Set which belongs to program point after current statement.
        /// WARNING: sets are copied because of avoiding unwanted changes.
        /// </summary>
        internal FlowOutputSet CurrentOutputSet { get { return _currentProgramPoint.OutSet; } }


        /// <summary>
        /// Return partial from current statement that should be processed
        /// </summary>
        internal LangElement CurrentPartial { get { throw new NotImplementedException(); } }
        /// <summary>
        /// Current statement to analyze according to worklist algorithm.
        /// </summary>
        internal Postfix CurrentStatement { get { return _currentProgramPoint.Statement; } }

        public ProgramPointGraph ProgramPointGraph { get { return _ppGraph; } }

        #endregion

        /// <summary>
        /// Program point graph for entry method CFG
        /// </summary>
        ProgramPointGraph _ppGraph;
        /// <summary>
        /// Currently proceeded workitem
        /// </summary>
        ProgramPoint _currentProgramPoint;
        /// <summary>
        /// Items that has to be processed.
        /// </summary>
        Queue<ProgramPoint> _worklist = new Queue<ProgramPoint>();


        BasicBlock _entryPoint;
        FlowInputSet _entryInSet;
        AnalysisServices _services;

        internal AnalysisCallContext(BasicBlock entryPoint, FlowInputSet entryInSet,AnalysisServices services)
        {
            _entryPoint = entryPoint;
            _entryInSet = entryInSet;
            _services = services;

            _ppGraph = initProgramPoints(entryInSet);

            enqueueDependencies(_ppGraph.Start);
            dequeueNextItem();
        }
        
        internal void ShiftNextPartial()
        {
            throw new NotImplementedException();
        }

        private void dequeueNextItem()
        {
            while (_worklist.Count > 0)
            {
                var work = _worklist.Dequeue();

                if (!work.IsEmpty)
                {
                    //program point that has to be processed                    
                    var inputs = collectInputs(work);

                    setInputs(work, inputs);
                    _currentProgramPoint = work;
                    return;
                }
            }
            _currentProgramPoint = null;
        }

        private void setInputs(ProgramPoint work, IEnumerable<FlowInputSet> inputs)
        {
            var inputSnapshots = from input in inputs select input.Input;
            var workInput=(work.InSet as FlowOutputSet).Output;

            workInput.Extend(inputSnapshots.ToArray());
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

        private void enqueueDependencies(ProgramPoint point)
        {
            foreach (var child in point.Children)
            {
                addWork(child);
            }
            point.ResetChanges();
        }

        /// <summary>
        /// Add edge's block to worklist, if isn't present yet
        /// </summary>
        /// <param name="edge"></param>
        private void addWork(ProgramPoint work)
        {
            if (_worklist.Contains(work))
            {
                return;
            }
            _worklist.Enqueue(work);
        }

    }
}
