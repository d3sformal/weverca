using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.Parsers;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Provides functionality for testing source code analysis
    /// </summary>
    internal class TestUtils
    {
        /// <summary>
        /// Create new <see cref="ProgramPointGraph"/> object from PHP source code
        /// </summary>
        /// <param name="code">The source code without &lt;?php and ?&gt;</param>
        /// <returns><see cref="ProgramPointGraph"/> that contains CFG and memory model</returns>
        public static ForwardAnalysis GenerateForwardAnalysis(string code)
        {
            var fileName = "./cfg_test.php";
            var fullPath = new FullPath(Path.GetDirectoryName(fileName));
            var file = new FileInfo(fileName);

            var sourceFile = new PhpSourceFile(fullPath, new FullPath(fileName));
            code = "<?php\n" + code + "\n?>";

            var parser = new SyntaxParser(sourceFile, code);
            parser.Parse();
            var cfg = Weverca.ControlFlowGraph.ControlFlowGraph.FromSource(parser.Ast, file);

            return new ForwardAnalysis(cfg, MemoryModels.MemoryModels.CopyMM, 50);
        }

        /// <summary>
        /// Analyzes the forward analysis and return the resulting <see cref="ProgramPointGraph"/>
        /// </summary>
        /// <param name="analysis"><see cref="ProgramPointGraph"/> that contains CFG and memory model</param>
        /// <returns><see cref="ProgramPointGraph"/> generated during the analysis</returns>
        public static ProgramPointGraph GeneratePpg(ForwardAnalysis analysis)
        {
            analysis.Analyse();
            return analysis.ProgramPointGraph;
        }

        /// <summary>
        /// Get output set of the last program point, where are the results of program execution
        /// </summary>
        /// <param name="ppg"><see cref="ProgramPointGraph"/> generated during the analysis</param>
        /// <returns>Output set of analysis</returns>
        public static FlowOutputSet GetResultOutputSet(ProgramPointGraph ppg)
        {
            return ppg.End.OutSet;
        }

        /// <summary>
        /// Analyzes the source code and return the resulting FlowOutputSet
        /// </summary>
        /// <param name="code">The source code without &lt;?php and ?&gt;</param>
        /// <returns>FlowOutputSet from last program point of analysis</returns>
        public static FlowOutputSet Analyze(string code)
        {
            var analysis = GenerateForwardAnalysis(code);
            var ppg = GeneratePpg(analysis);
            return GetResultOutputSet(ppg);
        }

        /// <summary>
        /// Determines when the FlowOutputSet contains analysis warning,
        /// which has the same cause as the second parameter
        /// </summary>
        /// <param name="outset">Output set, which possibly contains warnings.</param>
        /// <param name="cause">Cause, to match</param>
        /// <returns>True, if FlowOutputSet contains warning with given cause</returns>
        public static bool ContainsWarning(FlowOutputSet outset, AnalysisWarningCause cause)
        {
            var warnings = AnalysisWarningHandler.ReadWarnings<AnalysisWarning>(outset);
            foreach (var value in warnings)
            {
                var infoValue = (InfoValue<AnalysisWarning>)value;
                if (infoValue.Data.Cause == cause)
                {
                    return true;
                }
            }

            return false;
        }


        public static bool ContainsSecurityWarning(FlowOutputSet outset, FlagType cause)
        {
            var warnings = AnalysisWarningHandler.ReadWarnings<AnalysisSecurityWarning>(outset);
            foreach (var value in warnings)
            {
                var infoValue = (InfoValue<AnalysisSecurityWarning>)value;
                if (infoValue.Data.Flag == cause)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Runs the analysis of the code.
        /// </summary>
        /// <param name="code">The source code without &lt;?php</param>
        /// <param name="cause">Cause, to match</param>
        /// <returns>
        /// <c>true</c>, if the FlowOutputSet of the analysis end contains warning with specified cause
        /// </returns>
        public static bool ArgumentWarningTest(string code, AnalysisWarningCause cause)
        {
            return ContainsWarning(Analyze(code), cause);
        }

        /// <summary>
        /// Analyzes the source code are returns the first value of variable result.
        /// </summary>
        /// <param name="code">The source code without &lt;?php</param>
        /// <returns>First value of variable result from the last program point</returns>
        public static Value ResultTest(string code)
        {
            var outSet = Analyze(code);
            var snapshotEntry = outSet.GetVariable(new VariableIdentifier("result"));
            var entry = snapshotEntry.ReadMemory(outSet.Snapshot);
            return entry.PossibleValues.First();
        }

        /// <summary>
        /// Test value type If the type doesn't matches the test fails.
        /// </summary>
        /// <typeparam name="T">Type of type to match</typeparam>
        /// <param name="value">Value to check</param>
        /// <param name="type">Type to match</param>
        public static void testType<T>(Value value, T type)
        {
            Assert.AreEqual(type, value.GetType());
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
            var val = (ScalarValue<T>)value;
            Assert.IsTrue(val.Value.Equals(compareValue));
        }

        public static void IsDirty(Value value)
        {
            var flag = value.GetInfo<Flags>();
            Debug.Assert(flag.isDirty(FlagType.FilePathDirty) && flag.isDirty(FlagType.HTMLDirty) && flag.isDirty(FlagType.HTMLDirty));
        }

        public static void IsClean(Value value)
        {
            var flag = value.GetInfo<Flags>();
            if (flag == null)
            {
                return;
            }
            else
            {
                Debug.Assert(!flag.isDirty(FlagType.FilePathDirty) && !flag.isDirty(FlagType.HTMLDirty) && !flag.isDirty(FlagType.HTMLDirty));
            }
        }

        public static void IsClean(Value value, FlagType type)
        {
            var flag = value.GetInfo<Flags>();
            if (flag == null)
            {
                return;
            }
            else
            {
                Debug.Assert(!flag.isDirty(type));
            }
        }

        public static void HasValues<T>(MemoryEntry entry, params T[] types)
        {
            foreach (var value in entry.PossibleValues)
            {
                bool match = false;
                foreach (T t in types)
                {
                    if ((value as ScalarValue<T>).Value.Equals(t))
                    {
                        match = true;
                    }
                }
                Debug.Assert(match);
            }

        }

    }
}
