using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis
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

        protected override MemoryAssistantBase createAssistant()
        {
            return new MemoryAssistant();
        }

        #endregion

        protected void GlobalsInitializer()
        {
            var post = new VariableName("_POST");
            var postValue = EntryInput.AnyArrayValue;
            EntryInput.FetchFromGlobal(post);
            var postVariable = EntryInput.GetVariable(new VariableIdentifier(post), true);
            postVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(postValue));
            ValueInfoHandler.setDirty(EntryInput, postValue);


            var get = new VariableName("_GET");
            var getValue = EntryInput.AnyArrayValue;
            EntryInput.FetchFromGlobal(get);
            var getVariable = EntryInput.GetVariable(new VariableIdentifier(get), true);
            getVariable.WriteMemory(EntryInput.Snapshot , new MemoryEntry(getValue));
            ValueInfoHandler.setDirty(EntryInput, getValue);

            var contants = new VariableName(".constants");
            var constValue = EntryInput.CreateArray();
            var contantVariable = EntryInput.GetControlVariable(contants);
            contantVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(constValue));

            var warnings = new VariableName(".analysisWarning");
            var warningsVariable=EntryInput.GetControlVariable(warnings);
            warningsVariable.WriteMemory(EntryInput.Snapshot,new MemoryEntry(EntryInput.UndefinedValue));

            
            //TODO insert static variables
            /*var staticObject = new VariableName(".staticObjects");
            var staticObjectVariable = EntryInput.GetControlVariable(staticObject);
            staticObjectVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.CreateArray()));
            
            var nativeObjectAnalyzer = NativeObjectAnalyzer.GetInstance(EntryInput);
            foreach (ClassDecl classDeclaration in nativeObjectAnalyzer.GetAllClasses())
            {
                var field=staticObjectVariable.ReadIndex(EntryInput.Snapshot, new MemberIdentifier(classDeclaration.QualifiedName.Name.LowercaseValue));
                field.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.CreateArray()));

            }*/
        }
    }
}
