using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.ControlFlowGraph.UnitTest
{
    [TestClass]
    public class ForwardAnalysisTest
    {

        readonly string SingleBlock_CODE = @"
$str1='str1';
$str2='str2';
";

        readonly string ParallelBlock_CODE = @"
$str='f1';
if($unknown){
    $str='f1a';
}else{
    $str='f1b';
}
";

        [TestMethod]
        public void SingleBlockAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(SingleBlock_CODE);

            foreach (var variable in vars)
            {
                var test = CFGTestUtils.TestValues(variable, variable.Name);
                Assert.IsTrue(test, "Single value variable");
            }
        }

        [TestMethod]
        public void ParallelBlockAnalysis()
        {
            var vars = CFGTestUtils.GetEndPointInfo(ParallelBlock_CODE);
            var test = CFGTestUtils.TestValues(vars[0], "f1a", "f1b");
            Assert.IsTrue(test, "Merged value variable");
        }
    }
}
