using System.Diagnostics;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(Quantity.NumberOfLines)]
    internal class NumberOfLinesProcessor : QuantityProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, Quantity category,
            Parsers.SyntaxParser parser)
        {
            Debug.Assert(category == Quantity.NumberOfLines,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var statements = parser.Ast.Statements;
            if (statements.Count > 0)
            {
                var lastStatement = statements[statements.Count - 1];
                var occurrences = new Statement[] { lastStatement };
                return new Result(lastStatement.Position.LastLine, occurrences);
            }
            else
            {
                return new Result(0);
            }
        }

        #endregion MetricProcessor overrides
    }
}
