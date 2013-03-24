using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing
{
    abstract class MetricProcessor<MetricCategory, PropertyType>
    {
        public struct Result
        {
            /// <summary>
            /// Property of processed metric
            /// </summary>
            public readonly PropertyType Property;
            /// <summary>
            /// Is null if and only if resolveOccurances wasnt enabled
            /// </summary>
            public readonly ICollection<AstNode> Occurances;

            public Result(PropertyType property, ICollection<AstNode> occurances)
            {
                this.Property = property;
                this.Occurances = occurances;
            }

            /// <summary>
            /// Determine that result has enabled occurance resolving
            /// </summary>
            public bool HasResolvedOccurances { get { return Occurances != null; } }
        }


        public class ResultBatch
        {
            /// <summary>
            /// Data which were collected by processors
            /// </summary>
            Dictionary<MetricCategory, Result> results = new Dictionary<MetricCategory, Result>();

            /// <summary>
            /// Determine that this batch is already frozen - its values cannot be changed.
            /// </summary>
            public bool IsFrozen { get; private set; }

            public void Freeze()
            {
                if (IsFrozen)
                {
                    throw new NotSupportedException("Cannot freeze batch twice");
                }

                IsFrozen = true;
            }

            public void Insert(MetricCategory key, Result value)
            {
                if (IsFrozen)
                {
                    throw new NotSupportedException("Cannot insert values in frozen state");
                }

                results[key] = value;
            }

            public Result GetResult(MetricCategory key)
            {
                Result result;
                if(!results.TryGetValue(key,out result)){
                    //all implemented metrics has to have entry in results
                    throw new NotImplementedException("Metric '"+key+"' isn't implemented yet");
                }
                return result;
            }
        }

        /// <summary>
        /// Merge properties according to metric semantic
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        protected abstract PropertyType merge(PropertyType r1, PropertyType r2);
        /// <summary>
        /// Merge occurances according to metric semantic
        /// NOTE: Never is callled with null parameter.
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        protected abstract ICollection<AstNode> merge(ICollection<AstNode> o1, ICollection<AstNode> o2);
        /// <summary>
        /// Process given parser according to metric semantic.
        /// </summary>
        /// <param name="resolveOccurances"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        protected abstract Result process(bool resolveOccurances, MetricCategory catogory,SyntaxParser parser);

        public Result Process(bool resolveOccurances,MetricCategory category, SyntaxParser parser)
        {
            return process(resolveOccurances,category, parser);
        }

        /// <summary>
        /// Merge results from MetricProcessor
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public Result Merge(Result r1, Result r2)
        {
            ICollection<AstNode> occurances = null;
            if (r1.HasResolvedOccurances && r2.HasResolvedOccurances)
            {
                occurances = merge(r1.Occurances, r2.Occurances);
            }

            var property = merge(r1.Property, r2.Property);

            return new Result(property, occurances);
        }
    }
}
