using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    class NumberOfSourcesProcessor:QuantityProcessor
    {
        protected override Result process(bool resolveOccurances, Quantity category, Parsers.SyntaxParser parser)
        {
            //processing is made on single source
            return new Result(1);
        }
    }
}
