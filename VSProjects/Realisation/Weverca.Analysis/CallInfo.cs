using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

using Weverca.ControlFlowGraph;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{

    /// <summary>
    /// Represents dispatch to call
    /// </summary>
    public  class CallInfo
    {
        public readonly FlowInputSet InSet;
        /// <summary>
        /// Graph used for method analyzing - can be shared between multiple calls
        /// </summary>
        public readonly ProgramPointGraph MethodGraph;
 

        public CallInfo(FlowInputSet inSet, ProgramPointGraph methodGraph)
        {            
            InSet = inSet;
            MethodGraph = methodGraph;
        }
    }
}
