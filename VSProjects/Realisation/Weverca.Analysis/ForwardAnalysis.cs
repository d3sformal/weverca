/*
Copyright (c) 2012-2014 Marcel Kikta and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/

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
        public ForwardAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModels memoryModel, int simplifyLimit=3)
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

        private void initTaintedArray(string name)
        {
            // Get the variable
            var varName = new VariableName(name);
            EntryInput.FetchFromGlobal(varName);
            var variable = EntryInput.GetVariable(new VariableIdentifier(varName), true);

            // Create array and write it into the variable
            var array = EntryInput.CreateArray();
            array.SetInfo(new Flags(Flags.CreateDirtyFlags()));
            variable.WriteMemory(EntryInput.Snapshot, new MemoryEntry(array));

            // Create tainted any value
            var anyValue = EntryInput.AnyValue.SetInfo(new Flags(Flags.CreateDirtyFlags()));

            // Write tainted anyvalue to unknown field of the array (now stored in the variable)
            var entry = variable.ReadIndex(EntryInput.Snapshot, MemberIdentifier.getAnyMemberIdentifier());
            entry.WriteMemory(EntryInput.Snapshot, new MemoryEntry(anyValue));
        }


        /// <summary>
        /// Initialize php variables and control variables user by static analyser
        /// </summary>
        protected void GlobalsInitializer()
        {
            initTaintedArray("_POST");
            initTaintedArray("_GET");
            initTaintedArray("_SERVER");
            initTaintedArray("_COOKIE");
            initTaintedArray("_SESSION");
            initTaintedArray("_FILES");
            initTaintedArray("_REQUEST");
            initTaintedArray("GLOBALS");

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
