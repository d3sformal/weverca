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
        /// <param name="source"></param>
        /// <param name="testDescription"></param>
        internal static void RunTest(MetricPredicate predicate, string source, string testDescription)
        {
            string fileName = "./file.php";
            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            var parser = new SyntaxParser(source_file,"<?php "+ source+" ?>");            
            var metricInfo = MetricInfo.FromParsers(true, parser);

            Assert.IsTrue(predicate(metricInfo), testDescription);
        }

        internal static MetricPredicate GetContainsIndicatorPredicate(ConstructIndicator indicator)
        {
            return (info) => info.HasIndicator(indicator);
        }

        internal static MetricPredicate GetNegation(MetricPredicate predicate)
        {
            return (info) => !predicate(info);
        }
    }
}
