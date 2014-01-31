using System;
using System.IO;
using System.Linq;

using PHP.Core;

using Weverca.Analysis;
using Weverca.AnalysisFramework;
using Weverca.CodeMetrics;
using Weverca.Parsers;
using Weverca.Web.Models;

namespace Weverca.Web.Definitions
{
    static class Analyzer
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static ResultModel Run(string phpCode, AnalysisModel analysisModel)
        {
            string fileName = @"\UserInput\userInput.php";

            SyntaxParser parser = InitializeParser(phpCode, fileName);
            var cfg = ControlFlowGraph.ControlFlowGraph.FromSource(parser.Ast);

            var result = new ResultModel(phpCode);

            if (analysisModel.RunVerification)
            {
                result.Output = RunVerification(cfg, fileName);
            }

            if (analysisModel.RunMetrics)
            {
                RunMetrics(parser, analysisModel, result);
            }

            result.LoadWarnings();

            return result;
        }

        static SyntaxParser InitializeParser(string phpCode, string fileName)
        {
            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            SyntaxParser parser = new SyntaxParser(source_file, phpCode);
            parser.Parse();
            if (parser.Ast == null)
            {
                throw new ArgumentException("The specified input cannot be parsed.");
            }

            return parser;
        }

        static ProgramPointGraph Analyze(ControlFlowGraph.ControlFlowGraph entryMethod, string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            var analysis = new ForwardAnalysis(entryMethod, MemoryModels.MemoryModels.VirtualReferenceMM, fileInfo);
            analysis.Analyse();
            return analysis.ProgramPointGraph;
        }

        static string RunVerification(ControlFlowGraph.ControlFlowGraph controlFlowGraph, string fileName)
        {
            var ppGraph = Analyze(controlFlowGraph, fileName);
            var output = new WebOutput();
            var graphWalker = new CallGraphPrinter(ppGraph);
            graphWalker.Run(output);
            return output.Output;
        }

        static void RunMetrics(SyntaxParser parser, AnalysisModel analysisModel, ResultModel result)
        {
            MetricInfo metricInfo = MetricInfo.FromParsers(true, parser);

            if (analysisModel.RunIndicatorMetrics)
            {
                foreach (ConstructIndicator indicator in Enum.GetValues(typeof(ConstructIndicator)))
                {
                    try
                    {
                        MetricResult<bool> metricResult = new MetricResult<bool>();
                        metricResult.Occurences = metricInfo.GetOccurances(indicator);
                        metricResult.Result = metricResult.Occurences.Count() > 0;
                        result.IndicatorMetricsResult.Add(indicator, metricResult);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(string.Format("Metrics \"{0}\" failed", indicator), ex);
                    }
                }
            }

            if (analysisModel.RunQuantityMetrics)
            {
                foreach (Quantity quantity in Enum.GetValues(typeof(Quantity)))
                {
                    try
                    {
                        MetricResult<int> metricResult = new MetricResult<int>();
                        metricResult.Result = metricInfo.GetQuantity(quantity);
                        metricResult.Occurences = metricInfo.GetOccurances(quantity);
                        result.QuantityMetricsResult.Add(quantity, metricResult);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(string.Format("Metrics \"{0}\" failed", quantity), ex);
                    }
                }
            }

            if (analysisModel.RunRatingMetrics)
            {
                foreach (Rating rating in Enum.GetValues(typeof(Rating)))
                {
                    try
                    {
                        MetricResult<double> metricResult = new MetricResult<double>();
                        metricResult.Result = metricInfo.GetRating(rating);
                        metricResult.Occurences = metricInfo.GetOccurances(rating);
                        result.RatingMetricsResult.Add(rating, metricResult);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorException(string.Format("Metrics \"{0}\" failed", rating), ex);
                    }
                }
            }
        }
    }
}