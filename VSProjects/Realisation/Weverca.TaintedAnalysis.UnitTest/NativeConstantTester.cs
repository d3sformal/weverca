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
using Weverca.ControlFlowGraph;




    [TestClass]
    public class NativeConstantTester
    {
        public FlowOutputSet Analyze(string code)
        {
            var fileName = "./cfg_test.php";
            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            code = "<?php \n" + code + "?>";

            var parser = new SyntaxParser(sourceFile, code);
            parser.Parse();
            var cfg = new ControlFlowGraph(parser.Ast);

            var analysis = new ForwardAnalysis(cfg);
            analysis.Analyse();

            return analysis.ProgramPointGraph.End.OutSet;
        }

        public Value ResultTest(string code)
        {
            return Analyze(code).ReadValue(new VariableName("result")).PossibleValues.ElementAt(0);
        }

        string nativeConstantTrue = @"
            $result=true;
        ";

        string nativeConstantFalse = @"
            $result=false;
        ";

        string nativeConstantInt1 = @"
            $result=E_USER_ERROR;
        ";

        string nativeConstantInt2 = @"
            $result=e_PaRsE;
        ";

        string nativeConstantString1 = @"
            $result=DATE_COOKIE;
        ";

        string nativeConstantString2 = @"
            $result=DATE_W3C;
        ";

        string nativeConstantFloat1 = @"
            $result=M_E;
        ";

        string nativeConstantFloat2 = @"
            $result=INF;
        ";

        string nativeConstantResource = @"
            $result=STDIN;
        ";

        public void testType<T>(Value value, T type)
        {
            Assert.AreEqual(value.GetType(), type);
        }

        public void testValue<T>(Value value,T compareValue)
        {
            PrimitiveValue<T> val = (PrimitiveValue<T>)value;
            Assert.IsTrue(val.Value.Equals(compareValue));
        }


        [TestMethod]
        public void NativeConstantTrue()
        {
            var result=ResultTest(nativeConstantTrue);
            testType(result, typeof(BooleanValue));
            testValue(result, true);
        }

        [TestMethod]
        public void NativeConstantFalse()
        {
            var result = ResultTest(nativeConstantFalse);
            testType(result, typeof(BooleanValue));
            testValue(result, false);
        }

        [TestMethod]
        public void NativeConstantInt1()
        {
            var result = ResultTest(nativeConstantInt1);
            testType(result, typeof(IntegerValue));
            testValue(result, 256);
        }

        [TestMethod]
        public void NativeConstantInt2()
        {
            var result = ResultTest(nativeConstantInt2);
            testType(result, typeof(IntegerValue));
            testValue(result, 4);
        }

        [TestMethod]
        public void NativeConstantString1()
        {
            var result = ResultTest(nativeConstantString1);
            testType(result, typeof(StringValue));
            testValue(result, "l, d-M-y H:i:s T");
        }

        [TestMethod]
        public void NativeConstantString2()
        {
            var result = ResultTest(nativeConstantString2);
            testType(result, typeof(StringValue));
            testValue(result, @"Y-m-d\TH:i:sP");

        }

        [TestMethod]
        public void NativeConstantFloat1()
        {
            var result = ResultTest(nativeConstantFloat1);
            testType(result, typeof(FloatValue));
            testValue(result, 2.718281828459);
        }

        [TestMethod]
        public void NativeConstantFloat2()
        {
            var result = ResultTest(nativeConstantFloat2);
            testType(result, typeof(FloatValue));
            testValue(result, double.PositiveInfinity);

        }

        [TestMethod]
        public void NativeConstantResource()
        {
            var result = ResultTest(nativeConstantResource);
            testType(result, typeof(AnyResourceValue));

        }

        string globalConstDeclaration= @"
            const a=0;
            $result=a;
        ";

        string globalConstDeclarationCaseInsensitive = @"
            const aAa=4;
            $result=aaa;
        ";

        [TestMethod]
        public void GlobalConstDeclaration()
        {
            var result = ResultTest(globalConstDeclaration);
            testType(result, typeof(IntegerValue));
            testValue(result, 0);
        }

        [TestMethod]
        public void GlobalConstDeclarationCaseInsensitive()
        {
            var result = ResultTest(globalConstDeclarationCaseInsensitive);
            testType(result, typeof(UndefinedValue));
        }

        string constDeclaration = @"
            define('aaa',4);
            $result=aaa;
        ";

        string constDeclaration2 = @"
            define('aaa',4,false);
            $result=aaa;
        ";

        string constDeclarationCaseInsensitive = @"
            define('aaa',4,false);
            $result=aAa;
        ";

        string constDeclarationCaseInsensitive2 = @"
            define('aaa',4,true);
            $result=aAa;
        ";

        [TestMethod]
        public void ConstDeclaration()
        {
            var result = ResultTest(constDeclaration);
            testType(result, typeof(IntegerValue));
            testValue(result, 4);
        }

        [TestMethod]
        public void ConstDeclaration2()
        {
            var result = ResultTest(constDeclaration2);
            testType(result, typeof(IntegerValue));
            testValue(result, 4);
        }

        [TestMethod]
        public void constDeclarationCaseInsensitiveTest()
        {
            var result = ResultTest(constDeclarationCaseInsensitive);
            testType(result, typeof(UndefinedValue));
        }

        [TestMethod]
        public void constDeclarationCaseInsensitiveTest2()
        {
            var result = ResultTest(constDeclarationCaseInsensitive2);
            testType(result, typeof(IntegerValue));
            testValue(result, 4);
        }

    }

