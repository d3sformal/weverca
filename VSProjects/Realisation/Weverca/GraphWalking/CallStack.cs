using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis;

namespace Weverca.GraphWalking
{
    class CallStack:ReadonlyCallStack
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
