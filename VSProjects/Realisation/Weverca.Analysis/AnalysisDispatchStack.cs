using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.Analysis
{
    /// <summary>
    /// Handle dispatch stack for forward flow analysis.    
    /// Dispatch can be caused by call, include,...
    /// </summary>    
    class AnalysisDispatchStack
    {
        #region Private members
        /// <summary>
        /// Stack of call contexts
        /// </summary>        
        private Stack<DispatchLevel> _callStack = new Stack<DispatchLevel>();

        /// <summary>
        /// Available services obtained from analysis
        /// </summary>
        private readonly AnalysisServices _services;
        #endregion

        /// <summary>
        /// Current context of call from current dispatch level 
        /// </summary>
        internal AnalysisDispatchContext CurrentContext { get { return CurrentLevel.CurrentContext; } }
        /// <summary>
        /// Current dispatch level.
        /// </summary>
        internal DispatchLevel CurrentLevel { get { return _callStack.Peek(); } }
        /// <summary>
        /// Determine that call stack is empty
        /// </summary>
        internal bool IsEmpty { get { return _callStack.Count == 0; } }

        /// <summary>
        /// Creats analysis call stack
        /// </summary>
        /// <param name="services">Available services</param>
        internal AnalysisDispatchStack(AnalysisServices services)
        {
            _services = services;
        }

        /// <summary>
        /// Pushes context at top of call stack
        /// </summary>
        /// <param name="context">Pushed context</param>
        internal void Push(DispatchLevel context)
        {
            _callStack.Push(context);
        }

        /// <summary>
        /// Pop context from top of call stack
        /// </summary>
        internal void Pop()
        {

            _callStack.Pop();
        }
    }
}
