using System.Collections.Generic;
using System.IO;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;
using Weverca.Analysis.FlowResolver;

namespace Weverca.Analysis
{

    /// <summary>
    /// Provides functionnality to analyze php source codes
    /// </summary>
    public class ForwardAnalysis : ForwardAnalysisBase
    {

        /// <summary>
        /// Creates new Instance of ForwardAnalysis
        /// </summary>
        /// <param name="entryMethodGraph">ControlFlowGraph to analyze</param>
        /// <param name="memoryModel">Memory model used by analyser</param>
        /// <param name="entryScript">File to analyze</param>
        public ForwardAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModels memoryModel, FileInfo entryScript)
            : base(entryMethodGraph, memoryModel.CreateSnapshot, entryScript)
        {
            GlobalsInitializer();
        }

        #region ForwardAnalysis override

        /// <inheritdoc />
        protected override ExpressionEvaluatorBase createExpressionEvaluator()
        {
            return new ExpressionEvaluator.ExpressionEvaluator();
        }

        /// <inheritdoc />
        protected override FlowResolverBase createFlowResolver()
        {
            return new FlowResolver.FlowResolver();
        }

        /// <inheritdoc />
        protected override FunctionResolverBase createFunctionResolver()
        {
            var functionResolver = new FunctionResolver();
            functionResolver.globalCode = EntryCFG.globalCode;
            return functionResolver;
        }

        /// <inheritdoc />
        protected override MemoryAssistantBase createAssistant()
        {
            return new MemoryAssistant();
        }

        #endregion

        /// <summary>
        /// Initialize php variables and control variables user by static analyser
        /// </summary>
        protected void GlobalsInitializer()
        {
            var post = new VariableName("_POST");
            var postValue = EntryInput.AnyArrayValue.SetInfo(new Flags(Flags.CreateDirtyFlags()));
            EntryInput.FetchFromGlobal(post);
            var postVariable = EntryInput.GetVariable(new VariableIdentifier(post), true);
            postVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(postValue));


            var get = new VariableName("_GET");
            var getValue = EntryInput.AnyArrayValue.SetInfo(new Flags(Flags.CreateDirtyFlags()));
            EntryInput.FetchFromGlobal(get);
            var getVariable = EntryInput.GetVariable(new VariableIdentifier(get), true);
            getVariable.WriteMemory(EntryInput.Snapshot , new MemoryEntry(getValue));

            var staticVariables = new VariableName(".staticVariables");
            var staticVariablesArray = EntryInput.CreateArray();
            var staticVariable = EntryInput.GetControlVariable(staticVariables);
            staticVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(staticVariablesArray));

            var warnings = new VariableName(".analysisWarning");
            var warningsVariable=EntryInput.GetControlVariable(warnings);
            warningsVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.UndefinedValue));

            var securityWarnings = new VariableName(".analysisSecurityWarning");
            var securityWarningsVariable = EntryInput.GetControlVariable(warnings);
            securityWarningsVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.UndefinedValue));


            //stack for catchblocks
            EntryInput.GetControlVariable(new VariableName(".catchBlocks")).WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.CreateInfo(new TryBlockStack())));

            //array for global Constants
            var constantsVariable = EntryInput.GetControlVariable(UserDefinedConstantHandler.constantVariable);
            constantsVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.CreateArray()));

            this.WideningLimit = 10;
        }
    }
}
