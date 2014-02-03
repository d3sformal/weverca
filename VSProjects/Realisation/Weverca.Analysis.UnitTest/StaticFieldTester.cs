﻿using System;
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
    public class StaticFieldTester
    {

        string StaticFieldSelfTest = @"
            class a
            {
                static $x;
                static function call()
                {
                    self::$x=4;
                }
            }
            a::call();
            $result=a::$x;
        ";

        [TestMethod]
        public void StaticFieldSelf()
        {
            var result = TestUtils.ResultTest(StaticFieldSelfTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }

        string StaticFieldParentTest = @"
            class base
            {
                static $x;
            }
    
            class a extends base
            {
                
                static function call()
                {
                    parent::$x=4;
                }
            }
            a::call();
            $result=base::$x;
        ";

        [TestMethod]
        public void StaticFieldParent()
        {
            var result = TestUtils.ResultTest(StaticFieldParentTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);
        }


        string StaticFieldTest1 = @"
            class a
            {      
            }

            $result=a::$x;
        ";

        [TestMethod]
        public void StaticField1()
        {
            var outSet = TestUtils.Analyze(StaticFieldTest1);
            Debug.Assert(TestUtils.ContainsWarning(outSet, AnalysisWarningCause.STATIC_VARIABLE_DOESNT_EXIST));
        }

        string StaticFieldTest2 = @"
            $result=a::$x;
        ";

        [TestMethod]
        public void StaticField2()
        {
            var outSet = TestUtils.Analyze(StaticFieldTest2);
            Debug.Assert(TestUtils.ContainsWarning(outSet, AnalysisWarningCause.CLASS_DOESNT_EXIST));
        }

        string StaticFieldTest3 = @"
            $result=MongoCursor ::$timeout;
        ";

        [TestMethod]
        public void StaticField3()
        {
            var result = TestUtils.ResultTest(StaticFieldTest3);
            TestUtils.testType(result, typeof(IntegerValue));

        }

        string StaticFieldTest4 = @"
            $result=Exception ::$timeout;
        ";

        [TestMethod]
        public void StaticField4()
        {
            var outSet = TestUtils.Analyze(StaticFieldTest4);
            Debug.Assert(TestUtils.ContainsWarning(outSet, AnalysisWarningCause.STATIC_VARIABLE_DOESNT_EXIST));

        }


        string StaticFieldInitTest = @"
            class a
            {    
                static $x=4;  
            }

            $result=a::$x;
        ";

        [TestMethod]
        public void StaticFieldInit()
        {
            var result = TestUtils.ResultTest(StaticFieldInitTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 4);

        }

        string StaticInheritedFieldTest = @"
        class a 
        {
        public static $x=4;

        }
        class b extends a 
        {


        }

        $result = b::$x;
        a::$x=2;
        $result+=b::$x;
        ";

        [TestMethod]
        public void StaticInheritedField()
        {
            var result = TestUtils.ResultTest(StaticInheritedFieldTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 6);

        }

        string StaticInheritedFieldTest2 = @"
        class a 
        {
        public static $x=4;

        }
        class b extends a 
        {
         public static $x=6;

        }

        $result = b::$x+a::$x;
        ";

        [TestMethod]
        public void StaticInheritedField2()
        {
            var result = TestUtils.ResultTest(StaticInheritedFieldTest2);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 10);

        }

        string StaticInheritedFieldTest3 = @"
        class a 
        {
        public static $x=2;

        }
        class b extends a 
        {

        }

        class c extends b 
        {

        }

         class d extends a 
        {

        }
        d::$x=3;
        $result = b::$x*a::$x*c::$x*d::$x;
        ";

        [TestMethod]
        public void StaticInheritedField3()
        {
            var result = TestUtils.ResultTest(StaticInheritedFieldTest3);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 81);

        }


    }
}
