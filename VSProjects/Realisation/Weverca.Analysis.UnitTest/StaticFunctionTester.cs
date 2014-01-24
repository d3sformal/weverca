using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class StaticFunctionTester
    {

        string StaticCallTest = @"
         $result=0;
        class a
        {
            static function b()
            {
                global $result;
                $result=4;
            }
        }
        a::b();

        ";

        [TestMethod]
        public void StaticCall()
        {
            var result = TestUtils.ResultTest(StaticCallTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }


        string StaticCallTest2 = @"
         $result=0;
        class a
        {
            static function b()
            {
                global $result;
                $result=4;
            }
        }
        $a=new a();
        $a::b();

        ";

        [TestMethod]
        public void StaticCall2()
        {
            var result = TestUtils.ResultTest(StaticCallTest2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }

        string StaticCallTest3 = @"
         $result=0;
        class a
        {
            static function b()
            {
                global $result;
                $result=4;
            }
        }

        $p='b';
        a::$p();

        ";

        [TestMethod]
        public void StaticCall3()
        {
            var result = TestUtils.ResultTest(StaticCallTest3);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }

        string StaticCallTest4 = @"
         $result=0;
        class a
        {
            static function b()
            {
                global $result;
                $result=4;
            }
        }
        $a=new a();
        $p='b';
        $a::$p();

        ";

        [TestMethod]
        public void StaticCall4()
        {
            var result = TestUtils.ResultTest(StaticCallTest4);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }

    }
}
