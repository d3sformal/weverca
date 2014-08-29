/*
Copyright (c) 2012-2014 Marcel Kikta, David Skorvaga, Matyas Brenner, and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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


        string StaticCallWithParameterTest = @"
         $result=0;
        class a
        {
            static function b($x)
            {
                global $result;
                $result=$x;
            }
        }
        $a=new a();
        $p='b';
        $a::$p(8);

        ";

        [TestMethod]
        public void StaticCallWithParameter()
        {
            var result = TestUtils.ResultTest(StaticCallWithParameterTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 8);

        }

        string StaticCallWithRefParameterTest = @"

        class a
        {
            static function b(&$x)
            {
                $x=8;
            }
        }
        $a=new a();
        $p='b';
        $a::$p($result);

        ";

        [TestMethod]
        public void StaticCallWithRefParameter()
        {
            var result = TestUtils.ResultTest(StaticCallWithRefParameterTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 8);

        }

        string StaticCallWithSelfTest = @"
            class a
            {
            function b()
            {
            self::c();
            }
            function c()
            {
            global $result;
            $result='called';
            }

            }
            a::b();
        ";
        [TestMethod]
        public void StaticCallWithSelf()
        {
            var result = TestUtils.ResultTest(StaticCallWithSelfTest);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "called");

        }

        string StaticCallWithSelfTest2 = @"
            class a
            {
            static function b()
            {
            self::c();
            }
            static function c()
            {
            global $result;
            $result='called';
            }

            }
            a::b();
        ";
        [TestMethod]
        public void StaticCallWithSelf2()
        {
            var result = TestUtils.ResultTest(StaticCallWithSelfTest2);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "called");

        }


        string CallWithSelfTest = @"
            class a
            {
            function b()
            {
            self::c();
            }
            function c()
            {
            $this->x='called';
            }

            }
            $a=new a();
            $a->b();
            $result=$a->x;
        ";
        [TestMethod]
        public void CallWithSelf()
        {
            var result = TestUtils.ResultTest(CallWithSelfTest);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "called");

        }


        string CallWithParentTest = @"
            class base
            {
                 function c()
                {
                $this->x='base called';
                }
            }


            class a extends base
            {
            function b()
            {
            parent::c();
            }
            function c()
            {
            $this->x='called';
            }

            }
            $a=new a();
            $a->b();
            $result=$a->x;
        ";
        [TestMethod]
        public void CallWithParent()
        {
            var result = TestUtils.ResultTest(CallWithParentTest);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "base called");

        }

        string CannotAccesSelfTest = @"
            self::a();
        ";
        [TestMethod]
        public void CannotAccesSelf()
        {
            var outset = TestUtils.Analyze(CannotAccesSelfTest);
            TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_ACCCES_SELF_WHEN_NOT_IN_CLASS);

        }

        string CannotAccesParentTest = @"
            function p()
            {   
                self::a();
            }
        ";
        [TestMethod]
        public void CannotAccesParent()
        {
            var outset = TestUtils.Analyze(CannotAccesParentTest);
            TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_ACCCES_PARENT_WHEN_NOT_IN_CLASS);

        }

        string CannotAccesParentInClassWithNoParentTest = @"
            class base
            {
                function c()
                {
                   parent::c();
                }
            }
        ";
        [TestMethod]
        public void CannotAccesParentInClassWithNoParent()
        {
            var outset = TestUtils.Analyze(CannotAccesParentInClassWithNoParentTest);
            TestUtils.ContainsWarning(outset, AnalysisWarningCause.CANNOT_ACCCES_PARENT_CURRENT_CLASS_HAS_NO_PARENT);

        }

    }
}