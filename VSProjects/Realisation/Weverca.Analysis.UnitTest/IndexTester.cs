using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class IndexTester
    {
        string IndexIntergerTest = @"
            $a=4;
            $p=$a[""a""];    
        ";


        [TestMethod]
        public void IndexInterger()
        {
            var result = TestUtils.Analyze(IndexIntergerTest);
            Debug.Assert(TestUtils.ContainsWarning(result, AnalysisWarningCause.CANNOT_ACCESS_FIELD_OPERATOR_ON_NON_ARRAY));

        }

        string IndexIntergerTest2 = @"
            $a=4;
            $a[""a""]=15;    
        ";


        [TestMethod]
        public void IndexInterger2()
        {
            var result = TestUtils.Analyze(IndexIntergerTest2);
            Debug.Assert(TestUtils.ContainsWarning(result, AnalysisWarningCause.CANNOT_ACCESS_FIELD_OPERATOR_ON_NON_ARRAY));

        }


    }
}
