using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.CodeMetrics.UnitTest
{
    [TestClass]
    public class QuantityCodeMetricTests
    {

        #region Max depth inheritance tests
        readonly IEnumerable<SourceTest> InheritanceDepth_2 = new SourceTest[]{
            new SourceTest("Simple class inheritance",@"
                class A{
                }
                class B extends A{
                }
            "),

            new SourceTest("Interface class inheritance",@"
                interface I{
                }
                interface I2{
                }

                class A implements I,I2{
                }    
            "),

             new SourceTest("Mixed interface and class inheritance",@"
                interface I{
                }
                interface I2{
                }
                class A{
                }
                class B extends A implements I,I2{
                }    
            ")

        };
        
        readonly IEnumerable<SourceTest> InheritanceDepth_0 = new SourceTest[]{
            new SourceTest("No type inheritance",TestingUtilities.HelloWorldSource)
        };
        #endregion

        #region Number of lines tests
        SourceTest NumberOfLines_4 = new SourceTest("Simple lines counting", @"
            class A{
            }
        ");
        SourceTest NumberOfLines_5 = new SourceTest("Simple lines counting", @"
            class B{
                function foo(){}
            }
        ");
        #endregion

        #region Number of source tests 
            SourceTest SingleSource= new SourceTest("Single source", TestingUtilities.HelloWorldSource);
        #endregion

        [TestMethod]
        public void InheritanceDepth()
        {
            var inheritance0_predicate = TestingUtilities.GetQuantityPredicate(Quantity.MaxInheritanceDepth, 0);
            var inheritance2_predicate = TestingUtilities.GetQuantityPredicate(Quantity.MaxInheritanceDepth, 2);

            TestingUtilities.RunTests(inheritance0_predicate, InheritanceDepth_0);
            TestingUtilities.RunTests(inheritance2_predicate, InheritanceDepth_2);
        }

        [TestMethod]
        public void NumberOfLines()
        {
            var lines4_predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfLines, 4);
            var lines5_predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfLines, 5);
            var lines9_predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfLines, 9);

            TestingUtilities.RunTests(lines4_predicate, NumberOfLines_4);
            TestingUtilities.RunTests(lines5_predicate, NumberOfLines_5);

            //test merging
            var info_4=TestingUtilities.GetInfo(NumberOfLines_4);
            var info_5=TestingUtilities.GetInfo(NumberOfLines_5);
            var info_9 = info_4.Merge(info_5);

            Assert.IsTrue(lines9_predicate(info_9),"Merging two files");
        }

        [TestMethod]
        public void NumberOfSources()
        {
            var infoA = TestingUtilities.GetInfo(SingleSource);
            var infoB = TestingUtilities.GetInfo(SingleSource);
            var infoAB = infoA.Merge(infoB);
            var infoAAB = infoA.Merge(infoAB);

            var sourceCn1_predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfSources, 1);
            var sourceCn2_predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfSources, 2);

            Assert.IsTrue(sourceCn1_predicate(infoA),"Single source A");
            Assert.IsTrue(sourceCn1_predicate(infoB), "Single source B");
            Assert.IsTrue(sourceCn2_predicate(infoAB), "Merged A B sources");
            Assert.IsTrue(sourceCn2_predicate(infoAAB), "Merged A A B source");
        }
    }
}
