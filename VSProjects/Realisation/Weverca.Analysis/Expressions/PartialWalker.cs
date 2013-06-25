using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    class PartialWalker : TreeVisitor
    {
        Stack<object> _valueStack = new Stack<object>();

        ExpressionEvaluator _evaluator;
        FunctionResolver _resolver;
        FlowControler _currentControler;

        public PartialWalker(ExpressionEvaluator evaluator, FunctionResolver resolver)
        {
            _evaluator = evaluator;
            _resolver = resolver;
        }

        internal void Eval(FlowControler flow, LangElement partial)
        {
            flow.OutSet.Extend(flow.InSet);

            if (partial == null)
            {
                return;
            }

            _currentControler = flow;
            _evaluator.SetContext(flow, partial);
          
            partial.VisitMe(this);
        }
 
        internal void InsertReturnValue(MemoryEntry returnValue)
        {
            push(returnValue);
        }

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

        private VariableName popVariable()
        {
            var value = _valueStack.Pop();

            Debug.Assert(value is VariableName, "Variable was expected on stack - incorrect stack behaviour");

            return (VariableName)value;
        }

        private void push(MemoryEntry value)
        {
            _valueStack.Push(value);
        }

        private void push(VariableName variable)
        {
            _valueStack.Push(variable);
        }

        private List<MemoryEntry> getArguments(CallSignature signature)
        {

            var parCount = signature.Parameters.Count;

            List<MemoryEntry> parameters = new List<MemoryEntry>();
            for (int i = 0; i < parCount; ++i)
            {
                //TODO maybe no all parameters has to be present
                parameters.Add(popValue());
            }
            parameters.Reverse();
            return parameters;
        }

        #endregion

        #region TreeVisitor  overrides
        public override void VisitAssignEx(AssignEx x)
        {
            var value = popValue();
            var variable = popVariable();
            _evaluator.Assign(variable, value);
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
            var arguments = getArguments(x.CallSignature);
            var name = x.QualifiedName;

            //Result value won't be popped, because it's directly inserted from analysis
            var callInput = _currentControler.OutSet.CreateCall(null, arguments.ToArray());            
            var methodGraph=_resolver.InitializeCall(callInput, name);

            var info = new CallInfo(callInput, methodGraph);
            _currentControler.AddDispatch(info);
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            throw new NotImplementedException();
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

        
        internal void VisitNative(NativeAnalyzer nativeAnalyzer)
        {
            nativeAnalyzer.Method(_currentControler);
        }
    }
}
