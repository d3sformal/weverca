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
        DeclarationResolver _resolver;

        public PartialWalker(ExpressionEvaluator evaluator, DeclarationResolver resolver)
        {
            _evaluator = evaluator;
            _resolver = resolver;
        }


        internal void EvalNext(FlowControler flow,LangElement partial)
        {
            eval(flow, partial);
        }

        private void eval(FlowControler flow, LangElement partial)
        {
            _evaluator.SetContext(flow, partial);
            _resolver.SetContext(flow, partial);

            partial.VisitMe(this);
        }

        private MemoryEntry popValue()
        {
            throw new NotImplementedException();
        }

        private VariableName popVariable()
        {
            throw new NotImplementedException();
        }

        private void push(MemoryEntry value)
        {
            _valueStack.Push(value);
        }

        private void push(VariableName variable)
        {
            _valueStack.Push(variable);
        }

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
            throw new NotImplementedException();
        }

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            throw new NotImplementedException();
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            throw new NotImplementedException();
        }

        private List<MemoryEntry> getParameters(CallSignature signature)
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

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
