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

        public bool ArgumentWarningTest(string code,AnalysisWarningCause cause)
        {
            return ContainsWarning(Analyze(code), cause);
        }

        public Value ResultTest(string code)
        {
            return Analyze(code).ReadValue(new VariableName("result")).PossibleValues.ElementAt(0);
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

        [TestMethod]
        public void WrongArgumentCount1()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentcount1, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void WrongArgumentCount2()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentcount2, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void WrongArgumentCount3()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentcount3, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void WrongArgumentCount4()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentcount4, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

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
        public void CorrectArgumentCount1()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentcount1, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void CorrectArgumentCount2()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentcount2, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void CorrectArgumentCount3()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentcount3, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        [TestMethod]
        public void CorrectArgumentCount4()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentcount4, AnalysisWarningCause.WRONG_NUMBER_OF_ARGUMENTS));
        }

        string correctArgumentType1 = @"
           levenshtein ('a', 'b', 1,2,3 );
        ";

        string correctArgumentType2 = @"
            levenshtein ( 'a','b');
        ";

        string correctArgumentType3 = @"
           $fdf = fdf_open('test.fdf');
           fdf_close($fdf);
        ";

        string correctArgumentType4 = @"
             $a=array(1,0,1,5,8,7);
            sort($a,5);
        ";

        string correctArgumentType5 = @"
             $b=array('a','a','a');
            usort($b,'cmp');
        ";


        string correctArgumentType6 = @"
            $b[0]=4;
            $b[1]=5;
            $b[2]=3;
            $b[3]=1;
            $a=function($a,$b){};
            usort($b,$a);
        ";

        string correctArgumentType7 = @"
            wordwrap ('a', 10, 'a' , (1==1) );
        ";

        string correctArgumentType8 = @"
            acos(1.1);
            acos(8);
        ";

        [TestMethod]
        public void CorrectArgumentType1()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentType1, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void CorrectArgumentType2()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentType2, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void CorrectArgumentType3()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentType3, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void CorrectArgumentType4()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentType4, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void CorrectArgumentType5()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentType5, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void CorrectArgumentType6()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentType6, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void CorrectArgumentType7()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentType7, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        public void CorrectArgumentType8()
        {
            Assert.IsFalse(ArgumentWarningTest(correctArgumentType8, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        string wrongArgumentType1 = @"
           acosh ('aa'); 
        ";

        string wrongArgumentType2 = @"
          chunk_split ( 'a' , 'a' );
        ";

        string wrongArgumentType3 = @"
            chunk_split ( 'a' , 1 ,1);
        ";

        string wrongArgumentType4 = @"
            md5 ( 'a', 5 );
        ";

        string wrongArgumentType5 = @"
           strtr ( 'a','a' ) ;
        ";

        [TestMethod]
        public void WrongArgumentType1()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentType1, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void WrongArgumentType2()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentType2, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void WrongArgumentType3()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentType3, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void WrongArgumentType4()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentType4, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        [TestMethod]
        public void WrongArgumentType5()
        {
            Assert.IsTrue(ArgumentWarningTest(wrongArgumentType5, AnalysisWarningCause.WRONG_ARGUMENTS_TYPE));
        }

        string functionResult1 = @"
            $result=money_format ( '0' , 1 );
        ";

        string functionResult2 = @"
            $result=atan ( 1 );
        ";

        string functionResult3 = @"
            $result= intval ( 1==1 );
        ";

        string functionResult4 = @"
            $result=localeconv  (  );
        ";

        string functionResult5 = @"
            $result=sort ( $a );
        ";

        string functionResult6 = @"
            $result=mysql_connect ( );
        ";

        string functionResult7 = @"
            $result=mysqli_connect ();
        ";

        string functionResult8 = @"
            $result=substr_replace('a');
        ";

        [TestMethod]
        public void FunctionResult1()
        {
            Assert.AreEqual(ResultTest(functionResult1).GetType(), typeof(AnyStringValue));
        }

        [TestMethod]
        public void FunctionResult2()
        {
            Assert.AreEqual(ResultTest(functionResult2).GetType(), typeof(AnyFloatValue));
        }

        [TestMethod]
        public void FunctionResult3()
        {
            Assert.AreEqual(ResultTest(functionResult3).GetType(), typeof(AnyIntegerValue));
        }

        [TestMethod]
        public void FunctionResult4()
        {
            Assert.AreEqual(ResultTest(functionResult4).GetType(), typeof(AnyArrayValue));
        }

        [TestMethod]
        public void FunctionResult5()
        {
            Assert.AreEqual(ResultTest(functionResult5).GetType(), typeof(AnyBooleanValue));
        }

        [TestMethod]
        public void FunctionResult6()
        {
            Assert.AreEqual(ResultTest(functionResult6).GetType(), typeof(AnyResourceValue));
        }

        [TestMethod]
        public void FunctionResult7()
        {
            Assert.AreEqual(ResultTest(functionResult7).GetType(), typeof(AnyObjectValue));
        }

        [TestMethod]
        public void FunctionResult8()
        {
            Assert.AreEqual(ResultTest(functionResult8).GetType(), typeof(AnyValue));
        }

    }
}
