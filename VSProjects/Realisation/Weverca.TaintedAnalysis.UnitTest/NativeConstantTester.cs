using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.Analysis;
using Weverca.Analysis.Memory;
using Weverca.Parsers;

namespace Weverca.TaintedAnalysis.UnitTest
{
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
            var cfg = new ControlFlowGraph.ControlFlowGraph(parser.Ast);

            var analysis = new ForwardAnalysis(cfg);
            analysis.Analyse();

            return analysis.ProgramPointGraph.End.OutSet;
        }

        public Value ResultTest(string code)
        {
            return Analyze(code).ReadValue(new VariableName("result")).PossibleValues.ElementAt(0);
        }

        private string nativeConstantTrue = @"
            $result=true;
        ";

        private string nativeConstantFalse = @"
            $result=false;
        ";

        private string nativeConstantInt1 = @"
            $result=E_USER_ERROR;
        ";

        private string nativeConstantInt2 = @"
            $result=e_PaRsE;
        ";

        private string nativeConstantString1 = @"
            $result=DATE_COOKIE;
        ";

        private string nativeConstantString2 = @"
            $result=DATE_W3C;
        ";

        private string nativeConstantFloat1 = @"
            $result=M_E;
        ";

        private string nativeConstantFloat2 = @"
            $result=INF;
        ";

        private string nativeConstantResource = @"
            $result=STDIN;
        ";

        public void testType<T>(Value value, T type)
        {
            Assert.AreEqual(value.GetType(), type);
        }

        public void testValue<T>(Value value, T compareValue)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            PrimitiveValue<T> val = (PrimitiveValue<T>)value;
            Assert.IsTrue(val.Value.Equals(compareValue));
        }

        [TestMethod]
        public void NativeConstantTrue()
        {
            var result = ResultTest(nativeConstantTrue);
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

        private string globalConstDeclaration = @"
            const a=0;
            $result=a;
        ";

        private string globalConstDeclarationCaseInsensitive = @"
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
            testType(result, typeof(StringValue));
            testValue(result, "aaa");       
        }

        private string constDeclaration = @"
            define('aaa',4);
            $result=aaa;
        ";

        private string constDeclaration2 = @"
            define('aaa',4,false);
            $result=aaa;
        ";

        private string constDeclarationCaseInsensitive = @"
            define('aaa',4,false);
            $result=aAa;
        ";

        private string constDeclarationCaseInsensitive2 = @"
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
        public void ConstDeclarationCaseInsensitiveTest()
        {
            var result = ResultTest(constDeclarationCaseInsensitive);
            testType(result, typeof(StringValue));
            testValue(result, "aAa");
        }

        [TestMethod]
        public void ConstDeclarationCaseInsensitiveTest2()
        {
            var result = ResultTest(constDeclarationCaseInsensitive2);
            testType(result, typeof(IntegerValue));
            testValue(result, 4);
        }

        private string constDeclarationConstantMethod = @"
            const a=4;
            $result=constant('a');
        ";

        [TestMethod]
        public void ConstDeclarationConstantMethod()
        {
            var result = ResultTest(constDeclarationConstantMethod);
            testType(result, typeof(IntegerValue));
            testValue(result, 4);
        }

    }
}
