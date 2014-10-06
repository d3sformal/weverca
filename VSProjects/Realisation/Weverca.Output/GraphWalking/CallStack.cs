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


using Weverca.AnalysisFramework;

namespace Weverca.Output.GraphWalking
{
    /// <summary>
    /// Simple callstack walker implementation
    /// </summary>
    public class CallStack : ReadonlyCallStack
    {
        /// <summary>
        /// Push call onto stack
        /// </summary>
        /// <param name="ppGraph">Pushed call</param>
        internal void Push(ProgramPointGraph ppGraph)
        {
            callStack.Push(ppGraph);
        }

        /// <summary>
        /// Pop top most call from stack
        /// </summary>
        internal void Pop()
        {
            callStack.Pop();
        }
    }
}