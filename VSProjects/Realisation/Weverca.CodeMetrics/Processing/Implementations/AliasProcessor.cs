using System.Diagnostics;
using System.Linq;

using Weverca.CodeMetrics.Processing.ASTVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Check if there is costruct like $a = &$b used, which is an alias.
    /// </summary>
    [Metric(ConstructIndicator.Alias)]
    class AliasProcessor : IndicatorProcessor
    {
        #region IndicatorProcessor

        protected override Result process(bool resolveOccurances, ConstructIndicator category, SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.Alias);
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);

            AliasVisitor visitor = new AliasVisitor();
            parser.Ast.VisitMe(visitor);
            var occurences = visitor.GetOccurrences();

            return new Result(occurences.Count() > 0, occurences);
        }

        #endregion
    }
}
