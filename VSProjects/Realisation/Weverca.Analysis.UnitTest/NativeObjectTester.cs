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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.UnitTest
{
    [TestClass]
    public class NativeObjectTester
    {

        string ObjectConstructTest = @"
            $result=new RuntimeException('0');
        ";
        string ObjectFieldTest = @"
            $a=new DateInterval ('0');
            $result=$a->m;
        ";

        string ObjectMethodTest = @"
            $a=new DateInterval ('0');
            $result=$a->format('a');
        ";

        string ObjectParameterByReferenceTest = @"
$base=new  EventBase (new EventConfig());
$a=new EventBufferEvent ($base);
$a->read($result,0);";

        [TestMethod]
        public void ObjectConstruct()
        {
            var result = TestUtils.ResultTest(ObjectConstructTest);
            TestUtils.testType(result, typeof(ObjectValue));
        }

        [TestMethod]
        public void ObjectField()
        {
            var result = TestUtils.ResultTest(ObjectFieldTest);
            TestUtils.testType(result, typeof(AnyIntegerValue));
        }

        [TestMethod]
        public void ObjectMethod()
        {
            var result = TestUtils.ResultTest(ObjectMethodTest);
            TestUtils.testType(result, typeof(AnyStringValue));
        }

        [TestMethod]
        public void ObjectParameterByReference()
        {
            var result = TestUtils.ResultTest(ObjectParameterByReferenceTest);
            TestUtils.testType(result, typeof(AnyStringValue));
        }
    }
}