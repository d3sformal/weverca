using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.ASTVisitors
{
    /// <summary>
    /// Checks AST for Dynamic dereference like $$a
    /// </summary>
    class DynamicDereferenceVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        /// <summary>
        /// Visits the indirect variable use and checks it for dynamic dereference.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            // variable is dynamicaly dereferenced, if it has a variable use in it's name.
            if (x.VarNameEx is VariableUse)
            {
                occurrenceNodes.Push(x);
            }
            base.VisitIndirectVarUse(x);
        }

        #endregion
    }
}
