using System;
using System.Collections.Generic;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing
{
    /// <summary>
    /// Common ancestor for all processors that create some concrete metric.
    /// </summary>
    /// <remarks>
    /// This stateless class does two things: It processes source code to gain a result of a metric
    /// and merges more results to one. Both things can differ for every metric. <see cref="Process"/>
    /// method process the metric for the given source code. Every derived class overrides the method
    /// where is the main functionality of every metric processor. Merging is performed, because program
    /// can be divided into more files or fragments (e.g. via inclusion) sMerging of results depends
    /// on type of metric property. Usually, all metric in one category has the same merging method.
    /// </remarks>
    /// <typeparam name="TMetricCategory">Metric category which processors will be collected.</typeparam>
    /// <typeparam name="TPropertyType">Type of property, which metric works with.</typeparam>
    internal abstract class MetricProcessor<TMetricCategory, TPropertyType>
        where TMetricCategory : struct, IComparable, IFormattable, IConvertible
    {
        /// <summary>
        /// The results of processing one metric.
        /// </summary>
        /// <remarks>
        /// Every metric has a result that consist of a value, that can be measured and list of occurrences
        /// of the given property that the metric investigates.
        /// </remarks>
        /// <seealso cref="ResultBatch" />
        public struct Result
        {
            /// <summary>
            /// Property of processed metric.
            /// </summary>
            public readonly TPropertyType Property;

            /// <summary>
            /// Occurrences of a property. It is <c>null</c> if and only if resolving of them was disabled.
            /// </summary>
            public readonly IEnumerable<AstNode> Occurrences;

            /// <summary>
            /// Initializes a new instance of the <see cref="Result" /> struct.
            /// </summary>
            /// <param name="property">Property of processed metric.</param>
            /// <param name="occurrences">Occurrences of a property.</param>
            public Result(TPropertyType property, IEnumerable<AstNode> occurrences)
            {
                Property = property;
                Occurrences = occurrences;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Result" /> struct.
            /// </summary>
            /// <param name="property">Property of processed metric.</param>
            public Result(TPropertyType property)
            {
                Property = property;
                Occurrences = null;
            }

            /// <summary>
            /// Gets a value indicating whether result has enabled occurrence resolving.
            /// </summary>
            public bool HasResolvedOccurances
            {
                get
                {
                    return Occurrences != null;
                }
            }
        }

        /// <summary>
        /// List of results of the same metric category.
        /// </summary>
        /// <seealso cref="Result" />
        public class ResultBatch
        {
            /// <summary>
            /// Data which were collected by processors.
            /// </summary>
            private Dictionary<TMetricCategory, Result> results = new Dictionary<TMetricCategory, Result>();

            /// <summary>
            /// Gets a value indicating whether the batch is already frozen - its values cannot be changed.
            /// </summary>
            public bool IsFrozen { get; private set; }

            /// <summary>
            /// Freeze the results. The results then cannot be changes.
            /// </summary>
            /// <exception cref="System.InvalidOperationException">Thrown when results are frozen.</exception>
            public void Freeze()
            {
                if (IsFrozen)
                {
                    throw new InvalidOperationException("Cannot freeze batch twice");
                }

                IsFrozen = true;
            }

            /// <summary>
            /// Insert result of the given metric. It can override the old result.
            /// </summary>
            /// <param name="key">Metric identification.</param>
            /// <param name="value">Result of the given metric.</param>
            /// <exception cref="System.InvalidOperationException">Thrown when results are frozen.</exception>
            public void Insert(TMetricCategory key, Result value)
            {
                if (IsFrozen)
                {
                    throw new InvalidOperationException("Cannot insert values in frozen state");
                }

                results[key] = value;
            }

            /// <summary>
            /// Get result of the given metric.
            /// </summary>
            /// <param name="key">Metric identification.</param>
            /// <returns>Result of the given metric.</returns>
            /// <exception cref="System.NotImplementedException">
            /// Thrown when there is no result for the given metric.
            /// </exception>
            public Result GetResult(TMetricCategory key)
            {
                Result result;
                if (!results.TryGetValue(key, out result))
                {
                    // All implemented metrics has to have entry in results.
                    throw new NotImplementedException("Metric '"
                        + key.ToString() + "' isn't implemented yet");
                }

                return result;
            }
        }

        /// <summary>
        /// Merge properties according to metric semantic.
        /// NOTE: Is guaranteed that merge on disjoint sets of files will be proceeded only.
        /// </summary>
        /// <param name="firstProperty">Type of the first metric property.</param>
        /// <param name="secondProperty">Type of the second metric property.</param>
        /// <returns>Result property merged according to the metric semantic.</returns>
        protected abstract TPropertyType Merge(TPropertyType firstProperty, TPropertyType secondProperty);

        /// <summary>
        /// Merge occurrences according to metric semantic.
        /// NOTE: Never is called with null parameter.
        /// NOTE: Is guaranteed that merge on disjoint sets of files will be proceeded only.
        /// </summary>
        /// <param name="firstOccurrences">List of occurrences of the first metric.</param>
        /// <param name="secondOccurrences">List of occurrences of the second metric.</param>
        /// <returns>Result property occurrences merged according to the metric semantic.</returns>
        protected abstract IEnumerable<AstNode> Merge(IEnumerable<AstNode> firstOccurrences,
            IEnumerable<AstNode> secondOccurrences);

        /// <summary>
        /// Process given parser according to metric semantic.
        /// </summary>
        /// <param name="resolveOccurrences">
        /// Determine whether occurrences of metric property should be registered as part of result.
        /// </param>
        /// <param name="category">Metric of the given category to be processed.</param>
        /// <param name="parser">Syntax parsers with already parsed source code.</param>
        /// <returns>Result of metric processing.</returns>
        public abstract Result Process(bool resolveOccurrences, TMetricCategory category,
            SyntaxParser parser);

        /// <summary>
        /// Merge results according the metric of the object.
        /// </summary>
        /// <param name="firstResult">The first metric processing result.</param>
        /// <param name="secondResult">The second metric processing result.</param>
        /// <returns>Merged result of both parameters.</returns>
        public Result Merge(Result firstResult, Result secondResult)
        {
            IEnumerable<AstNode> occurances = null;
            if (firstResult.HasResolvedOccurances && secondResult.HasResolvedOccurances)
            {
                occurances = Merge(firstResult.Occurrences, secondResult.Occurrences);
            }

            var property = Merge(firstResult.Property, secondResult.Property);

            return new Result(property, occurances);
        }
    }
}
