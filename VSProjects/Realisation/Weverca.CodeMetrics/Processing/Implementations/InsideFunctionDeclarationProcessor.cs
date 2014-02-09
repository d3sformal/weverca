using System.Diagnostics;

using Weverca.CodeMetrics.Processing.AstVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all classes and functions declared inside another subroutine.
    /// </summary>
    [Metric(ConstructIndicator.InsideFunctionDeclaration)]
    internal class InsideFunctionDeclarationProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.InsideFunctionDeclaration,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            // PHP.Core.AST.FunctionDecl.Function.IsIdentityDefinite may imply that
            // if declaration is inside function, it is conditional (but not otherwise).
            var visitor = new InsideFunctionDeclarationVisitor();
            parser.Ast.VisitMe(visitor);

            var occurrences = visitor.GetOccurrences();
            var hasOccurrence = occurrences.GetEnumerator().MoveNext();

            // Return classes (TypeDecl) and functions (FunctionDecl) declared inside a subroutine.
            return new Result(hasOccurrence, occurrences);
        }

        #endregion MetricProcessor overrides
    }
}
