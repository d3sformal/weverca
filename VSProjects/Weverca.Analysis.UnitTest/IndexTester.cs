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
    public class IndexTester
    {
        string IndexIntergerTest = @"
            $a=4;
            $p=$a[""a""];    
        ";


        [TestMethod]
        public void IndexInterger()
        {
            var result = TestUtils.Analyze(IndexIntergerTest);
            Debug.Assert(TestUtils.ContainsWarning(result, AnalysisWarningCause.CANNOT_ACCESS_FIELD_OPERATOR_ON_NON_ARRAY));

        }

        string IndexIntergerTest2 = @"
            $a=4;
            $a[""a""]=15;    
        ";


        // TODO test: get this test working
        //[TestMethod]
        public void IndexInterger2()
        {
            var result = TestUtils.Analyze(IndexIntergerTest2);
            Debug.Assert(TestUtils.ContainsWarning(result, AnalysisWarningCause.CANNOT_ACCESS_FIELD_OPERATOR_ON_NON_ARRAY));

        }

        string IndexStringTest = @"
            $result=""Hello world"";
            $result[0]=5;
        ";


        [TestMethod]
        public void IndexString()
        {
            var result = TestUtils.ResultTest(IndexStringTest);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "5ello world");

        }

        string IndexStringTest2 = @"
            $result=""Hello world"";
            $result[2]=array();
        ";


        [TestMethod]
        public void IndexString2()
        {
            var result = TestUtils.ResultTest(IndexStringTest2);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "HeAlo world");

        }

        string IndexStringTest3 = @"
            $result=""Hello world"";
            $result[2000]=array();
        ";


        [TestMethod]
        public void IndexString3()
        {
            var result = TestUtils.ResultTest(IndexStringTest3);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "Hello world A");

        }

        string IndexStringTestTest4 = @"
            $result=""Hello world"";
            $result[0]=array();
            $result[4]=true;
        ";


        [TestMethod]
        public void IndexString4()
        {
            var result = TestUtils.ResultTest(IndexStringTestTest4);
            TestUtils.testType(result, typeof(StringValue));
            TestUtils.testValue(result, "Aell1 world");

        }

        string IndexStringTest5 = @"
            $result=""Hello world"";
            $result[-1]=array();
          
        ";


        [TestMethod]
        public void IndexString5()
        {
            var result = TestUtils.Analyze(IndexStringTest5);
            Debug.Assert(TestUtils.ContainsWarning(result, AnalysisWarningCause.INDEX_OUT_OF_RANGE));
        }


        string PropertyAccessOnIntegerTest = @"
            $result=4;
            $result->a=array();
          
        ";


        // TODO test: get this test working
        //[TestMethod]
        public void PropertyAccessOnInteger()
        {
            var result = TestUtils.Analyze(PropertyAccessOnIntegerTest);
            Debug.Assert(TestUtils.ContainsWarning(result, AnalysisWarningCause.CANNOT_ACCESS_OBJECT_OPERATOR_ON_NON_OBJECT));
        }


        string PropertyAccessOnIntegerTest2 = @"
            $result=4;
            $x=$result->a;
          
        ";


        [TestMethod]
        public void PropertyAccessOnInteger2()
        {
            var result = TestUtils.Analyze(PropertyAccessOnIntegerTest2);
            Debug.Assert(TestUtils.ContainsWarning(result, AnalysisWarningCause.CANNOT_ACCESS_OBJECT_OPERATOR_ON_NON_OBJECT));
        }


    }
}