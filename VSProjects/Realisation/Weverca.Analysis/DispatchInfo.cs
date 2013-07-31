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
    /// Represents info for graph dispatch
    /// </summary>
    public class DispatchInfo
    {
        /// <summary>
        /// Call input set
        /// </summary>
        public readonly FlowInputSet[] InSet;
        /// <summary>
        /// Graph used for method analyzing - can be shared between multiple calls
        /// </summary>
        public readonly ProgramPointGraph MethodGraph;


        public DispatchInfo(ProgramPointGraph methodGraph, params FlowInputSet[] inSet)
        {            
            InSet = inSet;
            MethodGraph = methodGraph;
        }
    }
}
