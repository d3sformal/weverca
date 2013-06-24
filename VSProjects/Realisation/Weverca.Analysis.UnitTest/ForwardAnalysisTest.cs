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

        [TestMethod]
        public void BranchMerge()
        {
            var snapshot = AnalysisTestUtils.GetEndPointSnapshot(ParallelBlock_CODE);
            
            snapshot.AssertVariable<string>(
                "str","Merging branches failed",
                "f1a","f1b"
                );
        }
    }
}
