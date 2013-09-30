using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.UnitTest
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

    }
}
