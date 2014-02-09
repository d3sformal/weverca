using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Checks AST for Dynamic dereference like $$a.
    /// </summary>
    internal class DynamicDereferenceVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        /// <remarks>
        /// The method checks indirect variable use for dynamic dereference.
        /// </remarks>
        /// <inheritdoc />
        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            // variable is dynamicaly dereferenced, if it has a variable use in it's name.
            if (x.VarNameEx is VariableUse)
            {
                occurrenceNodes.Enqueue(x);
            }

            base.VisitIndirectVarUse(x);
        }

        #endregion TreeVisitor overrides
    }
}
