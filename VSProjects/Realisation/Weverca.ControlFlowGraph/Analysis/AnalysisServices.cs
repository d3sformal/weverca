using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis
{

    delegate void MergeDelegate<FlowInfo>(FlowInputSet<FlowInfo> inSet1, FlowInputSet<FlowInfo> inSet2, FlowOutputSet<FlowInfo> outSet);

    delegate FlowOutputSet<FlowInfo> EmptySetDelegate<FlowInfo>();

    /// <summary>
    /// Handler for analysis services.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    class AnalysisServices<FlowInfo>
    {

        internal readonly MergeDelegate<FlowInfo> Merge;
        internal readonly EmptySetDelegate<FlowInfo> CreateEmptySet;

        public AnalysisServices(MergeDelegate<FlowInfo> merge,EmptySetDelegate<FlowInfo> emptySet){
            Merge = merge;
            CreateEmptySet = emptySet;
        }
    }
}
