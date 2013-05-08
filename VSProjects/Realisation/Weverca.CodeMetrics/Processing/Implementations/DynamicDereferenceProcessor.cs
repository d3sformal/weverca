using System.Diagnostics;
using System.Linq;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Checks code for Dynamic dereference like $$a
    /// </summary>
    [Metric(ConstructIndicator.DynamicDereference)]
    class DynamicDereferenceProcessor : IndicatorProcessor
    {
        protected override Result process(bool resolveOccurances, ConstructIndicator category, SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.DynamicDereference);
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);

            DynamicDereferenceVisitor visitor = new DynamicDereferenceVisitor();
            parser.Ast.VisitMe(visitor);
            var occurences = visitor.GetOccurrences();

            return new Result(occurences.Count() > 0, occurences);
        }
    }
}
