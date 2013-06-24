using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;


namespace Weverca.Analysis.Expressions
{
    static class Converter
    {
        static PostfixVisitorConverter _visitor=new PostfixVisitorConverter();

        static internal Postfix GetPostfix(LangElement element)
        {
            return _visitor.GetExpression(element);
        }
    }

    class PostfixVisitorConverter:TreeVisitor
    {
        Postfix _collectedExpression;
             

        public Postfix GetExpression(LangElement element)
        {
            _collectedExpression = new Postfix(element);
            element.VisitMe(this);

            //element where VisitMe is called is not traversed
            addItem(element);
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
