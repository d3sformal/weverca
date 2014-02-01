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
        class a extends Exception{}
        class b extends Exception{}

        try
        {
            try
            {
                throw new a();
            }
            catch(b $e)
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
       class a extends Exception{}
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
        catch(Exception $e)
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


        string ExceptionTest3 = @"
        class a extends Exception{}
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
        catch(a $e)
        {
            $result=2;
        }
        ";

        [TestMethod]
        public void Exception3()
        {
            var result = TestUtils.ResultTest(ExceptionTest3);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 1);

        }

        string ExceptionTest4 = @"
        try
        {
            try
            {
                throw new Exception();
            }
            catch(Exception $e)
            {
                $result=1;
                throw new Exception();
            }
        }
        catch(Exception $e)
        {
            $result=2;
        }
        ";

        [TestMethod]
        public void Exception4()
        {
            var result = TestUtils.ResultTest(ExceptionTest4);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 2);

        }

        string ExceptionTest5 = @"
        class a extends Exception{}        
        try
        {
            try
            {
                throw new Exception();
            }
            catch(Exception $e)
            {
                $result=1;
            }
            try
            {
                throw new Exception();
            }
            catch(a $e)
            {
                $result=1;
            }
        }
        catch(Exception $e)
        {
            $result=2;
        }
        ";

        [TestMethod]
        public void Exception5()
        {
            var result = TestUtils.ResultTest(ExceptionTest5);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 2);

        }

        string ExceptionInFunctionsTest = @"
        $result=0;        
        class x extends Exception{}      
        function a()
        {
            throw new x();
        }
        function b()
        {
            a();
        }
             
        try
        {
            b();
        } 
        catch(Exception $e)
        {
            $result=2;
        }
        ";

        [TestMethod]
        public void ExceptionInFunctions()
        {
            var result = TestUtils.ResultTest(ExceptionInFunctionsTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 2);

        }

    }
}
