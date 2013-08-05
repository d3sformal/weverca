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
    class PartialWalker : TreeVisitor
    {
        #region Private members

        /// <summary>
        /// Stack of values (can contains Value or VariableName)
        /// </summary>
        private Stack<IStackValue> _valueStack = new Stack<IStackValue>();
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
        private FlowController _flow;

        #endregion

        internal PartialWalker(ExpressionEvaluatorBase evaluator, FunctionResolverBase resolver)
        {
            _evaluator = evaluator;
            _functionResolver = resolver;
        }

        #region Internal API for evaluation

        /// <summary>
        /// Evalulate current partial in flow controller
        /// </summary>
        /// <param name="flow">Flow context of partial</param>
        internal void Eval(FlowController flow)
        {
            var partial = flow.CurrentPartial;

            if (partial == null)
            {
                return;
            }

            _flow = flow;
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
            return popRValue().ReadValue(_evaluator);
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
        /// <returns>popped values</returns>
        private MemoryEntry[] popArguments(CallSignature signature)
        {
            var parCount = signature.Parameters.Count;

            List<MemoryEntry> parameters = new List<MemoryEntry>();
            for (int i = 0; i < parCount; ++i)
            {
                //TODO maybe no all parameters has to be present
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
        }

        /// <summary>
        /// Pushes variable on stack
        /// </summary>
        /// <param name="variable">Pushed variable</param>
        private void push(VariableEntry variable)
        {
            push(new VariableEntryValue(variable));
        }

        private void push(IStackValue value)
        {
            _valueStack.Push(value);
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
                push(new FieldEntryValue(objValue, variableEntry));
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
            var itemHolder = popRValue().ReadIndex(_evaluator);
            var itemIndex = popValue();

            push(new ArrayItem(itemHolder, itemIndex));
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

            //TODO is there alias or value assign ?
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

            _flow.Arguments = arguments;

            if (x.IsMemberOf != null)
            {
                var calledObject = popValue();
                _flow.CalledObject = calledObject;

                _functionResolver.MethodCall(calledObject, name, arguments);
            }
            else
            {
                _functionResolver.Call(name, arguments);
            }

            //Return value won't be pushed, because it's directly inserted from analysis
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            var arguments = popArguments(x.CallSignature);
            var name = popValue();

            _flow.Arguments = arguments;

            if (x.IsMemberOf != null)
            {
                var calledObject = popValue();
                _flow.CalledObject = calledObject;

                _functionResolver.IndirectMethodCall(calledObject, name, arguments);
            }
            else
            {
                _functionResolver.IndirectCall(name, arguments);
            }

            //Return value won't be pushed, because it's directly inserted from analysis
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            _functionResolver.DeclareGlobal(x);
        }

        public override void VisitActualParam(ActualParam x)
        {
            //TODO what is its stack behaviour ?
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
            //no stack behaviour

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

            _flow.FlowResolver.Include(_flow, possibleFiles);
        }

        #endregion

        /// <summary>
        /// Visit method for NativeAnalyzer
        /// </summary>
        /// <param name="nativeAnalyzer">Native analyzer</param>
        internal void VisitNative(NativeAnalyzer nativeAnalyzer)
        {
            nativeAnalyzer.Method(_flow);
        }
    }
}
