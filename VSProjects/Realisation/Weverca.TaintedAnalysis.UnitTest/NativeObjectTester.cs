﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.UnitTest
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

        string ObjectMethodTest2 = @"
            $a=new RuntimeException ('0');
            $result=$a->getPrevious();
        ";

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

    /*    [TestMethod]
        public void ObjectMethod2()
        {
            var outset = TestUtils.Analyze(ObjectMethodTest2);
            var t=outset.ReadValue(new VariableName("result")).PossibleValues.GetEnumerator().Current;
           
        }*/

    }
}