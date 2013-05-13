using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    class MemoryContextVersion
    {
        

        internal readonly MemoryContextVersion Parent;
        

        public MemoryContextVersion(MemoryContextVersion parent)
        {
            this.Parent = parent;
        }

    }
}
