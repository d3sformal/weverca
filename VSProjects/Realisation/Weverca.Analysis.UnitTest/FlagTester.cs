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
            $a=$_POST['x'];
            $b=$a;
            $c=$b;
            $result=$c;
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

        string SimpleFlagTest5 = @"
          $result= htmlspecialchars($_POST['x']);
        ";

        [TestMethod]
        public void SimpleFlag5()
        {
            var result = TestUtils.ResultTest(SimpleFlagTest5);
            TestUtils.IsClean(result, DirtyType.HTMLDirty);

        }

        string SimpleFlagTest6 = @"
          $result=mysql_escape_string($_POST['x']);
        ";

        [TestMethod]
        public void SimpleFlag6()
        {
            var result = TestUtils.ResultTest(SimpleFlagTest6);
            TestUtils.IsClean(result, DirtyType.SQLDirty);

        }


        string SimpleFlagTest7 = @"
            $result= htmlspecialchars($_POST['x']);
            echo $result;
        ";

        [TestMethod]
        public void SimpleFlag7()
        {
            var result = TestUtils.Analyze(SimpleFlagTest7);
            Debug.Assert(TestUtils.ContainsSecurityWarning(result, DirtyType.HTMLDirty) == false);

        }

        string SimpleFlagTest8 = @"
            $result=mysql_escape_string($_POST['x']);
            echo $result;
        ";

        [TestMethod]
        public void SimpleFlag8()
        {
            var result = TestUtils.Analyze(SimpleFlagTest8);
            Debug.Assert(TestUtils.ContainsSecurityWarning(result, DirtyType.HTMLDirty));

        }

        string SimpleFlagTest9 = @"
            mysql_query('select *  from x where a='.$_POST['x']);
    
        ";


        [TestMethod]
        public void SimpleFlag9()
        {
            var result = TestUtils.Analyze(SimpleFlagTest9);
            Debug.Assert(TestUtils.ContainsSecurityWarning(result, DirtyType.SQLDirty));

        }


    }
}
