using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Handle call stack for forward flow analysis.    
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    class AnalysisCallStack<FlowInfo>
    {        
        /// <summary>
        /// Peek context of call stack
        /// </summary>
        internal AnalysisCallContext<FlowInfo> Peek { get { return _callStack.Peek(); } }

        /// <summary>
        /// Determine that call stack is empty
        /// </summary>
        internal bool IsEmpty { get { return _callStack.Count == 0; } }

        /// <summary>
        /// Stack of call contexts
        /// </summary>
        Stack<AnalysisCallContext<FlowInfo>> _callStack = new Stack<AnalysisCallContext<FlowInfo>>();

        /// <summary>
        /// Pushes context at top of call stack
        /// </summary>
        /// <param name="context"></param>
        internal void Push(AnalysisCallContext<FlowInfo> context)
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

        /// <summary>
        /// Add dispatches at one dispatch level  (result of all calls will be merged)
        /// </summary>
        /// <param name="dispatches"></param>
        internal void AddDispatchLevel(IEnumerable<CallDispatch> dispatches)
        {
            if (dispatches!= null && dispatches.Count() > 0)
            {
                throw new NotImplementedException();
            }
        }
    }
}
