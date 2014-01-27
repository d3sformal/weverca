using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// NOTE: Variable unknown is set by default as non-deterministic (AnyValue)
    /// </summary>
    [TestClass]
    public class ForwardAnalysisTest
    {
        readonly static TestCase BranchMerge_CASE = @"
$str='f1';
if($unknown){
    $str='f1a';
}else{
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

        readonly static TestCase NativeCallProcessing2Arguments_CASE = @"
$call_result=concat('A','B');
".AssertVariable("call_result").HasValues("AB");

        readonly static TestCase NativeCallProcessingNestedCalls_CASE = @"
$call_result=concat(strtolower('Ab'),strtoupper('Cd'));
".AssertVariable("call_result").HasValues("abCD");

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
 MemoryModel(MemoryModels.MemoryModels.CopyMM);

        readonly static TestCase EqualsAssumption_CASE = @"
$Var='init';
if($unknown=='PossibilityA'){
    $Var=$unknown;
}
".AssertVariable("Var").HasValues("init", "PossibilityA");

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
 .SetNonDeterministic("VarA", "VarB");

        readonly static TestCase CallEqualsAssumption_CASE = @"
if($unknown==strtolower(""TestValue"")){
    $Output=$unknown;
}
".AssertVariable("Output").HasUndefinedOrValues("testvalue");

        readonly static TestCase ReverseCallEqualsAssumption_CASE = @"
if(abs($unknown)==5){
    $Output=$unknown;
}

".AssertVariable("Output").HasUndefinedOrValues(5, -5);


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
".AssertVariable("FieldValue").HasValues("value");

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
".AssertVariable("FieldValue").HasValues("value1", "value2");

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
".AssertVariable("FieldValue").HasValues("newValue");

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
            .AssertVariable("FieldValueB2").HasValues("newValue");

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
".AssertVariable("FieldValue").HasValues("newValue");


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

        readonly static TestCase SimpleXSSDirty_CASE = @"
$x=$_POST['dirty'];
$x=$x;
".AssertVariable("x").IsXSSDirty().Analysis(Analyses.SimpleAnalysis);


        readonly static TestCase XSSSanitized_CASE = @"
$x=$_POST['dirty'];
$x='sanitized';
".AssertVariable("x").IsXSSClean().Analysis(Analyses.SimpleAnalysis);

        readonly static TestCase XSSPossibleDirty_CASE = @"
$x=$_POST['dirty'];
if($unknown){
    $x='sanitized';
}
".AssertVariable("x").IsXSSDirty().Analysis(Analyses.SimpleAnalysis);


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
".AssertVariable("test").HasValues("init", "val1", "val2", "val3");

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
function sharedFn($arg){
    $arg = 'fromSharedFunc';
}

$a = 'initA';
$b = 'initB';
sharedFn(&$a);
sharedFn(&$b);
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
 ;

        #endregion

        #endregion



        readonly static TestCase WriteArgument_CASE = @"
$argument=""Value"";
write_argument($argument);

".AssertVariable("argument").HasValues("Value_WrittenInArgument");

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
while($i<1000){
    ++$i;
}
$test2='Reachable';

".AssertVariable("test").HasValues("NotAffected")
 .AssertVariable("test2").HasValues("Reachable")
 //|.WideningLimit(20)
 .WideningLimit(1) // for debuging
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


        [TestMethod]
        public void FunctionTest()
        {
            AnalysisTestUtils.RunTestCase(FunctionTest_CASE);
        }

        [TestMethod]
        public void BranchMerge()
        {
            AnalysisTestUtils.RunTestCase(BranchMerge_CASE);
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
        public void IncludeReturn()
        {
            AnalysisTestUtils.RunTestCase(IncludeReturn_CASE);
        }

        [TestMethod]
        public void SimpleXSSDirty()
        {
            AnalysisTestUtils.RunTestCase(SimpleXSSDirty_CASE);
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
        public void ForeachIteration()
        {
            AnalysisTestUtils.RunTestCase(ForeachIteration_CASE);
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

        [TestMethod]
        public void SharedFunctionStrongUpdateGlobal()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionStrongUpdateGlobal_CASE);
        }

        [TestMethod]
        public void SharedFunctionStrongUpdateGlobaUndef()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionStrongUpdateGlobalUndef_CASE);
        }

        [TestMethod]
        public void SharedFunctionGlobalVariable()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionGlobalVariable_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasing()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasing_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingGlobal()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingGlobal_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingGlobalUndef()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingGlobalUndef_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingGlobal2()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingGlobal2_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasing2()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasing2_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingTwoArguments()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingTwoArguments_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingTwoArgumentsUndef()
        {
            AnalysisTestUtils.RunTestCase(SharedFunctionAliasingTwoArgumentsUndef_CASE);
        }

        [TestMethod]
        public void SharedFunctionAliasingMayTwoArguments()
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
        public void TransitiveAliasResolving()
        {
            AnalysisTestUtils.RunTestCase(TransitiveAliasResolving_CASE);
        }

    }
}
