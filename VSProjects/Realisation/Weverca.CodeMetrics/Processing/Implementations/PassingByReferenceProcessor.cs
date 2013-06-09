using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.CodeMetrics.Processing.ASTVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all function and static method calls with a parameter passed by reference
    /// </summary>
    [Metric(ConstructIndicator.PassingByReferenceAtCallSide)]
    class PassingByReferenceProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        protected override IndicatorProcessor.Result process(bool resolveOccurances,
            ConstructIndicator category, SyntaxParser parser)
        {
            System.Diagnostics.Debug.Assert(category == ConstructIndicator.PassingByReferenceAtCallSide);
            System.Diagnostics.Debug.Assert(parser.IsParsed);
            System.Diagnostics.Debug.Assert(!parser.Errors.AnyError);

            if ((parser.Functions == null) && (parser.Types == null))
            {
                // No type is declared
                if (resolveOccurances)
                {
                    return new Result(false, new FunctionCall[0]);
                }
                else
                {
                    return new Result(false);
                }
            }

            var functions = (parser.Functions != null) ? parser.Functions
                : new Dictionary<QualifiedName, ScopedDeclaration<DRoutine>>();
            var types = (parser.Types != null) ? parser.Types
                : new Dictionary<QualifiedName, PhpType>();

            var visitor = new PassingByReferenceVisitor(functions, types);
            parser.Ast.VisitMe(visitor);

            var occurrences = visitor.GetOccurrences();
            var hasOccurrence = occurrences.GetEnumerator().MoveNext();
            // Return function (DirectFcnCall) or static method (DirectStMtdCall) calls
            // (both are subtypes of FunctionCall) which pass at least one parameter by reference
            return new Result(hasOccurrence, occurrences);
        }

        #endregion
    }
}
