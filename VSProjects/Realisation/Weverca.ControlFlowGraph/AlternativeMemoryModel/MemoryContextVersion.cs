using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.AlternativeMemoryModel
{
    class MemoryContextVersion
    {
        internal readonly int VersionId;

        internal readonly MemoryContextVersion Parent;
        private MemoryContextVersion parent;

        public MemoryContextVersion(MemoryContextVersion parent)
        {
            // TODO: Complete member initialization
            this.parent = parent;
        }

    }
}
