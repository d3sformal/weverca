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
