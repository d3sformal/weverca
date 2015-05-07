/*
Copyright (c) 2012-2014 Matyas Brenner and David Hauzar

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
            var cfg = ControlFlowGraph.ControlFlowGraph.FromSource(parser.Ast, new FileInfo(fileName));

            var result = new ResultModel(phpCode);
            AnalysisWarningHandler.ResetWarnings();
            if (analysisModel.RunVerification)
            {
                result.Output = RunVerification(cfg, fileName, analysisModel.GetMemoryModel());
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

        static ProgramPointGraph Analyze(ControlFlowGraph.ControlFlowGraph entryMethod, string fileName, MemoryModels.MemoryModels memoryModel)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            var analysis = new ForwardAnalysis(entryMethod, memoryModel);
            analysis.Analyse();
            return analysis.ProgramPointGraph;
        }

        static string RunVerification(ControlFlowGraph.ControlFlowGraph controlFlowGraph, string fileName, MemoryModels.MemoryModels memoryModel)
        {

            var output = new WebOutput();
            try
            {
                var ppGraph = Analyze(controlFlowGraph, fileName, memoryModel);    
                if (ppGraph.End!=null)
                {
                    output.ProgramPointInfo("End point", ppGraph.End);
                }
                else
                {
                    output.Error("End point was not reached");
                }
            }
            catch (Exception e)
            {
                output.Error(e.Message);
            }
          
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
                        metricResult.Occurences = metricInfo.GetOccurrences(indicator);
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
                        metricResult.Occurences = metricInfo.GetOccurrences(quantity);
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
                        metricResult.Occurences = metricInfo.GetOccurrences(rating);
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