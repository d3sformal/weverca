using System.Collections.Generic;
using PHP.Core;
using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

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
            return new Weverca.MemoryModels.VirtualReferenceModel.Snapshot();
        }

        #endregion

        protected void GlobalsInitializer()
        {
            var post = new VariableName("_POST");
            var postValue = EntryInput.AnyArrayValue;
            EntryInput.FetchFromGlobal(post);
            EntryInput.Assign(post, new MemoryEntry(postValue));
            ValueInfoHandler.setDirty(EntryInput, postValue);

            var get = new VariableName("_GET");
            var getValue = EntryInput.AnyArrayValue;
            EntryInput.FetchFromGlobal(get);
            EntryInput.Assign(get, new MemoryEntry(getValue));
            ValueInfoHandler.setDirty(EntryInput, getValue);

            var contants = new VariableName(".constants");
            var constValue = EntryInput.CreateArray();
            EntryInput.FetchFromGlobal(contants);
            EntryInput.Assign(contants, new MemoryEntry(constValue));

            var warnings = new VariableName(".analysisWarning");
            EntryInput.FetchFromGlobal(warnings);
            EntryInput.Assign(warnings, new MemoryEntry(EntryInput.UndefinedValue));


            NativeObjectAnalyzer nativeObjectAnalyzer = NativeObjectAnalyzer.GetInstance(EntryInput);
            /*
            foreach (var obj in nativeObjectAnalyzer.nativeObjects.Values)
            {
                var standardClass = EntryInput.CreateType(obj);
                EntryInput.DeclareGlobal(standardClass);
            }*/
 
            // TODO: It will be replaced by global types mechanism
              var standardClassDeclaration = new NativeTypeDecl(
                  new QualifiedName(new Name("stdClass")), new NativeMethodInfo[0], new Dictionary<string, Value>(), new Dictionary<string, NativeFieldInfo>());
              var standardClass = EntryInput.CreateType(standardClassDeclaration);
              EntryInput.DeclareGlobal(standardClass);
        }
    }
}
