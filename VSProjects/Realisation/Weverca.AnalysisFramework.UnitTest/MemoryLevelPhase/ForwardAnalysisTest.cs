/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan

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

 //#define ENABLE_GRAPH_VISUALISATION

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// NOTE: Variable unknown is set by default as non-deterministic (AnyValue)
    /// </summary>
    [TestClass]
    public class ForwardAnalysisTest
    {
        readonly static TestCase SimpleAssign_CASE = @"
$a = 's';
$x = $a;
".AssertVariable("a").HasValues("s")
 .AssertVariable("x").HasValues("s");

        readonly static TestCase SelfConstant_CASE = @"
class A {
    const C = ""const"";
    function f () { return self::C; }
}
$a = new A();
$ret = $a->f();
".AssertVariable("ret").HasValues("const");

        readonly static TestCase BranchMerge_CASE = @"
$str='f1';
if($unknown){
    $str='f1a';
}else{
    $str='f1b';
}
".AssertVariable("str").HasValues("f1a", "f1b");

        readonly static TestCase BranchMerge2_CASE = @"
$str='f1a';
if($unknown){
    $str='f1b';
}
".AssertVariable("str").HasValues("f1a", "f1b");

        readonly static TestCase BranchMergeWithUndefined_CASE = @"
if($unknown){
    $str='f1a';
}
".AssertVariable("str").HasUndefinedAndValues("f1a");

        readonly static TestCase UnaryNegation_CASE = @"
$result=42;
$result=-$result;
".AssertVariable("result").HasValues(-42);

        readonly static TestCase NativeCallProcessing_CASE = @"
$call_result=strtolower('TEST');
".AssertVariable("call_result").HasValues("test");

        // TODO test: get this test working in weverca analysis. Implement some native function that takes 2 arguments (concat is not native function)
        readonly static TestCase NativeCallProcessing2Arguments_CASE = @"
$call_result=concat('A','B');
".AssertVariable("call_result").HasValues("AB")
 .Analysis(Analyses.SimpleAnalysis);

        // TODO test: get this test working in weverca analysis. Implement some native function that takes 2 arguments (concat is not native function)
        readonly static TestCase NativeCallProcessingNestedCalls_CASE = @"
$call_result=concat(strtolower('Ab'),strtoupper('Cd'));
".AssertVariable("call_result").HasValues("abCD")
 .Analysis(Analyses.SimpleAnalysis)
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        readonly static TestCase IndirectCall_CASE = @"
$call_name='strtolower';
$call_result=$call_name('TEST');
".AssertVariable("call_result").HasValues("test");

        readonly static TestCase BranchedIndirectCall_CASE = @"
if($unknown){
    $call_name='strtolower';
}else{
    $call_name='strtoupper';
}
$call_result=$call_name('TEst');
".AssertVariable("call_result").HasValues("TEST", "test");


        readonly static TestCase SingleBranchedIndirectCall_CASE = @"
$call_name='strtoupper';
if($unknown){
    $call_name='strtolower';
}

$call_result=$call_name('TEst');
".AssertVariable("call_result").HasValues("TEST", "test");

        readonly static TestCase MustAliasAssign_CASE = @"
$VarA='ValueA';
$VarB='ValueB';
$VarA=&$VarB;
".AssertVariable("VarA").HasValues("ValueB");

        /// <summary>
        /// This is virtual reference memory model specific test
        /// </summary>
        readonly static TestCase MayAliasAssignVirtualRefMM_CASE = @"
$VarA='ValueA';
$VarB='ValueB';
$VarC='ValueC';
if($unknown){
    $VarA=&$VarB;
}else{
    $VarA=&$VarC;
}
$VarA='Assigned';
".AssertVariable("VarA").HasValues("ValueB", "ValueC", "Assigned")
 .AssertVariable("VarB").HasValues("ValueB", "Assigned")
 .AssertVariable("VarC").HasValues("ValueC", "Assigned").
 MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        /// <summary>
        /// This is copy memory model specific test
        /// </summary>
        readonly static TestCase MayAliasAssignCopyMM_CASE = @"
$VarA='ValueA';
$VarB='ValueB';
$VarC='ValueC';
if($unknown){
    $VarA=&$VarB;
}else{
    $VarA=&$VarC;
}
$VarA='Assigned';
".AssertVariable("VarA").HasValues("Assigned")
 .AssertVariable("VarB").HasValues("ValueB", "Assigned")
 .AssertVariable("VarC").HasValues("ValueC", "Assigned").
 MemoryModel(MemoryModels.MemoryModels.ModularCopyMM);

        readonly static TestCase EqualsAssumption_CASE = @"
$Var='init';
if($unknown=='PossibilityA'){
    $Var=$unknown;
}
".AssertVariable("Var").HasValues("init", "PossibilityA");

        // TODO test: passes only simple analysis, weverca analysis should be fixed to also pass this test
        readonly static TestCase DynamicEqualsAssumption_CASE = @"
if($unknown){
    $Var='VarA';
}else{
    $Var='VarB';
}

if($unknown){
    $Value='Value1';
}else{
    $Value='Value2';
}

if($$Var==$Value){
    $OutputA=$VarA;
    $OutputB=$VarB;
}
".AssertVariable("OutputA").HasUndefinedAndValues("Value1", "Value2")
 .AssertVariable("OutputB").HasUndefinedAndValues("Value1", "Value2")
 .Analysis(Analyses.SimpleAnalysisTest)
 .SetNonDeterministic("VarA", "VarB");


        readonly static TestCase CallEqualsAssumption_CASE = @"
if($unknown==strtolower(""TestValue"")){
    $Output=$unknown;
}
".AssertVariable("Output").HasUndefinedOrValues("testvalue");
 //.Analysis(Analyses.WevercaAnalysisTest);

        // TODO test: get this test working also on WevercaAnalysis
        readonly static TestCase ReverseCallEqualsAssumption_CASE = @"
if(abs($unknown)==5){
    $Output=$unknown;
}

".AssertVariable("Output").HasUndefinedOrValues(5, -5)
 .Analysis(Analyses.SimpleAnalysisTest);


        readonly static TestCase IndirectVarAssign_CASE = @"
$Indirect='x';
$ID='Indirect';
$$ID='Indirectly assigned';
".AssertVariable("Indirect").HasValues("Indirectly assigned");


        readonly static TestCase MergedReturnValue_CASE = @"
function testFunction(){
    if($unknown){
        return 'ValueA';
    }else{
        return 'ValueB';
    }
}

$CallResult=testFunction();
".AssertVariable("CallResult").HasValues("ValueA", "ValueB");

        readonly static TestCase MergedFunctionDeclarations_CASE = @"
if($unknown){
    function testFunction(){
        return 'ValueA';
    }
}else{
    function testFunction(){
        return 'ValueB';
    }
}

$CallResult=testFunction();
".AssertVariable("CallResult").HasValues("ValueA", "ValueB");


        readonly static TestCase StaticFieldUse_CASE = @"
class Obj{
    static $a;
}

Obj::$a='abc';

$result=Obj::$a;
".AssertVariable("result").HasValues("abc");

        readonly static TestCase IndirectStaticFieldUse_CASE = @"
class Obj{
    static $a;
}

$name='Obj';
$name::$a='abc';

$result=$name::$a;
".AssertVariable("result").HasValues("abc");


        readonly static TestCase ObjectFieldMerge_CASE = @"
class Obj{
    var $a;
}

$obj=new Obj();
if($unknown){
    $obj->a='ValueA';
}else{
    $obj->a='ValueB';
}

$FieldValue=$obj->a;
".AssertVariable("FieldValue").HasValues("ValueA", "ValueB");


        readonly static TestCase StringIndex_CASE = @"
$string='test';
$res1=$string[0];

if($unknown){
    $string[0]='b';
}else{  
    $string[0]='w';
}
"
            .AssertVariable("res1").HasValues("t")
            .AssertVariable("string").HasValues("best", "west")
            ;

        readonly static TestCase ArrayFieldMerge_CASE = @"
$arr[0]='init';
if($unknown){
    $arr[0]='ValueA';
}else{
    $arr[0]='ValueB';
}

$ArrayValue=$arr[0];
".AssertVariable("ArrayValue").HasValues("ValueA", "ValueB");


        readonly static TestCase ImplicitArrayFieldMerge_CASE = @"
if($unknown){
    $arr[0]='ValueA';
}else{
    $arr[0]='ValueB';
}

$ArrayValue=$arr[0];
".AssertVariable("ArrayValue").HasValues("ValueA", "ValueB");

        readonly static TestCase ArrayFieldUpdateMultipleArrays_CASE = @"
if($unknown){
    $arr[0]='ValueA';
}else{
    $arr[0]='ValueB';
}

$arr[0] = 'NewValue';

$ArrayValue=$arr[0];
".AssertVariable("ArrayValue").HasValues("NewValue");


        readonly static TestCase ObjectMethodCallMerge_CASE = @"
class Obj{
    var $a;

    function setter($value){
        $this->a=$value;
    }
}

$obj=new Obj();
if($unknown){
    $obj->setter('ValueA');
}else{
    $obj->setter('ValueB');
}

$FieldValue=$obj->a;
".AssertVariable("FieldValue").HasValues("ValueA", "ValueB");

        readonly static TestCase ObjectMultipleObjectsInVariableRead_CASE = @"
class Cl {
    var $field;
}

if ($unknown) {
    $obj = new Cl();
    $obj->field = 'value';
} else {
    $obj = new Cl();
    $obj->field = 'value';
}
$FieldValue = $obj->field;
".AssertVariable("FieldValue").HasValues("value")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        readonly static TestCase ObjectMultipleObjectsInVariableWrite_CASE = @"
class Cl {
    var $field;
}

if ($unknown) {
    $obj = new Cl();
    $obj->field = 'value';
} else {
    $obj = new Cl();
    $obj->field = 'value';
}
// $obj->field can be strongly updated because the object stored in a variable $obj is not stored in anay other variable
// however, in general case the update must be weak (see ObjectMultipleObjectsInVariableMultipleVariablesWeakWrite_CASE)
$obj->field = 'newValue';
$FieldValue = $obj->field;
"
            //virtual reference model doesnt support write read semantics
            .AssertVariable("FieldValue").HasValues("value", "newValue")
            // more precise implementation would perform strong update:
            //.AssertVariable("FieldValue").HasValues("newValue")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
            ;

        readonly static TestCase ObjectMultipleObjectsInVariableMultipleVariablesWeakWrite_CASE = @"
class Cl {
    var $field;
}

$a = new Cl();
$a->field = 'value';
$b = new Cl();
$b->field = 'value';
if ($unknown) {
    $obj = $a;
} else {
    $obj = $b;
}
// $a->field and $b->field must be weakly updated
$obj->field = 'newValue';
$FieldValueObj = $obj->field;
$FieldValueA = $a->field;
$FieldValueB = $b->field;
// $a->field must be strongly updated, $obj->field should be weakly updated
$a->field = 'newValue2';
$FieldValueObj2 = $obj->field;
$FieldValueA2 = $a->field;
$FieldValueB2 = $b->field;
"
            .AssertVariable("FieldValueObj").HasValues("value", "newValue")
            .AssertVariable("FieldValueA").HasValues("value", "newValue")
            .AssertVariable("FieldValueB").HasValues("value", "newValue")
            .AssertVariable("FieldValueObj2").HasValues("value", "newValue", "newValue2")
            .AssertVariable("FieldValueA2").HasValues("newValue2")
            .AssertVariable("FieldValueB2").HasValues("value", "newValue")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
            ;

        readonly static TestCase ObjectMultipleObjectsInVariableDifferentClassRead_CASE = @"
class ClA {
    var $field;
}
class ClB {
    var $field;
}

if ($unknown) {
    $obj = new ClA();
    $obj->field = 'value1';
} else {
    $obj = new ClB();
    $obj->field = 'value2';
}
$FieldValue = $obj->field;
".AssertVariable("FieldValue").HasValues("value1", "value2")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        readonly static TestCase ObjectMethodObjectSensitivity_CASE = @"
class Cl {
    var $field;
    function f($arg) {$this->field = $arg;}
}
if ($unknown) {
    $obj = new Cl();
    $obj->field = 'originalValue';
}
else {
    $obj = new Cl();
    $obj->field = 'originalValue';
}
// it should call Cl::f() two times - each time for single instance of the class Cl being as $this. Both calls strongly update the value of the field to 'newValue'
$obj->f('newValue');
$FieldValue = $obj->field;
".AssertVariable("FieldValue").HasValues("newValue")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        readonly static TestCase ObjectMethodObjectSensitivityMultipleVariables_CASE = @"
class Cl {
    var $field;
    function f($arg) {$this->field = $arg;}
}
$a = new Cl();
$a->field = 'valueA';
$b = new Cl();
$b->field = 'valueB';
if ($unknown) {
    $obj = $a;
}
else {
    $obj = $b;
}
// it should call Cl::f() two times - each time for single instance of the class Cl being as $this. Both calls strongly update the value of the field to 'newValue'
$obj->f('newValue');
$FieldValueObj = $obj->field;
$FieldValueA = $a->field;
$FieldValueB = $b->field;
$a->f('newValue2');
$FieldValueObj2 = $obj->field;
$FieldValueA2 = $a->field;
$FieldValueB2 = $b->field;
"
            .AssertVariable("FieldValueObj").HasValues("newValue")
            .AssertVariable("FieldValueA").HasValues("newValue")
            .AssertVariable("FieldValueB").HasValues("newValue")
            .AssertVariable("FieldValueObj2").HasValues("newValue", "newValue2")
            .AssertVariable("FieldValueA2").HasValues("newValue2")
            .AssertVariable("FieldValueB2").HasValues("newValue")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        readonly static TestCase ObjectMethodObjectSensitivityDifferentClass_CASE = @"
class ClA {
    var $field = 'valueFromClA';
    function f($arg) {$this->field = $arg;}
}
class ClB {
    var $field = 'valueFromClA';
    function f($arg) {$this->field = $arg;}
}
if ($unknown) {
    $obj = new ClA();
    $obj->field = 'originalValueA';
}
else {
    $obj = new ClB();
    $obj->field = 'originalValueB';
}
// it should call ClA::f() with $this being the instance of ClA() and ClB::f() with $this being the instance of ClB() => it should perform a strong update
$obj->f('newValue');
$FieldValue = $obj->field;
".AssertVariable("FieldValue").HasValues("newValue")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        readonly static TestCase SimpleInclude_CASE = @"
include 'file_a.php';
".AssertVariable("IncludedVar").HasValues("ValueA")
 .Include("file_a.php", @"
    $IncludedVar='ValueA';
");

        readonly static TestCase DynamicIncludeMerge_CASE = @"
if($unknown){
    $file='file_a.php';
}else{
    $file='file_b.php';
}

include $file;
".AssertVariable("IncludedVar").HasValues("ValueA", "ValueB")
 .Include("file_a.php", @"
    $IncludedVar='ValueA';
")
 .Include("file_b.php", @"
    $IncludedVar='ValueB';
");


        readonly static TestCase IncludeReturn_CASE = @"
$IncludeResult=(include 'test.php');

".AssertVariable("IncludeResult").HasValues("IncludedReturn")
 .Include("test.php", @"
    return 'IncludedReturn';
");

        readonly static TestCase XSSDirtyFlag_CASE = @"
$x=$_POST['dirty'];
$x=$x;
".AssertVariable("x").IsXSSDirty().Analysis(Analyses.WevercaAnalysisTest);


        readonly static TestCase XSSSanitized_CASE = @"
$x=$_POST['dirty'];
$x='sanitized';
".AssertVariable("x").IsXSSClean().Analysis(Analyses.SimpleAnalysisTest);

        readonly static TestCase XSSPossibleDirty_CASE = @"
$x=$_POST['dirty'];
if($unknown){
    $i = 1;
    $x='sanitized';
} else {
    $i = 2;
}
".AssertVariable("i").HasValues(1, 2).Analysis(Analyses.SimpleAnalysisTest);
//".AssertVariable("x").IsXSSDirty().Analysis(Analyses.SimpleAnalysisTest);


        readonly static TestCase ConstantDeclaring_CASE = @"
const test='Direct constant';

if($unknown){
    define('declared','constant1');
}else{
    define('declared','constant2');
}

$x=declared;
$y=test;

".AssertVariable("x").HasValues("constant1", "constant2")
 .AssertVariable("y").HasValues("Direct constant");

        readonly static TestCase BoolResolving_CASE = @"
if($unknown){
    $x=true;
}else{
    $x=false;
}
".AssertVariable("x").HasValues(true, false);

        readonly static TestCase ForeachIteration_CASE = @"
$arr[0]='val1';
$arr[1]='val2';
$arr[2]='val3';

$test='init';

foreach($arr as $value){
    if($unknown ==  $value){
        $test=$value;
    }
}
".AssertVariable("test").HasValues("init", "val1", "val2", "val3")
 .SimplifyLimit(4)
 ;
        [TestMethod]
        public void ForeachIteration()
        {
            AnalysisTestUtils.RunTestCase(ForeachIteration_CASE);
        }

        // This test fails because the analysis is not able to assume that the cycle 
        // must be evaluated at least once.
        readonly static TestCase ForeachIteration2_CASE = @"
$arr[0]='val1';
$arr[1]='val2';
$arr[2]='val3';

$test='init';
$b = 0;

foreach($arr as $value){
    $test=$value;
    $b = 1;
}
".AssertVariable("test").HasValues("val1", "val2", "val3")
 .AssertVariable("b").HasValues(1);
        [TestMethod]
        public void ForeachIteration2FailingTODO()
        {
            AnalysisTestUtils.RunTestCase(ForeachIteration2_CASE);
        }

        // This test fails because all values stored in indices of $arr are assigned
        // to $value in the beginning of the cycle. However, the cycle is evaluated
        // just once and just the first value is assigned.
        readonly static TestCase ForeachWithBreak_CASE = @"
$arr[1] = 1;
$arr[2] = 2;
foreach ($arr as $value) {
    $a = $value;
    break;    
}
".AssertVariable("a").HasValues(1);
        [TestMethod]
        public void ForeachWithBreakFailingTODO()
        {
            AnalysisTestUtils.RunTestCase(ForeachWithBreak_CASE);
        }


        readonly static TestCase NativeObjectUsage_CASE = @"
    $obj=new NativeType('TestValue');
    $value=$obj->GetValue();
".AssertVariable("value").HasValues("TestValue")
         .DeclareType(SimpleNativeType.CreateType());


        readonly static TestCase GlobalStatement_CASE = @"

function setGlobal(){
    global $a;
    $a='ValueA';    
}

function setLocal(){
    $a='LocalValueA';
}

setGlobal();
setLocal();

".AssertVariable("a").HasValues("ValueA");

        readonly static TestCase List_CASE = @"
$info = array('coffee', 'brown', 'caffeine');

// Assign all indices
list($drink, $color, $power) = $info;
// Assign first 2 indices
list($drink1, $color1) = $info;
// Assign first and third index
list($drink2, , $power2) = $info;
// Try to assign to non-existent index
list($drink3, $color3, $power3, $next) = $info;

// The list statement should return copy of assigned array
$info2 = (list($a) = $info);
$drink4 = $info2[0];
$color4 = $info2[1];
$power4 = $info2[2];
// It is copy, the following statement should not change the original array
$info2[0] = 'modified';
$drink5 = $info[0];
".AssertVariable("drink").HasValues("coffee")
 .AssertVariable("color").HasValues("brown")
 .AssertVariable("power").HasValues("caffeine")
 .AssertVariable("drink1").HasValues("coffee")
 .AssertVariable("color1").HasValues("brown")
 .AssertVariable("drink2").HasValues("coffee")
 .AssertVariable("power2").HasValues("caffeine")
 .AssertVariable("drink3").HasValues("coffee")
 .AssertVariable("color3").HasValues("brown")
 .AssertVariable("power3").HasValues("caffeine")
 .AssertVariable("next").HasUndefinedValue()
 .AssertVariable("drink4").HasValues("coffee")
 .AssertVariable("color4").HasValues("brown")
 .AssertVariable("power4").HasValues("caffeine")
 .AssertVariable("drink5").HasValues("coffee");
        [TestMethod]
        public void ListExpression()
        {
            AnalysisTestUtils.RunTestCase(List_CASE);
        }

        #region For cycles

        readonly static TestCase ForCycle_CASE = @"
for ($i = 0; $i <= 2; $i++) {
    $result = ($i == 0);
}
".AssertVariable("result").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN);
        [TestMethod]
        public void ForCycle()
        {
            AnalysisTestUtils.RunTestCase(ForCycle_CASE);
        }

        #endregion


        /// <summary>
        /// Tests for cases where the program points of a function are shared among different calls of such function.
        /// This is used to implement context-insensitivity of functions.
        /// 
        /// 
        /// Note that virtual reference memory model propagates also local data (not local variables, just data) 
        /// of caller function to called function and thus merges also local contexts. This is done to implement
        /// propagating of changes to local variables of caller that are aliased with arguments of called function.
        /// 
        /// TODO: test correct work with arrays and objects.
        /// Arrays, objects passed as parameters, indices of arrays, objects passed as parameters, objects in global / local context.
        /// 
        /// </summary>
        /// 
        #region Shared functions
        readonly static TestCase SharedFunction_CASE = @"
function sharedFn($arg){
    return $arg;
}

sharedFn(1);
$resultA=sharedFn(2);

"
 .AssertVariable("resultA").HasValues(1, 2)
 .ShareFunctionGraph("sharedFn")
 ;



        /// <summary>
        /// If a function is shared, global context and parameters passed by all callers are merged and the function
        /// works with this merged context. Consequently, this can lead to weak updates of global variables.
        /// </summary>
        #region Shared functions merging global context
        readonly static TestCase SharedFunctionGlobalVariable_CASE = @"
function sharedFn(){
    global $g;
    return $g;
}

$g = 1;
sharedFn();
$g = 2;
$result = sharedFn();

"
 .AssertVariable("result").HasValues(1, 2)
 .ShareFunctionGraph("sharedFn")
 ;

        readonly static TestCase SharedFunctionStrongUpdateGlobal_CASE = @"
function sharedFn($arg){
    return $arg;
}

$resultA = 'InitA';
$resultB = 'InitB';
$resultA=sharedFn('ValueA');
$resultB=sharedFn('ValueB');

"
            // NOTE: Shared graphs cannot distinct between global contexts in places where theire called
            // so the second sharedFn call in second iteration will merge these global contexts 
            // {resultA: 'InitA', resultB: 'InitB'} {resultA: 'ValueA','ValueB', resultB: 'ValueA','ValueB'}
            // after the merge, resultB assign is processed.
            // .AssertVariable("resultA").HasValues("ValueA", "ValueB") This is incorrect because of global contexts cannot be distinguished
.AssertVariable("resultA").HasValues("InitA", "ValueA", "ValueB")
.AssertVariable("resultB").HasValues("ValueA", "ValueB")
.ShareFunctionGraph("sharedFn")
;

        readonly static TestCase SharedFunctionStrongUpdateGlobalUndef_CASE = @"
function sharedFn($arg){
    return $arg;
}

$resultA=sharedFn('ValueA');
$resultB=sharedFn('ValueB');

"
.AssertVariable("resultA").HasUndefinedAndValues("ValueA", "ValueB")
.AssertVariable("resultB").HasValues("ValueA", "ValueB")
.ShareFunctionGraph("sharedFn")
;

        readonly static TestCase SharedFunctionWithBranchingGlobal_CASE = @"
function sharedFn($arg){
    return $arg;
}

$resultA = 'InitA';
$resultB = 'InitB';
if($unknown){
    $resultA=sharedFn('ValueA');
}else{
    $resultB=sharedFn('ValueB');
}

"
         .AssertVariable("resultA").HasValues("InitA", "ValueA", "ValueB")
         .AssertVariable("resultB").HasValues("InitB", "ValueA", "ValueB")
         .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
         .ShareFunctionGraph("sharedFn")
         ;
        #endregion

        /// <summary>
        /// If a function is shared, local contexts do not need to be merged.
        /// 
        /// However, virtual reference memory model propagates also local data (not local variables, just data) 
        /// of caller function to called function and thus merges also local contexts. This is done to implement
        /// propagating of changes to local variables of caller that are aliased with arguments of called function.
        /// Note that this is not case of undefined value - undefined value is not propagated.
        /// to local variables.
        /// 
        /// TODO: tests for CopyMM
        /// </summary>
        #region Shared functions merging local context
        // Illustrates propagating local data to functions in VirtualReferenceMM
        readonly static TestCase SharedFunctionStrongUpdateLocal_CASE = @"
function sharedFn($arg){
    return $arg;
}

function local_wrap() {
    $a = 'InitA';
    $b = 'InitB';
    $a=sharedFn('ValueA');    
    $b=sharedFn('ValueB');

    $result[1]=$a;
    $result[2]=$b;
    return $result;
}

$resultG = local_wrap();
$resultA = $resultG[1];
$resultB = $resultG[2];

"
            //initA because of merging contexts on memory representation level in Virtual Reference model
 .AssertVariable("resultA").HasValues("InitA", "ValueA", "ValueB")
 .AssertVariable("resultB").HasValues("ValueA", "ValueB")
 .ShareFunctionGraph("sharedFn")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        // Illustrates not propagating undefined value in VirtualReferenceMM
        readonly static TestCase SharedFunctionStrongUpdateLocalUndef_CASE = @"
function sharedFn($arg){
    return $arg;
}

function local_wrap() {
    $a=sharedFn('ValueA');    
    $b=sharedFn('ValueB');

    $result[1]=$a;
    $result[2]=$b;
    return $result;
}

$resultG = local_wrap();
$resultA = $resultG[1];
$resultB = $resultG[2];

"
            //undef is not present because of optimalization in Virtual reference model
 .AssertVariable("resultA").HasValues("ValueA", "ValueB")
 .AssertVariable("resultB").HasValues("ValueA", "ValueB")
 .ShareFunctionGraph("sharedFn")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM);

        #endregion

        /// <summary>
        /// TODO
        /// </summary>
        #region Shared functions aliasing global context
        readonly static TestCase SharedFunctionAliasingGlobal_CASE = @"
function sharedFn(& $arg){
    $arg = 'fromSharedFunc';
}

$a = 'initA';
$b = 'initB';
sharedFn($a);
sharedFn($b);
"
        .AssertVariable("a").HasValues("initA", "fromSharedFunc")
            //b has undefined value because weak update must be performed
        .AssertVariable("b").HasValues("initB", "fromSharedFunc")

        .ShareFunctionGraph("sharedFn")
         ;

        readonly static TestCase SharedFunctionAliasingGlobalUndef_CASE = @"
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

sharedFn(&$a);
sharedFn(&$b);
"
            //There is no undefined value in $a because of optimalization of virtual reference model
        .AssertVariable("a").HasValues("fromSharedFunc")
            //b has undefined value because weak update must be performed
        .AssertVariable("b").HasUndefinedAndValues("fromSharedFunc")

        .ShareFunctionGraph("sharedFn")
        .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
         ;

        readonly static TestCase SharedFunctionAliasingGlobal2_CASE = @"
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

$a = 'initA';
$b = 'initB';
sharedFn(&$a);
sharedFn(&$b);
"
            //initA because of merging contexts as in SharedFunctionAliasing2
.AssertVariable("a").HasValues("initA", "fromSharedFunc")

//undefined because of weak update as in SharedFunctionAliasing
.AssertVariable("b").HasValues("initB", "fromSharedFunc")
.ShareFunctionGraph("sharedFn")
 ;
        #endregion

        /// <summary>
        /// TODO
        /// </summary>
        #region Shared functions aliasing local context
        readonly static TestCase SharedFunctionAliasing_CASE = @"
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

function local_wrap() {
    sharedFn(&$a);
    sharedFn(&$b);   

    $res[1] = $a;
    $res[2] = $b;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];
"
.AssertVariable("a").HasValues("fromSharedFunc")
            //in shared functionts we cannot distinguish between references accross
            //multiple call points - so update on reference b is weak, because arg
            //has two possible references here
.AssertVariable("b").HasUndefinedAndValues("fromSharedFunc")
.ShareFunctionGraph("sharedFn")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
 ;




        // TODO: fails for the same reason as SharedFunctionStrongUpdate_CASE
        readonly static TestCase SharedFunctionAliasing2_CASE = @"
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

function local_wrap() {
    $a = 'originalA';
    $b = 'originalB';
    sharedFn(&$a);
    sharedFn(&$b);

    $res[1] = $a;
    $res[2] = $b;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];
"
            //originalA because of merging caller context when first call 
            //with caller context of second call
.AssertVariable("a").HasValues("originalA", "fromSharedFunc")
            //originalB because of weak update as in SharedFunctionAliasing
.AssertVariable("b").HasValues("originalB", "fromSharedFunc")
.ShareFunctionGraph("sharedFn")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
 ;

        // TODO: fails for the same reason as SharedFunctionStrongUpdate_CASE
        readonly static TestCase SharedFunctionAliasingTwoArguments_CASE = @"
function sharedFn($arg, $arg2){
    $arg = $arg2;
}

function local_wrap() {
    $a = 'initA';
    $b = 'initB';
    sharedFn(&$a, 'fromCallSite1');
    sharedFn(&$b, 'fromCallSite2');

    $res[1] = $a;
    $res[2] = $b;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];


"
.AssertVariable("a").HasValues("initA", "fromCallSite1", "fromCallSite2")
            //undefined because of weak update as in SharedFunctionAliasing
.AssertVariable("b").HasValues("initB", "fromCallSite1", "fromCallSite2")
.ShareFunctionGraph("sharedFn")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
 ;

        // TODO: fails for the same reason as SharedFunctionStrongUpdate_CASE
        readonly static TestCase SharedFunctionAliasingTwoArgumentsUndef_CASE = @"
function sharedFn($arg, $arg2){
    $arg = $arg2;
}

function local_wrap() {
    sharedFn(&$a, 'fromCallSite1');
    sharedFn(&$b, 'fromCallSite2');

    $res[1] = $a;
    $res[2] = $b;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];


"
.AssertVariable("a").HasValues("fromCallSite1", "fromCallSite2")
            //undefined because of weak update as in SharedFunctionAliasing
.AssertVariable("b").HasUndefinedAndValues("fromCallSite1", "fromCallSite2")
.ShareFunctionGraph("sharedFn")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
 ;

        readonly static TestCase SharedFunctionAliasingMayTwoArguments_CASE = @"
function sharedFn($arg, $arg2){
    $arg = $arg2;
}

function local_wrap() {
    if ($unknown) $c = &$a;
    sharedFn(&$a, 'fromCallSite1');
    sharedFn(&$b, 'fromCallSite2');

    $res[1] = $a;
    $res[2] = $b;
    $res[3] = $c;
    return $res;
}

$result = local_wrap();
$a = $result[1];
$b = $result[2];
$c=  $result[3];
"
.AssertVariable("a").HasValues("fromCallSite1", "fromCallSite2")
.AssertVariable("b").HasUndefinedAndValues("fromCallSite1", "fromCallSite2")
.AssertVariable("c").HasUndefinedAndValues("fromCallSite1", "fromCallSite2")
.ShareFunctionGraph("sharedFn")
 .MemoryModel(MemoryModels.MemoryModels.VirtualReferenceMM)
 ;

        #endregion

        #endregion



        readonly static TestCase WriteArgument_CASE = @"
$argument=""Value"";
write_argument($argument);

".AssertVariable("argument").HasValues("Value_WrittenInArgument").Analysis(Analyses.SimpleAnalysis);

        readonly static TestCase SimpleNew_CASE = @"
class Obj{ 
    function __construct() { $this->c = ""constructed""; }
    function test($g) { $this->a = ""Value""; }
}

$obj=new Obj();
$obj->test(1);
$result=$obj->a;
".AssertVariable("result").HasValues("Value");

        readonly static TestCase IsSet_CASE = @"
$a='var';
$variable_p=isset($a);
$variable_n=isset($n);

$b[4]=$a;

$array_p=isset($b[4]);
$array_n=isset($b[5]);

"
            .AssertVariable("variable_p").HasValues(true)
            .AssertVariable("variable_n").HasValues(false)
            .AssertVariable("array_p").HasValues(true)
            .AssertVariable("array_n").HasValues(false)

            ;

        readonly static TestCase IndirectNewEx_CASE = @"
class Obj{
    var $a;

    function setter($value){
        $this->a=$value;
    }
}

$name=""Obj"";
$obj=new $name();

$obj->setter(""Value"");

$result=$obj->a;
".AssertVariable("result").HasValues("Value");

        readonly static TestCase ArgumentWrite_ExplicitAlias_CASE = @"

function setArg($arg){
    $arg=""Set"";
}

$result1=""NotSet"";
$result2=""NotSet"";

setArg($result1);
setArg(&$result2);
"
    .AssertVariable("result1").HasValues("NotSet")
    .AssertVariable("result2").HasValues("Set");

        readonly static TestCase ArgumentWrite_ExplicitAliasToUndefined_CASE = @"

function setArg($arg){
    $arg=""Set"";
}

setArg(&$result);

"
   .AssertVariable("result").HasValues("Set");


        readonly static TestCase ArgumentWrite_ExplicitAliasToUndefinedItem_CASE = @"

function setArg($arg){
    $arg=""Set"";
}


setArg(&$arr[0]);
$result=$arr[0];
"
.AssertVariable("result").HasValues("Set");


        readonly static TestCase StringConcatenation_CASE = @"
$a='A';
$b='B';

$result=$a.$b.'C';
$result.='D';
"
.AssertVariable("result").HasValues("ABCD");

        readonly static TestCase IncrementEval_CASE = @"
$a=3;
$a+=2;
$post_a=$a++;

$b=5;
$pre_b=++$b
"
            .AssertVariable("a").HasValues(6)
            .AssertVariable("post_a").HasValues(5)
            .AssertVariable("b").HasValues(6)
            .AssertVariable("pre_b").HasValues(6)
            ;

        readonly static TestCase DecrementEval_CASE = @"
$a=7;
$a-=2;
$post_a=$a--;

$b=5;
$pre_b=--$b
"
            .AssertVariable("a").HasValues(4)
            .AssertVariable("post_a").HasValues(5)
            .AssertVariable("b").HasValues(4)
            .AssertVariable("pre_b").HasValues(4)
            ;

        readonly static TestCase StringWithExpression_CASE = @"
$a='A';
$result=""Value $a"";
".AssertVariable("result").HasValues("Value A");


        readonly static TestCase LocalExceptionHandling_CASE = @"
$result='Not catched';
try{
    throw new Exception('Test');
}catch(Exception $ex){
    $result='Catched';
}

".AssertVariable("result").HasValues("Catched")
 .DeclareType(SimpleExceptionType.CreateType())
 ;

        readonly static TestCase CrossStackExceptionHandling_CASE = @"
function throwEx(){
    throw new Exception('Test');
}

$result='Not catched';
try{
   throwEx(); 
}catch(Exception $ex){
    $result='Catched';
}

".AssertVariable("result").HasValues("Catched")
.DeclareType(SimpleExceptionType.CreateType())
;
        // This test fails because the framework creates and initializes in CallPoint local variables for all functions that it is 
        // possible to call. That is, both local variables  $arg1 and $arg2 are initialized in CallPoint. These variables then flows 
        // to entry points of both f and g.
        // This problem should be solved by initializing local variables corresponding to function arguments in entry point of the function, not in call point
        readonly static TestCase InitializingArgumentsOfOthersCallees_CASE = @"
function f($arg1) { return $arg1;}
function g($arg2) { 
    return $arg1; // $arg1 should be undefined, not initialized
}
if ($unknown) $func = 'f';
else $func = 'g';
$result = $func(1); // in CallPoint, both f() and g() can be called
".AssertVariable("result").HasUndefinedValue().HasUndefinedOrValues(1)
;
        readonly static TestCase ParametersByAliasGlobal_CASE = @"
function f($arg) {
    $arg = 2; // changes also value of actual parameter
    $b = 3;
    $arg = &$b; // unaliases formal parameter with actual parameter
    $arg = 4; // does not change the value of actual parameter
}
f(&$result);
".AssertVariable("result").HasValues(2)
;

        // The same as ParametersByAliasGlobal_CASE, but passes parameter from local scope.
        readonly static TestCase ParametersByAliasLocal_CASE = @"
function f($arg) {
    $arg = 2; // changes also value of actual parameter
    $b = 3;
    $arg = &$b; // unaliases formal parameter with actual parameter
    $arg = 4; // does not change the value of actual parameter
}
function local_wrap() {
    $a = 1;
    f(&$a);
    return $a;
}
$result = local_wrap();
".AssertVariable("result").HasValues(2)
;

        // Without widened variable in loop condition.
        // tests justs that the test terminates
        readonly static TestCase LongLoopWidening_CASE = @"
$i=0;
while(true){
    ++$i;
}
".AssertVariable("").WideningLimit(20)
;
        // Widened variable in loop condition
        //Reason why it fails:
        //before widening of $i is performed, the condition $i<1000 is evaluated to true, this value is also visible FlowResolver.ConfirmAssumption
        // via EvaluationLog
        // $i is widened to AnyValue, the condition $<1000 is evaluated to true/false, however FlowResolver.ConfirmAssumption gets only value true from EvaluationLog
        // => the code after while cycle is never analyzed => ppGraph.End.OutSet is null
        readonly static TestCase LongLoopWidenedVariableInLoopCondition_CASE = @"
$test='NotAffected';

$i=0;
while($i<2000){
    ++$i;
}
$test2='Reachable';

".AssertVariable("test").HasValues("NotAffected")
 .AssertVariable("test2").HasValues("Reachable")
            //|.WideningLimit(20)
 .WideningLimit(20) // for debuging
;

        readonly static TestCase FunctionTest_CASE = @"
function f($arg) {
    $b = 3;
    g(2);
    return $b;    
}

function g($arg2) {
    $b = 4;
}

$a = 2;
$a = f(1);
".AssertVariable("a").HasValues(3)
;
        readonly static TestCase ArrayReturnValueTest_CASE = @"
function f() {
    $a[1] = ""index1"";
    $a[2] = ""index2"";
    return $a;    
}

$arr = f();
$result = $arr[1];
".AssertVariable("result").HasValues("index1")
            ;

        readonly static TestCase ArrayMergeReturnValueTest_CASE = @"
function f() {
    $a[1] = ""f.index1"";
    $a[2] = ""f.index2"";
    return $a;    
}

function g() {
    $a[1] = ""g.index1"";
    $a[2] = ""g.index2"";
    return $a;    
}

if ($unknown) {
    $x = ""f"";
}
else {
    $x = ""g"";
}

$arr = $x();
$result = $arr[1];
".AssertVariable("result")
 .HasValues("f.index1", "g.index1")
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\ArrayMergeReturnValueTest", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
.PrintSnapshotGraph(@"memory\ArrayMergeReturnValueTest")
#endif
;

        readonly static TestCase ArrayAliasedReturnValueTest_CASE = @"
function f() {
    $a[1] = ""f.index1"";
    $a[2] = ""f.index2"";
    return $a;    
}

$arr = & f();
$result = $arr[1];
".AssertVariable("result").HasValues("f.index1")
            ;


        readonly static TestCase ArrayCopySemantic_CASE = @"
$a[0]='initial';

$b=$a;
//because of copy a is not affected
$b[0]='valueB';


$c=&$b;
//because of alias b is affected
$c[1]='valueC';

$resA=$a[0];
$resB=$b[0];
$resC=$b[1];
"
            .AssertVariable("resA").HasValues("initial")
            .AssertVariable("resB").HasValues("valueB")
            .AssertVariable("resC").HasValues("valueC")
            ;

        readonly static TestCase ArrayScalar2ArrayMust_CASE = @"
$a = 1;
// it is not possible to create array in the variable in that must be scalar value
// the assignment will not be performed
$a[1] = 2;
$res1 = $a;
$res2 = $a[1];
"
            .AssertVariable("res1").HasValues(1)
            .AssertVariable("res2").HasUndefinedValue()
            .MemoryModel(MemoryModels.MemoryModels.ModularCopyMM)
            ;

        readonly static TestCase ArrayScalar2ArrayMay_CASE = @"
if ($unknown) {
    $a = 1;
}
// in the variable only may be scalar value, tha assignment thus may be performed
$a[1] = 2;
$res1 = $a;
$res2 = $a[1];
"
            // TODO: res1 should have also array value
            .AssertVariable("res1").HasValues(1)
            .AssertVariable("res2").HasUndefinedAndValues(2)
            .MemoryModel(MemoryModels.MemoryModels.ModularCopyMM)
            ;

        readonly static TestCase ArrayArray2ScalarMust_CASE = @"
$a[1] = 1;
// the array in $a will be overwritten by scalar value
// TODO: however, the analyzer should emit some warning!!
$a = 2;
$res1 = $a;
$res2 = $a[1];
"
            .AssertVariable("res1").HasValues(2)
            .AssertVariable("res2").HasUndefinedValue()
            .MemoryModel(MemoryModels.MemoryModels.ModularCopyMM)
            ;

        readonly static TestCase ArrayArray2ScalarMay_CASE = @"
$a[1] = 1;
if ($unknown) $a = 2;
$res1 = $a;
$res2 = $a[1];
"
            // TODO: test whether r1 has also array value
            .AssertVariable("res1").HasValues(2)
            .AssertVariable("res2").HasUndefinedAndValues(1)
            .MemoryModel(MemoryModels.MemoryModels.ModularCopyMM)
            ;


        readonly static TestCase TransitiveAliasResolving_CASE = @"
$a='valueA';
$b='valueB';

$c=&$a;
$a=&$b;

$d=&$a;
"
            .AssertVariable("a").HasValues("valueB")
            .AssertVariable("b").HasValues("valueB")
            .AssertVariable("c").HasValues("valueA")
            .AssertVariable("d").HasValues("valueB")
            ;




        readonly static TestCase OverridenAlias_CASE = @"
$alias = 1;

$arr[$_POST] = &$alias;

$arr2 = $arr;
$arr2 = $arr;

$result = $arr2[1];
".AssertVariable("result").HasUndefinedAndValues(1);

        readonly static TestCase CycledAlias_CASE = @"
$alias = 1;

$arr[$_POST] = &$alias;
$arr2 = $arr;
$arr[$_POST] = $arr2;

$result = $arr[1][1];
".AssertVariable("result").HasUndefinedAndValues(1);

        readonly static TestCase CycledAlias2_CASE = @"
$a = array();
$a[1] = &$a;
$a[2] = $a;

$result = 1;
".AssertVariable("result").HasValues(1);

        readonly static TestCase UndefinedAlias_CASE = @"
// $alias is undefined, no alias is created
$a = &$alias;
// $alias is not an alias of $a, thus it is not updated and stays undefined
$a = 2;
$resAlias = $alias;
// $alias is not an alias of $a, $a is not updated
$alias = 3;
$resA = $a;
".AssertVariable("resAlias").HasValues(2)
 .AssertVariable("resA").HasValues(3)
 .MemoryModel(MemoryModels.MemoryModels.ModularCopyMM);

        readonly static TestCase UndefinedAliasMay_CASE = @"
if ($unknown) $alias = 1;
$a = &$alias;
$a = 2;
$resAlias = $alias;
$alias = 3;
$resA = $a;
"
            // Note that this is an overapproximation. $resAlias can have only values undefined, 2.
            // if $alias is undefined at line 2, the alias is not created
            // if $alias is defined at line 2, it has a single value 1 and the alias is created
            // line 3 either does not affect $alias (it stays undefined) or set $alias to 2. It thus never has value 1 at line 4.
 .AssertVariable("resAlias").HasValues(2)
 .AssertVariable("resA").HasValues(3)
 .MemoryModel(MemoryModels.MemoryModels.ModularCopyMM);


        readonly static TestCase TernaryOperator_CASE = @"

function op1(){
    global $test;
    $test='op1';

    return $test;
}

function op2(){
    global $test;
    $test='op2';

    return $test;
}

$result1=true ? op1():op2();
$global1=$test;

$result2=false ? op1():op2();
$global2=$test;

$result3=$unknown ? op1():op2();
$global3=$test;

"
            .AssertVariable("result1").HasValues("op1")
            .AssertVariable("global1").HasValues("op1")

            .AssertVariable("result2").HasValues("op2")
            .AssertVariable("global2").HasValues("op2")

            .AssertVariable("result3").HasValues("op1", "op2")
            .AssertVariable("global3").HasValues("op1", "op2")
            ;


        readonly static TestCase EvalAssign_CASE = @"
$result='init';
eval('$result=""in eval""');

"
            .AssertVariable("result").HasValues("in eval")
            ;


        readonly static TestCase EvalReturn_CASE = @"
$result=eval('return ""from eval""');
"
    .AssertVariable("result").HasValues("from eval")
    ;


        readonly static TestCase BranchedEvalAssign_CASE = @"
$result='init';

if($unknown){
    $code='$result=""in eval1""';
}else{
    $code='$result=""in eval2""';
}

eval($code);

"
          .AssertVariable("result").HasValues("in eval1", "in eval2")
          ;


        readonly static TestCase StaticCallOnObjectInArray_CASE = @"
class X{    
    static function f(){
        return 'X::f()';
    }
}

$x=new X();

$arr=array();
$arr[0]=$x;
$result=$arr[0]::f();

".AssertVariable("result").HasValues("X::f()");

        readonly static TestCase WhileCycleComputation_CASE = @"
$i = 1;
while ($i <= 2) {
 $b = $i;
 $i++;
}

".AssertVariable("b").HasValues(1, 2);


        readonly static TestCase ConditionalExpressionResult_CASE = @"

$result = true ? 'a' : 'b';

".AssertVariable("result").HasValues("a");

        readonly static TestCase NestedConditionalExpressionResult_CASE = @"

$result = true ? false : true ? 'a' : 'b';

".AssertVariable("result").HasValues("b");


        readonly static TestCase ArrayConditionalExpressionResult_CASE = @"
$a[0]=$unknown;
$a[1]='a';
$a[2]='b';
$result = $a[0] ? $a[1] : $a[2];

".AssertVariable("result").HasValues("a","b");

        readonly static TestCase ArrayMayConditionalExpressionResult_CASE = @"
$a[0]='a';
$result= $unknown ? $a[0] : 'b' ;

".AssertVariable("result").HasValues("a", "b").Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase IndexingField_CASE = @"

class Test{
    var $b;
    var $t;
}

$b=new Test();
$b->t=$b;

$b->t->r[4][5] = 7;

$result=$b->r[4][5];

".AssertVariable("result").HasValues(7).Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase ReturnArray_CASE = @"

function GetLoginData($name = """", $id = -1, $adr = 'login')
{
  return Array(0,1);
}

$x = GetLoginData();

".AssertVariable("").Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase CycledMayAliasArray_CASE = @"
$arr[$_POST[1]] = &$alias;

$arr2 = $arr;
$arr2[1] = 1;
$arr[$_POST[1]] = $arr2;

$alias = array();
$arr[1][1] = 1;

".AssertVariable("").Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase NullReferenceErrorTest_CASE = @"
$arr[$_POST[1]] = &$alias;
$alias[1] = &$a[$_POST[1]];
$alias = array();
$arr[1][1] = 1;

".AssertVariable("").Analysis(Analyses.WevercaAnalysisTest);
        
        readonly static TestCase UnknownIndexAlias_CASE = @"
$alias = array();
$alias2 = 0;
if ($_POST) {
	$arr[$_POST] = &$alias;
} else {
	$arr[1][$_POST] = 7;
}
$arr[2][1] = &$alias2;

$a1 = $alias[1];

".AssertVariable("a1").HasUndefinedAndValues(0)
 .Analysis(Analyses.WevercaAnalysisTest);

        // Assignments to $arr[] are modeled as assignments to unknown indices.
        readonly static TestCase ArrayWithoutSpecifiedIndexAccess_CASE = @"
$arr[] = 0;
$arr[] = 1;
$arr[5] = 5;
$arr[] = 6;
$arr['a'] = 'a';
$arr[] = 7;
$a = $arr[0];
$b = $arr[1];
$c = $arr[5];
$d = $arr[6];
$e = $arr[7];
"
            .AssertVariable("a").HasUndefinedAndValues(7, 6, 1, 0) // .AssertVariable("a").HasValues(0) if assignments to $arr[] would be modeled precisely
            .AssertVariable("b").HasUndefinedAndValues(7, 6, 1, 0) // .AssertVariable("b").HasValues(1) if assignments to $arr[] would be modeled precisely
            .AssertVariable("c").HasValues(5)
            .AssertVariable("d").HasUndefinedAndValues(7, 6, 1, 0) // .AssertVariable("d").HasValues(6) if assignments to $arr[] would be modeled precisely
            .AssertVariable("e").HasUndefinedAndValues(7, 6, 1, 0) // .AssertVariable("e").HasValues(7) if assignments to $arr[] would be modeled precisely
            .SimplifyLimit(5);
        #region Switch tests

        /// <summary>
        /// Comes from BUG report on improved queue processing
        /// </summary>
        readonly static TestCase SwitchBranchProcessing_CASE = @"
switch($unknown){
    case 1: $result='a'; break;
    case 2: $result='b'; break; 
}

".AssertVariable("result").HasUndefinedAndValues("a", "b");

        readonly static TestCase SwitchUnreachable_CASE = @"
$a = 1;
switch($a){
    case 1: die();
    case 2: $result='b'; 
}
// this should be unreachable
$a = 0;

".AssertVariable("a").HasValues(1);
        [TestMethod]
        public void SwitchUnreachable()
        {
            AnalysisTestUtils.RunTestCase(SwitchUnreachable_CASE);
        }

        readonly static TestCase SwitchUnreachable2_CASE = @"
$a = ($unknown) ? 1 : 2;
switch($a){
    case 1: die();
    case 2: die();
    default: die(); 
}
// this should be unreachable
$a = 0;

".AssertVariable("a").HasValues(1, 2);
        [TestMethod]
        public void SwitchUnreachable2()
        {
            AnalysisTestUtils.RunTestCase(SwitchUnreachable2_CASE);
        }

        readonly static TestCase SwitchUnreachable3_CASE = @"
$a = ($unknown) ? 1 : 2;
switch($a){
    case 1: $result='a'; die();
    case 2: $result='b'; die(); 
}
// this should be unreachable
$a = 0;

".AssertVariable("a").HasValues(1, 2);

        // TODO: this test fails because the assumption in the else branch does not eliminate value 1 from the range of $a
        [TestMethod]
        public void SwitchUnreachable3FailingTODO()
        {
            AnalysisTestUtils.RunTestCase(SwitchUnreachable3_CASE);
        }

        readonly static TestCase SwitchWithoutBreak_CASE = @"
$a = 1;
switch($a){
    case 1: $b = 1;
    default: $c = 1;
    case 2: $d = 1;
    default: $e = 1;
}
$f = 1;

".AssertVariable("a").HasValues(1)
.AssertVariable("b").HasValues(1)
.AssertVariable("c").HasValues(1)
.AssertVariable("d").HasValues(1)
.AssertVariable("e").HasValues(1)
.AssertVariable("f").HasValues(1);
        [TestMethod]
        public void SwitchWithoutBreak()
        {
            AnalysisTestUtils.RunTestCase(SwitchWithoutBreak_CASE);
        }

        readonly static TestCase SwitchWithoutBreak2_CASE = @"
$a = 4;
switch ($a) {
	case 1:
		$b = 1;
        echo 'smt';
	default:
        $c = 1;
        echo 'smt';
	case 2:
        $d = 1;
	default:
        $e = 1;
	case 3:
        $f = 1;
}
$g = 1;

".AssertVariable("a").HasValues(4)
.AssertVariable("b").HasUndefinedValue()
.AssertVariable("c").HasUndefinedValue()
.AssertVariable("d").HasUndefinedValue()
.AssertVariable("e").HasValues(1)
.AssertVariable("f").HasValues(1)
.AssertVariable("g").HasValues(1);

        [TestMethod]
        public void SwitchWithoutBreak2()
        {
            AnalysisTestUtils.RunTestCase(SwitchWithoutBreak2_CASE);
        }


        readonly static TestCase SwitchWithOneBreak_CASE = @"
$a = 4;
switch ($a) {
	case 1:
		$b = 1;
	default:
        $c = 1;
        echo 'smt';
	case 2:
        $d = 1;
        echo 'smt';
	default:
        $e = 1;
        echo 'smt';
        break;
	case 3:
        $f = 1;
}
$g = 1;

".AssertVariable("a").HasValues(4)
.AssertVariable("b").HasUndefinedValue()
.AssertVariable("c").HasUndefinedValue()
.AssertVariable("d").HasUndefinedValue()
.AssertVariable("e").HasValues(1)
.AssertVariable("f").HasUndefinedValue()
.AssertVariable("g").HasValues(1);

        [TestMethod]
        public void SwitchWithOneBreak()
        {
            AnalysisTestUtils.RunTestCase(SwitchWithOneBreak_CASE);
        }

        readonly static TestCase SwitchWithOneBreak2_CASE = @"
$a = 1;
switch ($a) {
	case 1:
		$b = 1;
	default:
        $c = 1;
        echo 'smt';
    break;
	case 2:
        $d = 1;
	default:
        $e = 1;
	case 3:
        $f = 1;
}
$g = 1;

".AssertVariable("a").HasValues(1)
.AssertVariable("b").HasValues(1)
.AssertVariable("c").HasValues(1)
.AssertVariable("d").HasUndefinedValue()
.AssertVariable("e").HasUndefinedValue()
.AssertVariable("f").HasUndefinedValue()
.AssertVariable("g").HasValues(1);

        [TestMethod]
        public void SwitchWithOneBreak2()
        {
            AnalysisTestUtils.RunTestCase(SwitchWithOneBreak2_CASE);
        }

        readonly static TestCase SwitchWithOneBreak3_CASE = @"
$a = 1;
switch ($a) {
	case 1:
		$b = 1;
        echo 'smt';
    break;
	default:
        $c = 1;
	case 2:
        $d = 1;
	default:
        $e = 1;
	case 3:
        $f = 1;
}
$g = 1;

".AssertVariable("a").HasValues(1)
.AssertVariable("b").HasValues(1)
.AssertVariable("c").HasUndefinedValue()
.AssertVariable("d").HasUndefinedValue()
.AssertVariable("e").HasUndefinedValue()
.AssertVariable("f").HasUndefinedValue()
.AssertVariable("g").HasValues(1);

        readonly static TestCase Merge_CASE = @"

$any = $_POST[0];
$ar['a'] = 'a';
$ar[$any]['any'] = 'any';

if ($_POST[1]) {
    if ($_POST[2]) {
        $ar['b'] = array();
        $ar['b']['bA'] = 'bA';
        $ar['d'] = 'd';
    }
    else {
        $ar['b'] = array();
        $ar['b']['bB'] = 'bB';
        $ar['d'] = 'd';
    }
}
else {
    $ar['c']['cA'] = 'cA';
    $ar['d'] = 'd';
}

$a = $ar['a'];

$ba = $ar['b']['bA'];
$bb = $ar['b']['bB'];
$bany = $ar['b']['any'];

$ca = $ar['c']['cA'];
$cany = $ar['c']['any'];

$d = $ar['d'];

".AssertVariable("a").HasValues("a", "a")
.AssertVariable("ba").HasUndefinedAndValues("bA")
.AssertVariable("bb").HasUndefinedAndValues("bB")
.AssertVariable("bany").HasUndefinedAndValues("any")
.AssertVariable("ca").HasUndefinedAndValues("cA")
.AssertVariable("cany").HasUndefinedAndValues("any")
.AssertVariable("d").HasValues("d")
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\Merge")
#endif
;



        readonly static TestCase MergeAliases_CASE = @"
$any = $_POST[0];
$ar['a'] = 'a';
$ar[$any]['any'] = 'any';

$alias['a'] = & $ar['a'];
$alias['any'] = & $ar[$any]['any'];
$mustAlias = & $alias['any'];

if ($_POST[1]) {
    if ($_POST[2]) {
        $ar['b'] = array();
        $ar['b']['bA'] = 'bA';
        $ar['d'] = 'd';
        $ar['e']['any'] = 'eAny';

        $alias['bA'] = & $ar['b']['bA'];
        $alias['d'] = & $ar['d'];
        $alias['eAny'] = & $ar['e']['any'];
    }
    else {
        $ar['b'] = array();
        $ar['b']['bB'] = 'bB';
        $ar['d'] = 'd';
        $ar['e']['any'] = 'eAny';

        $alias['bB'] = & $ar['b']['bB'];
        $alias['d'] = & $ar['d'];
        $alias['eAny'] = & $ar['e']['any'];
    }
}
else {
    $ar['c']['cA'] = 'cA';
    $ar['d'] = 'd';
    $ar['e']['any'] = 'eAny';

    $alias['cA'] = & $ar['c']['cA'];
    $alias['d'] = & $ar['d'];
    $alias['eAny'] = & $ar['e']['any'];
}

$alias['a'] = 'alias_a';
$alias['any'] = 'alias_any';
$alias['bA'] = 'alias_bA';
$alias['bB'] = 'alias_bB';
$alias['cA'] = 'alias_cA';
$alias['d'] = 'alias_d';
$alias['eAny'] = 'alias_eAny';

$a = $ar['a'];
$ba = $ar['b']['bA'];
$bb = $ar['b']['bB'];
$bany = $ar['b']['any'];
$ca = $ar['c']['cA'];
$cany = $ar['c']['any'];
$d = $ar['d'];
$eany = $ar['e']['any'];

".AssertVariable("a").HasValues("alias_a")
.AssertVariable("ba").HasUndefinedAndValues("bA", "alias_bA")
.AssertVariable("bb").HasUndefinedAndValues("bB", "alias_bB")
.AssertVariable("bany").HasUndefinedAndValues("any", "alias_any")
.AssertVariable("ca").HasUndefinedAndValues("cA", "alias_cA")
.AssertVariable("cany").HasUndefinedAndValues("any", "alias_any")
.AssertVariable("d").HasValues("alias_d")
.AssertVariable("eany").HasValues("alias_eAny")
.AssertVariable("mustAlias").HasValues("alias_any", "alias_eAny");

        readonly static TestCase MergeIndirectAliases_CASE = @"
$any = $_POST[0];

$ar[$any]['any'] = 'any';
$alias['any'] = & $ar[$any]['any'];
$mustAlias = & $alias['any'];

$x = 1;

if ($_POST[1]) {
    $ar['b'] = array();
    $ar['b']['bA'] = 'bA';
}
else {
    $ar['c'] = array();
    $ar['c']['cA'] = 'cA';
}

$alias['any'] = 'alias_any';

$bany = $ar['b']['any'];
$cany = $ar['c']['any'];

".AssertVariable("bany").HasUndefinedAndValues("any", "alias_any")
 .AssertVariable("cany").HasUndefinedAndValues("any", "alias_any")
 .AssertVariable("mustAlias").HasValues("alias_any")
 ;


        readonly static TestCase LoopMerge_CASE = @"

$any = $_POST[0];

$x = 0;
while($x < 3) {

    $ar[$x] = array();
    $ar[$x]['a'] = 'a';

    if ($_POST[1]) {
        if ($_POST[2]) {
            $ar[$x]['b']['bA'] = 'bA';
            $ar[$x][$any]['any'] = 'any1';
        }
        else {
            $ar[$x]['b']['bB'] = 'bB';
            $ar[$x][$any]['any'] = 'any2';
        }
    }
    else {
        $ar[$x]['c']['cA'] = 'cA';
        $ar[$x][$any]['any'] = 'any3';
    }

    $x++;
}

$y = 0;
$aa = $ar[$y]['a']['a'];

$ba = $ar[$y]['b']['bA'];
$bb = $ar[$y]['b']['bB'];
$bany = $ar[$y]['b']['any'];

$ca = $ar[$y]['c']['cA'];
$cany = $ar[$y]['c']['any'];

".AssertVariable("x").HasValues(3)
.AssertVariable("aa").HasUndefinedAndValues("a")
.AssertVariable("ba").HasUndefinedAndValues("bA")
.AssertVariable("bb").HasUndefinedAndValues("bB")
.AssertVariable("bany").HasUndefinedAndValues("any1", "any2", "any3")
.AssertVariable("ca").HasUndefinedAndValues("cA")
.AssertVariable("cany").HasUndefinedAndValues("any1", "any2", "any3")
.WideningLimit(10)
.SimplifyLimit(4)
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\LoopMerge", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
.PrintSnapshotGraph(@"memory\LoopMerge")
#endif
;

        [TestMethod]
        public void Merge()
        {
            AnalysisTestUtils.RunTestCase(Merge_CASE);
        }

        [TestMethod]
        public void MergeAliases()
        {
            AnalysisTestUtils.RunTestCase(MergeAliases_CASE);
        }

        [TestMethod]
        public void MergeIndiretAliases()
        {
            AnalysisTestUtils.RunTestCase(MergeIndirectAliases_CASE);
        }

        [TestMethod]
        public void LoopMerge()
        {
            AnalysisTestUtils.RunTestCase(LoopMerge_CASE);
        }


        readonly static TestCase FunctionDefinitionMerge_CASE = @"
if ($_POST[0]) {
    function f() { return 'f'; }
}
else {
    function g() { return 'g'; }
}

$f = f();
$g = g();

"
.AssertVariable("f").HasValues("f")
.AssertVariable("g").HasValues("g")
;


        [TestMethod]
        public void FunctionDefinitionMerge()
        {
            AnalysisTestUtils.RunTestCase(FunctionDefinitionMerge_CASE);
        }

        readonly static TestCase ClassDefinitionMerge_CASE = @"
if ($_POST[0]) {
    class f { function m() { return 'f'; } }
}
else {
    class g { function m() { return 'g'; } }
}

$fo = new f();
$go = new g();

$f = $fo->m();
$g = $go->m();

"
            .AssertVariable("f").HasValues("f")
            .AssertVariable("g").HasValues("g")
            ;


        [TestMethod]
        public void ClassDefinitionMerge()
        {
            AnalysisTestUtils.RunTestCase(ClassDefinitionMerge_CASE);
        }

        readonly static TestCase LocalVariableSeparation_CASE = @"

function f() { $a = 'f'; return $a; }
function g() { $a = 'g'; f(); return $a; }

$f = f();
$g = g();
"
.AssertVariable("f").HasValues("f")
.AssertVariable("g").HasValues("g")
;


        [TestMethod]
        public void LocalVariableSeparation()
        {
            AnalysisTestUtils.RunTestCase(LocalVariableSeparation_CASE);
        }

        readonly static TestCase MergeReturn_CASE = @"

function m($p) {
    $a = 0;
    if ($p) { return 1; }
    else { return 2; }
}

$m = m($_POST[1]);

"
.AssertVariable("m").HasValues(1, 2)
;


        [TestMethod]
        public void MergeReturn()
        {
            AnalysisTestUtils.RunTestCase(MergeReturn_CASE);
        }

        readonly static TestCase MergeDeleteArray_CASE = @"

$arr[1][1] = 1;
$alias = & $arr[1][1];
$alias2 = & $alias;

if ($_POST[1]) {
    $arr = array();
    $arr[2] = 2;
}
else {
    $arr = array();
    $arr[2] = 2;
}

$alias = 3;

$a = $arr[1][1];
$b = $arr[2];

"
.AssertVariable("a").HasUndefinedValue()
.AssertVariable("b").HasValues(2)
.AssertVariable("alias2").HasValues(3)
;

        [TestMethod]
        public void MergeDeleteArray()
        {
            AnalysisTestUtils.RunTestCase(MergeDeleteArray_CASE);
        }

        readonly static TestCase MergeMethodCall_CASE = @"

class A {
    var $a = 1;
    function f($a) { $this->a = $a; }
}

$obj = new A();
$obj->f(2);
$a = $obj->a;

"
.AssertVariable("a").HasValues(2)
;


        [TestMethod]
        public void MergeMethodCall()
        {
            AnalysisTestUtils.RunTestCase(MergeMethodCall_CASE);
        }

        readonly static TestCase Recursion_CASE = @"

$t2 = 2;
function f($a) {
    if ($a < 10) return f($a + 1);
    else return $a;
}

$a = f(1);
$t = 1;

"
.AssertVariable("t").HasValues(1)
.AssertVariable("t2").HasValues(2)
.ShareFunctionGraph("f")
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\Recursion", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
.PrintSnapshotGraph(@"memory\Recursion")
#endif
;


        [TestMethod]
        public void Recursion()
        {
            AnalysisTestUtils.RunTestCase(Recursion_CASE);
        }


        readonly static TestCase InfiniteRecursion_CASE = @"

$t2 = 2;
function f($a) {
    return f($a + 1);
}

$a = f(1);
$t = 1;

"
.AssertVariable("t").HasUndefinedValue()
.ShareFunctionGraph("f")
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\InfiniteRecursion", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
.PrintSnapshotGraph(@"memory\InfiniteRecursion")
#endif
;


        [TestMethod]
        public void InfiniteRecursion()
        {
            AnalysisTestUtils.RunTestCase(InfiniteRecursion_CASE);
        }

        readonly static TestCase IndirectRecursion_CASE = @"

$t2 = 2;
function f($a) {
    if ($a < 10) return g($a + 1);
    else return $a;
}
function g($a) {
    return f($a);
}

$a = f(1);
$t = 1;

"
.AssertVariable("t").HasValues(1)
.ShareFunctionGraph("f")
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\IndirectRecursion", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
.PrintSnapshotGraph(@"memory\IndirectRecursion")
#endif
;


        [TestMethod]
        public void IndirectRecursion()
        {
            AnalysisTestUtils.RunTestCase(IndirectRecursion_CASE);
        }

        readonly static TestCase RecursionWithCall_CASE = @"
function f($a, $test) {
  if ($test) {
    return $a;
  }
  return 0;
}

function g($a, $test) {
    $b = f($a, $test);
    $x = 1;
    return $b;
}

function rec($a, $test) {
  $b = g($a - 1, $test);
  if ($b) {
    return rec($b, $test);
  }
  return 0;
}

rec(3, $_POST[1]);
$t = 1;
"
            .AssertVariable("t").HasValues(1)
            .ShareFunctionGraph("rec")
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\RecursionWithCall", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
            .PrintSnapshotGraph(@"memory\RecursionWithCall")
#endif
;


        [TestMethod]
        public void RecursionWithCall()
        {
            AnalysisTestUtils.RunTestCase(RecursionWithCall_CASE);
        }

        readonly static TestCase SharedFunctionSeparation_CASE = @"
function shared($val, & $glob1, & $glob2) {
  $glob1 = 'S' . $val;
}

function regular($val, & $glob1, & $glob2) {
  $glob2 = 'R' . $val;
}

function f($val, $func, & $loc, & $glob1, & $glob2) {
  $loc = $val;
  $func($val, $glob1, $glob2);
  return $val;
}

function g($val, $func, & $loc, & $glob1, & $glob2) {
  $loc = $val;
  $func($val, $glob1, $glob2);
  return $val;
}

$func = '';
if ($_POST[1]) {
  $func = 'shared';
}
else {
  $func = 'regular';
}

$f1loc = 'G';
$f1globS = 'G';
$f1globR = 'G';
$f1ret = f('F1', $func, $f1loc, $f1globS, $f1globR);

$f2loc = 'G';
$f2globS = 'G';
$f2globR = 'G';
$f2ret = g('F2', $func, $f2loc, $f2globS, $f2globR);
"
.AssertVariable("f1ret").HasValues("F1")
.AssertVariable("f1loc").HasValues("F1")
.AssertVariable("f1globS").HasValues("G", "SF1", "SF2")
.AssertVariable("f1globR").HasValues("G", "RF1")

.AssertVariable("f2ret").HasValues("F2")
.AssertVariable("f2loc").HasValues("F2")
.AssertVariable("f2globS").HasUndefinedAndValues("G", "SF1", "SF2")
.AssertVariable("f2globR").HasUndefinedAndValues("G", "RF2")

.ShareFunctionGraph("shared")
.SimplifyLimit(10)
.WideningLimit(10)
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\SharedFunctionSeparation", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
    .PrintSnapshotGraph(@"memory\SharedFunctionSeparation")
#endif
;


        [TestMethod]
        public void SharedFunctionSeparation()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionSeparation_CASE);
        }

        [TestMethod]
        public void SwitchWithOneBreak3()
        {
            AnalysisTestUtils.RunTestCase(SwitchWithOneBreak3_CASE);
        }

        #endregion

        #region Short circuit evaluation

        /// <summary>
        /// Testing of shortened behaviour as result of BUG report, when Evaluation log doesn't have operand values
        /// </summary>
        readonly static TestCase MultiOR_CASE = @"
$a = 'abc';

if (!$a or $b or $c)
{
    die();
}
".AssertVariable("a").HasUndefinedOrValues("abc").Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase IncompleteEvaluation_CASE = @"
function action(){
    global $result;

    $result='action done';   
    return false;
}

$result='no action';
true || action();
$result1=$result;

$result='no action';
false || action();
$result2=$result;
"
            .AssertVariable("result1").HasValues("no action")
            .AssertVariable("result2").HasValues("action done");


        readonly static TestCase IncompleteEvaluationDie_CASE = @"
$result='before die';
false || die();

$result='after die';
"
    .AssertVariable("result").HasValues("before die");

        readonly static TestCase IncompleteEvaluationDontDie_CASE = @"
$result='before die';
true || die();

$result='after die';
"
            .AssertVariable("result").HasValues("after die");

        readonly static TestCase IncompleteEvaluationMay_CASE = @"
function action(){
    global $result;

    $result='action done';   
    return false;
}

$result='no action';
$b = true;
if ($unknown) $b = false;
$b || action();
"
            .AssertVariable("result").HasValues("no action", "action done")
            .Analysis(Analyses.WevercaAnalysisTest);

        readonly static TestCase IncompleteEvaluationElseIf_CASE = @"
$a = ($_GET[1]) ? true : false;
$b = $_GET[1] ? true : false;

if ($a) {
    $result1 = ($a == true);
    $result2 = ($b == true);
}
elseif ($a || $b) {
    $result3 = ($a == true);
    $result4 = ($b == true);
} else {
    $result5 = ($a == true);
    $result6 = ($b == true);
}

"
            .AssertVariable("result1").HasUndefinedAndValues(true)
            .AssertVariable("result2").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result3").HasUndefinedAndValues(false)
            .AssertVariable("result4").HasUndefinedAndValues(true)
            .AssertVariable("result5").HasUndefinedAndValues(false)
            .AssertVariable("result6").HasUndefinedAndValues(false)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void IncompleteEvaluationElseIf()
        {
            AnalysisTestUtils.RunTestCase(IncompleteEvaluationElseIf_CASE);
        }

        readonly static TestCase IncompleteEvaluationXor_CASE = @"
$a = ($_GET[1]) ? true : false;
$b = $_GET[1] ? true : false;

if ($a xor $b) {
    $result1 = ($a == true);
    $result2 = ($b == true);
}

$a = true;
if ($a xor $b) {
//if (($a && !$b) || (!$a && $b)) {
    $result3 = ($b == true);
}

$a = ($_GET[1]) ? true : false;
if ($a && ($a xor $b)) {
    $result4 = ($a == true);
    $result5 = ($b == true);
}

if ($a xor ($a || $b)) {
}

"
            .AssertVariable("result1").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result2").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result3").HasUndefinedAndValues(false)
            .AssertVariable("result4").HasUndefinedAndValues(true)
            .AssertVariable("result5").HasUndefinedAndValues(false)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void IncompleteEvaluationXor()
        {
            AnalysisTestUtils.RunTestCase(IncompleteEvaluationXor_CASE);
        }

         #endregion

        #region Assumptions

        #region Assumptions - various statements

        readonly static TestCase AssumptionsWhile_CASE = @"
$i = 0;
$any = $_GET[1];
while ($any >= 1 && $any < 3 && $i < 2) 
{
    $result1 = ($any == 0);
    $result2 = ($any == 1);
    $result3 = ($any == 2);
    $result4 = ($any == 3);
    $i++;
}
"
            .AssertVariable("result1").HasUndefinedAndValues(false)
            .AssertVariable("result2").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result3").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result4").HasUndefinedAndValues(false)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsWhile()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsWhile_CASE);
        }

        readonly static TestCase AssumptionsFor_CASE = @"
$any = $_GET[1];
for ($i = 0; $any >= 1 && $any < 3 && $i < 2; $i++) 
{
    $result1 = ($any == 0);
    $result2 = ($any == 1);
    $result3 = ($any == 2);
    $result4 = ($any == 3);
}
"
            .AssertVariable("result1").HasUndefinedAndValues(false)
            .AssertVariable("result2").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result3").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result4").HasUndefinedAndValues(false)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsFor()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsFor_CASE);
        }

        #endregion

        readonly static TestCase AssumptionsMultipleUseVariable_CASE = @"
if (isset($_GET[1]))
    $v = $_GET[1];
if ($v >= 5 && $v <= 5) {
    $result = $v;
}
"
            .AssertVariable("result").HasUndefinedAndValues(5)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsMultipleUseVariable()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsMultipleUseVariable_CASE);
        }

        readonly static TestCase AssumptionTwoVariables_CASE = @"
$a = ($_GET[1]) ? 1 : 2;
$b = ($_GET[1]) ? 1 : 2;
$b = ($_GET[1]) ? $b : 3;
if ($a == $b) {
    $result1 = $a;
    $result2 = $b;
}
"
            .AssertVariable("result1").HasUndefinedAndValues(1, 2)
            .AssertVariable("result2").HasUndefinedAndValues(1, 2)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionTwoVariables()
        {
            AnalysisTestUtils.RunTestCase(AssumptionTwoVariables_CASE);
        }

        readonly static TestCase AssumptionIntervals_CASE = @"
$any = $_GET[1];
if ($any >= 2 && $any <= 3) {
    $result1 = ($any == 1);
    $result2 = ($any == 2);
    $result3 = ($any == 3);
    $result4 = ($any == 4); 
}
"
            .AssertVariable("result1").HasUndefinedAndValues(false)
            .AssertVariable("result2").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result3").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result4").HasUndefinedAndValues(false)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionIntervals()
        {
            AnalysisTestUtils.RunTestCase(AssumptionIntervals_CASE);
        }


        readonly static TestCase AssumptionTwoVariablesIntervals_CASE = @"
$x = 1 + 2;
$any = $_GET[1];
if ($any >= 2) $int1 = $any;

$any = $_GET[1];
if ($any >= 1 && $any <= 5) $int2 = $any;

// a = {<2, inf), 10}
$a = ($_GET[1]) ? $int1 : 10;
// b = {<1, 5>, 10}
$b = ($_GET[1]) ? $int2 : 10;
if ($a == $b) {
    // $a = b = {10, <2, 5>}

    $result1 = ($a == 10); // can be

    $result2 = ($a == 1); // cannot be
    
    $result3 = ($a == 2); // can be
    $result4 = ($a == 3); // can be
    $result5 = ($a == 4); // can be
    $result6 = ($a == 5); // can be

    $result7 = ($a == 6); // cannot be
}
"
            .AssertVariable("result1").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result2").HasUndefinedAndValues(false)
            .AssertVariable("result3").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result4").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result5").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result6").HasUndefinedAndSpecialValues(SpecialValues.ANY_BOOLEAN)
            .AssertVariable("result7").HasUndefinedAndValues(false)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionTwoVariablesIntervals()
        {
            AnalysisTestUtils.RunTestCase(AssumptionTwoVariablesIntervals_CASE);
        }

        readonly static TestCase AssumptionsMultipleUseIndirectVariable_CASE = @"
$a = $_GET[1];
$v = 'a';
if ($$v >= 5 && $$v <= 5) {
    $result = $a;
}
"
            .AssertVariable("result").HasUndefinedAndValues(5)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsMultipleUseIndirectVariable()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsMultipleUseIndirectVariable_CASE);
        }

        readonly static TestCase AssumptionsArrays_CASE = @"
$a[1] = ($_GET[1]) ? 1 : 2;
if ($a[1] == 1) $result = $a[1];
"
            .AssertVariable("result").HasUndefinedAndValues(1)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsArrays()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsArrays_CASE);
        }

        readonly static TestCase AssumptionsArrayTrueTest_CASE = @"
if ($_GET[1]) $arr[1] = 1;
// $arr[1] can have a value 1 or can be undefined

$result1 = 2;
if ($arr[1]) $result1 = $arr[1]; // the assumption should filter out the undefined value from $arr[1]

if ($_GET[1]) $arr[2] = 1;
else $arr[2] = 0;

$result2 = 2;
if ($arr[2]) $result2 = $arr[2]; // the assumption should filter out the value 0 from $arr[2]

if (!$arr[2]) $result3 = $arr[2];

"
            .AssertVariable("result1").HasValues(1, 2)
            .AssertVariable("result2").HasValues(1, 2)
            .AssertVariable("result3").HasUndefinedAndValues(0)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsArrayTrueTest()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsArrayTrueTest_CASE);
        }

        // True test for elements having AnyValue just filters out undefined value.
        // That is, after assuming an element holding AnyValue true, the subsequent true assumption is unknown.
        // However, the subsequent isset assumption is true.
        readonly static TestCase AssumptionsArrayAnyValueTrueTest_CASE = @"
if ($_GET[1]) {
    $result1 = isset($_GET[1]); // precisely computes true

    if ($_GET[1]) $result2 = 1;
    else $result2 = 2; // imprecision: just undefined value is filtered out from $_GET[1] from the first if => this branch can be taken
}
"
            .AssertVariable("result1").HasUndefinedAndValues(true)
            .AssertVariable("result2").HasUndefinedAndValues(1, 2) // not most possible precise result, overapproximation of our analyser
            //.AssertVariable("result2").HasUndefinedAndValues(1) // If the analyser was more precise, this would be correct result
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsArrayAnyValueTrueTest()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsArrayAnyValueTrueTest_CASE);
        }

        readonly static TestCase AssumptionsArrayIsset_CASE = @"
if ($_GET[1]) $arr[1] = -1;

if (isset($arr[1])) {
    $result0 = isset($arr[1]);
}

$result3 = 0;
if (!isset($arr[1])) {
    $result1 = isset($arr[1]);
    $arr[1] = 1;
    $result2 = isset($arr[1]);
    $result3 = $arr[1];
}
$result = isset($arr[1]);
"
            .AssertVariable("result0").HasUndefinedAndValues(true)
            .AssertVariable("result1").HasUndefinedAndValues(false)
            .AssertVariable("result2").HasUndefinedAndValues(true)
            .AssertVariable("result3").HasValues(0, 1)
            .AssertVariable("result").HasValues(true)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsArrayIsset()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsArrayIsset_CASE);
        }

        readonly static TestCase AssumptionsAnyArrayIsset_CASE = @"
if (isset($_SESSION[1])) {
    $result0 = isset($_SESSION[1]);
}

$result3 = 0;
if (!isset($_SESSION[1])) {
    $result1 = isset($_SESSION[1]);
    $_SESSION[1] = 1;
    $result2 = isset($_SESSION[1]);
    $result3 = $_SESSION[1];
}
$result = isset($_SESSION[1]);
"
            .AssertVariable("result0").HasUndefinedAndValues(true)
            .AssertVariable("result1").HasUndefinedAndValues(false)
            .AssertVariable("result2").HasUndefinedAndValues(true)
            .AssertVariable("result3").HasValues(0, 1)
            //.AssertVariable("result").HasValues(true)
            .Analysis(Analyses.WevercaAnalysisTest);
        [TestMethod]
        public void AssumptionsAnyArrayIsset()
        {
            AnalysisTestUtils.RunTestCase(AssumptionsAnyArrayIsset_CASE);
        }



        readonly static TestCase FunctionInCycle_CASE = @"

function f($a){
if ($a) { $x = 1; }
else { $x = 2; }

return $x;
}

$i = 0;
while($i < 5) {
f($_POST[$i]);
$i++;
}

$t = 1;
"
.AssertVariable("t").HasValues(1)
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\FunctionInCycle", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
.PrintSnapshotGraph(@"memory\FunctionInCycle")
#endif
;

        readonly static TestCase MultiCycle_CASE = @"

while ($i < 10) {
    $ii = 0;
    while ($ii < 3) {
        $ii++;
    }
    $i++;
}
$t = 1;
"
.AssertVariable("t").HasValues(1)
.AssertVariable("ii").HasValues(3)
#if ENABLE_GRAPH_VISUALISATION
.PrintProgramPointGraph(@"ppg\MultiCycle", typeof(ConstantPoint), typeof(VariablePoint), typeof(ItemUsePoint))
.PrintSnapshotGraph(@"memory\MultiCycle")
#endif
;
        
        [TestMethod]
        public void MultiCycle()
        {
            AnalysisTestUtils.RunTestCase(MultiCycle_CASE);
        }


        #endregion

        #region Worklist tests

        #region If worklist tests

        readonly static TestCase IfWorklist_CASE = @"
$a = $unknown;
if ($a) {
    $b = 1;
} else {
    $b = 2;
}

".AssertVariable("b").HasValues(1, 2)
 .AssertIterationCount();
        [TestMethod]
        public void IfWorklist()
        {
            AnalysisTestUtils.RunTestCase(IfWorklist_CASE);
        }

        readonly static TestCase IfWorklist2_CASE = @"
$a = $unknown;
if ($a) {
    $b = 1;
} else {
    $b = 2;
}
$c = 3;

".AssertVariable("b").HasValues(1, 2)
 .AssertVariable("c").HasValues(3)
 .AssertIterationCount();
        [TestMethod]
        public void IfWorklist2()
        {
            AnalysisTestUtils.RunTestCase(IfWorklist2_CASE);
        }

        readonly static TestCase IfShortCircuit1Worklist_CASE = @"
$a = $unknown;
if ($unknown && $a) {
    $b = 1;
} else {
    $b = 2;
}

".AssertVariable("b").HasValues(1, 2)
 .AssertIterationCount();
        [TestMethod]
        public void IfShortCircuitWorklist()
        {
            AnalysisTestUtils.RunTestCase(IfShortCircuit1Worklist_CASE);
        }

        readonly static TestCase IfShortCircuit2Worklist_CASE = @"
$a = $unknown;
if (false && $a) {
    $b = 1;
} else {
    $b = 2;
}

".AssertVariable("b").HasValues(2)
 .AssertIterationCount();
        [TestMethod]
        public void IfShortCircuit2Worklist()
        {
            AnalysisTestUtils.RunTestCase(IfShortCircuit2Worklist_CASE);
        }

        #endregion

        #region Switch worklist tests
        readonly static TestCase SwitchWorklist_CASE = @"
$a = $unknown;
switch ($a) {
    case 1:
        $b = 1;
        break;
    case 2:
        $b = 2;
        break;
}
$c = 1;    
".AssertVariable("b").HasUndefinedAndValues(1, 2)
 .AssertVariable("c").HasValues(1)
 .AssertIterationCount();
        [TestMethod]
        public void SwitchWorklist()
        {
            AnalysisTestUtils.RunTestCase(SwitchWorklist_CASE);
        }

        readonly static TestCase SwitchWithDefaultWorklist_CASE = @"
$a = $unknown;
switch ($a) {
    case 1:
        $b = 1;
    case 2:
        $b = 2;
        break;
    case 3:
        $b = 3;
    default:
        $b = 4;
}
$c = 1;
    
".AssertVariable("b").HasValues(2, 4)
 .AssertVariable("c").HasValues(1)
 .AssertIterationCount();
        [TestMethod]
        public void SwitchWithDefaultWorklist()
        {
            AnalysisTestUtils.RunTestCase(SwitchWithDefaultWorklist_CASE);
        }

        readonly static TestCase SwitchDefaultNotLastWorklist_CASE = @"
$a = $unknown;
switch ($a) {
    case 1:
        $b = 1;
    default:
        $b = 4;
    case 2:
        $b = 2;
        break;
    case 3:
        $b = 3;
}
$c = 1;
    
".AssertVariable("b").HasValues(2, 3)
 .AssertVariable("c").HasValues(1)
 .AssertIterationCount();
        [TestMethod]
        public void SwitchDefaultNotLastWorklist()
        {
            AnalysisTestUtils.RunTestCase(SwitchDefaultNotLastWorklist_CASE);
        }

        readonly static TestCase SwitchCertainWorklist_CASE = @"
$a = 1;
switch ($a) {
    case 1:
        $b = 1;
    default:
        $b = 4;
    case 2:
        $b = 2;
        break;
    case 3:
        $b = 3;
}
    
".AssertVariable("b").HasValues(2)
 .AssertIterationCount();
        [TestMethod]
        public void SwitchCertainWorklist()
        {
            AnalysisTestUtils.RunTestCase(SwitchCertainWorklist_CASE);
        }



        #endregion


        #region Foreach worklist tests

        readonly static TestCase ForeachWorklist_CASE = @"
$arr[1] = 1;
$arr[2] = 2;
foreach ($arr as $value) {
    if ($unknown == $value) $a = $value;
}
".AssertVariable("a").HasUndefinedAndValues(1, 2)
 .AssertIterationCount();
        [TestMethod]
        public void ForeachWorklist()
        {
            AnalysisTestUtils.RunTestCase(ForeachWorklist_CASE);
        }

        readonly static TestCase ForeachWithBreakWorklist_CASE = @"
$arr[1] = 1;
$arr[2] = 2;
foreach ($arr as $value) {
    if ($unknown == $value) $a = $value;
    break;    
}
".AssertIterationCount();
        [TestMethod]
        public void ForeachWithBreakWorklist()
        {
            AnalysisTestUtils.RunTestCase(ForeachWithBreakWorklist_CASE);
        }

        readonly static TestCase ForeachWithBreak2Worklist_CASE = @"
$arr[1] = 1;
$arr[2] = 2;
foreach ($arr as $value) {
    if ($unknown == $value) {
        $a = $value;
        break;
    }
}
".AssertIterationCount();
        [TestMethod]
        public void ForeachWithBreak2Worklist()
        {
            AnalysisTestUtils.RunTestCase(ForeachWithBreak2Worklist_CASE);
        }

        readonly static TestCase ForeachNestedWorklist_CASE = @"
$arr[1] = 1;
$arr[2] = 2;
$arr2[1] = 0;
$arr2[2] = 1;
foreach ($arr as $value) {
    if ($unknown == $value) $a = $value;
    foreach ($arr2 as $value2) {
        $b = 1;
        break;
    }
    break;
}
".AssertIterationCount();
        [TestMethod]
        public void ForeachNestedWorklist()
        {
            AnalysisTestUtils.RunTestCase(ForeachNestedWorklist_CASE);
        }

        #endregion

        #region For worklist tests
        readonly static TestCase ForWorklist_CASE = @"

for ($i = 0; $i <= $unknown; $i++) {
    $a = $i;
}
".AssertIterationCount();
        [TestMethod]
        public void ForWorklist()
        {
            AnalysisTestUtils.RunTestCase(ForWorklist_CASE);
        }
        #endregion

        #region While worklist tests
        readonly static TestCase WhileWorklist_CASE = @"
$i = 0;
while ($i <= $unknown) {
    $a = $i;
    $i++;
}
".AssertVariable("a")
 .AssertIterationCount();
        [TestMethod]
        public void WhileWorklist()
        {
            AnalysisTestUtils.RunTestCase(WhileWorklist_CASE);
        }
        #endregion


        #region Functions worklist tests

        readonly static TestCase FunctionWorklist_CASE = @"
function f() {
    if ($unknown) return;
    $a = 1;
    $b = 2;
}

f();
".AssertIterationCount();
        [TestMethod]
        public void FunctionWorklist()
        {
            AnalysisTestUtils.RunTestCase(FunctionWorklist_CASE);
        }

        readonly static TestCase IndirectFunctionsWorklist_CASE = @"
function f() {
    if ($unknown) return;
    $a = 1;
    $b = 2;
}
function g() {
    $a = 1;
}

$name = ($unknown) ? 'f' : 'g';
$name();
".AssertIterationCount();
        [TestMethod]
        public void IndirectFunctionsWorklist()
        {
            AnalysisTestUtils.RunTestCase(IndirectFunctionsWorklist_CASE);
        }

        #endregion

        #region Exit statements and exceptions
        readonly static TestCase ExitWorklist_CASE = @"
function f() {
if ($unknown) {
    switch ($unknown) {
        case 'editcom':
        case 'viewid':
            $mode = 1;
            break;
        default :
            exit();
    }   
} else {
    exit();
}
}

f();
".AssertIterationCount();
        [TestMethod]
        public void ExitWorklist()
        {
            AnalysisTestUtils.RunTestCase(ExitWorklist_CASE);
        }
        #endregion

        #endregion


        [TestMethod]
        public void SimpleAssignTest()
        {
            AnalysisTestUtils.RunTestCase(SimpleAssign_CASE);
        }
        
        [TestMethod]
        public void UnknownIndexAliasTest()
        {
            AnalysisTestUtils.RunTestCase(UnknownIndexAlias_CASE);
        }

        [TestMethod]
        public void NullReferenceErrorTest()
        {
            AnalysisTestUtils.RunTestCase(NullReferenceErrorTest_CASE);
        }

        [TestMethod]
        public void CycledMayAliasArray()
        {
            AnalysisTestUtils.RunTestCase(CycledMayAliasArray_CASE);
        }

        [TestMethod]
        public void ReturnArray()
        {
            AnalysisTestUtils.RunTestCase(ReturnArray_CASE);
        }

        [TestMethod]
        public void SelfConstant()
        {
            AnalysisTestUtils.RunTestCase(SelfConstant_CASE);
        }

        [TestMethod]
        public void IndexingField()
        {
            AnalysisTestUtils.RunTestCase(IndexingField_CASE);
        }

        
        [TestMethod]
        public void ArrayMayConditionalExpressionResult()
        {
            AnalysisTestUtils.RunTestCase(ArrayMayConditionalExpressionResult_CASE);
        }

        [TestMethod]
        public void ArrayConditionalExpressionResult()
        {
            AnalysisTestUtils.RunTestCase(ArrayConditionalExpressionResult_CASE);
        }

        [TestMethod]
        public void NestedConditionalExpressionResult()
        {
            AnalysisTestUtils.RunTestCase(NestedConditionalExpressionResult_CASE);
        }

        [TestMethod]
        public void ConditionalExpressionResult()
        {
            AnalysisTestUtils.RunTestCase(ConditionalExpressionResult_CASE);
        }
        
        [TestMethod]
        public void WhileCycleComputation()
        {
            AnalysisTestUtils.RunTestCase(WhileCycleComputation_CASE);
        }
        
        [TestMethod]
        public void StaticCallOnObjectInArray()
        {
            AnalysisTestUtils.RunTestCase(StaticCallOnObjectInArray_CASE);
        }

        [TestMethod]
        public void BranchedEvalAssign()
        {
            AnalysisTestUtils.RunTestCase(BranchedEvalAssign_CASE);
        }


        [TestMethod]
        public void EvalAssign()
        {
            AnalysisTestUtils.RunTestCase(EvalAssign_CASE);
        }

        [TestMethod]
        public void EvalReturn()
        {
            AnalysisTestUtils.RunTestCase(EvalReturn_CASE);
        }

        [TestMethod]
        public void TernaryOperator()
        {
            AnalysisTestUtils.RunTestCase(TernaryOperator_CASE);
        }

        [TestMethod]
        public void UndefinedAlias()
        {
            AnalysisTestUtils.RunTestCase(UndefinedAlias_CASE);
        }

        [TestMethod]
        public void UndefinedAliasMay()
        {
            AnalysisTestUtils.RunTestCase(UndefinedAliasMay_CASE);
        }

        [TestMethod]
        public void CycledAlias()
        {
            AnalysisTestUtils.RunTestCase(CycledAlias_CASE);
        }

        [TestMethod]
        public void CycledAlias2()
        {
            AnalysisTestUtils.RunTestCase(CycledAlias2_CASE);
        }

        [TestMethod]
        public void OverridenAlias()
        {
            AnalysisTestUtils.RunTestCase(OverridenAlias_CASE);
        }

        [TestMethod]
        public void FunctionTest()
        {
            AnalysisTestUtils.RunTestCase(FunctionTest_CASE);
        }

        [TestMethod]
        public void ArrayReturnValueTest()
        {
            AnalysisTestUtils.RunTestCase(ArrayReturnValueTest_CASE);
        }

        [TestMethod]
        public void ArrayMergeReturnValueTest()
        {
            AnalysisTestUtils.RunTestCase(ArrayMergeReturnValueTest_CASE);
        }

        [TestMethod]
        public void ArrayAliasedReturnValueTest()
        {
            AnalysisTestUtils.RunTestCase(ArrayAliasedReturnValueTest_CASE);
        }

        [TestMethod]
        public void BranchMerge()
        {
            AnalysisTestUtils.RunTestCase(BranchMerge_CASE);
        }

        [TestMethod]
        public void BranchMerge2()
        {
            AnalysisTestUtils.RunTestCase(BranchMerge2_CASE);
        }

        [TestMethod]
        public void BranchMergeWithUndefined()
        {
            AnalysisTestUtils.RunTestCase(BranchMergeWithUndefined_CASE);
        }

        [TestMethod]
        public void UnaryNegation()
        {
            AnalysisTestUtils.RunTestCase(UnaryNegation_CASE);
        }

        [TestMethod]
        public void NativeCallProcessing()
        {
            AnalysisTestUtils.RunTestCase(NativeCallProcessing_CASE);
        }

        [TestMethod]
        public void NativeCallProcessing2Arguments()
        {
            AnalysisTestUtils.RunTestCase(NativeCallProcessing2Arguments_CASE);
        }

        [TestMethod]
        public void NativeCallProcessingNestedCalls()
        {
            AnalysisTestUtils.RunTestCase(NativeCallProcessingNestedCalls_CASE);
        }

        [TestMethod]
        public void IndirectCall()
        {
            AnalysisTestUtils.RunTestCase(IndirectCall_CASE);
        }

        [TestMethod]
        public void BranchedIndirectCall()
        {
            AnalysisTestUtils.RunTestCase(BranchedIndirectCall_CASE);
        }

        [TestMethod]
        public void SingleBranchedIndirectCall()
        {
            AnalysisTestUtils.RunTestCase(SingleBranchedIndirectCall_CASE);
        }

        [TestMethod]
        public void IncompleteEvaluation()
        {
            AnalysisTestUtils.RunTestCase(IncompleteEvaluation_CASE);
        }

        [TestMethod]
        public void IncompleteEvaluationDontDie()
        {
            AnalysisTestUtils.RunTestCase(IncompleteEvaluationDontDie_CASE);
        }

        [TestMethod]
        public void IncompleteEvaluationDie()
        {
            AnalysisTestUtils.RunTestCase(IncompleteEvaluationDie_CASE);
        }

        [TestMethod]
        public void IncompleteEvaluationMay()
        {
            AnalysisTestUtils.RunTestCase(IncompleteEvaluationMay_CASE);
        }

        [TestMethod]
        public void MustAliasAssign()
        {
            AnalysisTestUtils.RunTestCase(MustAliasAssign_CASE);
        }


        [TestMethod]
        public void MayAliasAssignVirtualRefMM()
        {
            AnalysisTestUtils.RunTestCase(MayAliasAssignVirtualRefMM_CASE);
        }

        [TestMethod]
        public void MayAliasAssignCopyMM()
        {
            AnalysisTestUtils.RunTestCase(MayAliasAssignCopyMM_CASE);
        }

        [TestMethod]
        public void EqualsAssumption()
        {
            AnalysisTestUtils.RunTestCase(EqualsAssumption_CASE);
        }

        [TestMethod]
        public void DynamicEqualsAssumption()
        {
            AnalysisTestUtils.RunTestCase(DynamicEqualsAssumption_CASE);
        }

        [TestMethod]
        public void CallEqualsAssumption()
        {
            AnalysisTestUtils.RunTestCase(CallEqualsAssumption_CASE);
        }

        [TestMethod]
        public void MultiOr()
        {
            AnalysisTestUtils.RunTestCase(MultiOR_CASE);
        }

        [TestMethod]
        public void SwitchBranchProcessing()
        {
            AnalysisTestUtils.RunTestCase(SwitchBranchProcessing_CASE);
        }

        [TestMethod]
        public void ReverseCallEqualsAssumption()
        {
            AnalysisTestUtils.RunTestCase(ReverseCallEqualsAssumption_CASE);
        }

        [TestMethod]
        public void IndirectVarAssign()
        {
            AnalysisTestUtils.RunTestCase(IndirectVarAssign_CASE);
        }

        [TestMethod]
        public void MergedReturnValue()
        {
            AnalysisTestUtils.RunTestCase(MergedReturnValue_CASE);
        }

        [TestMethod]
        public void MergedFunctionDeclarations()
        {
            AnalysisTestUtils.RunTestCase(MergedFunctionDeclarations_CASE);
        }

        [TestMethod]
        public void ObjectFieldMerge()
        {
            AnalysisTestUtils.RunTestCase(ObjectFieldMerge_CASE);
        }

        [TestMethod]
        public void StaticFieldUse()
        {
            AnalysisTestUtils.RunTestCase(StaticFieldUse_CASE);
        }

        [TestMethod]
        public void IndirectStaticFieldUse()
        {
            AnalysisTestUtils.RunTestCase(IndirectStaticFieldUse_CASE);
        }


        [TestMethod]
        public void StringIndex()
        {
            AnalysisTestUtils.RunTestCase(StringIndex_CASE);
        }


        [TestMethod]
        public void ArrayFieldMerge()
        {
            AnalysisTestUtils.RunTestCase(ArrayFieldMerge_CASE);
        }

        [TestMethod]
        public void ImplicitArrayFieldMerge()
        {
            AnalysisTestUtils.RunTestCase(ImplicitArrayFieldMerge_CASE);
        }

        [TestMethod]
        public void ArrayFieldUpdateMultipleArrays()
        {
            AnalysisTestUtils.RunTestCase(ArrayFieldUpdateMultipleArrays_CASE);
        }

        [TestMethod]
        public void ObjectMethodCallMerge()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodCallMerge_CASE);
        }

        [TestMethod]
        public void ObjectMultipleObjectsInVariableRead()
        {
            AnalysisTestUtils.RunTestCase(ObjectMultipleObjectsInVariableRead_CASE);
        }

        [TestMethod]
        public void ObjectMultipleObjectsInVariableWrite()
        {
            AnalysisTestUtils.RunTestCase(ObjectMultipleObjectsInVariableWrite_CASE);
        }

        [TestMethod]
        public void ObjectMultipleObjectsInVariableMultipleVariablesWeakWrite()
        {
            AnalysisTestUtils.RunTestCase(ObjectMultipleObjectsInVariableMultipleVariablesWeakWrite_CASE);
        }

        [TestMethod]
        public void ObjectMultipleObjectsInVariableDifferentClassRead()
        {
            AnalysisTestUtils.RunTestCase(ObjectMultipleObjectsInVariableDifferentClassRead_CASE);
        }

        [TestMethod]
        public void ObjectMethodObjectSensitivity()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodObjectSensitivity_CASE);
        }

        [TestMethod]
        public void ObjectMethodObjectSensitivityMultipleVariables()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodObjectSensitivityMultipleVariables_CASE);
        }

        [TestMethod]
        public void ObjectMethodObjectSensitivityDifferentClass()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodObjectSensitivityDifferentClass_CASE);
        }

        [TestMethod]
        public void DynamicIncludeMerge()
        {
            AnalysisTestUtils.RunTestCase(DynamicIncludeMerge_CASE);
        }

        [TestMethod]
        public void SimpleInclude()
        {
            AnalysisTestUtils.RunTestCase(SimpleInclude_CASE);
        }

        [TestMethod]
        public void IncludeReturn()
        {
            AnalysisTestUtils.RunTestCase(IncludeReturn_CASE);
        }

        [TestMethod]
        public void XSSDirtyFlag()
        {
            AnalysisTestUtils.RunTestCase(XSSDirtyFlag_CASE);
        }

        [TestMethod]
        public void XSSSanitized()
        {
            AnalysisTestUtils.RunTestCase(XSSSanitized_CASE);
        }

        [TestMethod]
        public void XSSPossibleDirty()
        {
            AnalysisTestUtils.RunTestCase(XSSPossibleDirty_CASE);
        }

        [TestMethod]
        public void ConstantDeclaring()
        {
            AnalysisTestUtils.RunTestCase(ConstantDeclaring_CASE);
        }

        [TestMethod]
        public void BoolResolving()
        {
            AnalysisTestUtils.RunTestCase(BoolResolving_CASE);
        }        

        [TestMethod]
        public void NativeObjectUsage()
        {
            AnalysisTestUtils.RunTestCase(NativeObjectUsage_CASE);
        }

        [TestMethod]
        public void GlobalStatement()
        {
            AnalysisTestUtils.RunTestCase(GlobalStatement_CASE);
        }

        [TestMethod]
        public void SharedFunctionWithBranchingGlobal()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionWithBranchingGlobal_CASE);
        }

        [TestMethod]
        public void SharedFunction()
        {
            AnalysisTestUtils.RunTestCase(SharedFunction_CASE);
        }

        [TestMethod]
        public void SharedFunctionStrongUpdateLocal()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionStrongUpdateLocal_CASE);
        }

        [TestMethod]
        public void SharedFunctionStrongUpdateLocalUndef()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionStrongUpdateLocalUndef_CASE);
        }

        // Started failing after commit of prototype allocation-site abstraction in commit 969.
        [TestMethod]
        public void SharedFunctionStrongUpdateGlobalFailingAllocationsite969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionStrongUpdateGlobal_CASE);
        }

        // Started failing after commit of prototype allocation-site abstraction in commit 969.
        [TestMethod]
        public void SharedFunctionStrongUpdateGlobaUndefFailingAllocationsite969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionStrongUpdateGlobalUndef_CASE);
        }

        // Started failing after commit of prototype allocation-site abstraction in commit 969.
        [TestMethod]
        public void SharedFunctionGlobalVariableFailingAllocationsite969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionGlobalVariable_CASE);
        }

        // Started failing after commit of prototype allocation-site abstraction in commit 969.
        [TestMethod]
        public void SharedFunctionAliasinFailingAllocationsite969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasing_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingGlobal()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingGlobal_CASE);
        }

        // Started failing after commit of prototype allocation-site abstraction in commit 969.
        [TestMethod]
        public void SharedFunctionAliasingGlobalUndefFailingAllocationsite969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingGlobalUndef_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingGlobal2()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingGlobal2_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasing2FailingAllocationsite969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasing2_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingTwoArgumentsFailingAllocationsite969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingTwoArguments_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingTwoArgumentsUndefFailingAllocationsite969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingTwoArgumentsUndef_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingMayTwoArgumentsFailing969()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingMayTwoArguments_CASE);
        }

        [TestMethod]
        public void WriteArgument()
        {
            AnalysisTestUtils.RunTestCase(WriteArgument_CASE);
        }

        [TestMethod]
        public void ArgumentWrite_ExplicitAlias()
        {
            AnalysisTestUtils.RunTestCase(ArgumentWrite_ExplicitAlias_CASE);
        }

        [TestMethod]
        public void ArgumentWrite_ExplicitAliasToUndefined()
        {
            AnalysisTestUtils.RunTestCase(ArgumentWrite_ExplicitAliasToUndefined_CASE);
        }

        [TestMethod]
        public void ArgumentWrite_ExplicitAliasToUndefinedItem()
        {
            AnalysisTestUtils.RunTestCase(ArgumentWrite_ExplicitAliasToUndefinedItem_CASE);
        }

        [TestMethod]
        public void SimpleNew()
        {
            AnalysisTestUtils.RunTestCase(SimpleNew_CASE);
        }


        [TestMethod]
        public void IsSet()
        {
            AnalysisTestUtils.RunTestCase(IsSet_CASE);
        }

        [TestMethod]
        public void IndirectNewEx()
        {
            AnalysisTestUtils.RunTestCase(IndirectNewEx_CASE);
        }

        [TestMethod]
        public void StringConcatenation()
        {
            AnalysisTestUtils.RunTestCase(StringConcatenation_CASE);
        }

        [TestMethod]
        public void IncrementEval()
        {
            AnalysisTestUtils.RunTestCase(IncrementEval_CASE);
        }

        [TestMethod]
        public void DecrementEval()
        {
            AnalysisTestUtils.RunTestCase(DecrementEval_CASE);
        }

        [TestMethod]
        public void StringWithExpression()
        {
            AnalysisTestUtils.RunTestCase(StringWithExpression_CASE);
        }

        [TestMethod]
        public void LocalExceptionHandling()
        {
            AnalysisTestUtils.RunTestCase(LocalExceptionHandling_CASE);
        }

        [TestMethod]
        public void CrossStackExceptionHandling()
        {
            AnalysisTestUtils.RunTestCase(CrossStackExceptionHandling_CASE);
        }

        [TestMethod]
        public void LongLoopWidening()
        {
            AnalysisTestUtils.RunTestCase(LongLoopWidening_CASE);
        }

        [TestMethod]
        public void LongLoopWidenedVariableInLoopCondition()
        {
            AnalysisTestUtils.RunTestCase(LongLoopWidenedVariableInLoopCondition_CASE);
        }

        #region Function handling tests
        /// <summary>
        /// Tests whether the framework initializes only arguments of called function and not also arguments
        /// of other possible callees.
        /// </summary>
        [TestMethod]
        public void InitializingArgumentsOfOthersCallees()
        {
            AnalysisTestUtils.RunTestCase(InitializingArgumentsOfOthersCallees_CASE);
        }

        [TestMethod]
        public void ParametersByAliasLocal()
        {
            AnalysisTestUtils.RunTestCase(ParametersByAliasLocal_CASE);
        }
        [TestMethod]
        public void ParametersByAliasGlobal()
        {
            AnalysisTestUtils.RunTestCase(ParametersByAliasGlobal_CASE);
        }
        #endregion

        [TestMethod]
        public void ArrayCopySemantic()
        {
            AnalysisTestUtils.RunTestCase(ArrayCopySemantic_CASE);
        }

        [TestMethod]
        public void ArrayArray2ScalarMay()
        {
            AnalysisTestUtils.RunTestCase(ArrayArray2ScalarMay_CASE);
        }

        [TestMethod]
        public void ArrayArray2ScalarMust()
        {
            AnalysisTestUtils.RunTestCase(ArrayArray2ScalarMust_CASE);
        }

        [TestMethod]
        public void ArrayScalar2ArrayMay()
        {
            AnalysisTestUtils.RunTestCase(ArrayScalar2ArrayMay_CASE);
        }

        [TestMethod]
        public void ArrayScalar2ArrayMust()
        {
            AnalysisTestUtils.RunTestCase(ArrayScalar2ArrayMust_CASE);
        }

        [TestMethod]
        public void TransitiveAliasResolving()
        {
            AnalysisTestUtils.RunTestCase(TransitiveAliasResolving_CASE);
        }
        
        // TODO: this test fails, fix it
        [TestMethod]
        public void ArrayWithoutSpecifiedIndexAccess()
        {
            AnalysisTestUtils.RunTestCase(ArrayWithoutSpecifiedIndexAccess_CASE);
        }

    }
}