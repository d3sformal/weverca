using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    [TestClass]
    public class NextPhaseAnalysisTest
    {
        readonly static TestCase SimpleVariableTracking_CASE = @"
$t='transitive target';
$a='source';

$b=$a;
$t=$b;
".AssertVariable("a").IsPropagatedTo("b", "t");

        [TestMethod]
        public void SimpleVariableTracking()
        {
            AnalysisTestUtils.RunInfoLevelCase(SimpleVariableTracking_CASE);
        }
    }
}
