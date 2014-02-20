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
