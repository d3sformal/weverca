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
    internal delegate void OnPointCreated(ProgramPointBase point);

    /// <summary>
    /// Expands statement into chain of program points connected with stack connections
    /// Program points are ordered according to Postfix evaluation order
    /// </summary>
    internal class ElementExpander : TreeVisitor
    {
        /// <summary>
        /// expanded statement
        /// </summary>
        private LangElement _statement;

        /// <summary>
        /// Registered program points indexed by their lang elements
        /// </summary>
        private readonly Dictionary<LangElement, ProgramPointBase> _programPoints
            = new Dictionary<LangElement, ProgramPointBase>();

        /// <summary>
        /// Ordering expanded program point chain in postfix
        /// </summary>
        private readonly List<ProgramPointBase> _postfixChainOrdering = new List<ProgramPointBase>();

        /// <summary>
        /// Program points that are already contained within postfix chain
        /// </summary>
        private readonly HashSet<ProgramPointBase> _chainedPoints = new HashSet<ProgramPointBase>();

        /// <summary>
        /// Points that are prevent to be chained. Theire FlowChild is not implicitly added
        /// </summary>
        private readonly HashSet<ProgramPointBase> _preventedPoints = new HashSet<ProgramPointBase>();

        /// <summary>
        /// Factory used for creating RValues
        /// </summary>
        private readonly RValueFactory _rValueCreator;

        /// <summary>
        /// Prevents a default instance of the <see cref="ElementExpander" /> class from being created.
        /// </summary>
        private ElementExpander()
        {
            _rValueCreator = new RValueFactory(this);
        }

        /// <summary>
        /// Expand given statement into program point chain
        /// </summary>
        /// <param name="statement">Expanded statement</param>
        private void Expand(LangElement statement)
        {
            _statement = statement;
            statement.VisitMe(this);
        }

        /// <summary>
        /// Expand given statement into program point chain
        /// </summary>
        /// <param name="statement">Expanded statement</param>
        /// <param name="onPointCreated">Handler called for every created program point</param>
        /// <returns>Created program poitn chain</returns>
        public static ProgramPointBase[] ExpandStatement(LangElement statement,
            OnPointCreated onPointCreated)
        {
            var expander = new ElementExpander();
            expander.Expand(statement);

            var expandedChain = expander.createPointsChain().ToArray();

            registerCreatedPoints(expandedChain, onPointCreated);

            return expandedChain;
        }

        /// <summary>
        /// Create program point chain from partials orderd according to program flow
        /// </summary>        
        /// <returns>Created program point chain</returns>
        private IEnumerable<ProgramPointBase> createPointsChain()
        {
            for (var i = 1; i < _postfixChainOrdering.Count; ++i)
            {
                var last = _postfixChainOrdering[i - 1];
                var current = _postfixChainOrdering[i];

                if (_preventedPoints.Contains(last))
                    continue;

                last.AddFlowChild(current);
            }

            return _postfixChainOrdering;
        }


        /// <summary>
        /// Append given point at end of current chain
        /// </summary>
        /// <param name="appendedPoint">Point that is appended</param>
        internal void AppendToChain(ProgramPointBase appendedPoint)
        {
            if (!_chainedPoints.Add(appendedPoint))
                //point is already contained in the chain
                return;

            _postfixChainOrdering.Add(appendedPoint);
        }

        /// <summary>
        /// Prevent from adding edge when chaining points
        /// </summary>
        /// <param name="preventedPoint">Point which flow child wont be set</param>
        internal void PreventChainEdge(ProgramPointBase preventedPoint)
        {
            _preventedPoints.Add(preventedPoint);
        }

        /// <summary>
        /// Register created program points with given handler
        /// </summary>
        /// <param name="programPoints">Created program points</param>
        /// <param name="onPointCreated">Registering handler</param>
        private static void registerCreatedPoints(ProgramPointBase[] programPoints, OnPointCreated onPointCreated)
        {
            var pointSet = new HashSet<ProgramPointBase>(programPoints);
            foreach (var point in pointSet)
            {
                onPointCreated(point);
            }
        }

        #region Program point creation

        /// <summary>
        /// Create RValue from given element with forced thisObject if available
        /// </summary>
        /// <param name="el">Element which value will be created</param>
        /// <param name="thisObject">This object used by created RValue</param>
        /// <returns>Created value</returns>
        internal ValuePoint CreateRValue(LangElement el, ValuePoint thisObject = null)
        {
            if (el == null)
            {
                return null;
            }

            ProgramPointBase existingPoint;
            if (_programPoints.TryGetValue(el, out existingPoint))
            {
                return existingPoint as ValuePoint;
            }

            var result = _rValueCreator.CreateValue(el, thisObject);
            _programPoints.Add(el, result);

            AppendToChain(result);

            return result;
        }
        
        /// <summary>
        /// Creates LValue from given element
        /// </summary>
        /// <param name="el">Base element from created LValue</param>
        /// <returns>Created LValue</returns>
        internal LValuePoint CreateLValue(LangElement el)
        {
            var result = CreateRValue(el) as LValuePoint;

            if (result == null)
                throw new NotSupportedException("Element " + el + " is not supported LValue");

            return result;
        }

        /// <summary>
        /// Creates AliasValue from given element
        /// </summary>
        /// <param name="el">Base element from created AliasValue</param>
        /// <returns>Created AliasValue</returns>
        internal ValuePoint CreateAliasValue(LangElement el)
        {
            var result = CreateRValue(el) as ValuePoint;

            if (result == null)
                throw new NotSupportedException("Element " + el + " is not supported AliasValue");

            return result;
        }

        /// <summary>
        /// Register created program point
        /// </summary>
        /// <param name="point">Registered program point</param>
        internal void Register(ProgramPointBase point)
        {
            _programPoints.Add(point.Partial, point);
        }

        /// <summary>
        /// Set result of statement expansion
        /// </summary>
        /// <param name="point">Statement expansion</param>
        private void Result(ProgramPointBase point)
        {
            AppendToChain(point);
            Register(point);
        }

        /// <summary>
        /// Set result from created rvalue (doesn't add result twice into result)
        /// </summary>
        /// <param name="el">Result</param>
        private void RValueResult(LangElement el)
        {
            //result is added via creation call
            CreateRValue(el);
        }

        #endregion

        #region TreeVisitor implementation

        #region Global visiting

        /// <inheritdoc />
        public override void VisitGlobalConstantDecl(GlobalConstantDecl x)
        {
            var constantValue = CreateRValue(x.Initializer);

            Result(new ConstantDeclPoint(x, constantValue));
        }

        /// <inheritdoc />
        public override void VisitGlobalStmt(GlobalStmt x)
        {
            var variables = new List<LValuePoint>();
            foreach (var varItem in x.VarList)
            {
                var lValue = CreateLValue(varItem);

                variables.Add(lValue);
            }

            Result(new GlobalStmtPoint(x, variables.ToArray()));
        }

        #endregion

        #region Variable visiting

        /// <inheritdoc />
        public override void VisitItemUse(ItemUse x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitDirectVarUse(DirectVarUse x)
        {
            RValueResult(x);
        }

        #endregion

        #region Assign expressions visiting

        /// <inheritdoc />
        public override void VisitRefAssignEx(RefAssignEx x)
        {
            var rOperand = CreateAliasValue(x.RValue);
            var lOperand = CreateLValue(x.LValue);

            Result(new RefAssignPoint(x, lOperand, rOperand));
        }

        /// <inheritdoc />
        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            RValueResult(x);
        }

        #endregion

        #region Function visiting

        /// <inheritdoc />
        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitDirectStMtdCall(DirectStMtdCall x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitIndirectStMtdCall(IndirectStMtdCall x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitFunctionDecl(FunctionDecl x)
        {
            Result(new FunctionDeclPoint(x));
        }

        /// <inheritdoc />
        public override void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            RValueResult(x);
        }

        #endregion

        /// <inheritdoc />
        public override void VisitElement(LangElement element)
        {
            throw new NotSupportedException("Element " + _statement + " is not supported as statement");
        }

        /// <inheritdoc />
        public override void VisitBinaryEx(BinaryEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitConditionalEx(ConditionalEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitEvalEx(EvalEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitUnaryEx(UnaryEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitConcatEx(ConcatEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitIncDecEx(IncDecEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitArrayEx(ArrayEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitJumpStmt(JumpStmt x)
        {
            var expression = CreateRValue(x.Expression);
            Result(new JumpStmtPoint(expression, x));
        }

        /// <inheritdoc />
        public override void VisitTypeDecl(TypeDecl x)
        {
            Result(new TypeDeclPoint(x));
        }

        /// <inheritdoc />
        public override void VisitGlobalConstUse(GlobalConstUse x)
        {
            RValueResult(x);
        }

        public override void VisitClassConstUse(ClassConstUse x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitNewEx(NewEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitInstanceOfEx(InstanceOfEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitIncludingEx(IncludingEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitForeachStmt(ForeachStmt x)
        {
            var enumeree = CreateRValue(x.Enumeree);

            LValuePoint keyVar = null;
            LValuePoint valueVar = null;

            if (x.KeyVariable != null)
            {
                keyVar = CreateLValue(x.KeyVariable.Variable);
            }

            if (x.ValueVariable != null)
            {
                valueVar = CreateLValue(x.ValueVariable.Variable);
            }

            Result(new ForeachStmtPoint(x, enumeree, keyVar, valueVar));
        }

        /// <inheritdoc />
        public override void VisitEchoStmt(EchoStmt x)
        {
            var parameters = new List<ValuePoint>();
            foreach (var param in x.Parameters)
            {
                parameters.Add(CreateRValue(param));
            }

            Result(new EchoStmtPoint(x, parameters.ToArray()));
        }

        /// <inheritdoc />
        public override void VisitIssetEx(IssetEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitEmptyEx(EmptyEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitExitEx(ExitEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitThrowStmt(ThrowStmt x)
        {
            var throwedValue = CreateRValue(x.Expression);
            Result(new ThrowStmtPoint(x, throwedValue));
        }

        #region Literals

        /// <inheritdoc />
        public override void VisitIntLiteral(IntLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitStringLiteral(StringLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitBoolLiteral(BoolLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitNullLiteral(NullLiteral x)
        {
            RValueResult(x);
        }

        #endregion

        #endregion

        /// <summary>
        /// Visit method for NativeAnalyzer
        /// </summary>
        /// <param name="nativeAnalyzer">Native analyzer</param>
        internal void VisitNative(NativeAnalyzer nativeAnalyzer)
        {
            Result(new NativeAnalyzerPoint(nativeAnalyzer));
        }

    }
}