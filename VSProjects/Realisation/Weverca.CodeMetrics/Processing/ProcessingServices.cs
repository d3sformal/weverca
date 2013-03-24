using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing
{
    static class ProcessingServices
    {
        static ProcessingService<ConstructIndicator, bool> indicatorProcessors;

        static ProcessingServices()
        {
            //TODO Collect all implemented MetricProcessors
            indicatorProcessors = new ProcessingService<ConstructIndicator, bool>();
        }
        
        internal static IndicatorProcessor.ResultBatch MergeIndicators(IndicatorProcessor.ResultBatch b1, IndicatorProcessor.ResultBatch b2)
        {
            return indicatorProcessors.MergeIndicators(b1, b2);
        }

        internal static IndicatorProcessor.ResultBatch ProcessIndicators(bool resolveOccurances, SyntaxParser parser)
        {
            return indicatorProcessors.Process(resolveOccurances, parser);
        }
    }

    class ProcessingService<C,P>{
        
        Dictionary<C,MetricProcessor<C,P>> processors=new Dictionary<C,MetricProcessor<C,P>>();


        internal void SetProcessor(C property, MetricProcessor<C, P> processor)
        {
            processors[property] = processor;
        }

        /// <summary>
        /// Returns info about indicators in code.
        /// </summary>
        /// <param name="resolveOccurances"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal  MetricProcessor<C, P>.ResultBatch  Process(bool resolveOccurances, SyntaxParser parser)
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

        internal MetricProcessor<C, P>.ResultBatch MergeIndicators(MetricProcessor<C, P>.ResultBatch b1, MetricProcessor<C, P>.ResultBatch b2)
        {
            if (!b1.IsFrozen || !b2.IsFrozen)
            {
                throw new ArgumentException("Merged batches has to be frozen");
            }

            
            var batch = createBatch();
            foreach (var indicator in processors.Keys)
            {
                var processor = processors[indicator];
                var mergedResult=processor.Merge(b1.GetResult(indicator), b2.GetResult(indicator));
                batch.Insert(indicator, mergedResult);
            }

            batch.Freeze();
            return batch;
        }

        private  MetricProcessor<C, P>.ResultBatch createBatch()
        {
            return new MetricProcessor<C, P>.ResultBatch();
        }
    }
}
