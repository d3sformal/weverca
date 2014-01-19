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
        /// <summary>
        /// Enum sotring information about the cause of controflow exception
        /// </summary>
        public ControlFlowExceptionCause Cause { protected set; get; }
        
        /// <summary>
        /// Position in code where error occured
        /// </summary>
        public Position position { protected set; get; }
        
        /// <summary>
        /// Creates new istance of ControlFlowException
        /// </summary>
        /// <param name="cause">cause of exception</param>
        /// <param name="postion">position in the code</param>
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
        /// <summary>
        /// Break is outside of the cycle.
        /// </summary>
        BREAK_NOT_IN_CYCLE,

        /// <summary>
        /// continue is outside of cycle.
        /// </summary>
        CONTINUE_NOT_IN_CYCLE,

        /// <summary>
        /// Label is defined more than once.
        /// </summary>
        DUPLICATED_LABEL,

        /// <summary>
        /// Target of goto is not declared.
        /// </summary>
        MISSING_LABEL
    }
}
