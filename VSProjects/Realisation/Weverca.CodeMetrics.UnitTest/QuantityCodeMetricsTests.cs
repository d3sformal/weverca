using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.CodeMetrics.UnitTest
{
    [TestClass]
    public class QuantityCodeMetricTests
    {
        
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
        public void InheritanceDepth()
        {
            var inheritance0_predicate = TestingUtilities.GetQuantityPredicate(Quantity.MaxInheritanceDepth, 0);
            var inheritance2_predicate = TestingUtilities.GetQuantityPredicate(Quantity.MaxInheritanceDepth, 2);

            TestingUtilities.RunTests(inheritance0_predicate, InheritanceDepth0);
            TestingUtilities.RunTests(inheritance2_predicate, InheritanceDepth2);
        }
    }
}
