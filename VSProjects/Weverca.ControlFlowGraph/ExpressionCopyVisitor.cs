using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph
{
    /// <summary>
    /// Makes a copy of given AST expression.
    /// </summary>
    class ExpressionCopyVisitor : TreeVisitor
    {
        private LangElement result;

        /// <summary>
        /// Returns AST expression that is a deep copy of expression expression.
        /// </summary>
        /// <param name="expression">the expression to be copied.</param>
        /// <returns>a deep copy of the expression expression.</returns>
        public Expression MakeDeepCopy(Expression expression)
        {
            result = null;
            expression.VisitMe(this);
            return (Expression)result;
        }

        /// <inheritdoc />
        override public void VisitElement(LangElement element)
        {
            result = null;
            if (element != null)
                element.VisitMe(this);
            if (result == null) result = element;
        }

        /// <inheritdoc />
        override public void VisitDirectVarUse(DirectVarUse x)
        {
            result = new DirectVarUse(x.Position, x.VarName);
        }

        /// <inheritdoc />
        override public void VisitGlobalConstUse(GlobalConstUse x)
        {
            result = new GlobalConstUse(x.Position, x.Name, null);
        }

        /// <inheritdoc />
        override public void VisitClassConstUse(ClassConstUse x)
        {
            result = new ClassConstUse(x.Position, x.TypeRef, x.Name.Value, x.NamePosition);
        }

        /// <inheritdoc />
        override public void VisitPseudoConstUse(PseudoConstUse x)
        {
            result = new PseudoConstUse(x.Position, x.Type);
        }

        /// <inheritdoc />
        override public void VisitIndirectVarUse(IndirectVarUse x)
        {
            VisitElement(x.VarNameEx);
            result = new IndirectVarUse(x.Position, 1, (Expression)result);
        }

        /// <inheritdoc />
        override public void VisitIncludingEx(IncludingEx x)
        {
            VisitElement(x.Target);
            result = new IncludingEx(x.SourceUnit, x.Scope, x.IsConditional, x.Position, x.InclusionType, (Expression) result);
        }

        /// <inheritdoc />
        override public void VisitIssetEx(IssetEx x)
        {
            var varList = new List<VariableUse>(x.VarList.Count());
            foreach (VariableUse v in x.VarList) 
            {
                VisitElement(v);
                varList.Add((VariableUse)result);
            }

            result = new IssetEx(x.Position, varList);
        }

        /// <inheritdoc />
        override public void VisitEmptyEx(EmptyEx x)
        {
            VisitElement(x.Variable);
            result = new EmptyEx(x.Position, (VariableUse)result);
        }

        /// <inheritdoc />
        override public void VisitEvalEx(EvalEx x)
        {
            VisitElement(x.Code);
            result = new EvalEx(x.Position, (Expression)result, false);
        }

        /// <inheritdoc />
        override public void VisitExitEx(ExitEx x)
        {
            VisitElement(x.ResulExpr);
            result = new ExitEx(x.Position, (Expression)result);
        }

        /// <inheritdoc />
        override public void VisitBinaryEx(BinaryEx x)
        {
            VisitElement(x.LeftExpr);
            var leftEx = result;
            VisitElement(x.RightExpr);

            result = new BinaryEx(x.PublicOperation, (Expression) leftEx, (Expression)result);
        }

        /// <inheritdoc />
        override public void VisitShellEx(ShellEx x)
        {
            VisitElement(x.Command);
            result = new ShellEx(x.Position, (Expression)result);
        }

        /// <inheritdoc />
        override public void VisitItemUse(ItemUse x)
        {
            VisitElement(x.Index);
            var index = result;
            VisitElement(x.Array);

            result = new ItemUse(x.Position, (VarLikeConstructUse)result, (Expression)index, x.IsFunctionArrayDereferencing);

            //VisitVarLikeConstructUse(x);
        }

        /// <inheritdoc />
        override public void VisitDirectFcnCall(DirectFcnCall x)
        {
            result = new DirectFcnCall(x.Position, x.QualifiedName, null, x.NamePosition, x.CallSignature.Parameters, x.CallSignature.GenericParams);
        }

        /// <inheritdoc />
        override public void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            VisitElement(x.PublicNameExpr);

            result = new IndirectFcnCall(x.Position, (Expression) result, x.CallSignature.Parameters, x.CallSignature.GenericParams);
        }

        /// <inheritdoc />
        override public void VisitDirectStMtdCall(DirectStMtdCall x)
        {
            result = new DirectStMtdCall(x.Position, x.ClassName, x.ClassNamePosition, x.MethodName, x.NamePosition, x.CallSignature.Parameters, x.CallSignature.GenericParams);
        }

        /// <inheritdoc />
        override public void VisitIndirectStMtdCall(IndirectStMtdCall x)
        {
            VisitElement(x.MethodNameVar);
            result = new IndirectStMtdCall(x.Position, x.ClassName, x.ClassNamePosition, (CompoundVarUse)result, x.CallSignature.Parameters, x.CallSignature.GenericParams);
        }
        override public void VisitDirectStFldUse(DirectStFldUse x)
        {
            result = new DirectStFldUse(x.Position, x.TypeRef, x.PropertyName, x.NamePosition);
        }

        /// <inheritdoc />
        override public void VisitIndirectStFldUse(IndirectStFldUse x)
        {
            VisitElement(x.FieldNameExpr);
            result = new IndirectStFldUse(x.Position, x.TypeRef, (Expression) result);
        }

        /// <inheritdoc />
        override public void VisitArrayEx(ArrayEx x)
        {
            result = new ArrayEx(x.Position, x.Items);
        }

        /// <inheritdoc />
        override public void VisitConditionalEx(ConditionalEx x)
        {
            VisitElement(x.CondExpr);
            var condExpr = result;
            VisitElement(x.TrueExpr);
            var trueExpr = result;
            VisitElement(x.FalseExpr);
            var falseExpr = result;

            result = new ConditionalEx((Expression)condExpr, (Expression)trueExpr, (Expression) falseExpr);
        }

        /// <inheritdoc />
        override public void VisitIncDecEx(IncDecEx x)
        {
            VisitElement(x.Variable);

            result = new IncDecEx(x.Position, x.Inc, x.Post, (VariableUse)result);
        }

        /// <inheritdoc />
        override public void VisitValueAssignEx(ValueAssignEx x)
        {
            VisitElement(x.LValue);
            var lvalue = (VariableUse) result;
            VisitElement(x.RValue);
            result = new ValueAssignEx(x.Position, x.PublicOperation, lvalue, (Expression) result);
        }

        /// <inheritdoc />
        override public void VisitRefAssignEx(RefAssignEx x)
        {
            VisitAssignEx(x);
            var lvalue = (VariableUse) result;
            VisitElement(x.RValue);

            result = new RefAssignEx(x.Position, lvalue, (Expression)result);
        }

        /// <inheritdoc />
        override public void VisitUnaryEx(UnaryEx x)
        {
            VisitElement(x.Expr);

            result = new UnaryEx(x.PublicOperation, (Expression)result);
        }

        /// <inheritdoc />
        override public void VisitNewEx(NewEx x)
        {
            result = new NewEx(x.Position, x.ClassNameRef, x.CallSignature.Parameters);
        }

        /// <inheritdoc />
        override public void VisitInstanceOfEx(InstanceOfEx x)
        {
            VisitElement(x.Expression);

            result = new InstanceOfEx(x.Position, (Expression)result, x.ClassNameRef);
        }

        /// <inheritdoc />
        override public void VisitTypeOfEx(TypeOfEx x)
        {
            result = new TypeOfEx(x.Position, x.ClassNameRef);
        }

        /// <inheritdoc />
        override public void VisitConcatEx(ConcatEx x)
        {
            var expressions = visitExpressionList(x.Expressions);

            result = new ConcatEx(x.Position, expressions);
        }

        /// <inheritdoc />
        override public void VisitListEx(ListEx x)
        {
            var lvalues = visitExpressionList(x.LValues);
            VisitElement(x.RValue);

            result = new ListEx(x.Position, lvalues, (Expression)result);
        }

        /// <inheritdoc />
        override public void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            //result = new LambdaFunctionExpr(null, x.Position, x.EntireDeclarationPosition, x.HeadingEndPosition, x.DeclarationBodyPosition, null, null, x.Signature.AliasReturn, x.Signature.FormalParams, x.UseParams, x.Body);
            result = x;
        }

        private List<Expression> visitExpressionList(List<Expression> expressionList)
        {
            var copiedExpressions = new List<Expression>(expressionList.Count());
            foreach (var expression in expressionList)
            {
                VisitElement(expression);
                copiedExpressions.Add((Expression)result);
            }
            return copiedExpressions;
        }
    }
}
