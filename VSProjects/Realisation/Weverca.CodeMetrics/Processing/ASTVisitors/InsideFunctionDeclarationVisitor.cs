using PHP.Core;
using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.ASTVisitors
{
    /// <summary>
    /// Represents the visitor that finds all types declared in the body of a subroutine
    /// </summary>
    class InsideFunctionDeclarationVisitor : OccurrenceVisitor
    {
        private delegate void VisitSubroutineExprDelegate<T>(T x) where T : AstNode;

        /// <summary>
        /// Indicate whether subtree of a subroutine is traversing at this point
        /// </summary>
        private bool isInsideSubroutine = false;

        #region TreeVisitor overrides

        public override void VisitMethodDecl(MethodDecl x)
        {
            VisitSubroutineExpr<MethodDecl>(base.VisitMethodDecl, x);
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            if (isInsideSubroutine)
            {
                occurrenceNodes.Push(x);
            }
            VisitSubroutineExpr<FunctionDecl>(base.VisitFunctionDecl, x);
        }

        public override void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            VisitSubroutineExpr<LambdaFunctionExpr>(base.VisitLambdaFunctionExpr, x);
        }

        public override void VisitTypeDecl(TypeDecl x)
        {
            // Declaration of classes and interfaces too is non-standard construct
            if (((x.AttributeTarget & PhpAttributeTargets.Types) != 0)
                && (isInsideSubroutine))
            {
                // All classes and interfaces declared inside a function
                occurrenceNodes.Push(x);
            }

            base.VisitTypeDecl(x);
        }

        #endregion

        /// <summary>
        /// Set that visitor is traversing subtree of declaration and body of a subroutine
        /// </summary>
        /// <typeparam name="T">The specific type of AST node</typeparam>
        /// <param name="overriddenMethod">Method of AST which is overridden</param>
        /// <param name="x">AST node of the subroutine declaration</param>
        private void VisitSubroutineExpr<T>(VisitSubroutineExprDelegate<T>/*!*/ overriddenMethod,
            T x) where T : AstNode
        {
            if (isInsideSubroutine)
            {
                overriddenMethod(x);
                Debug.Assert(isInsideSubroutine);
            }
            else
            {
                isInsideSubroutine = true;
                try
                {
                    overriddenMethod(x);
                    Debug.Assert(isInsideSubroutine);
                }
                finally
                {
                    isInsideSubroutine = false;
                }
            }
        }
    }
}
