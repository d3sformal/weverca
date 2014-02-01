using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Summary description for ExceptionTester
    /// </summary>
    [TestClass]
    public class ExceptionTester
    {

        string ThrownStringTest = @"
           try
        {
            $x='x';
            throw $x;
        }
        catch(Exception $e)
        {

        }
        ";

        [TestMethod]
        public void ThrownString()
        {
            var outset = TestUtils.Analyze(ThrownStringTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ONLY_OBJECT_CAM_BE_THROWN));
        }


        string ExceptionTest = @"
        class a{}
        try
        {
            try
            {
                throw new a();
            }
            catch(Exception $e)
            {
                $result=2;
            }
        }
        catch(a $e)
        {
            $result=1;
        }
        ";

        [TestMethod]
        public void Exception()
        {
            var result = TestUtils.ResultTest(ExceptionTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 1);
            
        }

        string ExceptionTest2 = @"
        class a{}
        try
        {

            try
            {
            
                throw new a();
            }
            catch(a $e)
            {
                $result=1;
            }
        }
        catch(Excpetion $e)
        {
            $result=2;
        }
        ";

        [TestMethod]
        public void Exception2()
        {
            var result = TestUtils.ResultTest(ExceptionTest2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 1);

        }

    }
}
