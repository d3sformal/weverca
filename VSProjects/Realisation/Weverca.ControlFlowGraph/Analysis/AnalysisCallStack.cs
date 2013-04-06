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
        internal AnalysisCallContext<FlowInfo> Peek { get; private set; }

        internal bool IsEmpty { get; private set; }

        internal void Push(AnalysisCallContext<FlowInfo> entryContext)
        {
            throw new NotImplementedException();
        }

        internal void Pop()
        {
            throw new NotImplementedException();
        }


        internal void AddDispathes(IEnumerable<CallDispatch> enumerable)
        {
            throw new NotImplementedException();
        }
    }
}
