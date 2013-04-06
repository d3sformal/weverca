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

        [TestMethod]
        public void SingleBlockAnalysis()
        {
            var cfg=CFGTestUtils.CreateCFG(SingleBlock_CODE);
            var analysis = new StringAnalysis(cfg);
            analysis.Analyse();
            var vars = analysis.RootEndPoint.OutSet.CollectedInfo;

            foreach (var variable in vars)
            {
                var test = CFGTestUtils.TestValues(variable, variable.Name);
                Assert.IsTrue(test, "Single value variable");
            }
        }
    }
}
