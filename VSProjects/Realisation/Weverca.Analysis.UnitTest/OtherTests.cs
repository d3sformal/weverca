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
    public class Other
    {
        string DieTest = @"
        $result=0;
        die();
        $result=1;
        ";


        [TestMethod]
        public void Die()
        {
            var result = TestUtils.ResultTest(DieTest);
            TestUtils.testType(result, typeof(IntegerValue));
            TestUtils.testValue(result, 0);
        }

        
    }
}
