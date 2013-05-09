using System;
using System.Diagnostics;

using Weverca.CodeMetrics.Processing.ASTVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all classes and functions declared inside another subroutine
    /// </summary>
    [Metric(ConstructIndicator.InsideFunctionDeclaration)]
    class InsideFunctionDeclarationProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        protected override IndicatorProcessor.Result process(bool resolveOccurances,
            ConstructIndicator category, SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.InsideFunctionDeclaration);

            // PHP.Core.AST.FunctionDecl.Function.IsIdentityDefinite may imply that
            // if declaration is inside function, it is conditional (but not otherwise)
            var visitor = new InsideFunctionDeclarationVisitor();
            parser.Ast.VisitMe(visitor);

            var occurrences = visitor.GetOccurrences();
            var hasOccurrence = occurrences.GetEnumerator().MoveNext();
            // Return classes (TypeDecl) and functions (FunctionDecl) declared inside a subroutine
            return new Result(hasOccurrence, occurrences);
        }

        #endregion
    }
}
