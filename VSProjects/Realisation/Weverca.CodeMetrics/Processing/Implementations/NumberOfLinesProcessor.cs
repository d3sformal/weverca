using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(Quantity.NumberOfLines)]
    class NumberOfLinesProcessor:QuantityProcessor
    {
        protected override Result process(bool resolveOccurances, Quantity category, Parsers.SyntaxParser parser)
        {
            var lastStatementPosition = parser.Ast.Statements.Last().Position;

            return new Result(lastStatementPosition.LastLine);
        }
    }
}
