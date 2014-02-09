using System.Diagnostics;

using Weverca.CodeMetrics.Processing.AstVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(ConstructIndicator.SuperGlobalVariable)]
    internal class SuperGlobalVarProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.SuperGlobalVariable,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var visitor = new SuperGlobalVarVisitor();
            parser.Ast.VisitMe(visitor);

            var occurrences = visitor.GetOccurrences();
            var hasOccurrence = occurrences.GetEnumerator().MoveNext();

            return new Result(hasOccurrence, occurrences);
        }

        #endregion MetricProcessor overrides
    }
}
