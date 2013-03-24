using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Parsers;
using Weverca.CodeMetrics.Processing;


namespace Weverca.CodeMetrics
{
    /// <summary>
    /// Class representing code metrics for specified sources.
    /// NOTE: Is immutable.
    /// </summary>
    public class MetricInfo
    {
        #region Private members

        /// <summary>
        /// Here is stored info about indicators
        /// </summary>
        readonly IndicatorProcessor.ResultBatch indicatorBatch;

        #endregion

        #region MetricInfo constructors
        /// <summary>
        /// Creates metric info for given sources.
        /// </summary>
        /// <param name="resolveOccurances"></param>
        /// <param name="parser"></param>
        private MetricInfo(bool resolveOccurances, SyntaxParser parser)
        {
            if (!parser.IsParsed)
            {
                //we need content in parser to be parsed
                parser.Parse();
            }

            indicatorBatch = ProcessingServices.ProcessIndicators(resolveOccurances, parser);
        }

        private MetricInfo(IndicatorProcessor.ResultBatch indicatorBatch)
        {
            this.indicatorBatch = indicatorBatch;
        }
        #endregion

        #region MetricInfo API

        public static MetricInfo FromParsers(bool resolveOccurances, params SyntaxParser[] parsers)
        {
            if (parsers.Length < 1)
            {
                throw new ArgumentException("Needs at least one syntax parser specified");
            }

            var buffer = new MetricInfo(resolveOccurances, parsers[0]);

            //merge info for all parsers together
            for (var i = 1; i < parsers.Length; ++i)
            {
                buffer = MergeWithParser(resolveOccurances, buffer, parsers[i]);
            }

            return buffer;
        }


        /// <summary>
        /// Determine that metric info has specified indicator set on true
        /// </summary>
        /// <param name="indicators"></param>
        /// <returns></returns>
        public bool HasIndicators(ConstructIndicator indicator)
        {
            return indicatorBatch.GetResult(indicator).Property;          
        }

        /// <summary>
        /// Create new metric info by merging this info with given other info.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public MetricInfo Merge(MetricInfo other)
        {
            var resultIndicators = ProcessingServices.MergeIndicators(this.indicatorBatch, other.indicatorBatch);
            //TODO other metric batches

            return new MetricInfo(resultIndicators);
        }
        #endregion

        #region Utility methods
        /// <summary>
        /// Merge given metric with new metric created from parser
        /// </summary>
        /// <param name="resolveOccurances"></param>
        /// <param name="metric"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        private static MetricInfo MergeWithParser(bool resolveOccurances, MetricInfo metric, SyntaxParser parser)
        {
            var metricInfo = new MetricInfo(resolveOccurances, parser);
            return metric.Merge(metricInfo);
        }
        #endregion
    }
}
