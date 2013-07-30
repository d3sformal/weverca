using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis
{
    public class AdvancedForwardAnalysis : ForwardAnalysis
    {
        public AdvancedForwardAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph)
            : base(entryMethodGraph)
        {
        }

        #region ForwardAnalysis override

        protected override ExpressionEvaluator createExpressionEvaluator()
        {
            return new AndvancedExpressionEvaluator();
        }

        protected override FlowResolverBase createFlowResolver()
        {
            return new FlowResolver();
        }

        protected override FunctionResolver createFunctionResolver()
        {
            return new AdvancedFunctionResolver();
        }

        protected override SnapshotBase createSnapshot()
        {
            return new VirtualReferenceModel.Snapshot();
        }

        #endregion
    }
}
