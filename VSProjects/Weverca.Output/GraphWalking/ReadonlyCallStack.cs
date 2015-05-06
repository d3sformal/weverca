/*
Copyright (c) 2012-2014 Miroslav Vodolan

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


using System.Collections.Generic;
using System.Linq;

using Weverca.AnalysisFramework;

namespace Weverca.Output.GraphWalking
{
    /// <summary>
    /// Representation of program point call stack, without api changing callStack state
    /// </summary>
    public class ReadonlyCallStack
    {
        /// <summary>
        /// Protected storage which can be used for callStack manipulation from derived classes
        /// </summary>
        protected readonly Stack<ProgramPointGraph> callStack = new Stack<ProgramPointGraph>();

        #region Readonly API of program point graph

        /// <summary>
        /// Peek of stored call stack
        /// </summary>
        public ProgramPointGraph Peek { get { return callStack.Peek(); } }

        /// <summary>
        /// Get copy of call stack in array. Element order is ascending from bottom of stack.
        /// </summary>
        /// <returns>Copy of call stack</returns>
        public ProgramPointGraph[] GetStackCopy()
        {
            return callStack.Reverse().ToArray();
        }

        #endregion
    }
}