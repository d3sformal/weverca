using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Visitor which collect super global variable usage.
    /// </summary>
    internal class SuperGlobalVarVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        /// <inheritdoc />
        public override void VisitDirectVarUse(DirectVarUse x)
        {
            var name = x.VarName;
            if (name.IsAutoGlobal)
            {
                occurrenceNodes.Enqueue(x);
            }

            base.VisitDirectVarUse(x);
        }

        #endregion TreeVisitor overrides
    }
}
