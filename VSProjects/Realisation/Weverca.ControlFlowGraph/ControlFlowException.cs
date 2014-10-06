/*
Copyright (c) 2012-2014 Marcel Kikta and David Hauzar.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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
        public ControlFlowException(ControlFlowExceptionCause cause, Position postion)
            : base("Control flow creation error: " + cause.ToString() + " at line " + postion.FirstLine + " char " + postion.FirstColumn)
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