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
        /// Source of hello world php source
        /// </summary>
        internal static readonly string HelloWorldSource = @"echo 'Hello world';";
        /// <summary>
        /// Test given source according to predicate. On fail throws assertion error with testDescrpition        
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="sourceCode"></param>
        /// <param name="testDescription"></param>
        internal static void RunTest(MetricPredicate predicate, string sourceCode, string testDescription)
        {
            string fileName = "./file.php";
            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            var parser = new SyntaxParser(source_file,"<?php "+ sourceCode+" ?>");            
            var metricInfo = MetricInfo.FromParsers(true, parser);

            Assert.IsTrue(predicate(metricInfo), testDescription);
        }

        /// <summary>
        /// Test given source tests against predicate. On fail throws assertion error with failed test description
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="tests"></param>
        internal static void RunTests(MetricPredicate predicate, IEnumerable<SourceTest> tests)
        {
            foreach (var test in tests)
            {
                RunTest(predicate, test.SourceCode, test.SourceCode);
            }
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
        /// Get negation to given predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static MetricPredicate GetNegation(MetricPredicate predicate)
        {
            return (info) => !predicate(info);
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
