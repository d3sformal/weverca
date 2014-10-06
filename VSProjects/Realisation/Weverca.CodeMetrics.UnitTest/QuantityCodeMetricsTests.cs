/*
Copyright (c) 2012-2014 Miroslav Vodolan, David Skorvaga.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Weverca.CodeMetrics.UnitTest
{
    [TestClass]
    public class QuantityCodeMetricsTests
    {
        #region Max depth inheritance tests

        private readonly IEnumerable<SourceTest> inheritanceDepth2 = new SourceTest[]
        {
            new SourceTest("Simple class inheritance", @"
                class A{
                }
                class B extends A{
                }
            "),

            new SourceTest("Interface class inheritance", @"
                interface I{
                }
                interface I2{
                }

                class A implements I,I2{
                }
            "),

             new SourceTest("Mixed interface and class inheritance", @"
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

        private readonly IEnumerable<SourceTest> inheritanceDepth0 = new SourceTest[]
        {
            new SourceTest("No type inheritance", TestingUtilities.HelloWorldSource)
        };

        #endregion Max depth inheritance tests

        #region Number of lines tests

        private SourceTest numberOfLines4 = new SourceTest("Simple lines counting", @"
            class A{
            }
        ");

        private SourceTest numberOfLines5 = new SourceTest("Simple lines counting", @"
            class B{
                function foo(){}
            }
        ");

        #endregion Number of lines tests

        #region Number of source tests

        private SourceTest singleSource = new SourceTest("Single source", TestingUtilities.HelloWorldSource);

        #endregion Number of source tests

        #region Max method overriding depth

        private SourceTest[] overridingDepth0 = new SourceTest[]
        {
            new SourceTest("no method", TestingUtilities.HelloWorldSource),
            new SourceTest("no overriding", @"
                class B{
                    function foo(){}
                }

                class C extends B{ function aaa(){} }
            ")
        };

        private SourceTest[] overridingDepth1 = new SourceTest[]
        {
            new SourceTest("single tree", @"
                class Foo {
                    function myFoo() {
                        return ""Foo"";
                    }
                }

                class Bar extends Foo {
                    function myFoo() {
                        return ""Bar"";
                    }
                }"),
            new SourceTest("two trees", @"
                class Foo {
                    function myFoo() {
                        return ""Foo"";
                    }
                }

                class Bar extends Foo {
                    function myFoo() {
                        return ""Bar"";
                    }
                }

                class B{
                    function foo(){}
                }")
        };

        private SourceTest[] overridingDepth2 = new SourceTest[]
        {
            new SourceTest("single tree, depth 2", @"
                class Foo {
                    function myFoo() {
                        return ""Foo"";
                    }
                }

                class Bar extends Foo {
                    function myFoo() {
                        return ""Bar"";
                    }
                }

                class BarB extends Foo {
                    function myFoo() {
                        return ""BarB"";
                    }
                }

                class BarBar extends Bar {
                    function myFoo() {
                        return ""BarBar"";
                    }
                }")
        };

        #endregion Max method overriding depth

        [TestMethod]
        public void InheritanceDepth()
        {
            var inheritance0Predicate
                = TestingUtilities.GetQuantityPredicate(Quantity.MaxInheritanceDepth, 0);
            var inheritance2Predicate
                = TestingUtilities.GetQuantityPredicate(Quantity.MaxInheritanceDepth, 2);

            TestingUtilities.RunTests(inheritance0Predicate, inheritanceDepth0);
            TestingUtilities.RunTests(inheritance2Predicate, inheritanceDepth2);
        }

        [TestMethod]
        public void NumberOfLines()
        {
            var lines4Predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfLines, 4);
            var lines5Predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfLines, 5);
            var lines9Predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfLines, 9);

            TestingUtilities.RunTests(lines4Predicate, numberOfLines4);
            TestingUtilities.RunTests(lines5Predicate, numberOfLines5);

            // Test merging
            var info4 = TestingUtilities.GetInfo(numberOfLines4);
            var info5 = TestingUtilities.GetInfo(numberOfLines5);
            var info9 = info4.Merge(info5);

            Assert.IsTrue(lines9Predicate(info9), "Merging two files");
        }

        [TestMethod]
        public void NumberOfSources()
        {
            var infoA = TestingUtilities.GetInfo(singleSource);
            var infoB = TestingUtilities.GetInfo(singleSource);
            var infoAB = infoA.Merge(infoB);
            var infoAAB = infoA.Merge(infoAB);

            var sourceCn1Predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfSources, 1);
            var sourceCn2Predicate = TestingUtilities.GetQuantityPredicate(Quantity.NumberOfSources, 2);

            Assert.IsTrue(sourceCn1Predicate(infoA), "Single source A");
            Assert.IsTrue(sourceCn1Predicate(infoB), "Single source B");
            Assert.IsTrue(sourceCn2Predicate(infoAB), "Merged A B sources");
            Assert.IsTrue(sourceCn2Predicate(infoAAB), "Merged A A B source");
        }

        [TestMethod]
        public void MaxMethodOverridingDepth()
        {
            var overriding0Predicate
                = TestingUtilities.GetQuantityPredicate(Quantity.MaxMethodOverridingDepth, 0);
            var overriding1Predicate
                = TestingUtilities.GetQuantityPredicate(Quantity.MaxMethodOverridingDepth, 1);
            var overriding2Predicate
                = TestingUtilities.GetQuantityPredicate(Quantity.MaxMethodOverridingDepth, 2);

            TestingUtilities.RunTests(overriding0Predicate, overridingDepth0);
            TestingUtilities.RunTests(overriding1Predicate, overridingDepth1);
            TestingUtilities.RunTests(overriding2Predicate, overridingDepth2);
        }
    }
}