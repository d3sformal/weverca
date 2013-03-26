using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.CodeMetrics.UnitTest
{
    [TestClass]
    public class IndicatorCodeMetricTests
    {

        #region Eval metric tests
        readonly IEnumerable<SourceTest> EvalPositiveTests = new SourceTest[]{
            new SourceTest("Test on eval detection outside function/method.", @"
                eval('$x=3');
            "),

            new SourceTest("Test on eval detection inside function declaration", @"
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

        #region Max depth inheritance tests
        readonly IEnumerable<SourceTest> InheritanceDepth2 = new SourceTest[]{
            new SourceTest("Simple class inheritance",@"
                class A{
                }
                class B extends A{
                }
            "),

        };

        readonly IEnumerable<SourceTest> InheritanceDepth0 = new SourceTest[]{
            new SourceTest("No type inheritance",TestingUtilities.HelloWorldSource)
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
        public void InheritanceDepth()
        {
            var inheritance0_predicate = TestingUtilities.GetQuantityPredicate(Quantity.MaxInheritanceDepth, 0);
            var inheritance2_predicate = TestingUtilities.GetQuantityPredicate(Quantity.MaxInheritanceDepth, 2);

            TestingUtilities.RunTests(inheritance0_predicate, InheritanceDepth0);
            TestingUtilities.RunTests(inheritance2_predicate, InheritanceDepth2);
        }
    }
}
