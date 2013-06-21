using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

using Weverca.ControlFlowGraph;

namespace Weverca.Analysis
{

    /// <summary>
    /// Represents dispatch to call
    /// </summary>
    public  class CallDispatch
    {   
        public readonly BasicBlock EntryPoint;
        public readonly FlowInputSet InSet;
        
        public CallDispatch(BasicBlock entryPoint,FlowInputSet inSet)
        {
            EntryPoint = entryPoint;
            InSet = inSet;
        }
    }
}
