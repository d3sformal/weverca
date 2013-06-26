using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.UnitTest
{
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

        readonly static TestCase NativeCallProcessing_CASE = @"
$call_result=strtolower('TEST');
".AssertVariable("call_result").HasValues("test");
        
        readonly static TestCase NativeCallProcessing2Arguments_CASE = @"
$call_result=concat('A','B');
".AssertVariable("call_result").HasValues("AB");

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
".AssertVariable("call_result").HasValues("TEST","test");


        [TestMethod]
        public void BranchMerge()
        {
            AnalysisTestUtils.RunTestCase(ParallelBlock_CASE);
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
        public void IndirectCall()
        {
            AnalysisTestUtils.RunTestCase(IndirectCall_CASE);
        }

        [TestMethod]
        public void BranchedIndirectCall()
        {
            AnalysisTestUtils.RunTestCase(BranchedIndirectCall_CASE);
        }

    }
}
