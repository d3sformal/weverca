using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.Analysis;
using Weverca.Analysis.Memory;
using Weverca.Parsers;

namespace Weverca.TaintedAnalysis.UnitTest
{
    class TestUtils
    {
        public static FlowOutputSet Analyze(string code)
        {
            var fileName = "./cfg_test.php";
            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            code = "<?php \n" + code + "?>";

            var parser = new SyntaxParser(sourceFile, code);
            parser.Parse();
            var cfg = new Weverca.ControlFlowGraph.ControlFlowGraph(parser.Ast);

            var analysis = new ForwardAnalysis(cfg);
            analysis.Analyse();

            return analysis.ProgramPointGraph.End.OutSet;
        }

        public static bool ContainsWarning(FlowOutputSet outset, AnalysisWarningCause cause)
        {
            IEnumerable<Value> warnings = AnalysisWarningHandler.ReadWarnings(outset);
            foreach (var value in warnings)
            {
                InfoValue<AnalysisWarning> infoValue = (InfoValue<AnalysisWarning>)value;
                if (infoValue.Data.Cause == cause)
                    return true;
            }
            return false;
        }

        public static bool ArgumentWarningTest(string code, AnalysisWarningCause cause)
        {
            return ContainsWarning(Analyze(code), cause);
        }

        public static Value ResultTest(string code)
        {
            return Analyze(code).ReadValue(new VariableName("result")).PossibleValues.ElementAt(0);
        }

        public static void testType<T>(Value value, T type)
        {
            Assert.AreEqual(value.GetType(), type);
        }

        public static void testValue<T>(Value value, T compareValue)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            PrimitiveValue<T> val = (PrimitiveValue<T>)value;
            Assert.IsTrue(val.Value.Equals(compareValue));
        }

        public static void testObjectType(Value value, string type)
        {
            Assert.AreEqual(value.GetType(), type);
        }
    }
}
