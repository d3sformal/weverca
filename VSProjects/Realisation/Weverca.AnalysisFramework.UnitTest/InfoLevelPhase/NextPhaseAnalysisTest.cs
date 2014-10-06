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

        readonly static TestCase SimpleTaintAnalysisAliases_CASE = @"
$x = $_POST;
$y = &$x;
".AssertVariable("_POST").HasTaintStatus(true)
.AssertVariable("y").HasTaintStatus(true);

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
    global $_POST;
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
    global $_POST;
    $arg = $_POST;
}

function f_sanitize(&$arg) {
    $arg = 'sanit';
}

$x = $_POST;
$y = 'str';
f_taint($x); $a = $x;
f_taint($y); $b = $y;
f_sanitize($x); $c = $x;
f_sanitize($y); $d = $y;
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
 .Analysis(Analyses.WevercaAnalysisTest);

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
 .Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase SimpleTaintAnalysisNativeSanitizer_CASE = @"
$x = $_POST;
$a = strlen($b);
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(false)
 .Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase SimpleTaintAnalysisExpressions_CASE = @"
$x = $_POST;
$y = 1;
$a = $x + $y;
$b = $y + $x;
$c = $x + $x;
$d = $y + $y;
".AssertVariable("_POST").HasTaintStatus(true)
 .AssertVariable("a").HasTaintStatus(true)
 .AssertVariable("b").HasTaintStatus(true)
 .AssertVariable("c").HasTaintStatus(true)
 .AssertVariable("d").HasTaintStatus(false)
 .Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase SimpleTaintAnalysisWriteCharacter_CASE = @"
$x = $_POST['id'][0];
$str = ""test"";
$str[0] = $x;
".AssertVariable("_POST").HasTaintStatus(true)
            .AssertVariable("x").HasTaintStatus(true)
            .AssertVariable("str").HasTaintStatus(true)
            .Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase MayAliasTest_CASE = @"
$alias = &$a[$_POST[1]];
".AssertVariable("").Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase IntoMethod_CASE = @"
class human {
public $gender;

public function __construct($gender)
{
$this->gender = $gender;

echo self::get_gender();
}

public function get_gender()
{
return $this->gender;
}
}

class person extends human {
public $name;

public function set_name($name)
{
$this->name = $name;
}
}

$Johnny = new person('male');
echo $Johnny->get_gender();

$Mary = new person('female');
$Mary->set_name('Mary');

".AssertVariable("Johnny").HasTaintStatus(false)
            .Analysis(Analyses.WevercaAnalysisTest);


        readonly static TestCase AliasCycle_CASE = @"
$x[$_POST[1]][$_POST[1]] = & $x[$_POST[1]];
$x[1][1] = 1;
$t = $x[1];

".AssertVariable("t")
 .Analysis(Analyses.WevercaAnalysisTest);
 
        #endregion Taint analysis

        [TestMethod]
        public void AliasCycle()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(AliasCycle_CASE, false);
        }

        [TestMethod]
        public void IntoMethod()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(IntoMethod_CASE);
        }

        [TestMethod]
        public void MayAliasTest()
        {
            AnalysisTestUtils.RunInfoLevelBackwardPropagationCase(MayAliasTest_CASE);
        }

        // TODO test: get this test working
        //[TestMethod]
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
        public void SimpleTaintAnalysisAliases()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisAliases_CASE);
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

        [TestMethod]
        public void SimpleTaintAnalysisWriteCharacter()
        {
            AnalysisTestUtils.RunInfoLevelTaintAnalysisCase(SimpleTaintAnalysisWriteCharacter_CASE);
        }
    }
}