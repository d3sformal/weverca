using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework;

namespace Weverca.Analysis.UnitTest
{

    [TestClass]
    public class CondtionTester
    {

        string SimpleConditionTest = @"
        $i=0;
        if($i<6)
        {
            $i++;
        }
        ";

        [TestMethod]
        public void SimpleCondition()
        {
            var outset = TestUtils.Analyze(SimpleConditionTest);
            var result = outset.ReadVariable(new VariableIdentifier("i")).ReadMemory(outset.Snapshot);
            TestUtils.HasValues(result, 0,1);
            
        }

        string SimpleConditionTest2 = @"
        if(0)
        {
            $result=1;
        }
        else
        {
            $result=2;
        }   
        ";

        [TestMethod]
        public void SimpleCondition2()
        {
            var outset = TestUtils.Analyze(SimpleConditionTest2);
            var result = outset.ReadVariable(new VariableIdentifier("result")).ReadMemory(outset.Snapshot);
            TestUtils.HasValues(result,2);
        }

        string SimpleConditionTest3 = @"
        if(1)
        {
            $result=1;
        }
        else
        {
            $result=2;
        }   
        ";

        [TestMethod]
        public void SimpleCondition3()
        {
            var outset = TestUtils.Analyze(SimpleConditionTest3);
            var result = outset.ReadVariable(new VariableIdentifier("result")).ReadMemory(outset.Snapshot);
            TestUtils.HasValues(result, 1);
        }

    }
}
