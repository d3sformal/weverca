/*
Copyright (c) 2012-2014 Miroslav Vodolan, Matyas Brenner, David Skorvaga, David Hauzar.

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
using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.CodeMetrics.Processing;
using Weverca.Parsers;

namespace Weverca.CodeMetrics
{
    /// <summary>
    /// Class representing code metrics for specified sources. It is immutable.
    /// </summary>
    /// <remarks>
    /// The class store information about result of analysis. There are three categories of metrics:
    /// Ratings, quantities and indicators. The object can be create only by static constructor
    /// <see cref="FromParsers" /> that executes analysis and gather results. They are then accessible.
    /// </remarks>
    public class MetricInfo
    {
        /// <summary>
        /// Determine that occurrences has been resolved.
        /// </summary>
        public readonly bool HasResolvedOccurrences;

        #region ResultBatches collected from MetricProcessors

        /// <summary>
        /// Info about indicators.
        /// </summary>
        private readonly IndicatorProcessor.ResultBatch indicatorBatch;

        /// <summary>
        /// Info about ratings.
        /// </summary>
        private readonly RatingProcessor.ResultBatch ratingBatch;

        /// <summary>
        /// Info about quantities.
        /// </summary>
        private readonly QuantityProcessor.ResultBatch quantityBatch;

        #endregion ResultBatches collected from MetricProcessors

        #region Included sources

        /// <summary>
        /// Contains syntax parsers according to included source files.
        /// </summary>
        private Dictionary<PhpSourceFile, SyntaxParser> includedFiles
            = new Dictionary<PhpSourceFile, SyntaxParser>();

        #endregion Included sources

        #region MetricInfo constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricInfo" /> class.
        /// Creates metric info for given sources.
        /// </summary>
        /// <param name="resolveOccurances">
        /// Determine whether evidences of occurrences in sources code should be stored.
        /// </param>
        /// <param name="parser">Syntax parser of source code.</param>
        private MetricInfo(bool resolveOccurances, SyntaxParser parser)
        {
            if (!parser.IsParsed)
            {
                // We need content in parser to be parsed
                parser.Parse();
            }
            
            indicatorBatch = ProcessingServices.ProcessIndicators(resolveOccurances, parser);
            ratingBatch = ProcessingServices.ProcessRatings(resolveOccurances, parser);
            quantityBatch = ProcessingServices.ProcessQuantities(resolveOccurances, parser);

            HasResolvedOccurrences = resolveOccurances;
            IncludeParsers(parser);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricInfo" /> class.
        /// Creates metric info from given indicators.
        /// </summary>
        /// <param name="indicatorBatch">Info about indicators.</param>
        /// <param name="ratingBatch">Info about ratings.</param>
        /// <param name="quantityBatch">Info about quantities.</param>
        /// <param name="includedParsers">Syntax parsers of included source files.</param>
        /// <param name="hasResolvedOccurances">
        /// Determine whether info will contains occurrences of nodes related to metric.
        /// </param>
        private MetricInfo(
            IndicatorProcessor.ResultBatch indicatorBatch,
            RatingProcessor.ResultBatch ratingBatch,
            QuantityProcessor.ResultBatch quantityBatch,
            IEnumerable<SyntaxParser> includedParsers,
            bool hasResolvedOccurances)
        {
            this.indicatorBatch = indicatorBatch;
            this.ratingBatch = ratingBatch;
            this.quantityBatch = quantityBatch;
            this.HasResolvedOccurrences = hasResolvedOccurances;
            IncludeParsers(includedParsers.ToArray());
        }

        #endregion MetricInfo constructors

        #region MetricInfo API

        /// <summary>
        /// Create MetricInfo which is merged from given parsers.
        /// </summary>
        /// <param name="resolveOccurrences">
        /// Determine whether info will contains occurrences of nodes related to metric.
        /// </param>
        /// <param name="parsers">Parsers to be merged by metric info.</param>
        /// <returns>Metric info according to parsers.</returns>
        public static MetricInfo FromParsers(bool resolveOccurrences, params SyntaxParser[] parsers)
        {
            if (parsers.Length < 1)
            {
                throw new ArgumentException("Needs at least one syntax parser specified");
            }

            var buffer = new MetricInfo(resolveOccurrences, parsers[0]);

            // merge info for all parsers together
            for (var i = 1; i < parsers.Length; ++i)
            {
                buffer = MergeWithParser(resolveOccurrences, buffer, parsers[i]);
            }

            return buffer;
        }

        #region Indicator metrics API

        /// <summary>
        /// Determine that metric info has specified indicator set on true.
        /// </summary>
        /// <param name="indicator">A concrete metric from indicators category.</param>
        /// <returns><c>true</c> whether construct appeared in source code.</returns>
        public bool HasIndicator(ConstructIndicator indicator)
        {
            return indicatorBatch.GetResult(indicator).Property;
        }

        /// <summary>
        /// Get occurrences for given metric.
        /// NOTE: Occurrence resolving has to be enabled when creating metric info for GetOccurrences usage.
        /// </summary>
        /// <param name="indicator">Metric which occurrences will be returned.</param>
        /// <returns>Resolved occurrences.</returns>
        public IEnumerable<AstNode> GetOccurrences(ConstructIndicator indicator)
        {
            ThrowOnUnresolvedOccurrences();
            return indicatorBatch.GetResult(indicator).Occurrences;
        }
        #endregion

        #region Rating metrics API

        /// <summary>
        /// Returns value of given rating.
        /// </summary>
        /// <param name="rating">A concrete metric from ratings category.</param>
        /// <returns>Rating value of the metric.</returns>
        public double GetRating(Rating rating)
        {
            return ratingBatch.GetResult(rating).Property;
        }

        /// <summary>
        /// Get occurrences for given metric.
        /// NOTE: Occurrence resolving has to be enabled when creating metric info for GetOccurrences usage.
        /// </summary>
        /// <param name="rating">Metric which occurrences will be returned.</param>
        /// <returns>Resolved occurrences.</returns>
        public IEnumerable<AstNode> GetOccurrences(Rating rating)
        {
            ThrowOnUnresolvedOccurrences();
            return ratingBatch.GetResult(rating).Occurrences;
        }
        #endregion

        #region Quantity metrics API

        /// <summary>
        /// Returns quantity of given quantitative metric.
        /// </summary>
        /// <param name="quantity">A concrete metric from quantities category.</param>
        /// <returns>Number representing any property of source code.</returns>
        public int GetQuantity(Quantity quantity)
        {
            return quantityBatch.GetResult(quantity).Property;
        }

        /// <summary>
        /// Get occurrences for given metric.
        /// NOTE: Occurrence resolving has to be enabled when creating metric info for GetOccurrences usage.
        /// </summary>
        /// <param name="quantity">Metric which occurrences will be returned.</param>
        /// <returns>Resolved occurrences.</returns>
        public IEnumerable<AstNode> GetOccurrences(Quantity quantity)
        {
            ThrowOnUnresolvedOccurrences();
            return quantityBatch.GetResult(quantity).Occurrences;
        }
        #endregion

        /// <summary>
        /// Determine that given file is included in metric info.
        /// </summary>
        /// <param name="file">File to be checked.</param>
        /// <returns>True if file is included, false otherwise.</returns>
        public bool IsFileIncluded(PhpSourceFile file)
        {
            return includedFiles.ContainsKey(file);
        }

        /// <summary>
        /// Create new metric info by merging this info with given other info.
        /// </summary>
        /// <param name="other">The other <see cref="MetricInfo" /> object.</param>
        /// <returns><see cref="MetricInfo" /> merged from two other objects.</returns>
        public MetricInfo Merge(MetricInfo other)
        {
            var commonFiles = includedFiles.Keys.Intersect(other.includedFiles.Keys);
            if (commonFiles.Count() > 0)
            {
                var notIncludedParsers = new Queue<SyntaxParser>();
                var notInlcudedFiles = other.includedFiles.Keys.Except(commonFiles);
                foreach (var file in notInlcudedFiles)
                {
                    notIncludedParsers.Enqueue(other.includedFiles[file]);
                }

                other = FromParsers(other.HasResolvedOccurrences, notIncludedParsers.ToArray());
            }

            var resultIndicators = ProcessingServices.MergeIndicators(indicatorBatch, other.indicatorBatch);
            var resultRatings = ProcessingServices.MergeRatings(ratingBatch, other.ratingBatch);
            var resultQuantities = ProcessingServices.MergeQuantities(quantityBatch, other.quantityBatch);
            var includedParsers = includedFiles.Values.Union(other.includedFiles.Values);

            return new MetricInfo(resultIndicators, resultRatings, resultQuantities, includedParsers,
                HasResolvedOccurrences && other.HasResolvedOccurrences);
        }

        #endregion MetricInfo API

        #region Utility methods

        /// <summary>
        /// Merge given metric with new metric created from parser.
        /// </summary>
        /// <param name="resolveOccurrences"></param>
        /// <param name="metric"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        private static MetricInfo MergeWithParser(bool resolveOccurrences, MetricInfo metric,
            SyntaxParser parser)
        {
            var metricInfo = new MetricInfo(resolveOccurrences, parser);
            return metric.Merge(metricInfo);
        }

        /// <summary>
        /// Add files parsed by parsers into <c>includedFiles</c>.
        /// </summary>
        /// <param name="parsers">Syntax parsers of source code.</param>
        private void IncludeParsers(params SyntaxParser[] parsers)
        {
            foreach (var parser in parsers)
            {
                var sourceFile = parser.Ast.SourceUnit.SourceFile;
                includedFiles[sourceFile] = parser;
            }
        }

        /// <summary>
        /// Throws exception if occurrences has not been resolved.
        /// </summary>
        private void ThrowOnUnresolvedOccurrences()
        {
            if (!HasResolvedOccurrences)
            {
                throw new NotSupportedException("Cannot get occurrences, when they have not been resolved");
            }
        }

        #endregion Utility methods
    }
}