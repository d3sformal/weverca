
using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;
using Weverca.Analysis.FlowResolver;
using Weverca.Analysis.NativeAnalyzers;

namespace Weverca.Analysis
{

    /// <summary>
    /// Provides functionnality to analyze php source codes
    /// </summary>
    public class ForwardAnalysis : ForwardAnalysisBase
    {
        /// <summary>
        /// Reference to singleton class native object analyzer
        /// </summary>
        static public NativeObjectAnalyzer nativeObjectAnalyzer;

        /// <summary>
        /// Creates new Instance of ForwardAnalysis
        /// </summary>
        /// <param name="entryMethodGraph">ControlFlowGraph to analyze</param>
        /// <param name="memoryModel">Memory model used by analyser</param>
        /// <param name="simplifyLimit">Limit for calling simplificaion in memory model. It is different for tests</param>
		public ForwardAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModels memoryModel, int simplifyLimit=5)
            : base(entryMethodGraph, memoryModel.CreateSnapshot)
        {
            GlobalsInitializer();
            this.SimplifyLimit = simplifyLimit;
			this.WideningLimit = 5;
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
            return functionResolver;
        }

        /// <inheritdoc />
        protected override MemoryAssistantBase createAssistant()
        {
            return new MemoryAssistant();
        }

        #endregion

        private void initVariable(string name,Value value)
        {
            var variable = new VariableName(name);
            EntryInput.FetchFromGlobal(variable);
            EntryInput.GetVariable(new VariableIdentifier(variable), true).WriteMemory(EntryInput.Snapshot , new MemoryEntry(value));        
        }

        private void initTaintedVariable(string name)
        {
            var varName = new VariableName(name);
            var value = EntryInput.AnyArrayValue.SetInfo(new Flags(Flags.CreateDirtyFlags()));
            EntryInput.FetchFromGlobal(varName);
            var variable = EntryInput.GetVariable(new VariableIdentifier(varName), true);
            variable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(value));
        }


        /// <summary>
        /// Initialize php variables and control variables user by static analyser
        /// </summary>
        protected void GlobalsInitializer()
        {
            initTaintedVariable("_POST");
            initTaintedVariable("_GET");
            initTaintedVariable("_SERVER");
            initTaintedVariable("_COOKIE");
            initTaintedVariable("_SESSION");
            initTaintedVariable("_FILES");
            initTaintedVariable("_REQUEST");
            initTaintedVariable("GLOBALS");

            initVariable("_ENV",EntryInput.AnyArrayValue);
            initVariable("php_errormsg",EntryInput.AnyStringValue);
            initVariable("argc",EntryInput.AnyIntegerValue);
            initVariable("argv",EntryInput.AnyArrayValue);

            var staticVariablesArray = EntryInput.CreateArray();
            var staticVariable = EntryInput.GetControlVariable(FunctionResolver.staticVariables);
            staticVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(staticVariablesArray));

            var warnings = new VariableName(".analysisWarning");
            var warningsVariable=EntryInput.GetControlVariable(warnings);
            warningsVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.UndefinedValue));

            var securityWarnings = new VariableName(".analysisSecurityWarning");
            var securityWarningsVariable = EntryInput.GetControlVariable(securityWarnings);
            securityWarningsVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.UndefinedValue));

            //stack for catchblocks
            EntryInput.GetControlVariable(new VariableName(".catchBlocks")).WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.CreateInfo(new TryBlockStack())));

            //array for global Constants
            var constantsVariable = EntryInput.GetControlVariable(UserDefinedConstantHandler.constantVariable);
            constantsVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.CreateArray()));

            var staticVariableSink = FunctionResolver.staticVariableSink;
            var staticVariableSinkVariable = EntryInput.GetControlVariable(staticVariableSink);
            staticVariableSinkVariable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.UndefinedValue));

            EntryInput.GetControlVariable(FunctionResolver.evalDepth).WriteMemory(EntryInput.Snapshot, new MemoryEntry(EntryInput.CreateInt(0)));

            nativeObjectAnalyzer = NativeObjectAnalyzer.GetInstance(EntryInput);
        }
    }
}
