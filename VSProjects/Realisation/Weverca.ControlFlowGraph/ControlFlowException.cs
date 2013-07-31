using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;


namespace Weverca.ControlFlowGraph
{
    /// <summary>
    /// Exception throw by controflowgraph visitor, when there is an error in controlflow of program.
    /// </summary>
    public class ControlFlowException : Exception
    {
        public ControlFlowExceptionCause Cause { protected set; get; }
        public Position position { protected set; get; }
        public ControlFlowException(ControlFlowExceptionCause cause,Position postion) 
            : base("Control flow creation error: " + cause.ToString()+" at line "+postion.FirstLine+" char "+postion.FirstColumn)
        {
            this.Cause = cause;
        }
    }
    /// <summary>
    /// Cause of ControlFlowException.
    /// </summary>
    public enum ControlFlowExceptionCause
    {
        BREAK_NOT_IN_CYCLE, CONTINUE_NOT_IN_CYCLE,
        DUPLICATED_LABEL,
        MISSING_LABEL
    }
}
