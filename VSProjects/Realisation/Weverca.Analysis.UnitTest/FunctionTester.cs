using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class FunctionTester
    {

        string FunctionBeforeDeclarationCallTest= @"
            $result=a(0);
            function a($x)
            {
            return 4;
            }
        ";

        string MethodBeforeDeclarationCallTest = @"
            $x = new A();
            $result = $x->a(4);
            class A
            {
                function a($x)
                {
                return 4;
                }
            }
        ";

        string ParameterByAliasFunctionAtCalerSideTest = @"
            function f($arg) {
               $arg = 2;
            }
            f(&$result);
            ";
        // TODO: Similar to ParameterByAliasFunctionAtCalerSideTest, but fails. 
        // Because passing aliases at calee site is not yet implemented.
        // TODO: if the functionality of passing by alias is implemented at framework,
        // move this test to Weverca.AnalysisFramework.UnitTest.ForwardAnalysisTest
        string ParameterByAliasFunctionAtCaleeSideTest = @"
            function f(&$arg) {
               $arg = 2;
            }
            f($result);
            ";


        [TestMethod]
        public void FunctionBeforeDeclarationCall()
        {
            var result = TestUtils.ResultTest(FunctionBeforeDeclarationCallTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result,4);
        }

        [TestMethod]
        public void MethodBeforeDeclarationCall()
        {
            var result = TestUtils.ResultTest(MethodBeforeDeclarationCallTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }

        [TestMethod]
        public void ParameterByAliasFunctionAtCalerSide()
        {
            var result = TestUtils.ResultTest(ParameterByAliasFunctionAtCalerSideTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 2);
        }

        [TestMethod]
        public void ParameterByAliasFunctionAtCaleeSide()
        {
            var result = TestUtils.ResultTest(ParameterByAliasFunctionAtCaleeSideTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 2);
        }

    }
}
