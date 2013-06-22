using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;


namespace Weverca.Analysis.Expressions
{
    static class Converter
    {
        static internal Postfix GetPostfix(LangElement element)
        {
            var visitor = new PostfixVisitorConverter();
            element.VisitMe(visitor);

            return visitor.GetExpression();
        }
    }

    class PostfixVisitorConverter:TreeVisitor
    {
        Postfix _collectedExpression=new Postfix();
        public Postfix GetExpression()
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
