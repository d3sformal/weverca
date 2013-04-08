using System;
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

        readonly static string AssumptionTest_CODE = SwitchBlock_CODE + @"
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
        public void AssumptionUsageAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(AssumptionTest_CODE);
            var test = CFGTestUtils.TestValues("str2",vars, "1", "2", "default");
            Assert.IsTrue(test, "Merged value variable from switch blocks with assumption usage");
        }
    }
}
