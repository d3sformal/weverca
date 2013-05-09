using System;
using System.Diagnostics;

using Weverca.CodeMetrics.Processing.ASTVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all occurrences of members of objects which are used dynamically as variable
    /// </summary>
    [Metric(ConstructIndicator.DuckTyping)]
    class DuckTypingProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        protected override IndicatorProcessor.Result process(bool resolveOccurances,
            ConstructIndicator category, SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.DuckTyping);

            var visitor = new DuckTypingVisitor();
            parser.Ast.VisitMe(visitor);

            var occurrences = visitor.GetOccurrences();
            var hasOccurrence = occurrences.GetEnumerator().MoveNext();
            // Return all variable-like constructs (VarLikeConstructUse) with use of member of unknown object
            return new Result(hasOccurrence, occurrences);
        }

        #endregion
    }
}
