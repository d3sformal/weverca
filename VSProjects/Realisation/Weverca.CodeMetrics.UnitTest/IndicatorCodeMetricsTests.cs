using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.CodeMetrics.UnitTest
{
    [TestClass]
    public class IndicatorCodeMetricTests
    {

        #region Eval testing sources
        readonly string EvalSimple = @"
            eval('$x=3');
        ";
        readonly string EvalSimple_description = @"Test on eval detection outside function/method.";

        readonly string EvalInFunction = @"
            function test($param){
                eval($param);
            }
        ";
        readonly string EvalInFunction_description = @"Test on eval detection inside function declaration";

        readonly string EvalInMethod = @"
            class testClass{
                function testMethod($param){
                    eval($param);
                }
            }
        ";
        readonly string EvalInMethod_description = @"Test on eval detection inside method declaration";
        #endregion

        [TestMethod]
        public void Eval()
        {
            var hasEval=TestingUtilities.GetContainsIndicatorPredicate(ConstructIndicator.Eval);
            var doesntHaveEval=TestingUtilities.GetNegation(hasEval);

            TestingUtilities.RunTest(hasEval, EvalSimple, EvalSimple_description);
            TestingUtilities.RunTest(hasEval, EvalInFunction, EvalInFunction_description);
            TestingUtilities.RunTest(hasEval, EvalInMethod, EvalInMethod_description);

            TestingUtilities.RunTest(doesntHaveEval, TestingUtilities.HelloWorldSource, "No eval is present");
        }
    }
}
