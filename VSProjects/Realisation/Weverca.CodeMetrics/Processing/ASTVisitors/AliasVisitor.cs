using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Checks the AST for <see cref="RefAssignEx"/>.
    /// </summary>
    /// <remarks>
    /// If right side of the expression is <see cref="DirectVarUse"/> or an access
    /// to the array <see cref="ItemUse"/>, it is an alias.
    /// </remarks>
    internal class AliasVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        /// <inheritdoc />
        public override void VisitRefAssignEx(RefAssignEx x)
        {
            if ((x.RValue is DirectVarUse) || (x.RValue is ItemUse) || (x.RValue is IndirectVarUse))
            {
                occurrenceNodes.Enqueue(x);
            }

            base.VisitRefAssignEx(x);
        }

        #endregion TreeVisitor overrides
    }
}
