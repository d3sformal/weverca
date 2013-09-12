using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    class RValueCreator : TreeVisitor
    {
        private RValuePoint _resultPoint;

        private readonly ElementExpander _valueCreator;

        internal RValueCreator(ElementExpander valueCreator)
        {
            _valueCreator = valueCreator;
        }

        internal RValuePoint CreateValue(LangElement el)
        {
            _resultPoint = null;
            el.VisitMe(this);
            Debug.Assert(_resultPoint != null);

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

        private void Result(RValuePoint value)
        {
            Debug.Assert(_resultPoint == null, "Value has to be null - someone doesn't read it's value");
            _resultPoint = value;
        }

        private RValuePoint CreateRValue(LangElement el)
        {
            return _valueCreator.CreateRValue(el);
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
            throw new NotImplementedException("what is binary string literal ?");
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
            Result(new RVariablePoint(x));
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            var name = CreateRValue(x.VarNameEx);

            Result(new RIndirectVariablePoint(x, name));
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

        #endregion

        #region Function visiting

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            var arguments = getArguments(x.CallSignature);

            if (x.IsMemberOf != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                Result(new FunctionCallPoint(x, arguments));
            }
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            var arguments = getArguments(x.CallSignature);
            var name = CreateRValue(x.PublicNameExpr);

            Result(new IndirectFunctionCallPoint(x, name, arguments));
        }

        public override void VisitIncludingEx(IncludingEx x)
        {
            var possibleFiles = CreateRValue(x.Target);

            Result(new IncludePoint(x, possibleFiles));
        }

        public override void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            visitConstantNularyElement(x,(e)=>e.CreateLambda(x));
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

        private RValuePoint[] getArguments(CallSignature signature)
        {
            var args = new List<RValuePoint>();
            foreach (var param in signature.Parameters)
            {
                var value = CreateRValue(param.Expression);
                args.Add(value);
            }

            return args.ToArray();
        }
    }
}
