using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;


namespace Weverca.Analysis.Expressions
{
    /// <summary>
    /// Converts elements between representations
    /// </summary>
    public static class Converter
    {
        /// <summary>
        /// Singleton visitor for postfix converting
        /// </summary>
        private static PostfixVisitorConverter _visitor=new PostfixVisitorConverter();

        /// <summary>
        /// Convert given element into Postfix representation
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Postfix GetPostfix(LangElement element)
        {
            return _visitor.GetExpression(element);
        }
    }

    /// <summary>
    /// Visitor for postfix conversion
    /// </summary>
    class PostfixVisitorConverter:TreeVisitor
    {
        Postfix _collectedExpression;
             
        /// <summary>
        /// Get converted expression of element
        /// </summary>
        /// <param name="element">Converted element</param>
        /// <returns>Postfix representation of element</returns>
        internal Postfix GetExpression(LangElement element)
        {
            _collectedExpression = new Postfix(element);
            element.VisitMe(this);

            //element where VisitMe is called is not traversed
            appendElement(element);
            return _collectedExpression;
        }
        
        /// <summary>
        /// Append element into postfix representation
        /// </summary>
        /// <param name="element">Appended element</param>
        private void appendElement(LangElement element)
        {
            _collectedExpression.Append(element);
        }

        #region Vistor overrides
        public override void VisitElement(LangElement element)
        {
            base.VisitElement(element);
            appendElement(element);
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            //force traversing 
            VisitElement(x.VarNameEx);  
        }

        public override void VisitJumpStmt(JumpStmt x)
        {
            //force traversing
            VisitElement(x.Expression);
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            //no recursive traversing
            appendElement(x);
        }
        #endregion
    }
}
