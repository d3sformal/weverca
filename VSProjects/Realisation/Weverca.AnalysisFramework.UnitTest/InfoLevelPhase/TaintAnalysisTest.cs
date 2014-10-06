/*
Copyright (c) 2012-2014 Natalia Tyrpakova, David Hauzar

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Weverca.Taint;
using Weverca.AnalysisFramework;

namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    [TestClass]
    public class TaintAnalysisTest
    {
        readonly static TestCase TaintAnalysisBasic_CASE =
        @" $b = $_POST;
        ".AssertVariable("b").HasTaintStatus(new TaintStatus(true, true, new List<int> { 2 }));


        readonly static TestCase TaintAnalysisArraysAliases2_CASE =
        @" $c[1] = &$b;
        $b = $_POST;
        $a = $c[1];
        $y = $c[2];
        $d[1][2] = $c[1];
        $x = $d[1][2];
        $z = $x;
        $z = 1;
        ".AssertVariable("_POST").HasTaintStatus(new TaintStatus(true, true))
        .AssertVariable("a").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 3, 4 }))
        .AssertVariable("x").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 3, 6, 7 }))
        .AssertVariable("y").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 5 })) //null value
        .AssertVariable("z").HasTaintStatus(new TaintStatus(false, false));

        readonly static TestCase TaintAnalysisAliases_CASE =
        @" $x = $_POST;
        $y = &$x;
        ".AssertVariable("_POST").HasTaintStatus(new TaintStatus(true, true))
        .AssertVariable("y").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 3 }));


        readonly static TestCase TaintAnalysisAliases2_CASE =
         @" $b = 1;
        $a = &$b;
        $b = $_POST;
        ".AssertVariable("_POST").HasTaintStatus(new TaintStatus(true, true))
          .AssertVariable("b").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 4 }))
          .AssertVariable("a").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 4 }));

        readonly static TestCase TaintAnalysisNullValue_CASE =
        @" $x = null;
        $y = $x;
        ".AssertVariable("x").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 2 }))
        .AssertVariable("y").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 2, 3 }));

        //TODO nefunguje
        readonly static TestCase TaintAnalysisMultipleNullFlows_CASE =
       @" $x = null;
        $y = $x;
        if ($_POST) {
           $z = $x;
        }
        else {
            $z = $y;
        }
        $w = $z;
        ".AssertVariable("x").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 2 }))
       .AssertVariable("y").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 2, 3 }))
       .AssertVariable("w").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 2, 5, 10 }, new List<int>() { 2, 3, 8, 10 }));



        readonly static TestCase TaintAnalysisUndefinedValue_CASE =
        @" $x = $a;
        $y = $x;
        ".AssertVariable("x").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 2 }))
        .AssertVariable("y").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 2, 3 }));

        // Tests weak updates
        // weak because of indirect variable accesses
        // weak because of aliasing
        //TODO nefunguje
        readonly static TestCase TaintAnalysisArraysAliasesWeakUpdates_CASE =
        @" $any = $_POST;
        $x = $_POST;
        $y = 'str';
        $v = 'str';
        $w = 'str';
        $p = $x;
        $q = $x;
        $r = 'str';
        $s = 'str'; //line 10
        if ($any) {
            $v1 = 'x';
            $v2 = 'v';
            $p = &$q;
            $r = &$s;
    
        } else {
            $v1 = 'y';
            $v2 = 'w';
        } //line 20
        // updates $x, but the update is weak and $x should stay tainted
        $$v1 = 'str';

        // updates $v, the update is weak, but taint status is propagated
        $$v2 = $x;

        // updates $p, but the update is weak and $p should stay tainted
        $q = 'str';

        // updates $r, the update is weak, but taint status is propagated //line 30
        $s = $x;

        ".AssertVariable("_POST").HasTaintStatus(new TaintStatus(true, true))
        .AssertVariable("x").HasTaintStatus(new TaintStatus(true, false, new List<int>() { 3 }))
        .AssertVariable("v").HasTaintStatus(new TaintStatus(true, false, new List<int>() { 3, 25 }))
        .AssertVariable("w").HasTaintStatus(new TaintStatus(true, false, new List<int>() { 3, 25 }))
        .AssertVariable("p").HasTaintStatus(new TaintStatus(true, false, new List<int>() { 3, 7 }, new List<int>() { 3, 8 }, new List<int>() { 3, 8, 14 }))
        .AssertVariable("q").HasTaintStatus(new TaintStatus(false, false))
        .AssertVariable("s").HasTaintStatus(new TaintStatus(true, false, new List<int>() { 3, 31 }))
        .AssertVariable("r").HasTaintStatus(new TaintStatus(true, false, new List<int>() { 3, 31 }));

        readonly static TestCase TaintAnalysisFunctions_CASE =
        @" function f($arg) {
            return $arg;
        } 

        $x = $_POST;
        $y = 'str';
        $a = f($x);
        $b = f($y);
        ".AssertVariable("_POST").HasTaintStatus(new TaintStatus(true, true))
         .AssertVariable("a").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 6, 8, 3, 8 }))
         .AssertVariable("b").HasTaintStatus(new TaintStatus(false, false));

        readonly static TestCase TaintAnalysisFunctionsTwoArguments_CASE =
        @" function f($arg1, $arg2) {
            return $arg1.$arg2;
        } 

        $x = $_POST;
        $y = '1';
        $a = f($x, $y);
        $b = f($y, $x);
        $c = f($x, $x);
        $d = f($y, $y);
        ".AssertVariable("_POST").HasTaintStatus(new TaintStatus(true, true))
         .AssertVariable("a").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 6, 8, 3, 3, 8 }))
         .AssertVariable("b").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 6, 9, 3, 3, 9 }))
         .AssertVariable("c").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 6, 10, 3, 3, 10 }))
         .AssertVariable("d").HasTaintStatus(new TaintStatus(false, false));


        readonly static TestCase TaintAnalysisBinaryOperation_CASE =
        @" $x = $_POST;
        $y = '1';
        $z = $x + $y;
        ".AssertVariable("z").HasTaintStatus(new TaintStatus(false, false));

        readonly static TestCase TaintAnalysisParameterByAlias_CASE =
        @" function f($arg) {
            $x = $_POST;            
            $arg = $x;
            return $arg;
        }
        $x = 'str';
        $y = f(&$x);
            ".AssertVariable("x").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 3, 4 }))
             .AssertVariable("y").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 3, 4, 5, 8 }));

        readonly static TestCase TaintAnalysisFunctionTaintedParams_CASE =
        @" function f($a,$b,$c) {
            $x = $a.$b.$c;           
            return $x;
        }
        $x = 'str';
        $y = $_POST;
        $z = $_POST;
        $r = f($x,$y,$z);
            ".AssertVariable("r").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 7, 9, 3, 3, 4, 9 }, new List<int>() { 8, 9, 3, 3, 4, 9 }));

        readonly static TestCase TaintAnalysisFunctionNullParams_CASE =
        @" function f($a,$b,$c) {
            $x = $a.$b.$c;           
            return $x;
        }
        $x = 'str';
        $y = null;
        $z = $y;
        $r = f($x,$y,$z);
            ".AssertVariable("r").HasTaintStatus(new TaintStatus(false, true, new List<int>() { 7, 9, 3, 3, 4, 9 }, new List<int>() { 7, 8, 9, 3, 3, 4, 9 }));

        readonly static TestCase TaintAnalysisSanitizers_CASE =
        @" $x = $_POST;
        $y = htmlentities($x);
        $z = mysql_escape_string($x);
        $w = md5(y);   
            ".AssertVariable("x").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2 }))
             .AssertVariable("y").HasTaintStatus(new TaintStatus(false, false), Analysis.FlagType.HTMLDirty)
             .AssertVariable("y").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 3, 3 }), Analysis.FlagType.SQLDirty)
             .AssertVariable("z").HasTaintStatus(new TaintStatus(false, false), Analysis.FlagType.SQLDirty)
             .AssertVariable("z").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 4, 4 }), Analysis.FlagType.FilePathDirty)
             .AssertVariable("w").HasTaintStatus(new TaintStatus(false, false));

        readonly static TestCase TaintAnalysisWeakSanitizers_CASE =
        @" $x = $_POST;
        $y = $x;
        if ($_POST) {
            $y = mysql_escape_string($x);
        } 
            ".AssertVariable("y").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 3 }, new List<int>() { 2, 5, 5 }), Analysis.FlagType.HTMLDirty)
             .AssertVariable("y").HasTaintStatus(new TaintStatus(true, false, new List<int>() { 2, 3 }), Analysis.FlagType.SQLDirty);

        readonly static TestCase TaintAnalysisSimpleCycle_CASE =
        @" $a = $_POST;
        $i = 1;
        while ($i <= 2) {
            $c = $b;
            $b = $a;
            $i++;
        }
            ".AssertVariable("c").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 6, 5 }))
            .AssertVariable("b").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 6 }));

        readonly static TestCase TaintAnalysisCycle_CASE =
        @" $a = $_POST;
        $i = 1;
        while ($i <= 4) {
            if ($_POST){
                $x = $d;
            }
            else{
                $x = $c;
            }
            $d = $c;
            $c = $b;
            $b = $a;
            $i++;
        }
            ".AssertVariable("x").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 13, 12, 11, 6 }, new List<int>() { 2, 13, 12, 9 }))
            .AssertVariable("c").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 13, 12 }))
            .AssertVariable("d").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 13, 12, 11 }));

        //OK
        readonly static TestCase TaintAnalysisFunctionCallInCycle_CASE =
        @" function f($x) {          
            return $x;
        }
        $x = $_POST;
        $i = 1;
        while ($i < 10) {
            $a = f($x);
            $i++;
        }
            ".AssertVariable("a").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 5, 8, 3, 8 }));

        readonly static TestCase TaintAnalysisFunctionCallInCycle2_CASE =
        @" function f($x) {          
            return $x;
        }
        $x = $_POST;
        $i = 1;
        while ($i < 11) {
            $a = f($x);
            $i++;
        }
            ".AssertVariable("a").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 5, 8, 3, 8 }));



        readonly static TestCase TaintAnalysisConditionalAssign_CASE =
        @" $a = $_POST;
        $b = $a;
        $x = ($_POST) ? $a : $b;
            ".AssertVariable("x").HasTaintStatus(new TaintStatus(true, true, new List<int>() { 2, 4, 4 }, new List<int>() { 2, 3, 4, 4 }));



        [TestMethod]
        public void TaintAnalysisBasic()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisBasic_CASE, false);
        }
      
        [TestMethod]
        public void TaintAnalysisArraysAliases2()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisArraysAliases2_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisAliases()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisAliases_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisAliases2()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisAliases2_CASE, false);
        }


        [TestMethod]
        public void TaintAnalysisNullValue()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisNullValue_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisMultipleNullFlows()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisMultipleNullFlows_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisUndefinedValue()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisUndefinedValue_CASE, false);
        }

        
        [TestMethod]
        public void TaintAnalysisArraysAliasesWeakUpdates()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisArraysAliasesWeakUpdates_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisFunctions()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisFunctions_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisFunctionsTwoArguments()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisFunctionsTwoArguments_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisBinaryOperation()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisBinaryOperation_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisParameterByAlias()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisParameterByAlias_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisFunctionTaintedParams()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisFunctionTaintedParams_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisFunctionNullParams()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisFunctionNullParams_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisSanitizers()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisSanitizers_CASE, false);
        }

        [TestMethod]
        public void TaintAnalysisWeakSanitizers()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisWeakSanitizers_CASE, false);
        }

         [TestMethod]
        public void TaintAnalysisSimpleCycle()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisSimpleCycle_CASE, false);
        }

         [TestMethod]
         public void TaintAnalysisCycle()
         {
             AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisCycle_CASE, false);
         }

         [TestMethod]
         public void TaintAnalysisFunctionCallInCycle()
         {
             AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisFunctionCallInCycle_CASE, false);
         }

         [TestMethod]
         public void TaintAnalysisFunctionCallInCycle2()
         {
             AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisFunctionCallInCycle2_CASE, false);
         }

        [TestMethod]
         public void TaintAnalysisConditionalAssign()
         {
             AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(TaintAnalysisConditionalAssign_CASE, false);
         }
        
        
        
    }

    /// <summary>
    /// Auxiliary class used for the taint status checks
    /// </summary>
    class TaintStatus
    {
        public Taint.Taint tainted;
        public TaintPriority priority;
        public List<List<int>>  lines;

        public TaintStatus(bool tainted, bool highPriority, params List<int>[] lines)
        {
            this.tainted = new Taint.Taint(tainted);
            this.priority = new TaintPriority(highPriority);
            this.lines = new List<List<int>>(lines);
        }

        public TaintStatus(Taint.Taint tainted, TaintPriority priority, params List<int>[] lines)
        {
            this.tainted = tainted;
            this.priority = priority;
            this.lines = new List<List<int>>(lines);
        }

        public TaintStatus(bool tainted, TaintPriority priority, params List<int>[] lines)
        {
            this.tainted = new Taint.Taint(tainted);
            this.priority = priority;
            this.lines = new List<List<int>>(lines);
        }

        public TaintStatus(TaintInfo taintInfo)
        {
            this.tainted = taintInfo.taint;
            this.priority = taintInfo.priority;

            lines = getLines(taintInfo);           
        }

        private List<List<int>> getLines(TaintInfo info)
        {
            List<List<int>> result = new List<List<int>>();
            if (info.point == null || info.point.Partial == null || (info.taint.allFalse() && !info.nullValue)) return result;
            int firstLine = info.point.Partial.Position.FirstLine;
            foreach (TaintFlow flowPair in info.possibleTaintFlows)
            {
                var flow = flowPair.flow;
                List<List<int>> flows = getLines(flow);
                foreach (List<int> oneFlow in flows)
                {
                    oneFlow.Add(firstLine);
                }
                result.AddRange(flows);
            }
            if (result.Count == 0) result.Add(new List<int>() { firstLine });
            return result;
        }

        public TaintStatus(TaintInfo taintInfo, Analysis.FlagType flag)
        {
            this.tainted = taintInfo.taint;
            this.priority = taintInfo.priority;

            lines = getLines(taintInfo, flag);
        }

        private List<List<int>> getLines(TaintInfo info, Analysis.FlagType flag)
        {
            List<List<int>> result = new List<List<int>>();
            if ((!info.taint.get(flag) && !info.nullValue) || info.point == null || info.point.Partial == null) return result;
            int firstLine = info.point.Partial.Position.FirstLine;
            if (info.possibleTaintFlows.Count == 0) result.Add(new List<int>() { firstLine });
            foreach (TaintFlow flowPair in info.possibleTaintFlows)
            {
                var flow = flowPair.flow;
                if (!flow.taint.get(flag)) continue;
                List<List<int>> flows = getLines(flow);
                foreach (List<int> oneFlow in flows)
                {
                    oneFlow.Add(firstLine);
                }
                result.AddRange(flows);
            }
            if (result.Count == 0) result.Add(new List<int>() { firstLine });
            return result;
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.AppendLine("Tainted: " + this.tainted);
            result.AppendLine("Priority: " + this.priority);
            foreach (List<int> flow in lines)
            {
                var flowString = new StringBuilder();
                flowString.Append("Flow: ");
                foreach (int i in flow)
                {
                    flowString.Append(i + " ");
                }
                result.AppendLine(flowString.ToString());
            }

            return result.ToString();
        }

        public string ToString(Analysis.FlagType flag)
        {
            var result = new StringBuilder();

            result.AppendLine("Tainted: " + this.tainted.get(flag));
            result.AppendLine("Priority: " + this.priority.get(flag));
            foreach (List<int> flow in lines)
            {
                var flowString = new StringBuilder();
                flowString.Append("Flow: ");
                foreach (int i in flow)
                {
                    flowString.Append(i + " ");
                }
                result.AppendLine(flowString.ToString());
            }

            return result.ToString();
        }

        public bool EqualTo(TaintStatus other)
        {
            if (!this.tainted.equalTo(other.tainted)) return false;
            if (!this.priority.equalTo(other.priority)) return false;
            if (!checkFlowEquality(other.lines)) return false;

            return true;
        }

        public bool EqualTo(TaintStatus other,Analysis.FlagType flag)
        {
            if (this.tainted.get(flag) != other.tainted.get(flag)) return false;
            if (this.priority.get(flag) != other.priority.get(flag)) return false;
            if (!checkFlowEquality(other.lines)) return false;

            return true;
        }

        private bool checkFlowEquality(List<List<int>>  lines)
        {
            if (this.lines.Count != lines.Count) return false;
            foreach (List<int> flow in lines)
            {
                if (!checkFlowContained(flow)) return false;
            }
            return true;
        }

        private bool checkFlowContained(List<int> flow)
        {
            foreach (List<int> thisFlow in this.lines)
            {
                if (thisFlow.SequenceEqual(flow)) return true;
            }

            return false;
        }
    }
}