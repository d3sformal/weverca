using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    [TestClass]
    public class NextPhaseAnalysisTest
    {
        #region Variable tracking
        readonly static TestCase SimpleVariableTracking_CASE = @"
$t='transitive target';
$a='source';

$b[1]=$a;
$d=$b[1];
$t=$d;
".AssertVariable("a").IsPropagatedTo("b", "d", "t");
        #endregion Variable tracking

        #region Taint analysis

        readonly static TestCase SimpleTaintAnalysisArraysAliases_CASE = @"
$c[1] = &$b;
$b = $_POST;
$a = $c[1];
$y = $c[2];
$d[1][2] = $c[1];
$x = $d[1][2];
$z = $x;
$z = 1;
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("x").HasTaintStatus(true)
 .AssertVariable("y").HasTaintStatus(false)
 .AssertVariable("z").HasTaintStatus(false);

        readonly static TestCase SimpleTaintAnalysisArraysAliases2_CASE = @"
$b[1] = 1;
$a = &$b[1];
$b[1] = $_POST;
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true);

        // Tests weak updates
        // weak because of indirect variable accesses
        // weak because of aliasing
        readonly static TestCase SimpleTaintAnalysisArraysAliasesWeakUpdates_CASE = @"
$any = $_POST;
$x = $_POST;
$y = 'str';
$v = 'str';
$w = 'str';
$p = $x;
$q = $x;
$r = 'str';
$s = 'str';
if ($any) {
    $v1 = 'x';
    $v2 = 'v';
    $p = &$q;
    $r = &$s;
    
} else {
    $v1 = 'y';
    $v2 = 'w';
}
// updates $x, but the update is weak and $x should stay tainted
$$v1 = 'str';

// updates $v, the update is weak, but taint status is propagated
$$v2 = $x;

// updates $p, but the update is weak and $p should stay tainted
$q = 'str';

// updates $r, the update is weak, but taint status is propagated
$s = $x;

".AssertVariable("_POST").HasTaintStatus(true)
.AssertVariable("x").HasTaintStatus(true)
.AssertVariable("v").HasTaintStatus(true)
.AssertVariable("w").HasTaintStatus(true)
.AssertVariable("p").HasTaintStatus(true)
.AssertVariable("q").HasTaintStatus(false)
.AssertVariable("s").HasTaintStatus(true)
.AssertVariable("r").HasTaintStatus(true);

        readonly static TestCase SimpleTaintAnalysisFunctions_CASE = @"
function f($arg) {
    return $arg;
} 

$x = $_POST;
$y = 'str';
$a = f($x);
$b = f($y);
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("b").HasTaintStatus(false);

        readonly static TestCase SimpleTaintAnalysisFunctionsTwoArguments_CASE = @"
function f($arg1, $arg2) {
    return $arg1 + $arg2;
} 

$x = $_POST;
$y = '1';
$a = f($x, $y);
$b = f($y, $x);
$c = f($x, $x);
$d = f($y, $y);
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("b").HasTaintStatus(true)
 .AssertVariable("c").HasTaintStatus(true)
 .AssertVariable("d").HasTaintStatus(false);

        readonly static TestCase SimpleTaintAnalysisFunctionsTaintInsideReturnValue_CASE = @"
function f($arg) {
    return $arg + $_POST;
}

$x = $_POST;
$y = 'str';
$a = f($x);
$b = f($y);
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("b").HasTaintStatus(true);

        readonly static TestCase SimpleTaintAnalysisFunctionsTaintInsideAlias_CASE = @"
function f_taint(&$arg) {
    $arg = $_POST;
}

function f_sanitize(&$arg) {
    $arg = 'sanit';
}

$x = $_POST;
$y = 'str';
$a = f_taint($x);
$b = f_taint($y);
$c = f_sanitize($x);
$d = f_sanitize($y);
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("b").HasTaintStatus(true)
 .AssertVariable("c").HasTaintStatus(false)
 .AssertVariable("d").HasTaintStatus(false);

        readonly static TestCase SimpleTaintAnalysisNativeFunctions_CASE = @"
$x = $_POST;
$y = 'str';
$a = strtolower($x);
$b = strtolower($y);
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("b").HasTaintStatus(false)
 .Analysis(Analyses.WevercaAnalysis);

        readonly static TestCase SimpleTaintAnalysisNativeFunctionsTwoArguments_CASE = @"
$x = $_POST;
$y = '1';
$a = concat($x, $y);
$b = concat($y, $x);
$c = concat($x, $x);
$d = concat($y, $y);
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("b").HasTaintStatus(true)
 .AssertVariable("c").HasTaintStatus(true)
 .AssertVariable("d").HasTaintStatus(false)
 .Analysis(Analyses.WevercaAnalysis);

        readonly static TestCase SimpleTaintAnalysisNativeSanitizer_CASE = @"
$x = $_POST;
$a = strlen($b);
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(false)
 .Analysis(Analyses.WevercaAnalysis);

        readonly static TestCase SimpleTaintAnalysisExpressions_CASE = @"
$x = $POST;
$y = 1;
$a = $x + $y;
$b = $y + $x;
$c = $x + $x;
$d = $y + $y;
".AssertVariable("POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("b").HasTaintStatus(true)
 .AssertVariable("c").HasTaintStatus(true)
 .AssertVariable("d").HasTaintStatus(false)
 .Analysis(Analyses.WevercaAnalysis);

        #endregion Taint analysis

        [TestMethod]
        public void SimpleVariableTracking()
        {
            AnalysisTestUtils.RunInfoLevelBackwardPropagationCase(SimpleVariableTracking_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisArraysAliases()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisArraysAliases_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisArraysAliases2()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisArraysAliases2_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisArraysAliasesWeakUpdates()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisArraysAliasesWeakUpdates_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisFunctions()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisFunctions_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisFunctionsTwoArguments()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisFunctionsTwoArguments_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisFunctionsTaintInsideReturnValue()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisFunctionsTaintInsideReturnValue_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisFunctionsTaintInsideAlias()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisFunctionsTaintInsideAlias_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisNativeFunctions()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisNativeFunctions_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisNativeFunctionsTwoArguments()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisNativeFunctionsTwoArguments_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisNativeSanitizer()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisNativeSanitizer_CASE);
        }

        [TestMethod]
        public void SimpleTaintAnalysisExpressions()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisExpressions_CASE);
        }
    }
}
