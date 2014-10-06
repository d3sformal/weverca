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