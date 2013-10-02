using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.Analysis.UnitTest
{
    [TestClass]
    public class Deprecated_ForwardAnalysisTest
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

        readonly static string AssumptionPrunning_CODE = 
            SwitchBlock_CODE + @"
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

   /*     readonly static string DynamicCall_CODE = @"
$globVar='init';

function m1(){
    global $globVar;
    $globVar='call1';
}

function m2(){
    global $globVar;
    $globVar='call2';
}

function m3(){
    global $globVar;
    $globVar='call3';
}

switch($str){
    case 'm1':
    case 'm2':
        $str=$str;
        break;
    default:
        $str='m2';
        break;
}

$str();
";*/

        [TestMethod]
        public void SingleBlockAnalysis()
        {
            var vars = AnalysisTestUtils.GetEndPointInfo(SingleBlock_CODE);

            foreach (var variable in vars)
            {
                var test = AnalysisTestUtils.TestValues(variable.Name.Value,vars, variable.Name.Value);
                Assert.IsTrue(test, "Single value variable");
            }
        }

        [TestMethod]
        public void ParallelMergeAnalysis()
        {
            var vars = AnalysisTestUtils.GetEndPointInfo(ParallelBlock_CODE);
            var test = AnalysisTestUtils.TestValues("str",vars, "f1a", "f1b");
            Assert.IsTrue(test, "Merged value variable from if blocks");
        }


        [TestMethod]
        public void SwitchMergeAnalysis()
        {
            var vars = AnalysisTestUtils.GetEndPointInfo(SwitchBlock_CODE);
            var test = AnalysisTestUtils.TestValues("str",vars, "1", "2", "3", "default");
            Assert.IsTrue(test, "Merged value variable from switch blocks");
        }

        [TestMethod]
        public void AssumptionPrunningAnalysis()
        {
            var vars = AnalysisTestUtils.GetEndPointInfo(AssumptionPrunning_CODE);
            var test = AnalysisTestUtils.TestValues("str2",vars, "1", "2", "default");
            Assert.IsTrue(test, "Merged value variable from switch blocks with assumption prunning");
        }

        [TestMethod]
        public void AssumptionToValueAnalysis()
        {
            var vars = AnalysisTestUtils.GetEndPointInfo(AssumptionToValue_CODE);
            var test = AnalysisTestUtils.TestValues("str", vars, "before","1", "2", "3");
            Assert.IsTrue(test, "Value based on assumption condition resolving");
        }

       /* [TestMethod]
        public void DynamicCallAnalysis()
        {
            var vars = AnalysisTestUtils.GetEndPointInfo(DynamicCall_CODE);
            var test = AnalysisTestUtils.TestValues("globVar", vars, "call1","call2");
            Assert.IsTrue(test, "Global value based on dynamic call");
        }*/
    }
}
