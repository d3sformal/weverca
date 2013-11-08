using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;
using Weverca.Parsers;

namespace Weverca.AnalysisFramework.UnitTest
{
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
            var cfg = new ControlFlowGraph.ControlFlowGraph(parser.Ast);

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

        internal static FlowOutputSet GetEndPointOutSet(TestCase test)
        {
            var cfg = AnalysisTestUtils.CreateCFG(test.PhpCode);
            var analysis = new SimpleAnalysis(cfg, test.EnvironmentInitializer);

            test.IncludeInitializer(analysis);

            GLOBAL_ENVIRONMENT_INITIALIZER(analysis.EntryInput);
            test.EnvironmentInitializer(analysis.EntryInput);
            analysis.Analyse();

            return analysis.ProgramPointGraph.End.OutSet;
        }

        internal static void RunTestCase(TestCase testCase)
        {
            var output = GetEndPointOutSet(testCase);

            testCase.Assert(output);
        }

        internal static void AssertVariable<T>(this FlowOutputSet outset, string variableName, string message, params T[] expectedValues)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            var variable = outset.ReadVariable(new VariableIdentifier(variableName));
            var entry = variable.ReadMemory(outset.Snapshot);

            var actualValues = (from ScalarValue<T> value in entry.PossibleValues select value.Value).ToArray();

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
                outSet.Assign(new VariableName(nonDeterministic), new MemoryEntry(outSet.AnyValue));
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

        internal void IncludeInitializer(SimpleAnalysis analysis)
        {
            foreach (var include in _includedFiles)
            {
                analysis.SetInclude(include.Key, include.Value);
            }

            foreach (var share in _sharedFunctions)
            {
                analysis.SetFunctionShare(share);
            }

            if (PreviousTest != null)
            {
                PreviousTest.IncludeInitializer(analysis);
            }
        }
    }
}
