using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing
{
    internal static class ProcessingServices
    {
        // TODO: Collect all implemented MetricProcessors
        #region Private members

        private static ProcessingService<ConstructIndicator, bool> indicatorProcessors
            = new ProcessingService<ConstructIndicator, bool>();

        private static ProcessingService<Rating, double> ratingProcessors
            = new ProcessingService<Rating, double>();

        private static ProcessingService<Quantity, int> quantityProcessors
            = new ProcessingService<Quantity, int>();

        #endregion Private members

        #region Indicator services

        internal static IndicatorProcessor.ResultBatch MergeIndicators(IndicatorProcessor.ResultBatch b1,
            IndicatorProcessor.ResultBatch b2)
        {
            return indicatorProcessors.MergeResults(b1, b2);
        }

        internal static IndicatorProcessor.ResultBatch ProcessIndicators(bool resolveOccurances,
            SyntaxParser parser)
        {
            return indicatorProcessors.Process(resolveOccurances, parser);
        }

        #endregion Indicator services

        #region Rating services

        internal static RatingProcessor.ResultBatch MergeRatings(RatingProcessor.ResultBatch b1,
            RatingProcessor.ResultBatch b2)
        {
            return ratingProcessors.MergeResults(b1, b2);
        }

        internal static RatingProcessor.ResultBatch ProcessRatings(bool resolveOccurances,
            SyntaxParser parser)
        {
            return ratingProcessors.Process(resolveOccurances, parser);
        }

        #endregion Rating services

        #region Quantity services

        internal static QuantityProcessor.ResultBatch MergeQuantities(QuantityProcessor.ResultBatch b1,
            QuantityProcessor.ResultBatch b2)
        {
            return quantityProcessors.MergeResults(b1, b2);
        }

        internal static QuantityProcessor.ResultBatch ProcessQuantities(bool resolveOccurances,
            SyntaxParser parser)
        {
            return quantityProcessors.Process(resolveOccurances, parser);
        }

        #endregion Quantity services
    }

    /// <summary>
    /// Service collecting MetricProcessors with matching Category,Property signature.
    /// </summary>
    /// <typeparam name="TCategory">Type of metric which processors will be collected.</typeparam>
    /// <typeparam name="TProperty">Type of property, which metric works with.</typeparam>
    internal class ProcessingService<TCategory, TProperty>
         where TCategory : struct, IComparable, IFormattable, IConvertible
    {
        private Dictionary<TCategory, MetricProcessor<TCategory, TProperty>> processors
            = new Dictionary<TCategory, MetricProcessor<TCategory, TProperty>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingService{TCategory,TProperty}" /> class.
        /// </summary>
        public ProcessingService()
        {
            var types = GetTypesWithAttribute(typeof(MetricAttribute));
            foreach (var type in types)
            {
                var categories = GetMetricCategories(type);
                if (categories == null)
                {
                    continue;
                }

                var processor = Activator.CreateInstance(type) as MetricProcessor<TCategory, TProperty>;
                if (processor == null)
                {
                    continue;
                }

                foreach (var category in categories)
                {
                    SetProcessor(category, processor);
                }
            }
        }

        #region Processing methods

        /// <summary>
        /// Returns info about indicators in code.
        /// </summary>
        /// <param name="resolveOccurances"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        internal MetricProcessor<TCategory, TProperty>.ResultBatch Process(bool resolveOccurances,
            SyntaxParser parser)
        {
            var batch = CreateBatch();

            foreach (var indicator in processors.Keys)
            {
                var processor = processors[indicator];
                var result = processor.Process(resolveOccurances, indicator, parser);
                batch.Insert(indicator, result);
            }

            batch.Freeze();
            return batch;
        }

        internal MetricProcessor<TCategory, TProperty>.ResultBatch MergeResults(
            MetricProcessor<TCategory, TProperty>.ResultBatch b1,
            MetricProcessor<TCategory, TProperty>.ResultBatch b2)
        {
            if (!b1.IsFrozen || !b2.IsFrozen)
            {
                throw new ArgumentException("Merged batches has to be frozen");
            }

            var batch = CreateBatch();
            foreach (var indicator in processors.Keys)
            {
                var processor = processors[indicator];
                var mergedResult = processor.Merge(b1.GetResult(indicator), b2.GetResult(indicator));
                batch.Insert(indicator, mergedResult);
            }

            batch.Freeze();
            return batch;
        }

        #endregion Processing methods

        #region Metric processors collecting utils

        /// <summary>
        /// Collect types from current assembly with given attribute.
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        private static IEnumerable<Type> GetTypesWithAttribute(Type attributeType)
        {
            var currentAssembly = typeof(ProcessingServices).Assembly;
            foreach (var type in currentAssembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(MetricAttribute), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Returns data from metric attribute for given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<TCategory> GetMetricCategories(Type type)
        {
            var attributesData = type.GetCustomAttributesData();
            var customAttributes = type.GetCustomAttributes(true);
            var attributes = attributesData.Zip(customAttributes, (a, b) => new { data = a, attribute = b });

            foreach (var attr in attributes)
            {
                if (attr.attribute is MetricAttribute)
                {
                    var constructorArgument = attr.data.ConstructorArguments[0];
                    if (constructorArgument.ArgumentType == typeof(TCategory[]))
                    {
                        return GetMetricAttributeArgument(constructorArgument);
                    }
                }
            }

            return null;
        }

        private static IEnumerable<TCategory> GetMetricAttributeArgument(
            CustomAttributeTypedArgument constructorArgument)
        {
            var typedArgs = constructorArgument.Value as IEnumerable<CustomAttributeTypedArgument>;
            var categories = new List<TCategory>();
            foreach (var arg in typedArgs)
            {
                categories.Add((TCategory)arg.Value);
            }

            // Metric attribute doesnt allow multiples so we return first one
            return categories;
        }

        #endregion Metric processors collecting utils

        #region Private utilities

        private void SetProcessor(TCategory property, MetricProcessor<TCategory, TProperty> processor)
        {
            if (processors.ContainsKey(property))
            {
                throw new NotSupportedException("Cannot set twice processor for category: " + property);
            }

            processors[property] = processor;
        }

        private static MetricProcessor<TCategory, TProperty>.ResultBatch CreateBatch()
        {
            return new MetricProcessor<TCategory, TProperty>.ResultBatch();
        }

        #endregion Private utilities
    }
}
