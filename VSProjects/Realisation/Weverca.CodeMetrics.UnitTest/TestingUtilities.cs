using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.UnitTest
{
    delegate bool MetricPredicate(MetricInfo info);

    static class TestingUtilities
    {
        /// <summary>
        /// Keeps UID for test files. Is incremented whenever new file is generated.
        /// </summary>
        private static int testFileUID = 0;
        /// <summary>
        /// Source of hello world php source
        /// </summary>
        internal static readonly string HelloWorldSource = @"echo 'Hello world';";
        /// <summary>
        /// Test given source according to predicate. On fail throws assertion error with testDescrpition        
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="sourceCode"></param>
        /// <param name="testDescription"></param>
        internal static void RunTest(MetricPredicate predicate, SourceTest test)
        {
            var metricInfo = GetInfo(test);
            Assert.IsTrue(predicate(metricInfo), test.Description);
        }

        /// <summary>
        /// Test given source tests against predicate. On fail throws assertion error with failed test description
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
        /// Test given source tests against predicate. On fail throws assertion error with failed test description
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="tests"></param>
        internal static void RunTests(MetricPredicate predicate, IEnumerable<SourceTest> tests)
        {
            RunTests(predicate, tests.ToArray());
        }

        /// <summary>
        /// Returns metric info created from given test
        /// </summary>
        /// <param name="test">Test which source will be used for metric info generating.</param>
        /// <returns>Generated metric info.</returns>
        internal static MetricInfo GetInfo(SourceTest test)
        {
            var uid = getTestFileUID();
            string fileName = string.Format("./test{0}.php",uid);            
            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            var parser = new SyntaxParser(source_file, "<?php " + test.SourceCode + " ?>");
            return MetricInfo.FromParsers(true, parser);
        }

        /// <summary>
        /// Get predicate for testing of presence given indicator
        /// </summary>
        /// <param name="indicator"></param>
        /// <returns></returns>
        internal static MetricPredicate GetContainsIndicatorPredicate(ConstructIndicator indicator)
        {
            return (info) => info.HasIndicator(indicator);
        }

        /// <summary>
        /// Get predicate for testing quantity metric value
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static MetricPredicate GetQuantityPredicate(Quantity quantity, int value)
        {
            return (info) => info.GetQuantity(quantity) == value;
        }

        /// <summary>
        /// Get negation to given predicate
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
        /// <returns>UID for test file</returns>
        private static int getTestFileUID()
        {
            return ++testFileUID;
        }
    }

    /// <summary>
    /// Represents test on source code
    /// </summary>
    class SourceTest
    {
        /// <summary>
        /// Descrpition for this test.
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
