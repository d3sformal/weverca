using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;


namespace Weverca.Analysis.Expressions
{
    class ExpressionConverter:TreeVisitor
    {
        PostfixExpression _collectedExpression=new PostfixExpression();
        public PostfixExpression GetExpression()
        {
            return _collectedExpression;
        }
        
        private void addItem(LangElement element)
        {
            _collectedExpression.Add(element);
        }

        public override void VisitElement(LangElement element)
        {
            base.VisitElement(element);
            addItem(element);
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            //no recursive traversing
            addItem(x);
        }


     /*   public override void VisitAssignEx(AssignEx x)
        {
            base.VisitAssignEx(x);
            addItem(x);
        }
        public override void VisitBinaryEx(BinaryEx x)
        {
            base.VisitBinaryEx(x);
            addItem(x);
        }
        public override void VisitDirectVarUse(DirectVarUse x)
        {
            base.VisitDirectVarUse(x);
            addItem(x);
        }
        public override void VisitStringLiteral(StringLiteral x)
        {
            base.VisitStringLiteral(x);
            addItem(x);
        }*/
    }
}
