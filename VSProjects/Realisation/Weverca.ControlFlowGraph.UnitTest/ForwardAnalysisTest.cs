﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.ControlFlowGraph.UnitTest
{
    [TestClass]
    public class ForwardAnalysisTest
    {

        readonly static string SingleBlock_CODE = @"
$str1='str1';
$str2='str2';
";

        readonly static string ParallelBlock_CODE = @"
$str='f1';
if($unknown){
    $str='f1a';
}else{
    $str='f1b';
}
";

        readonly static string SwitchBlock_CODE = @"
$str='0';
switch($unknown){
    case '1':
        $str='1';
        break;
    case '2':
        $str='2';
        break;
    case '3':
        $str='3';
        break;
    default:
        $str='default';
        break;
}

";

        readonly static string AssumptionPrunning_CODE = SwitchBlock_CODE + @"
$str2='before';
switch($str){
    case '1':
        $str2='1';
        break;
    case '2':
        $str2='2';
        break;
    case '4':
        $str2='cannot reach';
        break;
    default:
        $str2='default';
        break;
}
";

        readonly static string AssumptionToValue_CODE = @"
$str='before';
switch($str2){
    case '1':    
    case '2':
    case '3':
        $str=$str2;
        break;   
}
";

        readonly static string DynamicCall_CODE = @"
$globVar='init';

function m1(){
    global $globVar;
    $globVar='m1 call';
}

function m2(){
    global $globVar;
    $globVar='m2 call';
}

function m3(){
    global $globVar;
    $globVar='m3 call';
}

switch($str){
    case 'm1':
    case 'm2':
        $str=$str;
        break;
    default:
        $str='m3';
        break;
}

$str();

";

        [TestMethod]
        public void SingleBlockAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(SingleBlock_CODE);

            foreach (var variable in vars)
            {
                var test = CFGTestUtils.TestValues(variable.Name.Value,vars, variable.Name.Value);
                Assert.IsTrue(test, "Single value variable");
            }
        }

        [TestMethod]
        public void ParallelMergeAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(ParallelBlock_CODE);
            var test = CFGTestUtils.TestValues("str",vars, "f1a", "f1b");
            Assert.IsTrue(test, "Merged value variable from if blocks");
        }


        [TestMethod]
        public void SwitchMergeAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(SwitchBlock_CODE);
            var test = CFGTestUtils.TestValues("str",vars, "1", "2", "3", "default");
            Assert.IsTrue(test, "Merged value variable from switch blocks");
        }

        [TestMethod]
        public void AssumptionPrunningAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(AssumptionPrunning_CODE);
            var test = CFGTestUtils.TestValues("str2",vars, "1", "2", "default");
            Assert.IsTrue(test, "Merged value variable from switch blocks with assumption prunning");
        }

        [TestMethod]
        public void AssumptionToValueAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(AssumptionToValue_CODE);
            var test = CFGTestUtils.TestValues("str", vars, "before","1", "2", "3");
            Assert.IsTrue(test, "Value based on assumption condition resolving");
        }

        [TestMethod]
        public void DynamicCallAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(DynamicCall_CODE);
            var test = CFGTestUtils.TestValues("globVar", vars, "m1 call","m2 call","m3 call");
            Assert.IsTrue(test, "Global value based on dynamic call");
        }
    }
}
