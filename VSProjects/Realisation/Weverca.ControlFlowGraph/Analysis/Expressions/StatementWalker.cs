﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis.Expressions
{
    class StatementWalker<FlowInfo> : TreeVisitor
    {
        Stack<FlowInfo> _valueStack = new Stack<FlowInfo>();

        ExpressionEvaluator<FlowInfo> _evaluator;
        DeclarationResolver<FlowInfo> _resolver;
        PostfixExpression _expression;

        int _position;

        public bool IsComplete { get; private set; }
        public bool AtStart { get { return _position == 0; } }
        public FlowInfo Result { get; private set; }
        public bool CanEvalNext { get { return !IsComplete && !AwaitingCallReturn; } }
        public bool AwaitingCallReturn { get; private set; }


        public StatementWalker(PostfixExpression expression, ExpressionEvaluator<FlowInfo> evaluator, DeclarationResolver<FlowInfo> resolver)
        {
            _expression = expression;
            _evaluator = evaluator;
            _resolver = resolver;
        }
        
        internal void InsertCallReturn(FlowInfo returnValue)
        {
            if (!AwaitingCallReturn)
                throw new InvalidOperationException("Cannot insert return value when no return value is expected");

            push(returnValue);
            AwaitingCallReturn = false;
        }

        internal void EvalNext(FlowInputSet<FlowInfo> inSet, FlowOutputSet<FlowInfo> outSet)
        {
            while ( _position< _expression.Length)
            {
                var element = _expression.GetElement(_position);
                ++_position;

                eval(inSet, element, outSet);
                if (outSet.CallDispatches != null)
                {
                    AwaitingCallReturn = true;
                    //suspend evaluation
                    return;
                }
            }
            onComplete();
        }

        private void onComplete()
        {
            if (_valueStack.Count > 0)
            {
                Result = pop();
            }
            IsComplete = true;
        }



        private void eval(FlowInputSet<FlowInfo> inSet, LangElement element, FlowOutputSet<FlowInfo> outSet)
        {
            _evaluator.SetContext(inSet, element, outSet);
            _resolver.SetContext(inSet, element, outSet);

            element.VisitMe(this);
        }

        internal FlowInfo pop()
        {
            return _valueStack.Pop();
        }

        private void push(FlowInfo info)
        {
            _valueStack.Push(info);
        }


        #region TreeVisitor  overrides
        public override void VisitAssignEx(AssignEx x)
        {
            var o2 = pop();
            var o1 = pop();
            push(_evaluator.Assign(o1, o2));
        }

        public override void VisitStringLiteral(StringLiteral x)
        {
            push(_evaluator.StringLiteral(x));
        }

        public override void VisitBinaryEx(BinaryEx x)
        {
            var o2 = pop();
            var o1 = pop();
            push(_evaluator.BinaryEx(o1, x.PublicOperation, o2));
        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            var info = _evaluator.ResolveVar(x);

            if (info == null)
            {
                info = _evaluator.Declare(x);

            }
            else
            {
                info = _evaluator.Copy(info);
            }

            push(info);
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            var fcnName = pop();

            var parCount=x.CallSignature.Parameters.Count;

            List<FlowInfo> parameters = new List<FlowInfo>();            
            for (int i = 0; i < parCount; ++i)
            {
                //TODO maybe no all parameters has to be present
                parameters.Add(pop());
            }
            parameters.Reverse();
            _resolver.CallDispatch(fcnName, parameters.ToArray());                  
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            _resolver.Declare(x);
            return;
        }

        #endregion
    }
}
