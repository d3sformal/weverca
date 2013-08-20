using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    /// <summary>
    /// Partial walker is used for postfix evaluation of Postfix expressions/statements
    /// </summary>
    internal class PartialWalker : TreeVisitor
    {
        #region Private members

        /// <summary>
        /// Stack of values (can contains Value or VariableName)
        /// </summary>
        private Stack<IStackValue> _valueStack = new Stack<IStackValue>();

        /// <summary>
        /// Log of values evaluated for partials
        /// </summary>
        private EvaluationLog _log = new EvaluationLog();

        /// <summary>
        /// Available expression evaluator
        /// </summary>
        private ExpressionEvaluatorBase _evaluator;

        /// <summary>
        /// Available function resolver
        /// </summary>
        private FunctionResolverBase _functionResolver;

        /// <summary>
        /// Controller available for current eval
        /// </summary>
        internal FlowController CurrentFlow { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialWalker" /> class.
        /// </summary>
        /// <param name="evaluator"></param>
        /// <param name="resolver"></param>
        internal PartialWalker(ExpressionEvaluatorBase evaluator, FunctionResolverBase resolver)
        {
            _evaluator = evaluator;
            _functionResolver = resolver;
        }

        #region Internal API for evaluation

        /// <summary>
        /// Evaluate current partial in flow controller
        /// </summary>
        /// <param name="flow">Flow context of partial</param>
        internal void Eval(FlowController flow)
        {
            var partial = flow.CurrentPartial;

            if (partial == null)
            {
                return;
            }

            CurrentFlow = flow;
            CurrentFlow.SetLog(_log);
            _evaluator.SetContext(flow);
            _functionResolver.SetContext(flow);

            partial.VisitMe(this);
        }

        /// <summary>
        /// Insert return value into valueStack
        /// NOTE:
        ///     Is called from outside, because of non-recursive call handling
        /// </summary>
        /// <param name="returnValue">Inserted return value</param>
        internal void InsertReturnValue(MemoryEntry returnValue)
        {
            push(returnValue);
        }

        /// <summary>
        /// Pop all values currently available on stack
        /// </summary>
        /// <returns>Popped values</returns>
        internal MemoryEntry[] PopAllValues()
        {
            var result = new MemoryEntry[_valueStack.Count];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = popValue();
            }

            return result;
        }

        #endregion

        #region Value stack operations

        /// <summary>
        /// Reset partial walker between statement processing
        /// NOTE:
        ///     Is needed, because expression can be on line alone - but its value isn't consumed anywhere
        /// </summary>
        internal void Reset()
        {
            _valueStack.Clear();
        }

        /// <summary>
        /// Pop top of stack as rvalue
        /// </summary>
        /// <returns>Popped value</returns>
        private RValue popRValue()
        {
            var rValue = _valueStack.Pop() as RValue;

            Debug.Assert(rValue != null, "RValue expected on stack - incorrect stack behaviour");

            return rValue;
        }

        private MemoryEntry popValue()
        {
            var rValue = popRValue();
            var value = rValue.ReadValue(_evaluator);

            tryLogValue(rValue, value);

            return value;
        }

        private void tryLogValue(IStackValue stackValue,MemoryEntry value)
        {
            var lValue = stackValue as LValue;
            if (lValue != null)
            {
                //LValues doesn't have logged result value
                _log.AssociateValue(lValue.AssociatedPartial, value);
            }
        }

        /// <summary>
        /// Pop top of stack as VariableName
        /// </summary>
        /// <returns>Popped variable name</returns>
        private LValue popLValue()
        {
            var lValue = _valueStack.Pop() as LValue;

            Debug.Assert(lValue != null, "LValue expected on stack - incorrect stack behaviour");

            return lValue;
        }

        /// <summary>
        /// Pop arguments according to call signature
        /// </summary>
        /// <param name="signature">Popped call signature</param>
        /// <returns>Popped values</returns>
        private MemoryEntry[] popArguments(CallSignature signature)
        {
            var parCount = signature.Parameters.Count;

            var parameters = new List<MemoryEntry>();
            for (int i = 0; i < parCount; ++i)
            {
                // TODO: Maybe no all parameters has to be present
                parameters.Add(popValue());
            }

            parameters.Reverse();
            return parameters.ToArray();
        }

        /// <summary>
        /// Pushes value on stack
        /// </summary>
        /// <param name="value">Pushed value</param>
        private void push(MemoryEntry value)
        {
            push(new MemoryEntryValue(value));
            _log.AssociateValue(CurrentFlow.CurrentPartial, value);
        }

        /// <summary>
        /// Pushes variable on stack
        /// </summary>
        /// <param name="variable">Pushed variable</param>
        private void push(VariableEntry variable)
        {
            var associatedPartial = CurrentFlow.CurrentPartial;
            push(new VariableEntryValue(associatedPartial, variable));
            _log.AssociateVariable(associatedPartial, variable);
        }

        private void push(IStackValue value)
        {
            _valueStack.Push(value);
        }

        internal void OnComplete()
        {
            //we has to compute possible last variable/expression on stack
            if (_valueStack.Count > 0)
            {
                var value = popValue();
                push(value);
            }
        }

        #endregion

        #region TreeVisitor overrides - used for evaluating

        #region Literals visiting

        public override void VisitStringLiteral(StringLiteral x)
        {
            push(_evaluator.StringLiteral(x));
        }

        public override void VisitBoolLiteral(BoolLiteral x)
        {
            push(_evaluator.BoolLiteral(x));
        }

        public override void VisitIntLiteral(IntLiteral x)
        {
            push(_evaluator.IntLiteral(x));
        }

        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            push(_evaluator.LongIntLiteral(x));
        }

        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            push(_evaluator.DoubleLiteral(x));
        }

        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            throw new NotImplementedException("What is binary string literal ? ");
        }

        public override void VisitNullLiteral(NullLiteral x)
        {
            push(_evaluator.NullLiteral(x));
        }

        public override void VisitGlobalConstUse(GlobalConstUse x)
        {
            push(_evaluator.Constant(x));
        }

        public override void VisitGlobalConstantDecl(GlobalConstantDecl x)
        {
            var constantValue = popValue();
            _evaluator.ConstantDeclaration(x, constantValue);
        }

        #endregion

        #region Variable visiting

        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            var varValue = popValue();

            var varNames = _evaluator.VariableNames(varValue);
            if (varNames == null)
            {
                varNames = new string[0];
            }

            push(new VariableEntry(varNames));
        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            var variableEntry = new VariableEntry(x.VarName);

            if (x.IsMemberOf != null)
            {
                var objValue = popValue();
                push(new FieldEntryValue(x.IsMemberOf, objValue, variableEntry));
            }
            else
            {
                push(variableEntry);
            }
        }

        #endregion

        #region Assign expressions visiting

        public override void VisitItemUse(ItemUse x)
        {
            var item = popRValue();
            var itemHolder = item.ReadIndex(_evaluator);
            var itemIndex = popValue();

            tryLogValue(item, itemHolder);

            push(new ArrayItem(x, itemHolder, itemIndex));
        }

        public override void VisitAssignEx(AssignEx x)
        {
            throw new NotImplementedException();
        }

        public override void VisitRefAssignEx(RefAssignEx x)
        {
            var aliasedVariable = popRValue();
            var assignedVariable = popLValue();

            var alias = aliasedVariable.ReadAlias(_evaluator);
            assignedVariable.AssignAlias(_evaluator, alias);

            // TODO: Is there alias or value assign?
            push(aliasedVariable);
        }

        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            var value = popValue();

            var assignedVariable = popLValue();

            assignedVariable.AssignValue(_evaluator, value);

            push(value);
        }

        #endregion

        #region Function visiting

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            var arguments = popArguments(x.CallSignature);
            var name = x.QualifiedName;

            CurrentFlow.Arguments = arguments;

            if (x.IsMemberOf != null)
            {
                var calledObject = popValue();
                CurrentFlow.CalledObject = calledObject;

                _functionResolver.MethodCall(calledObject, name, arguments);
            }
            else
            {
                _functionResolver.Call(name, arguments);
            }

            // Return value won't be pushed, because it's directly inserted from analysis
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            var arguments = popArguments(x.CallSignature);
            var name = popValue();

            CurrentFlow.Arguments = arguments;

            if (x.IsMemberOf != null)
            {
                var calledObject = popValue();
                CurrentFlow.CalledObject = calledObject;

                _functionResolver.IndirectMethodCall(calledObject, name, arguments);
            }
            else
            {
                _functionResolver.IndirectCall(name, arguments);
            }

            // Return value won't be pushed, because it's directly inserted from analysis
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            _functionResolver.DeclareGlobal(x);
        }

        public override void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            push(_evaluator.CreateLambda(x));
        }

        public override void VisitActualParam(ActualParam x)
        {
            // TODO: what is its stack behaviour ?
        }

        public override void VisitBlockStmt(BlockStmt x)
        {
            // TODO: what is its stack behaviour ?
        }

        #endregion

        public override void VisitBinaryEx(BinaryEx x)
        {
            var rightOperand = popValue();
            var leftOperand = popValue();

            var result = _evaluator.BinaryEx(leftOperand, x.PublicOperation, rightOperand);
            push(result);
        }

        public override void VisitUnaryEx(UnaryEx x)
        {
            var operand = popValue();

            var result = _evaluator.UnaryEx(x.PublicOperation, operand);
            push(result);
        }

        public override void VisitArrayEx(ArrayEx x)
        {
            var operands = new Stack<KeyValuePair<MemoryEntry, MemoryEntry>>(x.Items.Count);
            for (int i = x.Items.Count - 1; i >= 0; --i)
            {
                var value = popValue();
                MemoryEntry key;
                if (x.Items[i].Index != null)
                {
                    key = popValue();
                }
                else
                {
                    key = null;
                }
                operands.Push(new KeyValuePair<MemoryEntry, MemoryEntry>(key, value));
            }

            var result = _evaluator.ArrayEx(operands);
            push(result);
        }

        public override void VisitJumpStmt(JumpStmt x)
        {
            switch (x.Type)
            {
                case JumpStmt.Types.Return:
                    var value = popValue();
                    push(_functionResolver.Return(value));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void VisitTypeDecl(TypeDecl x)
        {
            // No stack behaviour

            _functionResolver.DeclareGlobal(x);
        }

        public override void VisitNewEx(NewEx x)
        {
            var arguments = popArguments(x.CallSignature);
            var possibleObjects = _evaluator.CreateObject(x.ClassNameRef.GenericQualifiedName.QualifiedName);

            push(possibleObjects);
        }

        public override void VisitIncludingEx(IncludingEx x)
        {
            var possibleFiles = popValue();

            CurrentFlow.FlowResolver.Include(CurrentFlow, possibleFiles);
        }

        public override void VisitForeachStmt(ForeachStmt x)
        {
            var enumerre = popValue();

            VariableEntry keyVar = null;
            VariableEntry valueVar = null;

            if (x.KeyVariable != null)
            {
                keyVar = popLValue().GetVariableEntry();
            }

            if (x.ValueVariable != null)
            {
                valueVar = popLValue().GetVariableEntry();
            }

            _evaluator.Foreach(enumerre, keyVar, valueVar);
        }

        #endregion

        /// <summary>
        /// Visit method for NativeAnalyzer
        /// </summary>
        /// <param name="nativeAnalyzer">Native analyzer</param>
        internal void VisitNative(NativeAnalyzer nativeAnalyzer)
        {
            nativeAnalyzer.Method(CurrentFlow);
        }
    }
}
