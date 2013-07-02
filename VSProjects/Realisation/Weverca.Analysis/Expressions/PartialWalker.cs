using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private Stack<object> _valueStack = new Stack<object>();
        /// <summary>
        /// Available expression evaluator
        /// </summary>
        private ExpressionEvaluator _evaluator;
        /// <summary>
        /// Available function resolver
        /// </summary>
        private FunctionResolver _functionResolver;
        /// <summary>
        /// Controller available for current eval
        /// </summary>
        private FlowController _currentControler;
        #endregion

        internal PartialWalker(ExpressionEvaluator evaluator, FunctionResolver resolver)
        {
            _evaluator = evaluator;
            _functionResolver = resolver;
        }

        /// <summary>
        /// Eval given partial in context of flow
        /// </summary>
        /// <param name="flow">Flow context of partial</param>
        /// <param name="partial">Partial of evaluated expression/statement</param>
        internal void Eval(FlowController flow, LangElement partial)
        {
            //all outsets has to be extended by its in sets
            flow.OutSet.Extend(flow.InSet);

            if (partial == null)
            {
                return;
            }

            _currentControler = flow;
            _evaluator.SetContext(flow, partial);

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
        /// Pop top of stack as value (variableNames will be resolved)
        /// </summary>
        /// <returns>Popped value</returns>
        private MemoryEntry popValue()
        {
            var value = _valueStack.Pop();
            if (value is VariableEntry)
            {
                return _evaluator.ResolveVariable((VariableEntry)value);
            }
            else
            {
                Debug.Assert(value is MemoryEntry, "Unknown type has been pushed on stack - incorrect stack behaviour");
                return value as MemoryEntry;
            }
        }

        /// <summary>
        /// Pop top of stack as VariableName
        /// </summary>
        /// <returns>Popped variable name</returns>
        private VariableEntry popVariable()
        {
            var value = _valueStack.Pop();

            Debug.Assert(value is VariableEntry, "Variable was expected on stack - incorrect stack behaviour");

            return (VariableEntry)value;
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
            _valueStack.Push(value);
        }

        /// <summary>
        /// Pushes variable on stack
        /// </summary>
        /// <param name="variable">Pushed variable</param>
        private void push(VariableEntry variable)
        {
            _valueStack.Push(variable);
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
        #endregion


        #region Variable visiting
        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            var varValue = popValue();

            var varNames = _evaluator.VariableNames(varValue);


            push(new VariableEntry(varNames));
        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            push(new VariableEntry(x.VarName));
        }
        #endregion


        #region Assign expressions visiting
        public override void VisitAssignEx(AssignEx x)
        {
            throw new NotImplementedException();
        }

        public override void VisitRefAssignEx(RefAssignEx x)
        {
            var aliasedVariable = popVariable();
            var assignedVariable = popVariable();

            var alias = _evaluator.ResolveAlias(aliasedVariable);
            _evaluator.AliasAssign(assignedVariable, alias);

            //TODO is there alias or value assign ?
            push(aliasedVariable);
        }

        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            var value = popValue();
            var assignedVariable = popVariable();

            _evaluator.Assign(assignedVariable, value);

            push(value);
        }

        #endregion

        #region Function visiting
        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            var arguments = popArguments(x.CallSignature);
            var name = x.QualifiedName;

            addDispatch(name, arguments);

            //Result value won't be pushed, because it's directly inserted from analysis
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            var arguments = popArguments(x.CallSignature);
            var functionNameValue = popValue();

            var names = _functionResolver.GetFunctionNames(functionNameValue);

            foreach (var name in names)
            {
                addDispatch(name, arguments);
            }
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            _functionResolver.DeclareGlobal(_currentControler.OutSet, x);
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
            push(_evaluator.BinaryEx(leftOperand, x.PublicOperation, rightOperand));
        }

        public override void VisitJumpStmt(JumpStmt x)
        {
            switch (x.Type)
            {
                case JumpStmt.Types.Return:
                    var value = popValue();
                    push(_functionResolver.Return(_currentControler.OutSet, value));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        /// <summary>
        /// Visit method for NativeAnalyzer
        /// </summary>
        /// <param name="nativeAnalyzer">Native analyzer</param>
        internal void VisitNative(NativeAnalyzer nativeAnalyzer)
        {
            nativeAnalyzer.Method(_currentControler);
        }

        /// <summary>
        /// Add dispatch into _currentController
        /// </summary>
        /// <param name="name">Dispatched method name</param>
        /// <param name="arguments">Arguments for call dispatch</param>
        private void addDispatch(QualifiedName name, MemoryEntry[] arguments)
        {
            var declarations = _functionResolver.ResolveFunction(_currentControler.OutSet, name);
            foreach (var declaration in declarations)
            {

                var callInput = _currentControler.OutSet.CreateCall(null, arguments);
                var methodGraph = _functionResolver.InitializeCall(callInput, declaration);

                var info = new CallInfo(callInput, methodGraph);
                _currentControler.AddDispatch(info);
            }
        }
    }
}
