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
        readonly static TestCase ParallelBlock_CASE = @"
$str='f1';
if($unknown){
    $str='f1a';
}else{
    $str='f1b';
}
".AssertVariable("str").HasValues("f1a", "f1b");

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
        /// This is virtual reference model specific test
        /// </summary>
        readonly static TestCase MayAliasAssign_CASE = @"
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
 .AssertVariable("VarC").HasValues("ValueC", "Assigned");

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
".AssertVariable("OutputA").HasValues("Value1", "Value2")
 .AssertVariable("OutputB").HasValues("Value1", "Value2")
 .SetNonDeterministic("VarA", "VarB");

        readonly static TestCase CallEqualsAssumption_CASE = @"
if($unknown==strtolower(""TestValue"")){
    $Output=$unknown;
}
".AssertVariable("Output").HasValues("testvalue");

        readonly static TestCase ReverseCallEqualsAssumption_CASE = @"
if(abs($unknown)==5){
    $Output=$unknown;
}

".AssertVariable("Output").HasValues(5, -5);


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
if($unknown){
    $arr[0]='ValueA';
}else{
    $arr[0]='ValueB';
}

$ArrayValue=$arr[0];
".AssertVariable("ArrayValue").HasValues("ValueA", "ValueB");


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
".AssertVariable("x").IsXSSDirty();


        readonly static TestCase XSSSanitized_CASE = @"
$x=$_POST['dirty'];
$x='sanitized';
".AssertVariable("x").IsXSSClean();

        readonly static TestCase XSSPossibleDirty_CASE = @"
$x=$_POST['dirty'];
if($unknown){
    $x='sanitized';
}
".AssertVariable("x").IsXSSDirty();


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

foreach($arr as $value){
    if($unknown ==  $value){
        $test=$value;
    }
}
".AssertVariable("test").HasValues("val1", "val2", "val3");

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


        readonly static TestCase SharedFunction_CASE = @"
function sharedFn($arg){
    return $arg;
}

if($unknown){
    $resultA=sharedFn('ValueA');
}else{
    $resultB=sharedFn('ValueB');
}

"
         .AssertVariable("resultA").HasValues("ValueA", "ValueB")
         .AssertVariable("resultB").HasValues("ValueA", "ValueB")
         .ShareFunctionGraph("sharedFn")
         ;

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



        [TestMethod]
        public void BranchMerge()
        {
            AnalysisTestUtils.RunTestCase(ParallelBlock_CASE);
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
        public void MayAliasAssign()
        {
            AnalysisTestUtils.RunTestCase(MayAliasAssign_CASE);
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
        public void ObjectMethodCallMerge()
        {
            AnalysisTestUtils.RunTestCase(ObjectMethodCallMerge_CASE);
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
        public void SharedFunction()
        {
            AnalysisTestUtils.RunTestCase(SharedFunction_CASE);
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
    }
}
