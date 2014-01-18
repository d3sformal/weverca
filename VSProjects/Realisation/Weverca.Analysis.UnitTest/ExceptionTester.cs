using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Summary description for ExceptionTester
    /// </summary>
    [TestClass]
    public class ExceptionTester
    {

        string ThrownStringTest = @"
           try
        {
            $x='x';
            throw $x;
        }
        catch(Exception $e)
        {

        }
        ";

        [TestMethod]
        public void ThrownString()
        {
            var outset = TestUtils.Analyze(ThrownStringTest);
            Debug.Assert(TestUtils.ContainsWarning(outset, AnalysisWarningCause.ONLY_OBJECT_CAM_BE_THROWN));
        }

    }
}
