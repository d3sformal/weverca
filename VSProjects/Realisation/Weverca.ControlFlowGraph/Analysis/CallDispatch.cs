using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{

    /// <summary>
    /// Represents dispatch to call
    /// </summary>
    public  class CallDispatch<FlowInfo>
    {   
        public readonly BasicBlock EntryPoint;
        public readonly FlowInputSet<FlowInfo> InSet;



        public CallDispatch(BasicBlock entryPoint,FlowInputSet<FlowInfo> inSet)
        {
            EntryPoint = entryPoint;
            InSet = inSet;
        }
    }
}
