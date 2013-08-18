using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;
using PHP.Core;
namespace Weverca.TaintedAnalysis
{
    public class ForwardAnalysis : ForwardAnalysisBase
    {
        public ForwardAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph)
            : base(entryMethodGraph)
        {
            GlobalsInitializer();
        }

        #region ForwardAnalysis override

        protected override ExpressionEvaluatorBase createExpressionEvaluator()
        {
            return new ExpressionEvaluator.ExpressionEvaluator();
        }

        protected override FlowResolverBase createFlowResolver()
        {
            return new FlowResolver.FlowResolver();
        }

        protected override FunctionResolverBase createFunctionResolver()
        {
            return new FunctionResolver();
        }

        protected override SnapshotBase createSnapshot()
        {
            return new VirtualReferenceModel.Snapshot();
        }

        #endregion

        protected void GlobalsInitializer()
        { 
            var post = new VariableName("_POST");
            var postValue = EntryInput.AnyArrayValue;
            EntryInput.Assign(post, new MemoryEntry(postValue));
            VariableInfoHandler.setDirty(EntryInput, postValue);

            var get = new VariableName("_GET");
            var getValue = EntryInput.AnyArrayValue;
            EntryInput.Assign(post, new MemoryEntry(getValue));
            VariableInfoHandler.setDirty(EntryInput, getValue);

            var contants = new VariableName(".constants");
            var constValue = EntryInput.CreateArray();
            EntryInput.Assign(contants, new MemoryEntry(constValue));


        }
    }
}
