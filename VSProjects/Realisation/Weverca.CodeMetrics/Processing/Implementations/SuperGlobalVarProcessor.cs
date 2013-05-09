using System.Linq;

using Weverca.CodeMetrics.Processing.ASTVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(ConstructIndicator.SuperGlobalVariable)]
    class SuperGlobalVarProcessor:IndicatorProcessor
    {
        protected override IndicatorProcessor.Result process(bool resolveOccurances, ConstructIndicator category, SyntaxParser parser)
        {
            var visitor = new SuperGlobalVarVisitor();
            parser.Ast.VisitMe(visitor);

            var variables=visitor.GetVariables();
            var hasOccurance = variables.Count() > 0;
            return new Result(hasOccurance, variables);
        }
    }
}
