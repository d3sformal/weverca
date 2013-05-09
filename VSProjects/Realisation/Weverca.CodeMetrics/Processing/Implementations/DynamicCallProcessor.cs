using System.Diagnostics;
using System.Linq;

using Weverca.CodeMetrics.Processing.ASTVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Determines whether there is a dynamic function call in the code.
    /// </summary>
    [Metric(ConstructIndicator.DynamicCall)]
    class DynamicCallProcessor : IndicatorProcessor
    {
        protected override Result process(bool resolveOccurances, ConstructIndicator category, SyntaxParser parser)
        {

            Debug.Assert(category == ConstructIndicator.DynamicCall);
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);
            
            DynamicCallVisitor visitor = new DynamicCallVisitor();
            parser.Ast.VisitMe(visitor);

            var occurences = visitor.GetOccurrences();

            return new Result(occurences.Count() > 0, occurences);
        }
    }
}
