using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph.Analysis.Memory;

namespace Weverca.ControlFlowGraph.Analysis
{
    public class CallInfo
    {
        public readonly FunctionDecl CalledFunction;
        public readonly MemoryEntry[] Arguments;
    }

    public class CallResult
    {
        /// <summary>
        /// Call was initiated with given info
        /// </summary>
        public readonly CallInfo CallInfo;
        /// <summary>
        /// Return value of call
        /// </summary>
        public readonly MemoryEntry ReturnValue;
    }
}
