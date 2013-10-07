using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.Parsers;

namespace Weverca.Analysis.UnitTest
{

    /// <summary>
    /// Provides functionality for testing souce code analysis
    /// </summary>
    class TestUtils
    {
        /// <summary>
        /// Analyzes the source code and return the resulting FlowOutputSet
        /// </summary>
        /// <param name="code">the source code without &tl?php </param>
        /// <returns>FlowOutputSet from last program point of analysis</returns>
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

        /// <summary>
        /// Determines when the FlowOutputSet contains analysis warning, which has the same cause as the second parameter
        /// </summary>
        /// <param name="outset">Output set, which possibly contains warnings.</param>
        /// <param name="cause">Cause, to match</param>
        /// <returns>True, if FlowOutputSet conains warning with given cause</returns>
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

        /// <summary>
        /// Runs the analysis of the code.
        /// </summary>
        /// <param name="code">the source code without &tl?php</param>
        /// <param name="cause">Cause, to match</param>
        /// <returns>true, if the FlowOutputSet of the analysis end contains warning with specified cause</returns>
        public static bool ArgumentWarningTest(string code, AnalysisWarningCause cause)
        {
            return ContainsWarning(Analyze(code), cause);
        }

        /// <summary>
        /// Analyzes the source code are returns the first value of variable result.
        /// </summary>
        /// <param name="code">the source code without &tl?php</param>
        /// <returns>First value of variable result from the last program point</returns>
        public static Value ResultTest(string code)
        {
            return Analyze(code).ReadValue(new VariableName("result")).PossibleValues.ElementAt(0);
        }

        /// <summary>
        /// Test value type If the type doesn't matches the test fails.
        /// </summary>
        /// <typeparam name="T">Type to match</typeparam>
        /// <param name="value">Value to check</param>
        /// <param name="type">Type to match</param>
        public static void testType<T>(Value value, T type)
        {
            Assert.AreEqual(value.GetType(), type);
        }

        /// <summary>
        /// Test if the value is equals given values, if it doesn't test fails.
        /// </summary>
        /// <typeparam name="T">Type of compared value</typeparam>
        /// <param name="value">Values to compare</param>
        /// <param name="compareValue">Value to match</param>
        public static void testValue<T>(Value value, T compareValue)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            PrimitiveValue<T> val = (PrimitiveValue<T>)value;
            Assert.IsTrue(val.Value.Equals(compareValue));
        }
    }
}
