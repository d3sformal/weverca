using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{
    class AnalysisCallContext<FlowInfo>
    {
        public bool IsEmpty { get; private set; }
      
        public FlowInputSet<FlowInfo> CurrentInputSet { get; private set; }

        public AnalysisCallContext(ControlFlowGraph methodGraph)
        {
            throw new NotImplementedException();
        }

        internal void AddDispathes(IEnumerable<BlockDispatch> blockDispatches)
        {
            throw new NotImplementedException();
        }

        internal LangElement DequeueNextStatement()
        {
            throw new NotImplementedException();
        }
    }
}
