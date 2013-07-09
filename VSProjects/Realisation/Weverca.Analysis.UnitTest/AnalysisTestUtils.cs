using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core; 

using Weverca.Parsers;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.UnitTest
{
    static class AnalysisTestUtils
    {
        static internal ControlFlowGraph.ControlFlowGraph CreateCFG(string code)
        {
            var fileName="./cfg_test.php";
            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            code = "<?php \n" + code + "?>";
            var parser=new SyntaxParser(sourceFile,code);
            parser.Parse();
            var cfg = new ControlFlowGraph.ControlFlowGraph(parser.Ast);

            return cfg;
        }

        /// <summary>
        /// Test that possible values of given info contains all given values.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        static internal bool TestValues(string varName,IEnumerable<ValueInfo> infos, params string[] values)
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

        static internal ValueInfo getVarInfo(string varName, IEnumerable<ValueInfo> infos)
        {
            foreach (var info in infos)
            {
                if (info.Name.Value == varName)
                {
                    return info;
                }
            }
            Debug.Assert(false, "Variable of name: '" + varName + "' hasn't been found");
            return null;
        }

        static internal ValueInfo[] GetEndPointInfo(string code)
        {
            var cfg = AnalysisTestUtils.CreateCFG(code);
            var analysis = new StringAnalysis(cfg);
            analysis.Analyse();
            var list=new List<ValueInfo>(analysis.ProgramPointGraph.End.OutSet.CollectedInfo);
            return list.ToArray();
        }

        static internal FlowOutputSet GetEndPointOutSet(string code)
        {
            var cfg = AnalysisTestUtils.CreateCFG(code);
            var analysis = new SimpleAnalysis(cfg);
            analysis.Analyse();

            return analysis.ProgramPointGraph.End.OutSet;
        }

        static internal void RunTestCase(TestCase testCase)
        {
            var output = GetEndPointOutSet(testCase.PhpCode);

            while (testCase != null)
            {
                testCase.Assert(output);
                testCase = testCase.PreviousTest;
            }
        }

        static internal void AssertVariable<T>(this FlowOutputSet outset, string variableName,string message,params T[] expectedValues)            
        {
            var entry=outset.ReadValue(new VariableName(variableName));

            var actualValues = (from PrimitiveValue<T> value in entry.PossibleValues select value.Value ).ToArray();

            CollectionAssert.AreEquivalent(expectedValues, actualValues,message);
            
        }

        static internal TestCase AssertVariable(this string test_CODE, string variableName, string assertMessage = null)
        {
            return new TestCase(test_CODE, variableName, assertMessage);
        }

        static internal T GetSingle<T>(this MemoryEntry entry)            
        {
            if (entry.PossibleValues.Count() != 1)
            {
                throw new NotImplementedException("Needs to implement multiple possible values behvaiour");
            }

            return (T)(object)entry.PossibleValues.First();
        }
    }

    delegate void AssertRunner(FlowOutputSet output);

    class TestCase
    {
        internal readonly string PhpCode;
        internal readonly string AssertMessage;
        internal readonly string VariableName;
        
        internal readonly TestCase PreviousTest;

        internal AssertRunner Assert { get; private set; }

        internal TestCase(string phpCode, string variableName, string assertMessage,TestCase previousTest=null)
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

        internal TestCase HasValues<T>(params T[] expectedValues)
        {
            Assert = (output) => AnalysisTestUtils.AssertVariable<T>(output, VariableName, AssertMessage, expectedValues);
            return this;
        }
    }
}
