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
    public class FlagTester
    {

        string SimpleFlagTest = @"
          $result=$_POST['x'];
        ";

        [TestMethod]
        public void SimpleFlag()
        {
            var result = TestUtils.ResultTest(SimpleFlagTest);
            TestUtils.IsDirty(result);
           
        }

        string SimpleFlagTest2 = @"
          $result=substr ($_POST['x'],0);
        ";

        [TestMethod]
        public void SimpleFlag2()
        {
            var result = TestUtils.ResultTest(SimpleFlagTest2);
            TestUtils.IsDirty(result);

        }

        string SimpleFlagTest3 = @"
          $result=strlen ($_POST['x']);
        ";

        [TestMethod]
        public void SimpleFlag3()
        {
            var result = TestUtils.ResultTest(SimpleFlagTest3);
            TestUtils.IsClean(result);

        }

        string SimpleFlagTest4 = @"
          $result= $_POST['x'].'a';
        ";

        [TestMethod]
        public void SimpleFlag4()
        {
            var result = TestUtils.ResultTest(SimpleFlagTest4);
            TestUtils.IsDirty(result);

        }

    }
}
