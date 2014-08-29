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

        string ExceptionCatchTest = @"
        try
        {
        throw new Exception();
        }
        catch(Exception $e)
        {
        $result=$e;
        }";

        [TestMethod]
        public void ExceptionCatch()
        {
            var result = TestUtils.ResultTest(ExceptionCatchTest);
            TestUtils.testType(result, typeof(ObjectValue));

        }
    }
}