using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.ASTVisitors
{
    /// <summary>
    /// Checks the tree for dynamic function call.
    /// </summary>
    class DynamicCallVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            //x.NameExpr is VariableUse -- cannot be done. NameExpr is internal.
            // Is it enoght that we have IndirectFunctionCall? Could thete be something different that var use in nameExpr?

            occurrenceNodes.Push(x);
            base.VisitIndirectFcnCall(x);
        }


        public override void VisitNewEx(NewEx x)
        {
            if (x.ClassNameRef is IndirectTypeRef)
            {
                occurrenceNodes.Push(x);
            }
            base.VisitNewEx(x);
        }

        #endregion

    }
}
