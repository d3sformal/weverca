using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.Parsers;

namespace Weverca.Analysis.UnitTest
{
    [TestClass]
    public class NativeConstantTester
    {

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

       

        [TestMethod]
        public void NativeConstantTrue()
        {
            var result = TestUtils.ResultTest(nativeConstantTrue);
            TestUtils.testType(result, typeof(BooleanValue));
            TestUtils.testValue(result, true);
        }

        [TestMethod]
        public void NativeConstantFalse()
        {
            var result = TestUtils.ResultTest(nativeConstantFalse);
            TestUtils.testType(result, typeof(BooleanValue));
            TestUtils.testValue(result, false);
        }

        [TestMethod]
        public void NativeConstantInt1()
        {
            var result = TestUtils.ResultTest(nativeConstantInt1);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 256);
        }

        [TestMethod]
        public void NativeConstantInt2()
        {
            var result = TestUtils.ResultTest(nativeConstantInt2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }

        [TestMethod]
        public void NativeConstantString1()
        {
            var result = TestUtils.ResultTest(nativeConstantString1);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "l, d-M-y H:i:s T");
        }

        [TestMethod]
        public void NativeConstantString2()
        {
            var result = TestUtils.ResultTest(nativeConstantString2);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, @"Y-m-d\TH:i:sP");
        }

        [TestMethod]
        public void NativeConstantFloat1()
        {
            var result = TestUtils.ResultTest(nativeConstantFloat1);
            TestUtils.testType(result, typeof(FloatValue));
            TestUtils.testValue(result, 2.718281828459);
        }

        [TestMethod]
        public void NativeConstantFloat2()
        {
            var result = TestUtils.ResultTest(nativeConstantFloat2);
            TestUtils.testType(result, typeof(FloatValue));
            TestUtils.testValue(result, double.PositiveInfinity);
        }

        [TestMethod]
        public void NativeConstantResource()
        {
            var result = TestUtils.ResultTest(nativeConstantResource);
            TestUtils.testType(result, typeof(AnyResourceValue));
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
            var result = TestUtils.ResultTest(globalConstDeclaration);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 0);
        }

        [TestMethod]
        public void GlobalConstDeclarationCaseInsensitive()
        {
            var result = TestUtils.ResultTest(globalConstDeclarationCaseInsensitive);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "aaa");       
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
            var result = TestUtils.ResultTest(constDeclaration);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }

        [TestMethod]
        public void ConstDeclaration2()
        {
            var result = TestUtils.ResultTest(constDeclaration2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }

        [TestMethod]
        public void ConstDeclarationCaseInsensitiveTest()
        {
            var result = TestUtils.ResultTest(constDeclarationCaseInsensitive);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "aAa");
        }

        [TestMethod]
        public void ConstDeclarationCaseInsensitiveTest2()
        {
            var result = TestUtils.ResultTest(constDeclarationCaseInsensitive2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }

        private string constDeclarationConstantMethod = @"
            const a=4;
            $result=constant('a');
        ";

        [TestMethod]
        public void ConstDeclarationConstantMethod()
        {
            var result = TestUtils.ResultTest(constDeclarationConstantMethod);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }

    }
}
