using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Weverca.Analysis;
using PHP.Core;
using Weverca.Analysis.Memory;
using Weverca.Parsers;
using Weverca.TaintedAnalysis;

using PHP.Core.Parsers;


namespace Weverca.TaintedAnalysis.UnitTest
{

    [TestClass]
    public class NativeFuntionTester
    {
        public FlowOutputSet Analyze(string code)
        {
            var fileName = "./cfg_test.php";
            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            code = "<?php \n" + code + "?>";

            var parser = new SyntaxParser(sourceFile, code);
            parser.Parse();
            var cfg = new ControlFlowGraph.ControlFlowGraph(parser.Ast);

            var analysis = new ForwardAnalysis(cfg);
            analysis.Analyse();

            return analysis.ProgramPointGraph.End.OutSet;
        }

        public bool ContainsWarning(FlowOutputSet outset, AnalysisWarningCause cause)
        {
            IEnumerable<Value> warnings=AnalysisWarningHandler.ReadWarnings(outset);
            foreach (var value in warnings)
            {
                InfoValue<AnalysisWarning> infoValue = (InfoValue<AnalysisWarning>) value;
                if (infoValue.Data.Cause == cause)
                    return true;
            }
            return false;
        }

        public bool ArgumentNumberTest(string code,AnalysisWarningCause cause)
        {
            return ContainsWarning(Analyze(code), cause);
        }

        string wrongArgumentcount1 = @"
            min();
        ";

        string wrongArgumentcount2 = @"
            sin(1,1,1,1,58);
        ";

        string wrongArgumentcount3 = @"
            strstr(1,1,1,1);
        ";

        string wrongArgumentcount4 = @"
            strstr(1);
        ";

        string correctArgumentcount1 = @"
            min(1,2,3,4,5,6,7,8);
        ";

        string correctArgumentcount2 = @"
            sin(1.0);
        ";

        string correctArgumentcount3 = @"
            strstr(1,1,1);
        ";

        string correctArgumentcount4 = @"
            strstr(1,8);
        ";

        [TestMethod]
        public void WrongArgumentCount1()
        {
            Assert.IsTrue(ArgumentNumberTest(wrongArgumentcount1, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void WrongArgumentCount2()
        {
            Assert.IsTrue(ArgumentNumberTest(wrongArgumentcount2, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void WrongArgumentCount3()
        {
            Assert.IsTrue(ArgumentNumberTest(wrongArgumentcount3, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void WrongArgumentCount4()
        {
            Assert.IsTrue(ArgumentNumberTest(wrongArgumentcount4, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void CorrectArgumentCount1()
        {
            Assert.IsFalse(ArgumentNumberTest(correctArgumentcount1, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void CorrectArgumentCount2()
        {
            Assert.IsFalse(ArgumentNumberTest(correctArgumentcount2, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void CorrectArgumentCount3()
        {
            Assert.IsFalse(ArgumentNumberTest(correctArgumentcount3, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void CorrectArgumentCount4()
        {
            Assert.IsFalse(ArgumentNumberTest(correctArgumentcount4, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

    }
}
