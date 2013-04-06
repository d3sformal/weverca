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
        Stack<AnalysisCallContext<FlowInfo>> _callStack = new Stack<AnalysisCallContext<FlowInfo>>();

        internal AnalysisCallContext<FlowInfo> Peek { get { return _callStack.Peek(); } }

        internal bool IsEmpty { get { return _callStack.Count == 0; } }

        internal void Push(AnalysisCallContext<FlowInfo> context)
        {
            _callStack.Push(context);
        }

        internal void Pop()
        {
            _callStack.Pop();
        }


        internal void AddDispathes(IEnumerable<CallDispatch> dispatches)
        {
            if (dispatches!= null && dispatches.Count() > 0)
            {
                throw new NotImplementedException();
            }
        }
    }
}
