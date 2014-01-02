﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Creates RValue points (created values has stack edges connected, but no flow edges)
    /// <remarks>RValuePoint provides MemoryEntry as result</remarks>
    /// </summary>
    internal class RValueFactory : TreeVisitor
    {
        /// <summary>
        /// Here is stored result of CreateValue operation
        /// </summary>
        private ValuePoint _resultPoint;

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
        internal ValuePoint CreateValue(LangElement el)
        {
            //empty current result, because of avoiding incorrect use
            _resultPoint = null;

            el.VisitMe(this);
            //assert that result has been set
            Debug.Assert(_resultPoint != null, "Visiting of rvalue element must have a result");

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
        /// <param name="provider"></param>
        private void visitConstantNularyElement(LangElement e, ConstantProvider provider)
        {
            Result(new ConstantPoint(e, provider));
        }

        /// <summary>
        /// Set given RValue as CreateValue result
        /// </summary>
        /// <param name="value">Result value</param>
        private void Result(ValuePoint value)
        {
            Debug.Assert(_resultPoint == null, "Value has to be null - someone doesn't read it's value");
            _resultPoint = value;
        }

        /// <summary>
        /// Create RValue from given element
        /// </summary>
        /// <param name="el">Element which value will be created</param>
        /// <returns>Created value</returns>
        private ValuePoint CreateRValue(LangElement el)
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
        /// <param name="signature">Signature which arguments will be created</param>
        /// <returns>Created arguments</returns>
        private ValuePoint[] CreateArguments(CallSignature signature)
        {
            var args = new List<ValuePoint>();
            foreach (var param in signature.Parameters)
            {
                var value = CreateRValue(param.Expression);
                args.Add(value);
            }

            return args.ToArray();
        }

        #endregion

        #region TreeVisitor implementation

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
            ValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            Result(new VariablePoint(x, thisObj));
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            ValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            var name = CreateRValue(x.VarNameEx);
            Result(new IndirectVariablePoint(x, name, thisObj));
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
            var expressions = new List<ValuePoint>();

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

        public override void VisitInstanceOfEx(InstanceOfEx x)
        {
            var expression = CreateRValue(x.Expression);

            ValuePoint name = null;
            if (!(x.ClassNameRef is DirectTypeRef))
            {
                name = CreateRValue(x.ClassNameRef);
            }

            Result(new InstanceOfExPoint(x, expression, name));
        }

        public override void VisitIssetEx(IssetEx x)
        {
            var variables = new List<LValuePoint>();
            foreach (var varItem in x.VarList)
            {
                var lValue = CreateLValue(varItem);

                variables.Add(lValue);
            }

            Result(new IssetPoint(x, variables.ToArray()));
        }

        public override void VisitEmptyEx(EmptyEx x)
        {
            var lValue = CreateLValue(x.Variable);

            Result(new EmptyExPoint(x, lValue));
        }

        public override void VisitExitEx(ExitEx x)
        {
            var resultValue = CreateRValue(x.ResulExpr);

            Result(new ExitExPoint(x, resultValue));
        }

        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            var rValue = CreateRValue(x.RValue);
            switch (x.PublicOperation)
            {
                case Operations.AssignAppend:
                case Operations.AssignPrepend:
                    var concatedValue = CreateRValue(x.LValue);
                    Result(new AssignConcatPoint(x, concatedValue, rValue));
                    return;
                case Operations.AssignValue:
                    var lValue = CreateLValue(x.LValue);
                    Result(new AssignPoint(x, lValue, rValue));
                    return;
                default:
                    //it's assign with binary operation
                    var leftOperand = CreateLValue(x.LValue);
                    Result(new AssignOperationPoint(x, leftOperand, rValue));
                    return;
            }
        }

        #endregion

        #region Function visiting

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            var arguments = CreateArguments(x.CallSignature);

            ValuePoint thisObj = null;
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

            ValuePoint thisObj = null;
            if (x.IsMemberOf != null)
            {
                thisObj = CreateRValue(x.IsMemberOf);
            }

            Result(new IndirectFunctionCallPoint(x, name, thisObj, arguments));
        }

        public override void VisitDirectStMtdCall(DirectStMtdCall x)
        {
            var arguments = CreateArguments(x.CallSignature);

            Debug.Assert(x.IsMemberOf == null, "Static method cannot be called on object");

            Result(new StaticMethodCallPoint(x, arguments));
        }

        public override void VisitIndirectStMtdCall(IndirectStMtdCall x)
        {
            var arguments = CreateArguments(x.CallSignature);
            var name = CreateRValue(x.MethodNameVar);

            Debug.Assert(x.IsMemberOf == null, "Indirect static method cannot be called on object");

            Result(new IndirectStaticMethodCallPoint(x, name, arguments));
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

            ValuePoint name = null;
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

            Result(new ItemUsePoint(x, array, index));
        }

        public override void VisitArrayEx(ArrayEx x)
        {
            var operands = new LinkedList<KeyValuePair<ValuePoint, ValuePoint>>();

            foreach (var item in x.Items)
            {
                ValuePoint index = null;
                if (item.Index != null)
                {
                    index = CreateRValue(item.Index);
                }

                ValuePoint value = null;
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

                operands.AddLast(new KeyValuePair<ValuePoint, ValuePoint>(index, value));
            }

            Result(new ArrayExPoint(x, operands));
        }

        #endregion

        #endregion
    }
}
