using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Checks the tree for dynamic function call.
    /// </summary>
    internal class DynamicCallVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        /// <inheritdoc />
        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            // x.NameExpr is VariableUse -- cannot be done. NameExpr is internal.
            // Is it enough that we have IndirectFunctionCall?
            // Could there be something different that var use in nameExpr?
            occurrenceNodes.Enqueue(x);
            base.VisitIndirectFcnCall(x);
        }

        /// <inheritdoc />
        public override void VisitNewEx(NewEx x)
        {
            if (x.ClassNameRef is IndirectTypeRef)
            {
                occurrenceNodes.Enqueue(x);
            }

            base.VisitNewEx(x);
        }

        #endregion TreeVisitor overrides
    }
}
