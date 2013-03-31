using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.CodeMetrics.UnitTest
{
    [TestClass]
    public class IndicatorCodeMetricTests
    {

        #region Eval indicator tests
        readonly IEnumerable<SourceTest> EvalPositiveTests = new SourceTest[]{
            new SourceTest("Test on eval detection outside function/method.", @"
                eval('$x=3');
            "),

            new SourceTest("Test on eval detection inside global function", @"
                function test($param){
                    eval($param);
                }
            "),

            new SourceTest("Test on eval detection inside method declaration", @"
                class testClass{
                    function testMethod($param){
                        eval($param);
                    }
                }
            ")
        };

        readonly IEnumerable<SourceTest> EvalNegativeTests = new SourceTest[]{
            new SourceTest("No eval is present",TestingUtilities.HelloWorldSource)
        };

        #endregion


        #region Session indicator tests
        readonly IEnumerable<SourceTest> SessionPositiveTests = new SourceTest[]{
            new SourceTest("Session function call outside function/method",@"
                session_start();
            "),
            new SourceTest("Session function call inside global function",@"
                function test($param){
                    session_write_close();
                }
            "),
            new SourceTest("Session function call inside method",@"
                class testClass{
                    function testMethod($param){
                        session_unset();
                    }
                }
            ")
        };

        readonly IEnumerable<SourceTest> SessionNegativeTests = new SourceTest[]{
            new SourceTest("No session is present",TestingUtilities.HelloWorldSource)
        };
        #endregion

        [TestMethod]
        public void Eval()
        {
            var hasEval = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.Eval);
            var doesntHaveEval = TestingUtilities.GetNegation(hasEval);

            TestingUtilities.RunTests(hasEval, EvalPositiveTests);
            TestingUtilities.RunTests(doesntHaveEval, EvalNegativeTests);
        }

        [TestMethod]
        public void Session()
        {
            var hasSession = TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.Session);
            var doesntHaveSession = TestingUtilities.GetNegation(hasSession);

            TestingUtilities.RunTests(hasSession, SessionPositiveTests);
            TestingUtilities.RunTests(doesntHaveSession, SessionNegativeTests);
        }
    }
}
