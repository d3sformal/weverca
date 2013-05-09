using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.ASTVisitors
{
    /// <summary>
    /// Visitor which collect super global variable usage
    /// </summary>
    class SuperGlobalVarVisitor : TreeVisitor
    {
        List<AstNode> foundSuperGlobals = new List<AstNode>();

        /// <summary>
        /// Returns calls which were founded during visiting tree
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<AstNode> GetVariables()
        {
            //Copy result because of make it immutable
            return foundSuperGlobals.ToArray();
        }

        #region TreeVisitor overrides

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            var name = x.VarName;
            if (name.IsAutoGlobal)
            {
                foundSuperGlobals.Add(x);
            }

            base.VisitDirectVarUse(x);
        }

        #endregion
    }
}
