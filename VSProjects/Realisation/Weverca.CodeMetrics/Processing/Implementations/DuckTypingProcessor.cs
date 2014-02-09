using System.Diagnostics;

using Weverca.CodeMetrics.Processing.AstVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all occurrences of members of objects which are used dynamically as variable.
    /// </summary>
    [Metric(ConstructIndicator.DuckTyping)]
    internal class DuckTypingProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.DuckTyping,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var visitor = new DuckTypingVisitor();
            parser.Ast.VisitMe(visitor);

            var occurrences = visitor.GetOccurrences();
            var hasOccurrence = occurrences.GetEnumerator().MoveNext();

            // Return all variable-like constructs (VarLikeConstructUse) with use of member of unknown object
            return new Result(hasOccurrence, occurrences);
        }

        #endregion MetricProcessor overrides
    }
}
