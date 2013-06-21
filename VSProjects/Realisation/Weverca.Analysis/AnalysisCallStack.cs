using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.Analysis
{
    /// <summary>
    /// Handle call stack for forward flow analysis.    
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    class AnalysisCallStack
    {        
        /// <summary>
        /// Current context of call from current dispatch level 
        /// </summary>
        internal AnalysisCallContext CurrentContext { get { return CurrentLevel.CurrentContext; } }

        /// <summary>
        /// Current dispatch level.
        /// </summary>
        internal CallDispatchLevel CurrentLevel { get { return _callStack.Peek(); } }

        /// <summary>
        /// Determine that call stack is empty
        /// </summary>
        internal bool IsEmpty { get { return _callStack.Count == 0; } }

        /// <summary>
        /// Stack of call contexts
        /// </summary>        
        Stack<CallDispatchLevel> _callStack = new Stack<CallDispatchLevel>();

        /// <summary>
        /// Pushes context at top of call stack
        /// </summary>
        /// <param name="context"></param>
        internal void Push(CallDispatchLevel context)
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
