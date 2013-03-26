using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Parsers;
using Weverca.CodeMetrics.Processing;
using Weverca.CodeMetrics.Processing.Implementations;


namespace Weverca.CodeMetrics
{
    /// <summary>
    /// Class representing code metrics for specified sources.
    /// NOTE: Is immutable.
    /// </summary>
    public class MetricInfo
    {
        /// <summary>
        /// Determine that occurances has been resolved
        /// </summary>
        public readonly bool HasResolvedOccurances;

        #region ResultBatches collected from MetricProcessors

        /// <summary>
        /// Here is stored info about indicators
        /// </summary>
        readonly IndicatorProcessor.ResultBatch indicatorBatch;
        /// <summary>
        /// Here is stored info about ratings
        /// </summary>
        readonly RatingProcessor.ResultBatch ratingBatch;
        /// <summary>
        /// Here is stored info about quantities
        /// </summary>
        readonly QuantityProcessor.ResultBatch quantityBatch;

        #endregion

        #region Included sources
        /// <summary>
        /// Contains syntax parsers according to included source files
        /// </summary>
        Dictionary<PhpSourceFile, SyntaxParser> includedFiles = new Dictionary<PhpSourceFile, SyntaxParser>();
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
            ratingBatch = ProcessingServices.ProcessRatings(resolveOccurances, parser);
            quantityBatch = ProcessingServices.ProcessQuantities(resolveOccurances, parser);

            HasResolvedOccurances = resolveOccurances;
            includeParser(parser);
        }

        /// <summary>
        /// Creates metric info from given indicators
        /// </summary>
        /// <param name="indicatorBatch"></param>
        /// <param name="ratingBatch"></param>
        /// <param name="quantityBatch"></param>
        private MetricInfo(
            IndicatorProcessor.ResultBatch indicatorBatch,
            RatingProcessor.ResultBatch ratingBatch,
            QuantityProcessor.ResultBatch quantityBatch,
            IEnumerable<SyntaxParser> includedParsers,
            bool hasResolvedOccurances
            )
        {
            this.indicatorBatch = indicatorBatch;
            this.ratingBatch = ratingBatch;
            this.quantityBatch = quantityBatch;
            this.HasResolvedOccurances = hasResolvedOccurances;
        }
        #endregion

        #region MetricInfo API

        /// <summary>
        /// Create MetricInfo which is merged from given parsers
        /// </summary>
        /// <param name="resolveOccurances">Determine that info will contains occurances of nodes related to metric</param>
        /// <param name="parsers">Parsers to be merged by metric info</param>
        /// <returns>Metric info according to parsers</returns>
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

        #region Indicator metrics API
        /// <summary>
        /// Determine that metric info has specified indicator set on true
        /// </summary>
        /// <param name="indicators"></param>
        /// <returns></returns>
        public bool HasIndicator(ConstructIndicator indicator)
        {
            return indicatorBatch.GetResult(indicator).Property;
        }

        /// <summary>
        /// Get occurances for given metric.
        /// NOTE: Occurance resolving has to be enabled when creating metric info for GetOccurances usage.
        /// </summary>
        /// <param name="indicator">Metrich whic occurances will be returned</param>
        /// <returns>Resolved occurances.</returns>
        public IEnumerable<AstNode> GetOccurances(ConstructIndicator indicator)
        {
            throwOnUnresolvedOccurances();
            return indicatorBatch.GetResult(indicator).Occurances;
        }
        #endregion

        #region Rating metrics API
        /// <summary>
        /// Returns value of given rating.
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public double GetRating(Rating rating)
        {
            return ratingBatch.GetResult(rating).Property;
        }

        /// <summary>
        /// Get occurances for given metric.
        /// NOTE: Occurance resolving has to be enabled when creating metric info for GetOccurances usage.
        /// </summary>
        /// <param name="rating">Metrich whic occurances will be returned</param>
        /// <returns>Resolved occurances.</returns>
        public IEnumerable<AstNode> GetOccurances(Rating rating)
        {
            throwOnUnresolvedOccurances();
            return ratingBatch.GetResult(rating).Occurances;
        }
        #endregion

        #region Quantity metrics API
        /// <summary>
        /// Returns quantity of given quantitative metric.
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public int GetQuantity(Quantity quantity)
        {
            return quantityBatch.GetResult(quantity).Property;
        }

        /// <summary>
        /// Get occurances for given metric.
        /// NOTE: Occurance resolving has to be enabled when creating metric info for GetOccurances usage.
        /// </summary>
        /// <param name="quantity">Metrich whic occurances will be returned</param>
        /// <returns>Resolved occurances.</returns>
        public IEnumerable<AstNode> GetOccurances(Quantity quantity)
        {
            throwOnUnresolvedOccurances();
            return quantityBatch.GetResult(quantity).Occurances;
        }
        #endregion

        /// <summary>
        /// Determine that given file is included in metric info
        /// </summary>
        /// <param name="file">File to be checked.</param>
        /// <returns>True if file is included, false otherwise</returns>
        public bool IsFileIncluded(PhpSourceFile file)
        {
            return includedFiles.ContainsKey(file);
        }

        /// <summary>
        /// Create new metric info by merging this info with given other info.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public MetricInfo Merge(MetricInfo other)
        {
            var commonFiles = includedFiles.Keys.Intersect(other.includedFiles.Keys);
            if (commonFiles.Count() > 0)
            {
                var notIncludedParsers = new List<SyntaxParser>();
                var notInlcudedFiles = other.includedFiles.Keys.Except(commonFiles);
                foreach (var file in notInlcudedFiles)
                {
                    notIncludedParsers.Add(other.includedFiles[file]);
                }
                other = FromParsers(other.HasResolvedOccurances, notIncludedParsers.ToArray());
            }

            var resultIndicators = ProcessingServices.MergeIndicators(this.indicatorBatch, other.indicatorBatch);
            var resultRatings = ProcessingServices.MergeRatings(this.ratingBatch, other.ratingBatch);
            var resultQuantities = ProcessingServices.MergeQuantities(this.quantityBatch, other.quantityBatch);
            var includedParsers = includedFiles.Values.Union(other.includedFiles.Values);

            return new MetricInfo(resultIndicators, resultRatings, resultQuantities, includedParsers, HasResolvedOccurances && other.HasResolvedOccurances);
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

        /// <summary>
        /// Add file parsed by parser into includedFiles
        /// </summary>
        /// <param name="parser"></param>
        private void includeParser(SyntaxParser parser)
        {
            var sourceFile = parser.Ast.SourceUnit.SourceFile;
            includedFiles[sourceFile] = parser;
        }

        /// <summary>
        /// Throws exception if occurances hasn't been resolved.
        /// </summary>
        private void throwOnUnresolvedOccurances()
        {
            if (!HasResolvedOccurances)
            {
                throw new NotSupportedException("Cannot get occurances, when they hasn't been resolved");
            }
        }
        #endregion
    }
}
