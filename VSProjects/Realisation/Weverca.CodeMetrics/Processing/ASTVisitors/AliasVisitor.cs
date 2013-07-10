using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.ASTVisitors
{
    /// <summary>
    /// Checks the AST for <see cref="RefAssignEx"/>.
    /// If right side of the expression is <see cref="DirectVarUse"/> or an access to the array <see cref="ItemUse"/>, it is an alias.
    /// </summary>
    class AliasVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        public override void VisitRefAssignEx(RefAssignEx x)
        {
            if (x.RValue is DirectVarUse || x.RValue is ItemUse || x.RValue is IndirectVarUse)
            {
                occurrenceNodes.Push(x);
            }
            
            base.VisitRefAssignEx(x);
        }

        #endregion
    }
}
