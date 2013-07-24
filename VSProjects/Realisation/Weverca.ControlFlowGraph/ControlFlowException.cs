using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph
{
    /// <summary>
    /// Exception throw by controflowgraph visitor, when there is an error in controlflow of program.
    /// </summary>
    class ControlFlowException : Exception
    {
        public ControlFlowExceptionCause Cause { private set; get; }
        public ControlFlowException(ControlFlowExceptionCause cause) { 
            this.Cause=cause;
        }
    }
    /// <summary>
    /// Cause of ControlFlowException.
    /// </summary>
    enum ControlFlowExceptionCause{
        BREAK_NOT_IN_CYCLE, CONTINUE_NOT_IN_CYCLE, MULTIPLE_DEFAULT_USE
    }
}
