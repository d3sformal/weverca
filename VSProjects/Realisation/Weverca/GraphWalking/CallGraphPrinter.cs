/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan, Marcel Kikta, Pavel Bastecky, David Skorvaga, and Matyas Brenner

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
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.Output.GraphWalking;

namespace Weverca.GraphWalking
{
    /// <summary>
    /// Implementation of call graph printer
    /// </summary>
    class CallGraphPrinter : GraphWalkerBase
    {
        /// <summary>
        /// Create call graph printer
        /// </summary>
        /// <param name="entryGraph">Program point graph, where walking starts</param>
        internal CallGraphPrinter(ProgramPointGraph entryGraph):
            base(entryGraph)
        {
        }

        #region GraphWalkerBase implementation

        /// <summary>
        /// Handle call push - print start program point info and increase indentation
        /// </summary>
        protected override void afterPushCall()
        {
            printDelimiter('#');
            Output.ProgramPointInfo(getStackDescription()+" --->", CallStack.Peek.Start);
            Output.Indent();
        }

        protected override void onWalkPoint(ProgramPointBase point)
        {
            //only print start and end point of call
        }

        /// <summary>
        /// Handle call pop - print end program point info and decrease indentation
        /// </summary>
        protected override void beforePopCall()
        {
            Output.Dedent();
            Output.ProgramPointInfo(getStackDescription()+" <---", CallStack.Peek.End);
            printDelimiter('#');
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Return string representation of stack, that is displayed in call graph
        /// </summary>
        /// <returns>Call stack string representation</returns>
        private string getStackDescription()
        {
            var output = new StringBuilder();
            var stack = CallStack.GetStackCopy();

            output.Append("#GLOBALCODE");

            for(int i=1;i<stack.Length;++i)
            {
                var ppGraph = stack[i];
                var name = getName(ppGraph);


                output.AppendFormat("/{0}",name);
            }

            return output.ToString();
        }

        /// <summary>
        /// Print delimiter line containing delimiterChars
        /// </summary>
        /// <param name="delimiterChar">Char used for delimiter line</param>
        private void printDelimiter(char delimiterChar)
        {
            Output.CommentLine("".PadLeft(50, delimiterChar));
        }

        /// <summary>
        /// Get representative name of given ppGraph
        /// </summary>
        /// <param name="ppGraph">Program point graph</param>
        /// <returns>Representative name of given ppGraph</returns>
        private string getName(ProgramPointGraph ppGraph)
        {
            return NameResolver.Resolve(ppGraph.SourceObject);
        }

        #endregion
    }
}