using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.Parsers;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Analyses used for a test case (enumeration class)
    /// </summary>
    internal abstract class Analyses
    {
        /// <summary>
        /// Use simple analysis (Weverca.AnalysisFramework.UnitTest.SimpleAnalysis)
        /// </summary>
        internal static readonly Analyses SimpleAnalysis = new SimpleAnalysisCl();
        /// <summary>
        /// Use main weverca analysis (Weverca.Analysis.ForwardAnalysis)
        /// </summary>
        internal static readonly Analyses WevercaAnalysis = new WevercaAnalysisCl();

        /// <summary>
        /// Creates an instance of ForwardAnalysis corresponding to given enumeration item.
        /// </summary>
        /// <returns>an instance of ForwardAnalysis corresponding to given enumeration item</returns>
        public abstract ForwardAnalysisBase createAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModels memoryModel, EnvironmentInitializer initializer);

        private Analyses() { }

        private class SimpleAnalysisCl : Analyses
        {
            public override ForwardAnalysisBase createAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModels memoryModel, EnvironmentInitializer initializer)
            {
                return new SimpleAnalysis(entryMethodGraph, memoryModel, initializer);
            }
        }
        private class WevercaAnalysisCl : Analyses
        {
            public override ForwardAnalysisBase createAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModels memoryModel, EnvironmentInitializer initializer)
            {
                return new WevercaAnalysisTest(entryMethodGraph, memoryModel, initializer);
            }
        }


    }

    #region Weverca.Analysis test implementation
    internal class WevercaAnalysisTest : Weverca.Analysis.ForwardAnalysis, TestAnalysisSettings
    {
        private readonly WevercaFlowResolverTest _flowResolver;
        private readonly WevercaFunctionResolverTest _functionResolver;

        public WevercaAnalysisTest(ControlFlowGraph.ControlFlowGraph entryMethodGraph, MemoryModels.MemoryModels memoryModel, EnvironmentInitializer initializer)
            : base(entryMethodGraph, memoryModel, null)
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
                var cfg = AnalysisTestUtils.CreateCFG(_includes[file]);
                var ppGraph = ProgramPointGraph.FromSource(cfg, null);
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

        public override void InitializeCall(ProgramPointGraph extensionGraph, MemoryEntry[] arguments)
        {
            _environmentInitializer(OutSet);
            base.InitializeCall(extensionGraph, arguments);
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

    internal static class AnalysisTestUtils
    {
        /// <summary>
        /// Initializer which sets environment for tests before analyzing
        /// </summary>
        /// <param name="outSet"></param>
        private static void GLOBAL_ENVIRONMENT_INITIALIZER(FlowOutputSet outSet)
        {
            var POSTVar = outSet.GetVariable(new VariableIdentifier("_POST"), true);
            var POST = outSet.AnyArrayValue.SetInfo(new SimpleInfo(xssSanitized: false));

            POSTVar.WriteMemory(outSet.Snapshot, new MemoryEntry(POST));
        }

        internal static ControlFlowGraph.ControlFlowGraph CreateCFG(string code)
        {
            var fileName = "./cfg_test.php";
            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            code = "<?php \n" + code + "?>";
            var parser = new SyntaxParser(sourceFile, code);
            parser.Parse();
            var cfg = ControlFlowGraph.ControlFlowGraph.FromSource(parser.Ast);

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

        /// <summary>
        /// Test that possible values of given info contains all given values.
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="infos"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static bool TestValues(string varName, IEnumerable<ValueInfo> infos, params string[] values)
        {
            var info = getVarInfo(varName, infos);

            if (info.PossibleValues.Count != values.Length)
            {
                return false;
            }

            foreach (var val in values)
            {
                if (!info.PossibleValues.Contains(val))
                {
                    return false;
                }
            }

            return true;
        }

        internal static ValueInfo getVarInfo(string varName, IEnumerable<ValueInfo> infos)
        {
            foreach (var info in infos)
            {
                if (info.Name.Value == varName)
                {
                    return info;
                }
            }

            Debug.Fail("Variable of name: '" + varName + "' hasn't been found");
            return null;
        }

        internal static ValueInfo[] GetEndPointInfo(string code)
        {
            var cfg = AnalysisTestUtils.CreateCFG(code);
            var analysis = new StringAnalysis(cfg);
            analysis.Analyse();
            var list = new List<ValueInfo>(analysis.ProgramPointGraph.End.OutSet.CollectedInfo);
            return list.ToArray();
        }

        internal static FlowOutputSet GetEndPointOutSet(TestCase test, ForwardAnalysisBase analysis)
        {
            test.ApplyTestSettings((TestAnalysisSettings)analysis);

            GLOBAL_ENVIRONMENT_INITIALIZER(analysis.EntryInput);
            test.EnvironmentInitializer(analysis.EntryInput);
            analysis.Analyse();

            return analysis.ProgramPointGraph.End.OutSet;
        }

        internal static void RunTestCase(TestCase testCase)
        {
            var cfg = AnalysisTestUtils.CreateCFG(testCase.PhpCode);
            var analyses = testCase.CreateAnalyses(cfg);

            foreach (var analysis in analyses)
            {
                var output = GetEndPointOutSet(testCase, analysis);

                testCase.Assert(output);
            }
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

            var actualValues = (from ScalarValue<T> value in entry.PossibleValues select value.Value).ToArray();

            if (message == null)
                message = string.Format(" in variable ${0} containing {1}", variableName, entry);

            CollectionAssert.AreEquivalent(expectedValues, actualValues, message);
        }

        internal static void AssertVariableWithUndefined<T>(this FlowOutputSet outset, string variableName, string message, params T[] expectedValues)
          where T : IComparable, IComparable<T>, IEquatable<T>
        {
            var variable = outset.ReadVariable(new VariableIdentifier(variableName));
            var entry = variable.ReadMemory(outset.Snapshot);


            var actualValues = new List<T>();
            foreach (var value in entry.PossibleValues) {
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

        internal static TestCase AssertVariable(this string test_CODE, string variableName, string assertMessage = null, string nonDeterministic = "unknown")
        {
            var testCase = new TestCase(test_CODE, variableName, assertMessage);
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
            foreach (var value in values)
            {
                var info = value.GetInfo<SimpleInfo>();
                if (info != null && !info.XssSanitized)
                {
                    return;
                }
            }

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
    }

    internal delegate void AssertRunner(FlowOutputSet output);

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

        private readonly List<MemoryModels.MemoryModels> _memoryModels = new List<MemoryModels.MemoryModels>() { MemoryModels.MemoryModels.VirtualReferenceMM};
        private readonly List<Analyses> _analyses = new List<Analyses>() { Analyses.SimpleAnalysis, Analyses.WevercaAnalysis};

        /// <summary>
        /// Values below zero means that there is no limit
        /// </summary>
        private int _wideningLimit = -1;

        internal TestCase(string phpCode, string variableName, string assertMessage, TestCase previousTest = null)
        {
            PhpCode = phpCode;
            VariableName = variableName;
            AssertMessage = assertMessage;
            PreviousTest = previousTest;
        }

        internal TestCase AssertVariable(string variableName, string assertMessage = null)
        {
            return new TestCase(PhpCode, variableName, assertMessage, this);
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


        /// <summary>
        /// Set a memory model to be used in the test case.
        /// </summary>
        /// <param name="memoryModel">the memory model to be used in the test case</param>
        /// <returns></returns>
        internal TestCase MemoryModel(MemoryModels.MemoryModels memoryModel)
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

        #region Assert providers

        internal TestCase HasUndefinedValue()
        {
            _asserts.Add((output) =>
            {
                AnalysisTestUtils.AssertUndefined(output, VariableName, AssertMessage);

            });

            return this;
        }

        internal TestCase HasValues<T>(params T[] expectedValues)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            _asserts.Add((output) =>
            {
                AnalysisTestUtils.AssertVariable<T>(output, VariableName, AssertMessage, expectedValues);
            });
            return this;
        }

        internal TestCase HasUndefinedOrValues<T>(params T[] expectedValues)
           where T : IComparable, IComparable<T>, IEquatable<T>
        {
            _asserts.Add((output) =>
            {
                AnalysisTestUtils.AssertVariableWithUndefined<T>(output, VariableName, AssertMessage, expectedValues);
            });
            return this;
        }

        internal TestCase HasUndefinedAndValues<T>(params T[] expectedValues)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return HasUndefinedValue().HasUndefinedOrValues(expectedValues);
        }

        internal TestCase IsXSSDirty()
        {
            _asserts.Add((output) =>
            {
                AnalysisTestUtils.AssertIsXSSDirty(output, VariableName, AssertMessage);
            });
            return this;
        }

        internal TestCase IsXSSClean()
        {
            _asserts.Add((output) =>
            {
                AnalysisTestUtils.AssertIsXSSClean(output, VariableName, AssertMessage);
            });
            return this;
        }

        #endregion

        internal void Assert(FlowOutputSet outSet)
        {
            foreach (var assert in _asserts)
            {
                assert(outSet);
            }

            if (PreviousTest != null)
            {
                PreviousTest.Assert(outSet);
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
    }
}
