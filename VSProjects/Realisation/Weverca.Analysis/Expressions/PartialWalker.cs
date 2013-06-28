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
            if (value is VariableName)
            {
                return _evaluator.ResolveVariable((VariableName)value);
            }
            else
            {
                Debug.Assert(value is MemoryEntry,"Unknown type has been pushed on stack - incorrect stack behaviour");
                return value as MemoryEntry;
            }
        }

        /// <summary>
        /// Pop top of stack as VariableName
        /// </summary>
        /// <returns>Popped variable name</returns>
        private VariableName popVariable()
        {
            var value = _valueStack.Pop();

            Debug.Assert(value is VariableName, "Variable was expected on stack - incorrect stack behaviour");

            return (VariableName)value;
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
        private void push(VariableName variable)
        {
            _valueStack.Push(variable);
        }
        
        #endregion

        #region TreeVisitor overrides - used for evaluating

        public override void VisitAssignEx(AssignEx x)
        {
            throw new NotImplementedException();
        }

        public override void VisitRefAssignEx(RefAssignEx x)
        {
            var aliasedVariable = popVariable();
            var assignedVariable = popVariable();

            var alias=_evaluator.ResolveAlias(aliasedVariable);
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

        public override void VisitStringLiteral(StringLiteral x)
        {
            push(_evaluator.StringLiteral(x));
        }

        public override void VisitBinaryEx(BinaryEx x)
        {
            var rightOperand = popValue();
            var leftOperand = popValue();
            push(_evaluator.BinaryEx(leftOperand, x.PublicOperation, rightOperand));
        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            push(x.VarName);
        }

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
            throw new NotImplementedException();
        }

        public override void VisitActualParam(ActualParam x)
        {
            //TODO what is its stack behaviour ?
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
            var callInput = _currentControler.OutSet.CreateCall(null, arguments);
            var methodGraph = _functionResolver.InitializeCall(callInput, name);

            var info = new CallInfo(callInput, methodGraph);
            _currentControler.AddDispatch(info);            
        }
    }
}
