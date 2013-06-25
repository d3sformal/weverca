using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.UnitTest
{
    [TestClass]
    public class ForwardAnalysisTest
    {
        readonly static string ParallelBlock_CODE = @"
$str='f1';
if($unknown){
    $str='f1a';
}else{
    $str='f1b';
}
";

        readonly static string NativeCallProcessing_CODE = @"
$call_result=strtolower('TEST');
";

readonly static string NativeCallProcessing2Arguments_CODE = @"
$call_result=concat('A','B');
";


        [TestMethod]
        public void BranchMerge()
        {
            var outSet = AnalysisTestUtils.GetEndPointOutSet(ParallelBlock_CODE);
            
            outSet.AssertVariable<string>(
                "str","Merging if branches",
                "f1a","f1b"
                );
        }

        [TestMethod]
        public void NativeCallProcessing()
        {
             var outSet = AnalysisTestUtils.GetEndPointOutSet(NativeCallProcessing_CODE);

             outSet.AssertVariable<string>(
                 "call_result","Processing native strtolower call",
                 "test"
                 );
        }

        [TestMethod]
        public void NativeCallProcessing2Arguments()
        {
            var outSet = AnalysisTestUtils.GetEndPointOutSet(NativeCallProcessing2Arguments_CODE);

             outSet.AssertVariable<string>(
                  "call_result", "Processing native concat call",
                  "AB"
                  );
        }



    }
}
