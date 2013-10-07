using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;

namespace Weverca.GraphWalking
{
    /// <summary>
    /// Representation of program point call stack, without api changing callStack state
    /// </summary>
    class ReadonlyCallStack
    {
        /// <summary>
        /// Protected storage which can be used for callStack manipulation from derived classes
        /// </summary>
        protected readonly Stack<ProgramPointGraph> callStack = new Stack<ProgramPointGraph>();

        #region Readonly API of program point graph

        /// <summary>
        /// Peek of stored call stack
        /// </summary>
        internal ProgramPointGraph Peek { get { return callStack.Peek(); } }

        /// <summary>
        /// Get copy of call stack in array. Element order is ascending from bottom of stack.
        /// </summary>
        /// <returns>Copy of call stack</returns>
        internal ProgramPointGraph[] GetStackCopy()
        {
            return callStack.Reverse().ToArray();
        }

        #endregion
    }
}
