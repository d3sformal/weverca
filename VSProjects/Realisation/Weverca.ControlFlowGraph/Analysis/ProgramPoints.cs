using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis
{
    class ProgramPoint<FlowInfo>
    {
        internal FlowOutputSet<FlowInfo> OutSet { get; set; }
        internal bool HasChanged { get; private set; }
        internal bool HasUpdate { get; private set; }

        internal void UpdateOutSet(FlowOutputSet<FlowInfo> outSet)
        {
            throw new NotImplementedException();
        }

        internal bool CommitUpdate()
        {
            throw new NotImplementedException();
        }
    }

}
