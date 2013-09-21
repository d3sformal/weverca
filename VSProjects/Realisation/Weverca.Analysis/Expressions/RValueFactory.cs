using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;
using Weverca.Analysis.ProgramPoints;

namespace Weverca.Analysis.Expressions
{
    /// <summary>
    /// Creates RValue points (created values has stack edges connected, but no flow edges)
    /// <remarks>RValuePoint provides MemoryEntry as result</remarks>
    /// </summary>
    class RValueFactory : TreeVisitor
    {
        /// <summary>
        /// Here is stored result of CreateValue operation
        /// </summary>
        private RValuePoint _resultPoint;

        /// <summary>
        /// Expander is used for creating sub values
        /// </summary>
        private readonly ElementExpander _valueCreator;

        internal RValueFactory(ElementExpander valueCreator)
        {
            _valueCreator = valueCreator;
        }

        /// <summary>
        /// Create RValue point from given element
        /// </summary>
        /// <param name="el">Element from which RValue point will be created</param>
        /// <returns>Created RValue</returns>
        internal RValuePoint CreateValue(LangElement el)
        {
            //empty current result, because of avoiding incorrect use
            _resultPoint = null;

            el.VisitMe(this);
            //assert that result has been set
            Debug.Assert(_resultPoint != null);

            //empty result because of incorrect use
            var result = _resultPoint;
            _resultPoint = null;

            return result;
        }

        #region Creating program points

        /// <summary>
        /// Visit element, which doesn't accept any values and it's value is constant during whole computation
        /// </summary>
        /// <param name="e">Visited element</param>
        private void visitConstantNularyElement(LangElement e, ConstantProvider provider)
        {
            Result(new ConstantProgramPoint(e, provider));
        }

        /// <summary>
        /// Set given RValue as CreateValue result
        /// </summary>
        /// <param name="value">Result value</param>
        private void Result(RValuePoint value)
        {
            Debug.Assert(_resultPoint == null, "Value has to be null - someone doesn't read it's value");
            _resultPoint = value;
        }

        /// <summary>
        /// Create RValue from given element
        /// </summary>
        /// <param name="el">Element which value will be created</param>
        /// <returns>Created value</returns>
        private RValuePoint CreateRValue(LangElement el)
        {
            return _valueCreator.CreateRValue(el);
        }

        /// <summary>
        /// Create LValue from given element
        /// </summary>
        /// <param name="el">Element which value will be created</param>
        /// <returns>Created value</returns>
        private LValuePoint CreateLValue(LangElement el)
        {
            return _valueCreator.CreateLValue(el);
        }

        /// <summary>
        /// Create argument rvalues according to given signature
        /// </summary>
        /// <param name="signature">Signature whicha arguments will be created</param>
        /// <returns>Created arguments</returns>
        private RValuePoint[] CreateArguments(CallSignature signature)
        {
            var args = new List<RValuePoint>();
            foreach (var param in signature.Parameters)
            {
                var value = CreateRValue(param.Expression);
                args.Add(value);
            }

            return args.ToArray();
        }

        #endregion

        public override void VisitElement(LangElement element)
        {
            throw new NotSupportedException("Given element is not supported RValue");
        }

        #region Literal visiting

        public override void VisitStringLiteral(StringLiteral x)
        {
            visitConstantNularyElement(x,
                (e) => e.StringLiteral(x)
                );
        }

        public override void VisitBoolLiteral(BoolLiteral x)
        {
            visitConstantNularyElement(x,
                (e) => e.BoolLiteral(x)
                );
        }

