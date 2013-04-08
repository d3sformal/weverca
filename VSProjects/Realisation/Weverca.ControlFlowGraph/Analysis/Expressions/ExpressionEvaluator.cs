using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis.Expressions
{
    class ExpressionWalker<FlowInfo> : TreeVisitor
    {
        Stack<FlowInfo> _valueStack = new Stack<FlowInfo>();

        ExpressionEvaluator<FlowInfo> _evaluator;

        public ExpressionWalker(ExpressionEvaluator<FlowInfo> evaluator)
        {
            _evaluator = evaluator;
        }

        internal void Eval(FlowInputSet<FlowInfo> inSet, LangElement element, FlowOutputSet<FlowInfo> outSet)
        {            
            _evaluator.InSet = inSet;
            _evaluator.OutSet = outSet;
            _evaluator.Element = element;
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
            var info = _evaluator.ResolveInfo(x);

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

        internal void Clear()
        {
            _valueStack.Clear();
        }

        internal FlowInfo Pop()
        {
            return pop();
        }
    }

    public abstract class ExpressionEvaluator<FlowInfo>
    {

        public FlowInputSet<FlowInfo> InSet { get; internal set; }

        public FlowOutputSet<FlowInfo> OutSet { get; internal set; }

        public LangElement Element { get; internal set; }

        abstract public FlowInfo Copy(FlowInfo info);
        abstract public FlowInfo Assign(FlowInfo p1, FlowInfo p2);
        abstract public FlowInfo Declare(DirectVarUse x);
        abstract public FlowInfo BinaryEx(FlowInfo op1, Operations operation, FlowInfo op2);
        abstract public FlowInfo StringLiteral(StringLiteral x);

        internal virtual FlowInfo ResolveInfo(DirectVarUse x)
        {
            FlowInfo result;
            InSet.TryGetInfo(x.VarName, out result);
            return result;
        }

        
    }
}
