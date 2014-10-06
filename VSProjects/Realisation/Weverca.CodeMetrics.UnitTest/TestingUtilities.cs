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
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.UnitTest
{
    internal delegate bool MetricPredicate(MetricInfo info);

    internal static class TestingUtilities
    {
        /// <summary>
        /// Keeps UID for test files. Is incremented whenever new file is generated.
        /// </summary>
        private static int testFileUID = 0;

        /// <summary>
        /// Source of hello world php source.
        /// </summary>
        internal const string HelloWorldSource = @"echo 'Hello world';";

        /// <summary>
        /// Test given source according to predicate. On fail throws assertion error with testDescription.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="test"></param>
        internal static void RunTest(MetricPredicate predicate, SourceTest test)
        {
            var metricInfo = GetInfo(test);
            Assert.IsTrue(predicate(metricInfo), test.Description);
        }

        /// <summary>
        /// Test given source tests against predicate. On fail throws assertion error with failed test description.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="tests"></param>
        internal static void RunTests(MetricPredicate predicate, params SourceTest[] tests)
        {
            foreach (var test in tests)
            {
                RunTest(predicate, test);
            }
        }

        /// <summary>
        /// Test given source tests against predicate. On fail throws assertion error with failed test description.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="tests"></param>
        internal static void RunTests(MetricPredicate predicate, IEnumerable<SourceTest> tests)
        {
            RunTests(predicate, tests.ToArray());
        }

        /// <summary>
        /// Returns metric info created from given test.
        /// </summary>
        /// <param name="test">Test which source will be used for metric info generating.</param>
        /// <returns>Generated metric info.</returns>
        internal static MetricInfo GetInfo(SourceTest test)
        {
            var uid = GetTestFileUID();
            var fileName = string.Format(CultureInfo.InvariantCulture, "./test{0}.php", uid);
            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)),
                new FullPath(fileName));

            using (var parser = new SyntaxParser(sourceFile, "<?php " + test.SourceCode + " ?>"))
            {
                return MetricInfo.FromParsers(true, parser);
            }
        }

        /// <summary>
        /// Get predicate for testing of presence given indicator.
        /// </summary>
        /// <param name="indicator"></param>
        /// <returns></returns>
        internal static MetricPredicate GetContainsIndicatorPredicate(ConstructIndicator indicator)
        {
            return (info) => info.HasIndicator(indicator);
        }

        /// <summary>
        /// Get predicate for testing quantity metric value.
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static MetricPredicate GetQuantityPredicate(Quantity quantity, int value)
        {
            return (info) => info.GetQuantity(quantity) == value;
        }

        /// <summary>
        /// Get negation to given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static MetricPredicate GetNegation(MetricPredicate predicate)
        {
            return (info) => !predicate(info);
        }

        /// <summary>
        /// Get UID for test file.
        /// </summary>
        /// <returns>UID for test file.</returns>
        private static int GetTestFileUID()
        {
            return ++testFileUID;
        }
    }

    /// <summary>
    /// Represents test on source code.
    /// </summary>
    internal class SourceTest
    {
        /// <summary>
        /// Description for this test.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Source code for this test.
        /// </summary>
        public readonly string SourceCode;

        /// <summary>
        /// Create source test with given description and source code.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="sourceCode"></param>
        public SourceTest(string description, string sourceCode)
        {
            this.Description = description;
            this.SourceCode = sourceCode;
        }
    }
}