        public override void VisitIntLiteral(IntLiteral x)
        {
            visitConstantNularyElement(x,
                (e) => e.IntLiteral(x)
                );
        }

        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            visitConstantNularyElement(x,
                (e) => e.LongIntLiteral(x)
                );
        }

        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            visitConstantNularyElement(x,
                (e) => e.DoubleLiteral(x)
                );
        }

        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            throw new NotImplementedException("TODO: what is binary string literal ?");
        }

        public override void VisitNullLiteral(NullLiteral x)
        {
            visitConstantNularyElement(x,
                (e) => e.NullLiteral(x)
                );
        }

        public override void VisitGlobalConstUse(GlobalConstUse x)
        {
            visitConstantNularyElement(x,
                    (e) => e.Constant(x)
                    );
        }

        #endregion

        #region Variable visiting

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            RValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            Result(new RVariablePoint(x, thisObj));
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            RValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            var name = CreateRValue(x.VarNameEx);
            Result(new RIndirectVariablePoint(x, name, thisObj));
        }

        public override void VisitIndirectTypeRef(IndirectTypeRef x)
        {
            var variable = CreateRValue(x.ClassNameVar);
            Result(variable);
        }

        #endregion

        #region Expression visiting

        public override void VisitUnaryEx(UnaryEx x)
        {
            var operand = CreateRValue(x.Expr);

            Result(new UnaryExPoint(x, operand));
        }

        public override void VisitBinaryEx(BinaryEx x)
        {
            var lOperand = CreateRValue(x.LeftExpr);
            var rOperand = CreateRValue(x.RightExpr);

            Result(new BinaryExPoint(x, lOperand, rOperand));
        }

        public override void VisitConcatEx(ConcatEx x)
        {
            var expressions = new List<RValuePoint>();

            foreach (var expression in x.Expressions)
            {
                expressions.Add(CreateRValue(expression));
            }

            Result(new ConcatExPoint(x, expressions));
        }

        public override void VisitIncDecEx(IncDecEx x)
        {
            var variable = CreateRValue(x.Variable);
            Result(new IncDecExPoint(x, variable));
        }

        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            var rValue = CreateRValue(x.RValue);
            switch (x.PublicOperation)
            {
                case Operations.AssignAppend:
                case Operations.AssignPrepend:
                    var concatedValue=CreateRValue(x.LValue);
                    Result(new AssignConcatPoint(x, concatedValue, rValue));
                    return;
                case Operations.AssignValue:
                    var lValue = CreateLValue(x.LValue);
                    Result(new AssignPoint(x, lValue, rValue));
                    return;
                default:
                    //it's assign with binary operation
                    var leftOperand = CreateRValue(x.LValue);
                    Result(new AssignOperationPoint(x, leftOperand, rValue));
                    return;
            }
        }

        #endregion

        #region Function visiting

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            var arguments = CreateArguments(x.CallSignature);

            RValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            Result(new FunctionCallPoint(x, thisObj, arguments));
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            var arguments = CreateArguments(x.CallSignature);
            var name = CreateRValue(x.PublicNameExpr);

            RValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            Result(new IndirectFunctionCallPoint(x, name, thisObj, arguments));
        }

        public override void VisitIncludingEx(IncludingEx x)
        {
            var possibleFiles = CreateRValue(x.Target);


            Result(new IncludingExPoint(x, possibleFiles));
        }

        public override void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            visitConstantNularyElement(x, (e) => e.CreateLambda(x));
        }

        public override void VisitNewEx(NewEx x)
        {
            var arguments = CreateArguments(x.CallSignature);

            RValuePoint name = null;
            if (!(x.ClassNameRef is DirectTypeRef))
            {
                name = CreateRValue(x.ClassNameRef);
            }


            Result(new NewExPoint(x, name, arguments));
        }

        #endregion

        #region Structured values visiting

        public override void VisitItemUse(ItemUse x)
        {
            if (x.IsMemberOf != null)
            {
                throw new NotImplementedException();
            }

            var array = CreateRValue(x.Array);
            var index = CreateRValue(x.Index);

            Result(new RItemUsePoint(x, array, index));

        }

        public override void VisitArrayEx(ArrayEx x)
        {
            var operands = new LinkedList<KeyValuePair<RValuePoint, RValuePoint>>();

            foreach (var item in x.Items)
            {

                RValuePoint index = null;
                if (item.Index != null)
                {
                    index = CreateRValue(item.Index);
                }

                RValuePoint value = null;
                var valueItem = item as ValueItem;
                if (valueItem != null)
                {
                    value = CreateRValue(valueItem.ValueExpr);
                }
                else
                {
                    var refItem = item as RefItem;
                    if (refItem != null)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotSupportedException("There is no other array item type");
                    }
                }

                operands.AddLast(new KeyValuePair<RValuePoint, RValuePoint>(index, value));
            }

            Result(new ArrayExPoint(x, operands));
        }
        #endregion


    }
}
