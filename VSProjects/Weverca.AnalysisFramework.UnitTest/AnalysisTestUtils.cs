/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.AnalysisFramework.UnitTest.InfoLevelPhase;

using Weverca.Analysis;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.Parsers;
using Weverca.Taint;
using Weverca.AnalysisFramework.ProgramPoints;
using Weverca.AnalysisFramework.GraphVisualizer;
using Weverca.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Implemented analyses (enumeration class)
    /// </summary>
    public abstract class Analyses
    {
        /// <summary>
        /// Simple analysis - cannot be used for testing testing (Weverca.AnalysisFramework.UnitTest.SimpleAnalysis)
        /// </summary>
        internal static readonly Analyses SimpleAnalysisTest = new SimpleAnalysisTestCl();
        /// <summary>
        /// Simple analysis - should be used only for testing (Weverca.AnalysisFramework.UnitTest.SimpleAnalysis)
        /// </summary>
        public static readonly Analyses SimpleAnalysis = new SimpleAnalysisCl();
        /// <summary>
        /// <summary>
        /// Main weverca analysis - should be used only for testing (Weverca.Analysis.ForwardAnalysis)
        /// </summary>
        internal static readonly Analyses WevercaAnalysisTest = new WevercaAnalysisTestCl();
        /// <summary>
        /// Main weverca analysis - cannot be used for testing (Weverca.Analysis.ForwardAnalysis)
        /// </summary>
        public static readonly Analyses WevercaAnalysis = new WevercaAnalysisCl();

        /// <summary>
        /// Creates an instance of ForwardAnalysis corresponding to given enumeration item.
        /// </summary>
        /// <returns>an instance of ForwardAnalysis corresponding to given enumeration item</returns>
        internal abstract ForwardAnalysisBase createAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel, EnvironmentInitializer initializer);
        /// <summary>
        /// Creates an instance of ForwardAnalysis corresponding to given enumeration item.
        /// </summary>
        /// <param name="entryMethodGraph">the method where the analysis starts</param>
        /// <param name="memoryModel">memory model used for the analysis</param>
        /// <returns></returns>
        public abstract ForwardAnalysisBase CreateAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel);

        private Analyses() { }

        private class SimpleAnalysisTestCl : Analyses
        {
            internal override ForwardAnalysisBase createAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel, EnvironmentInitializer initializer)
            {
                return new SimpleAnalysis(entryMethodGraph, memoryModel, initializer);
            }
            public override ForwardAnalysisBase CreateAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel)
            {
                throw new NotImplementedException();
            }
        }
        private class WevercaAnalysisTestCl : Analyses
        {
            internal override ForwardAnalysisBase createAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel, EnvironmentInitializer initializer)
            {
                return new WevercaAnalysisTest(entryMethodGraph, memoryModel, initializer);
            }
            public override ForwardAnalysisBase CreateAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel)
            {
                throw new NotImplementedException();
            }
        }
        private class SimpleAnalysisCl : Analyses
        {
            internal override ForwardAnalysisBase createAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel, EnvironmentInitializer initializer)
            {
                return new SimpleAnalysis(entryMethodGraph, memoryModel, initializer);
            }
            public override ForwardAnalysisBase CreateAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel)
            {
                return createAnalysis(entryMethodGraph, memoryModel, EnvironmentInitializer);
            }

            private void EnvironmentInitializer(FlowOutputSet outSet)
            {
                outSet.GetVariable(new VariableIdentifier("_POST"), true).WriteMemory(outSet.Snapshot, new MemoryEntry(outSet.AnyArrayValue));
            }
        }
        private class WevercaAnalysisCl : Analyses
        {
            internal override ForwardAnalysisBase createAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel, EnvironmentInitializer initializer)
            {
                return new Weverca.Analysis.ForwardAnalysis(entryMethodGraph, memoryModel);
            }
            public override ForwardAnalysisBase CreateAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel)
            {
                return createAnalysis(entryMethodGraph, memoryModel, delegate(FlowOutputSet o) { });
            }
        }


    }

    #region Weverca.Analysis test implementation
    internal class WevercaAnalysisTest : Weverca.Analysis.ForwardAnalysis, TestAnalysisSettings
    {
        private readonly WevercaFlowResolverTest _flowResolver;
        private readonly WevercaFunctionResolverTest _functionResolver;

        public WevercaAnalysisTest(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModelFactory memoryModel, EnvironmentInitializer initializer)
            : base(entryMethodGraph, memoryModel)
        {
            _flowResolver = new WevercaFlowResolverTest();
            _functionResolver = new WevercaFunctionResolverTest(initializer);
        }

        public void SetInclude(string fileName, string fileCode)
        {
            _flowResolver.SetInclude(fileName, fileCode);
        }
        public void SetFunctionShare(string functionName)
        {
            _functionResolver.SetFunctionShare(functionName);
        }
        protected override Expressions.FunctionResolverBase createFunctionResolver()
        {
            return _functionResolver;
        }
        protected override FlowResolverBase createFlowResolver()
        {
            //return new Weverca.Analysis.FlowResolver.FlowResolver();
            //return new SimpleFlowResolver();
            return _flowResolver;
        }

        public void SetWideningLimit(int limit)
        {
            WideningLimit = limit;
        }


        public void SetSimplifyLimit(int limit)
        {
            SimplifyLimit = limit;
        }
    }

    internal class WevercaFlowResolverTest : Weverca.Analysis.FlowResolver.FlowResolver
    {
        private readonly Dictionary<string, string> _includes = new Dictionary<string, string>();

        /// <summary>
        /// Set code included for given file name (used for include testing)
        /// </summary>
        /// <param name="fileName">Name of included file</param>
        /// <param name="fileCode">PHP code of included file</param>
        internal void SetInclude(string fileName, string fileCode)
        {
            _includes.Add(fileName, fileCode);
        }

        // Note: Implemetation copied from SimpleFlowResolver
        public override void Include(FlowController flow, MemoryEntry includeFile)
        {
            //extend current program point as Include

            var files = new HashSet<string>();
            foreach (StringValue possibleFile in includeFile.PossibleValues)
            {
                files.Add(possibleFile.Value);
            }

            foreach (var branchKey in flow.ExtensionKeys)
            {
                if (!files.Remove(branchKey as string))
                {
                    //this include is now not resolved as possible include branch
                    flow.RemoveExtension(branchKey);
                }
            }

            foreach (var file in files)
            {
                //Create graph for every include - NOTE: we can share pp graphs
                var cfg = AnalysisTestUtils.CreateCFG(_includes[file], new FileInfo(file));
                var ppGraph = ProgramPointGraph.FromSource(cfg);
                flow.AddExtension(file, ppGraph, ExtensionType.ParallelInclude);
            }
        }
    }

    internal class WevercaFunctionResolverTest : Weverca.Analysis.FunctionResolver
    {
        private readonly EnvironmentInitializer _environmentInitializer;
        private readonly HashSet<string> _sharedFunctionNames = new HashSet<string>();
        private readonly Dictionary<string, ProgramPointGraph> _sharedPpGraphs = new Dictionary<string, ProgramPointGraph>();

        public WevercaFunctionResolverTest(EnvironmentInitializer envinronmentInitializer)
        {
            _environmentInitializer = envinronmentInitializer;
        }

        public override void InitializeCall(ProgramPointBase caller, ProgramPointGraph extensionGraph, MemoryEntry[] arguments)
        {
            _environmentInitializer(OutSet);
            base.InitializeCall(caller, extensionGraph, arguments);
        }

        // Note: implementation copied from SimpleFunctionResolver to make it possible to test also program point graph sharing
        protected override void addCallBranch(FunctionValue function)
        {
            var functionName = function.Name.Value;
            ProgramPointGraph functionGraph;
            if (_sharedFunctionNames.Contains(functionName))
            {
                //set graph sharing for this function
                if (!_sharedPpGraphs.ContainsKey(functionName))
                {
                    //create single graph instance
                    _sharedPpGraphs[functionName] = ProgramPointGraph.From(function);
                }

                //get shared instance of program point graph
                functionGraph = _sharedPpGraphs[functionName];
            }
            else
            {
                functionGraph = ProgramPointGraph.From(function);
            }

            Flow.AddExtension(function.DeclaringElement, functionGraph, ExtensionType.ParallelCall);
        }

        internal void SetFunctionShare(string functionName)
        {
            _sharedFunctionNames.Add(functionName);
        }
    }
    #endregion


    /// <summary>
    /// Enumeration class with special values. This special values can be used for testing whether
    /// corresponding special values have been computed.
    /// </summary>
    internal abstract class SpecialValues 
    {
        internal readonly static SpecialValues ANY_BOOLEAN = new AnyBoolean();

        internal abstract Value getValue(FlowOutputSet outputSet);

        private class AnyBoolean : SpecialValues 
        {
            internal override Value getValue(FlowOutputSet outputSet) 
            {
                return outputSet.AnyBooleanValue;
            }
        }
    }

    internal static class AnalysisTestUtils
    {

        internal static ControlFlowGraph.ControlFlowGraph CreateCFG(string code, FileInfo file)
        {
            var fileName = "./cfg_test.php";
            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            code = "<?php \n" + code + "?>";
            var parser = new SyntaxParser(sourceFile, code);
            parser.Parse();
            var cfg = ControlFlowGraph.ControlFlowGraph.FromSource(parser.Ast, file);

            return cfg;
        }

        internal static void CopyInfo(FlowOutputSet outSet, MemoryEntry source, MemoryEntry target)
        {
            var infos = new HashSet<InfoValue>();

            foreach (var sourceValue in source.PossibleValues)
            {
                var info = outSet.ReadInfo(sourceValue);
                infos.UnionWith(info);
            }

            var infoArray = infos.ToArray();
            foreach (var targetValue in target.PossibleValues)
            {
                outSet.SetInfo(targetValue, infoArray);
            }
        }

        internal static ProgramPointGraph GetAnalyzedGraph(TestCase test, ForwardAnalysisBase analysis)
        {
            test.ApplyTestSettings((TestAnalysisSettings)analysis);

            test.EnvironmentInitializer(analysis.EntryInput);

            analysis.Analyse();

            return analysis.ProgramPointGraph;
        }


        internal static void RunTestCase(TestCase testCase)
        {

            Snapshot.Visualizer.Enabled(testCase.IsSnapshotGraphVisualizationenabled());
            Snapshot.Visualizer.Clear();

            var analyses = CreateAnalyses(testCase);

            foreach (var analysis in analyses)
            {
                var ppg = GetAnalyzedGraph(testCase, analysis);

                testCase.VisualizeProgramPointGraph(ppg, TrunkStructure.GRAPHVIZ_PATH);
                testCase.VisualizeSnapshotGraph(ppg, TrunkStructure.GRAPHVIZ_PATH);

                testCase.Assert(ppg);
            }

            Snapshot.Benchmark.WriteResultsToFile("benchmark.csv");
        }

        internal static void RunInfoLevelBackwardPropagationCase(TestCase testCase)
        {
            var analyses = CreateAnalyses(testCase);

            foreach (var analysis in analyses)
            {
                var ppg = GetAnalyzedGraph(testCase, analysis);

                var nextPhase = new SimpleBackwardAnalysis(ppg);
                nextPhase.Analyse();

                testCase.Assert(ppg);
            }
        }

        internal static void RunInfoLevelTaintAnalysisCase(TestCase testCase, bool prototype = true)
        {
            var analyses = CreateAnalyses(testCase);

            foreach (var analysis in analyses)
            {
                var ppg = GetAnalyzedGraph(testCase, analysis);

                if (prototype)
                {
                    var nextPhase = new SimpleTaintForwardAnalysis(ppg);
                    nextPhase.Analyse();
                }
                else
                {
                    var nextPhase = new TaintForwardAnalysis(ppg);
                    nextPhase.Analyse();
                }

                testCase.Assert(ppg);
            }
        }

        internal static void AssertIterationCount(this FlowOutputSet outSet, int iterationCount, string message)
        {
            Assert.AreEqual(iterationCount, outSet.CommitCount, message);
        }

        internal static void AssertVariable<T>(this FlowOutputSet outset, string variableName, string message, params T[] expectedValues)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            var variable = outset.ReadVariable(new VariableIdentifier(variableName));
            var entry = variable.ReadMemory(outset.Snapshot);

            foreach (var value in entry.PossibleValues)
            {
                if (value is UndefinedValue)
                    Assert.Fail("Undefined value is not allowed for variable ${0} in {1}", variableName, entry);
            }

            var actualValues = new List<T>();
            foreach (Value value in entry.PossibleValues)
            {
                ScalarValue<T> scalar = value as ScalarValue<T>;
                if (scalar != null)
                {
                    actualValues.Add(scalar.Value);
                }
            }

            if (message == null)
                message = string.Format(" in variable ${0} containing {1}", variableName, entry);

            CollectionAssert.AreEquivalent(expectedValues, actualValues.ToArray(), message);
        }

        internal static void AssertVariableWithUndefined<T>(this FlowOutputSet outset, string variableName, string message, params T[] expectedValues)
          where T : IComparable, IComparable<T>, IEquatable<T>
        {
            var variable = outset.ReadVariable(new VariableIdentifier(variableName));
            var entry = variable.ReadMemory(outset.Snapshot);


            var actualValues = new List<T>();
            foreach (var value in entry.PossibleValues)
            {
                //filter undefined values
                if (value is UndefinedValue)
                    continue;

                if (!(value is ScalarValue<T>))
                    Assert.Fail("Cannot convert {0} to {1} for variable ${2} in {3}. {4}", value, typeof(T), variableName, entry, message);

                actualValues.Add((value as ScalarValue<T>).Value);
            }

            if (message == null)
                message = string.Format(" in variable ${0} containing {1}", variableName, entry);

            CollectionAssert.AreEquivalent(expectedValues, actualValues, message);
        }

        internal static void AssertUndefined(this FlowOutputSet outset, string variableName, string message)
        {
            var variable = outset.ReadVariable(new VariableIdentifier(variableName));
            var entry = variable.ReadMemory(outset.Snapshot);

            bool hasUndefValue = false;
            foreach (var value in entry.PossibleValues)
            {
                if (value.GetType() == typeof(UndefinedValue)) hasUndefValue = true;
            }

            if (!hasUndefValue) Assert.Fail("The variable ${0} does not contain undefined value.", variableName);
        }


        /// <summary>
        /// Extension method of class string.
        /// For constructing TestCases from strings.
        /// </summary>
        internal static TestCase AssertVariable(this string test_CODE, string variableName, string assertMessage = null, string nonDeterministic = "unknown")
        {
            var testCase = new TestCase(test_CODE, variableName, assertMessage);
            testCase.SetNonDeterministic(nonDeterministic);
            return testCase;
        }

        /// <summary>
        /// Extension method of class string.
        /// For constructing TestCases from strings.
        /// </summary>
        internal static TestCase AssertIterationCount(this string test_CODE, int iterationCount = 1, string assertMessage = null, string nonDeterministic = "unknown")
        {
            var testCase = new TestCase(test_CODE, iterationCount, assertMessage);
            testCase.SetNonDeterministic(nonDeterministic);
            return testCase;
        }



        internal static T GetSingle<T>(this MemoryEntry entry)
        {
            if (entry.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException("Needs to implement multiple possible values behvaiour");
            }

            return (T)(object)entry.PossibleValues.First();
        }

        internal static void AssertIsXSSDirty(FlowOutputSet outSet, string variableName, string assertMessage)
        {
            var variable = outSet.GetVariable(new VariableIdentifier(variableName));
            var values = variable.ReadMemory(outSet.Snapshot).PossibleValues.ToArray();
            if (!FlagsHandler.IsDirty(values, FlagType.SQLDirty))
                Assert.Fail("No possible value for variable ${0} is dirty", variableName);
        }

        internal static void AssertIsXSSClean(FlowOutputSet outSet, string variableName, string assertMessage)
        {
            var variable = outSet.GetVariable(new VariableIdentifier(variableName));
            var values = variable.ReadMemory(outSet.Snapshot).PossibleValues.ToArray();
            foreach (var value in values)
            {
                var info = value.GetInfo<SimpleInfo>();
                if (info != null && !info.XssSanitized)
                {
                    Assert.IsTrue(info.XssSanitized, "Variable ${0} with value {1} is not sanitized", variableName, value);
                }
            }
        }

        internal static void AssertIsPropagatedTo(FlowOutputSet declarationSet, string variableName, string assertMessage, string[] expectedTargets)
        {
            var expectedCollection = new List<string>(expectedTargets);
            expectedCollection.Add(variableName);

            var expectedTargetsMessage = string.Join(", ", expectedCollection);

            var snapshot = declarationSet.Snapshot;
            var variable = declarationSet.GetVariable(new VariableIdentifier(variableName));
            if (variable == null || !variable.IsDefined(snapshot))
            {
                Assert.Fail("Variable {0} is not defined", variableName);
            }

            var actualValues = variable.ReadMemory(snapshot);
            if (actualValues.Count != 1)
            {
                Assert.Fail("Expected single propagation info");
            }

            var info = actualValues.PossibleValues.First();
            if (info is UndefinedValue)
            {
                Assert.IsTrue(expectedTargets.Count() == 0, "Variable is propagated nowhere instead of {0}", expectedTargetsMessage);
                return;
            }

            var data = (info as InfoValue<PropagationInfo>).Data;
            var actualCollection = new List<string>(data.Targets);

            var actualTargetMessage = string.Join(", ", actualCollection.ToArray());
            CollectionAssert.AreEquivalent(expectedCollection, actualCollection, "Wrong targets for propagation {0}, expected {1}", actualTargetMessage, expectedTargetsMessage);
        }

        internal static void AssertHasTaintStatus(FlowOutputSet outSet, string variableName, string assertMessage, bool taintStatus)
        {
            var variable = outSet.GetVariable(new VariableIdentifier(variableName));
            var values = variable.ReadMemory(outSet.Snapshot).PossibleValues.ToArray();
            var computedTaintStatus = false;
            foreach (var value in values)
            {
                computedTaintStatus = computedTaintStatus || (value as InfoValue<bool>).Data;
            }
            Assert.IsTrue(taintStatus == computedTaintStatus, "Taint status of the variable ${0} should be {1}, taint analysis computed {2}", variableName, taintStatus, computedTaintStatus);

        }

        internal static void AssertHasTaintStatus(FlowOutputSet outSet, string variableName, string assertMessage, TaintStatus taintStatus)
        {
            var varID = new VariableIdentifier(variableName);
            var variable = outSet.GetVariable(varID);
            var values = variable.ReadMemory(outSet.Snapshot).PossibleValues.ToArray();
            var computedTaintStatus = new TaintStatus(false,true);
            if (values.Count() == 0) computedTaintStatus.priority.setAll(false);   
            foreach (var value in values)
            {
                if (!(value is InfoValue<TaintInfo>)) continue;
                TaintInfo valueTaintInfo = (value as InfoValue<TaintInfo>).Data;
                TaintStatus valueTaintStatus = new TaintStatus(valueTaintInfo);
                computedTaintStatus.tainted.copyTaint(true, valueTaintStatus.tainted);
                computedTaintStatus.priority.copyTaint(false, valueTaintStatus.priority);
                computedTaintStatus.lines.AddRange(valueTaintStatus.lines);
            }
            Assert.IsTrue(taintStatus.EqualTo(computedTaintStatus), "Taint status of the variable ${0} should be {1}, taint analysis computed {2}", variableName, taintStatus, computedTaintStatus);
        }

        internal static void AssertHasTaintStatus(FlowOutputSet outSet, string variableName, string assertMessage, TaintStatus taintStatus, Analysis.FlagType flag)
        {
            var varID = new VariableIdentifier(variableName);
            var variable = outSet.GetVariable(varID);
            var values = variable.ReadMemory(outSet.Snapshot).PossibleValues.ToArray();
            var computedTaintStatus = new TaintStatus(false, true);
            if (values.Count() == 0) computedTaintStatus.priority.setAll(false);
            foreach (var value in values)
            {
                if (!(value is InfoValue<TaintInfo>)) continue;
                TaintInfo valueTaintInfo = (value as InfoValue<TaintInfo>).Data;
                TaintStatus valueTaintStatus = new TaintStatus(valueTaintInfo, flag);
                computedTaintStatus.tainted.copyTaint(true, valueTaintStatus.tainted);
                computedTaintStatus.priority.copyTaint(false, valueTaintStatus.priority);
                computedTaintStatus.lines.AddRange(valueTaintStatus.lines);
            }
            String taintStatusString = taintStatus.ToString(flag);
            String computedTaintStatusString = computedTaintStatus.ToString(flag);
            Assert.IsTrue(taintStatus.EqualTo(computedTaintStatus,flag), "Taint status of the taint type {0} of variable ${1} should be {2}, taint analysis computed {3}",flag, variableName, taintStatusString, computedTaintStatusString);
        }

        

        private static ControlFlowGraph.ControlFlowGraph CreateCFG(TestCase testCase)
        {
            return CreateCFG(testCase.PhpCode, new FileInfo("test.php"));
        }

        private static IEnumerable<ForwardAnalysisBase> CreateAnalyses(TestCase testCase)
        {
            var cfg = CreateCFG(testCase);
            return testCase.CreateAnalyses(cfg);
        }
    }

    internal delegate void AssertRunner(ProgramPointGraph ppg);

    internal class TestCase
    {
        internal readonly string PhpCode;
        internal readonly string AssertMessage;
        internal readonly string VariableName;

        internal readonly TestCase PreviousTest;

        private readonly List<AssertRunner> _asserts = new List<AssertRunner>();
        private readonly List<EnvironmentInitializer> _initializers = new List<EnvironmentInitializer>();

        private readonly Dictionary<string, string> _includedFiles = new Dictionary<string, string>();
        private readonly HashSet<string> _nonDeterminiticVariables = new HashSet<string>();
        private readonly HashSet<string> _sharedFunctions = new HashSet<string>();

        private readonly List<MemoryModels.MemoryModelFactory> _memoryModels = new List<MemoryModels.MemoryModelFactory>() { MemoryModels.MemoryModels.ModularCopyMM };
        private readonly List<Analyses> _analyses = new List<Analyses>() { Analyses.WevercaAnalysisTest };

        /// <summary>
        /// Values below zero means that there is no limit
        /// </summary>
        private int _wideningLimit = -1;

        private int _simplifyLimit = -1;

        private Type[] skipProgramPoints;
        private string ppgFileName;
        private string snapshotGraphFileName;

        internal TestCase(string phpCode, string variableName, string assertMessage, TestCase previousTest = null)
        {
            PhpCode = phpCode;
            VariableName = variableName;
            AssertMessage = assertMessage;
            PreviousTest = previousTest;
        }

        internal TestCase(string phpCode, int iterationCount, string assertMessage, TestCase previousTest = null)
        {
            PhpCode = phpCode;
            AssertMessage = assertMessage;
            VariableName = null;
            PreviousTest = previousTest;

            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                output.AssertIterationCount(iterationCount, AssertMessage);
            });
        }

        internal TestCase AssertVariable(string variableName, string assertMessage = null)
        {
            return new TestCase(PhpCode, variableName, assertMessage, this);
        }

        internal TestCase AssertIterationCount(int iterationCount = 1, string assertMessage = null)
        {
            return new TestCase(PhpCode, iterationCount, assertMessage, this);
        }

        internal TestCase SetNonDeterministic(params string[] variables)
        {
            _nonDeterminiticVariables.UnionWith(variables);
            return this;
        }

        internal TestCase Include(string fileName, string fileCode)
        {
            _includedFiles.Add(fileName, fileCode);
            return this;
        }

        /// <summary>
        /// Set function which PPGraph will be shared across all calls
        /// </summary>
        /// <param name="sharedFunctionName">Name of shared function</param>
        /// <returns></returns>
        internal TestCase ShareFunctionGraph(string sharedFunctionName)
        {
            _sharedFunctions.Add(sharedFunctionName);
            return this;
        }

        internal TestCase WideningLimit(int limit)
        {
            _wideningLimit = limit;
            return this;
        }

        internal TestCase SimplifyLimit(int limit)
        {
            _simplifyLimit = limit;
            return this;
        }


        /// <summary>
        /// Set a memory model to be used in the test case.
        /// </summary>
        /// <param name="memoryModel">the memory model to be used in the test case</param>
        /// <returns></returns>
        internal TestCase MemoryModel(MemoryModels.MemoryModelFactory memoryModel)
        {
            _memoryModels.Clear();
            _memoryModels.Add(memoryModel);
            return this;
        }

        /// <summary>
        /// Set an analysis to be used in the test case.
        /// </summary>
        /// <param name="memoryModel">the analysis to be used in the test case</param>
        /// <returns></returns>
        internal TestCase Analysis(Analyses analysis)
        {
            _analyses.Clear();
            _analyses.Add(analysis);
            return this;
        }

        internal TestCase DeclareType(ClassDecl typeDeclaration)
        {
            _initializers.Add((outSet) =>
            {
                var type = outSet.CreateType(typeDeclaration);
                outSet.DeclareGlobal(type);
            });
            return this;
        }

        internal TestCase PrintProgramPointGraph(string ppgFileName, params Type[] skipProgramPoints)
        {
            this.ppgFileName = ppgFileName;
            this.skipProgramPoints = skipProgramPoints;

            return this;
        }

        internal TestCase PrintSnapshotGraph(string snapshotGraphFileName)
        {
            this.snapshotGraphFileName = snapshotGraphFileName;
            return this;
        }

        internal bool IsSnapshotGraphVisualizationenabled()
        {
            return this.snapshotGraphFileName != null;
        }


        #region Assert providers

        internal TestCase HasUndefinedValue()
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                AnalysisTestUtils.AssertUndefined(output, VariableName, AssertMessage);

            });

            return this;
        }

        internal TestCase HasValues<T>(params T[] expectedValues)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                AnalysisTestUtils.AssertVariable<T>(output, VariableName, AssertMessage, expectedValues);
            });
            return this;
        }

        internal TestCase HasUndefinedOrValues<T>(params T[] expectedValues)
           where T : IComparable, IComparable<T>, IEquatable<T>
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                AnalysisTestUtils.AssertVariableWithUndefined<T>(output, VariableName, AssertMessage, expectedValues);
            });
            return this;
        }

        internal TestCase HasUndefinedAndValues<T>(params T[] expectedValues)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return HasUndefinedValue().HasUndefinedOrValues(expectedValues);
        }

        internal TestCase HasUndefinedAndSpecialValues(params SpecialValues[] assertedValues)
        {
            return HasUndefinedValue().HasSpecialValues(assertedValues);
        }

        internal TestCase HasSpecialValues(params SpecialValues[] assertedValues) 
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);

                var variable = output.ReadVariable(new VariableIdentifier(VariableName));
                var entry = variable.ReadMemory(output.Snapshot);

                var computedValues = entry.PossibleValues.Where(a => !(a is UndefinedValue)).ToArray();
                var expectedValues = assertedValues.Select(a => a.getValue(output)).ToArray();

                // Check if asserted and computed values are equivalent
                areAllValuesPresent(computedValues, expectedValues, true);
                areAllValuesPresent(expectedValues, computedValues, false);
            });

            return this;
        }

        private void areAllValuesPresent(Value[] values1, Value[] values2, bool expectedInComputed)
        {
            foreach (var value1 in values1)
            {
                bool isPresent = false;
                foreach (var value2 in values2)
                {
                    if (value1.GetType() == value2.GetType())
                    {
                        isPresent = true;
                        continue;
                    }
                }

                if (!isPresent)
                    if (expectedInComputed)
                        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Variable ${1} has a value {0} that is not asserted. {2}", value1, VariableName, AssertMessage);
                    else
                        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail("Variable ${1} has not a value {0} that is asserted. {2}", value1, VariableName, AssertMessage);
            }
        }

        internal TestCase IsXSSDirty()
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                AnalysisTestUtils.AssertIsXSSDirty(output, VariableName, AssertMessage);
            });
            return this;
        }

        internal TestCase IsXSSClean()
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                AnalysisTestUtils.AssertIsXSSClean(output, VariableName, AssertMessage);
            });
            return this;
        }


        internal TestCase IsPropagatedTo(params string[] track)
        {
            _asserts.Add((ppg) =>
           {
               var declarationSet = GetDeclarationOutput(ppg, VariableName);

               AnalysisTestUtils.AssertIsPropagatedTo(declarationSet, VariableName, AssertMessage, track);
           });
            return this;
        }

        internal TestCase HasTaintStatus(bool taintStatus)
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                AnalysisTestUtils.AssertHasTaintStatus(output, VariableName, AssertMessage, taintStatus);
            });
            return this;
        }

        internal TestCase HasTaintStatus(TaintStatus taintStatus)
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                AnalysisTestUtils.AssertHasTaintStatus(output, VariableName, AssertMessage, taintStatus);
            });
            return this;
        }

        internal TestCase HasTaintStatus(TaintStatus taintStatus, Analysis.FlagType flag)
        {
            _asserts.Add((ppg) =>
            {
                var output = GetEndOutput(ppg);
                AnalysisTestUtils.AssertHasTaintStatus(output, VariableName, AssertMessage, taintStatus, flag);
            });
            return this;
        }



        #endregion

        internal void Assert(ProgramPointGraph ppg)
        {
            foreach (var assert in _asserts)
            {
                assert(ppg);
            }

            if (PreviousTest != null)
            {
                PreviousTest.Assert(ppg);
            }
        }

        internal void VisualizeProgramPointGraph(ProgramPointGraph ppg, string graphvizPath)
        {
            if (ppgFileName != null)
            {
                string visualisationDir = new FileInfo(ppgFileName).Directory.FullName;
                if (!Directory.Exists(visualisationDir))
                {
                    Directory.CreateDirectory(visualisationDir);
                }

                DotGraphVisualizer visualizer = new DotGraphVisualizer(graphvizPath);
                ppg.BuildGraphVisualisation(visualizer, skipProgramPoints);
                visualizer.CreateVisualization(ppgFileName);
            }
        }

        internal void VisualizeSnapshotGraph(ProgramPointGraph ppg, string graphvizPath)
        {
            if (snapshotGraphFileName != null)
            {
                string visualisationDir = new FileInfo(snapshotGraphFileName).Directory.FullName;
                if (!Directory.Exists(visualisationDir))
                {
                    Directory.CreateDirectory(visualisationDir);
                }

                DotGraphVisualizer visualizer = new DotGraphVisualizer(graphvizPath);
                Snapshot.Visualizer.BuildGraphVisualisation(visualizer);
                visualizer.CreateVisualization(snapshotGraphFileName);
            }
        }

        internal void EnvironmentInitializer(FlowOutputSet outSet)
        {
            foreach (var nonDeterministic in _nonDeterminiticVariables)
            {
                outSet.GetVariable(new VariableIdentifier(nonDeterministic)).WriteMemory(outSet.Snapshot, new MemoryEntry(outSet.AnyValue));
            }

            foreach (var initializer in _initializers)
            {
                initializer(outSet);
            }

            if (PreviousTest != null)
            {
                PreviousTest.EnvironmentInitializer(outSet);
            }
        }

        internal void ApplyTestSettings(TestAnalysisSettings analysis)
        {
            foreach (var include in _includedFiles)
            {
                analysis.SetInclude(include.Key, include.Value);
            }

            foreach (var share in _sharedFunctions)
            {
                analysis.SetFunctionShare(share);
            }

            if (_wideningLimit > 0)
                analysis.SetWideningLimit(_wideningLimit);

            if (_simplifyLimit > 0)
                analysis.SetSimplifyLimit(_simplifyLimit);

            if (PreviousTest != null)
            {
                PreviousTest.ApplyTestSettings(analysis);
            }
        }

        /// <summary>
        /// Creates analyses used for the test case.
        /// </summary>
        /// <param name="cfg"></param>
        /// <returns></returns>
        internal List<ForwardAnalysisBase> CreateAnalyses(ControlFlowGraph.ControlFlowGraph cfg)
        {
            var analyses = new List<ForwardAnalysisBase>();

            foreach (var analysis in _analyses)
            {
                foreach (var memoryModel in _memoryModels)
                {
                    analyses.Add(analysis.createAnalysis(cfg, memoryModel, EnvironmentInitializer));
                }
            }

            return analyses;
        }

        internal FlowOutputSet GetEndOutput(ProgramPointGraph ppg)
        {
            return ppg.End.OutSet;
        }

        internal FlowOutputSet GetDeclarationOutput(ProgramPointGraph ppg, string name)
        {
            var visitedPoints = new HashSet<ProgramPointBase>();
            var bfsQueue = new Queue<ProgramPointBase>();
            bfsQueue.Enqueue(ppg.Start);

            while (bfsQueue.Count > 0)
            {
                var point = bfsQueue.Dequeue();
                var variable = point.OutSet.GetVariable(new VariableIdentifier(name));
                if (variable.IsDefined(point.OutSnapshot))
                {
                    //we have found first place where variable is defined
                    return point.OutSet;
                }

                visitedPoints.Add(point);
                foreach (var child in point.FlowChildren)
                {
                    if (visitedPoints.Contains(child))
                        continue;

                    bfsQueue.Enqueue(child);
                }
            }

            return null;
        }
    }
}
