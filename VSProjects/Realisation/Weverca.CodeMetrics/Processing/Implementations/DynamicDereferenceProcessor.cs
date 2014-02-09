using System.Diagnostics;

using Weverca.CodeMetrics.Processing.AstVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Checks code for Dynamic dereference like $$a.
    /// </summary>
    [Metric(ConstructIndicator.DynamicDereference)]
    internal class DynamicDereferenceProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.DynamicDereference,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var visitor = new DynamicDereferenceVisitor();
            parser.Ast.VisitMe(visitor);

            var occurrences = visitor.GetOccurrences();
            var hasOccurrence = occurrences.GetEnumerator().MoveNext();

            return new Result(hasOccurrence, occurrences);
        }

        #endregion MetricProcessor overrides
    }
}
