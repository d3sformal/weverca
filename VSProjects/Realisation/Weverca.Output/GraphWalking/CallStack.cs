using Weverca.AnalysisFramework;

namespace Weverca.Output.GraphWalking
{
    public class CallStack : ReadonlyCallStack
    {
        internal void Push(ProgramPointGraph ppGraph)
        {
            callStack.Push(ppGraph);
        }

        internal void Pop()
        {
            callStack.Pop();
        }
    }
}
