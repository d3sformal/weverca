using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(ConstructIndicator.Eval, ConstructIndicator.Session, ConstructIndicator.MySQL, ConstructIndicator.ClassAlias)]
    sealed class FunctionPresenceProcessor : IndicatorProcessor
    {
        protected override Result process(bool resolveOccurances, ConstructIndicator category, SyntaxParser parser)
        {
            var functions=MetricRelatedFunctions.Get(category);

            var calls = findCalls(parser, functions);
            return new Result(calls.Count() > 0, calls);            
        }
    }
}
