using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using PHP.Core.AST;

using Weverca.Parsers;
using Weverca.CodeMetrics.Processing.Implementations;

namespace Weverca.CodeMetrics.Processing
{
    static class ProcessingServices
    {
        static ProcessingService<ConstructIndicator, bool> indicatorProcessors;

        static ProcessingServices()
        {
            //Collect all implemented MetricProcessors
            indicatorProcessors = new ProcessingService<ConstructIndicator, bool>();
         }

        internal static IndicatorProcessor.ResultBatch MergeIndicators(IndicatorProcessor.ResultBatch b1, IndicatorProcessor.ResultBatch b2)
        {
            return indicatorProcessors.MergeResults(b1, b2);
        }

        internal static IndicatorProcessor.ResultBatch ProcessIndicators(bool resolveOccurances, SyntaxParser parser)
        {
            return indicatorProcessors.Process(resolveOccurances, parser);
        }

     
    }

    class ProcessingService<Category, Property>
    {
        Dictionary<Category, MetricProcessor<Category, Property>> processors = new Dictionary<Category, MetricProcessor<Category, Property>>();

        public ProcessingService()
        {
            var types=getTypesWithAttribute(typeof(MetricAttribute));
            foreach (var type in types)
            {
                var categories = getMetricCategories(type);                
                if (categories == null)
                    continue;

                var processor = Activator.CreateInstance(type) as MetricProcessor<Category, Property>;
                if (processor == null)
                    continue;

                foreach (var category in categories)
                {
                    setProcessor(category,processor);
                }
            }
        }

        #region Processing methods
        /// <summary>
        /// Returns info about indicators in code.
        /// </summary>
        /// <param name="resolveOccurances"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal MetricProcessor<Category, Property>.ResultBatch Process(bool resolveOccurances, SyntaxParser parser)
        {
            var batch = createBatch();

            foreach (var indicator in processors.Keys)
            {
                var processor = processors[indicator];
                var result = processor.Process(resolveOccurances, indicator, parser);
                batch.Insert(indicator, result);
            }

            batch.Freeze();
            return batch;
        }

        internal MetricProcessor<Category, Property>.ResultBatch MergeResults(MetricProcessor<Category, Property>.ResultBatch b1, MetricProcessor<Category, Property>.ResultBatch b2)
        {
            if (!b1.IsFrozen || !b2.IsFrozen)
            {
                throw new ArgumentException("Merged batches has to be frozen");
            }


            var batch = createBatch();
            foreach (var indicator in processors.Keys)
            {
                var processor = processors[indicator];
                var mergedResult = processor.Merge(b1.GetResult(indicator), b2.GetResult(indicator));
                batch.Insert(indicator, mergedResult);
            }

            batch.Freeze();
            return batch;
        }
        #endregion
        
        #region Metric processors collecting utils
        /// <summary>
        /// Collect types from current assembly with given attribute
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        private static IEnumerable<Type> getTypesWithAttribute(Type attributeType)
        {
            var currentAssembly = typeof(ProcessingServices).Assembly;
            foreach (Type type in currentAssembly.GetTypes())
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
        private static IEnumerable<Category> getMetricCategories(Type type)
        {            
            foreach (var attr in type.CustomAttributes)
            {                
                if (attr.AttributeType==typeof(MetricAttribute))
                {
                    var constructorArgument = attr.ConstructorArguments[0];
                    if (constructorArgument.ArgumentType != typeof(Category[]))
                        continue;

                    return getMetricAttributeArgument(constructorArgument);
                }
            }
            return null;
        }

        private static IEnumerable<Category> getMetricAttributeArgument(CustomAttributeTypedArgument constructorArgument)
        {
            var typedArgs = constructorArgument.Value as IReadOnlyCollection<CustomAttributeTypedArgument>;
            var categories = new List<Category>();
            foreach (var arg in typedArgs)
            {
                categories.Add((Category)arg.Value);
            }

            //Metric attribute doesnt allow multiples so we return first one
            return categories;
        }
        #endregion

        #region Private utilities

        private void setProcessor(Category property, MetricProcessor<Category, Property> processor)
        {
            if (processors.ContainsKey(property))
            {
                throw new NotSupportedException("Cannot set twice processor for cateogry: " + property);
            }
            processors[property] = processor;
        }

        private MetricProcessor<Category, Property>.ResultBatch createBatch()
        {
            return new MetricProcessor<Category, Property>.ResultBatch();
        }
        #endregion
    }
}
