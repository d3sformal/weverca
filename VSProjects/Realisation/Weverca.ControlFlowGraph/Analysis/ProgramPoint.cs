using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis
{
    public class ProgramPoint<FlowInfo>
    {
        public FlowInputSet<FlowInfo> InSet { get; internal set; }
        public FlowOutputSet<FlowInfo> OutSet { get; private set; }

        internal bool HasChanged { get; private set; }
        internal bool HasUpdate { get; private set; }

        private FlowOutputSet<FlowInfo> _outSetUpdate;

        internal void UpdateOutSet(FlowOutputSet<FlowInfo> outSet)
        {
            HasUpdate = true;
            _outSetUpdate = outSet;
        }

        internal bool CommitUpdate()
        {            
            if (!HasUpdate || _outSetUpdate == OutSet)
            {
                HasUpdate = false;
                return false;
            }

            HasUpdate = false;
            OutSet = _outSetUpdate;
            return true;
        }
    }

}
