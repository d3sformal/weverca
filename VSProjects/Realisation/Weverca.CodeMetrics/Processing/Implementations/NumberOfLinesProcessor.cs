using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    class NumberOfLinesProcessor:QuantityProcessor
    {
        protected override Result process(bool resolveOccurances, Quantity category, Parsers.SyntaxParser parser)
        {
            var phpDocPosition = parser.Ast.PHPDoc.Position;

            int lastLine = phpDocPosition.LastLine;
            int firstLine = phpDocPosition.FirstLine;

            return new Result(lastLine - firstLine);
        }
    }
}
