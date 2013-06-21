using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

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
        /// Current statement to analyze according to worklist algorithm.
        /// </summary>
        internal LangElement CurrentStatement { get { return _currentProgramPoint.Statement; } }

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

        internal AnalysisCallContext(BasicBlock entryPoint, FlowInputSet entryInSet)
        {
            _entryPoint = entryPoint;
            _entryInSet = entryInSet;          
        }

     
    }
}
