/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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
    /// </summary>F
    internal class RValueFactory : TreeVisitor
    {
        /// <summary>
        /// Here is stored result of CreateValue operation
        /// </summary>
        private ValuePoint _resultPoint;

        private readonly Stack<ValuePoint> _forcedThisObjects = new Stack<ValuePoint>();


        /// <summary>
        /// Expander is used for creating sub values
        /// </summary>
        private readonly ElementExpander _valueCreator;

        internal RValueFactory(ElementExpander valueCreator)
        {
            _valueCreator = valueCreator;
        }

        /// <summary>
        /// Create RValue point from given element with forced thisObject
        /// </summary>
        /// <param name="el">Element from which RValue point will be created</param>
        /// <param name="thisObject">This object used by created RValue if specified</param>
        /// <returns>Created RValue</returns>
        internal ValuePoint CreateValue(LangElement el, ValuePoint thisObject)
        {
            //empty current result, because of avoiding incorrect use
            _resultPoint = null;

            _forcedThisObjects.Push(thisObject);
            el.VisitMe(this);
            _forcedThisObjects.Pop();

            //assert that result has been set
            if (_resultPoint == null)
                throw new NotSupportedException("Element " + el + " is not supported RValue");

            //empty result because of incorrect use
            var result = _resultPoint;
            _resultPoint = null;

            return result;
        }

        /// <summary>
        /// Append given point at end of current chain
        /// </summary>
        /// <param name="appendedPoint">Point that is appended</param>
        internal void AppendToChain(ProgramPointBase appendedPoint)
        {
            _valueCreator.AppendToChain(appendedPoint);
        }

        /// <summary>
        /// Prevent from adding edge when chaining points
        /// </summary>
        /// <param name="preventedPoint">Point which flow child wont be set</param>
        internal void PreventChainEdge(ProgramPointBase preventedPoint)
        {
            _valueCreator.PreventChainEdge(preventedPoint);
        }

        #region Creating program points

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
        /// Visit element, which doesn't accept any values and it's value is constant during whole computation
        /// </summary>
        /// <param name="e">Visited element</param>
        /// <param name="provider"></param>
        private void visitConstantNularyElement(LangElement e, ConstantProvider provider)
        {
            Result(new ConstantPoint(e, provider));
        }

        /// <summary>
        /// All program points that are not returne via Result method has to be registered by current method
        /// </summary>
        /// <param name="point">Registered point</param>
        private void RegisterPoint(ProgramPointBase point)
        {
            _valueCreator.Register(point);
        }

        /// <summary>
        /// Create RValue from given element with forced thisObject
        /// </summary>
        /// <param name="el">Element which value will be created</param>
        /// <param name="thisObject">This object used by created RValue</param>
        /// <returns>Created value</returns>
        private ValuePoint CreateRValue(LangElement el, ValuePoint thisObject = null)
        {
            return _valueCreator.CreateRValue(el, thisObject);
        }

        /// <summary>
        /// Get member of value point for given element if possible. Forced member of points are prioritized
        /// </summary>
        /// <param name="el">Element which member of is needed</param>
        /// <returns>Value point of MemberOf if available</returns>
        private ValuePoint GetMemberOf(VarLikeConstructUse el)
        {
            var forcedMemberOf = _forcedThisObjects.Peek();
            if (forcedMemberOf != null)
                return forcedMemberOf;

            var memberEl = el.IsMemberOf;
            if (memberEl == null)
                return null;

            return CreateRValue(memberEl);
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
            //Unsupported elements are catched in Factory methods    
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

        public override void VisitClassConstUse(ClassConstUse x)
        {
            ValuePoint thisObj = null;
            if (x.ClassName.QualifiedName.Name.Value == "")
            {
                thisObj = CreateRValue(x.TypeRef);
            }

            Result(new ClassConstPoint(x, thisObj));
        }

        public override void VisitPseudoConstUse(PseudoConstUse x)
        {
            Result(new PseudoConstantPoint(x));
        }

        #endregion

        #region Variable visiting

        public override void VisitDirectStFldUse(DirectStFldUse x)
        {
            var indirectType = x.TypeRef as IndirectTypeRef;

            if (indirectType == null)
            {
                Result(new StaticFieldPoint(x));
            }
            else
            {
                var typeName = CreateRValue(indirectType);
                Result(new StaticFieldPoint(x, typeName));
            }
        }

        public override void VisitIndirectStFldUse(IndirectStFldUse x)
        {
            var indirectType = x.TypeRef as IndirectTypeRef;
            var variableName = CreateRValue(x.FieldNameExpr);
            if (indirectType == null)
            {
                Result(new IndirectStaticFieldPoint(x, variableName));
            }
            else
            {
                var typeName = CreateRValue(indirectType);

                Result(new IndirectStaticFieldPoint(x, variableName, typeName));
            }

        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            var thisObj = GetMemberOf(x);
            Result(new VariablePoint(x, thisObj));
        }

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            var thisObj = GetMemberOf(x);
            var name = CreateRValue(x.VarNameEx);
            Result(new IndirectVariablePoint(x, name, thisObj));
        }

        public override void VisitIndirectTypeRef(IndirectTypeRef x)
        {
            var variable = CreateRValue(x.ClassNameVar);
            Result(variable);
        }

        public override void VisitStaticStmt(StaticStmt x)
        {
            base.VisitStaticStmt(x);
        }


        #endregion

        #region Expression visiting

        public override void VisitConditionalEx(ConditionalEx x)
        {
            /* Points are created in current ordering
                1. conditionExpr,
                2. trueAssume,
                3. trueExpr,
                4. falseAssume,
                5. falseExpr,              
             */

            //1.
            // starts branching
            var conditionExpr = CreateRValue(x.CondExpr);


            //create true branch
            var trueAssumeCond = new AssumptionCondition(ConditionForm.All, x.CondExpr);
            var trueAssume = new AssumePoint(trueAssumeCond, new[] { conditionExpr });
            //2.
            AppendToChain(trueAssume);
            //3.
            var trueExpr = CreateRValue(x.TrueExpr);

            //create false branch
            var falseAssumeCond = new AssumptionCondition(ConditionForm.None, x.CondExpr);
            var falseAssume = new AssumePoint(falseAssumeCond, new[] { conditionExpr });
            //4.
            AppendToChain(falseAssume);

            //5.
            var falseExpr = CreateRValue(x.FalseExpr);

            //connect condition - true assume will be connected via chaining            
            conditionExpr.AddFlowChild(falseAssume);

            //create result
            var expression = new ConditionalExPoint(x, conditionExpr, trueAssume, falseAssume, trueExpr, falseExpr);

            //false expr is added when processing substitution
            PreventChainEdge(trueExpr);
            trueExpr.AddFlowChild(expression);

            // Both branches must be processed before the expression
            conditionExpr.CreateWorklistSegment(expression);

            Result(expression);
        }

        public override void VisitUnaryEx(UnaryEx x)
        {
            var operand = CreateRValue(x.Expr);

            Result(new UnaryExPoint(x, operand));
        }

        public override void VisitBinaryEx(BinaryEx x)
        {
            var lOperand = CreateRValue(x.LeftExpr);
            ValuePoint rOperand;

            BinaryExPoint expression;
            switch (x.PublicOperation)
            {
                case Operations.And:
                case Operations.Or:

                    /* Points are created in current ordering
                          1. blockStart,
                          2. shortendPath,
                          3. nonShortendPath,
                          4. rOperand
                     */

                    var shortableForm = x.PublicOperation == Operations.And ? ConditionForm.None : ConditionForm.All;
                    var nonShortableForm = shortableForm == ConditionForm.All ? ConditionForm.None : ConditionForm.All;

                    var shortableCondition = new AssumptionCondition(shortableForm, x.LeftExpr);
                    //shortened evaluation path
                    var shortendPath = new AssumePoint(shortableCondition, new[] { lOperand });

                    var nonShortableCondition = new AssumptionCondition(nonShortableForm, x.LeftExpr);
                    //normal evaluation
                    var nonShortendPath = new AssumePoint(nonShortableCondition, new[] { lOperand });

                    //block borders
                    var blockStart = new EmptyProgramPoint();
                    //1.
                    AppendToChain(blockStart);
                    //2.
                    AppendToChain(shortendPath);
                    //3. 
                    AppendToChain(nonShortendPath);
                    //4.
                    rOperand = CreateRValue(x.RightExpr);

                    expression = new BinaryExPoint(x, lOperand, rOperand);

                    //shortend path is added via chain
                    blockStart.AddFlowChild(nonShortendPath);

                    //set explicit edge
                    PreventChainEdge(shortendPath);
                    shortendPath.AddFlowChild(expression);



                    break;
                default:
                    rOperand = CreateRValue(x.RightExpr);
                    expression = new BinaryExPoint(x, lOperand, rOperand);
                    break;
            }

            Result(expression);
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

        public override void VisitListEx(ListEx x)
        {
            // Create lvalues for all elements that are assigned
            var assignedTo = new List<LValuePoint>(x.LValues.Count());
            foreach (var lValue in x.LValues)
            {
                if (lValue != null)
                    assignedTo.Add(CreateLValue(lValue));
                else
                {
                    assignedTo.Add(null);
                }
            }
            // Create rvalue
            var assignedFrom = CreateRValue(x.RValue);

            Result(new AssignListPoint(x, assignedTo, assignedFrom));
        }

        #endregion

        #region Function visiting

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            var arguments = CreateArguments(x.CallSignature);

            var thisObj = GetMemberOf(x);

            Result(new FunctionCallPoint(x, thisObj, arguments));
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            var arguments = CreateArguments(x.CallSignature);
            var name = CreateRValue(x.PublicNameExpr);

            var thisObj = GetMemberOf(x);

            Result(new IndirectFunctionCallPoint(x, name, thisObj, arguments));
        }

        public override void VisitDirectStMtdCall(DirectStMtdCall x)
        {
            var arguments = CreateArguments(x.CallSignature);

            ValuePoint thisObj = null;
            if (x.ClassName.QualifiedName.Name.Value == "")
            {
                thisObj = CreateRValue(x.PublicTypeRef);
            }

            Result(new StaticMethodCallPoint(x, thisObj, arguments));
        }

        public override void VisitIndirectStMtdCall(IndirectStMtdCall x)
        {
            var arguments = CreateArguments(x.CallSignature);
            var name = CreateRValue(x.MethodNameVar);

            ValuePoint thisObj = null;
            if (x.ClassName.QualifiedName.Name.Value == "")
            {
                thisObj = CreateRValue(x.PublicTypeRef);
            }
            Result(new IndirectStaticMethodCallPoint(x, thisObj, name, arguments));
        }

        public override void VisitIncludingEx(IncludingEx x)
        {
            var possibleFiles = CreateRValue(x.Target);

            Result(new IncludingExPoint(x, possibleFiles));
        }

        public override void VisitEvalEx(EvalEx x)
        {
            var code = CreateRValue(x.Code);

            Result(new EvalExPoint(x, code));
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
            var index = CreateRValue(x.Index);

            var thisObj = GetMemberOf(x);

            var array = CreateRValue(x.Array, thisObj);


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
                        throw new NotSupportedException("Array RefItem is not supported now");
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