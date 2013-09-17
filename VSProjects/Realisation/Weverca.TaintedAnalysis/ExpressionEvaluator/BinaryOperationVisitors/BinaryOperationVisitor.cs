using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    public class BinaryOperationVisitor : AbstractValueVisitor
    {
        private LeftOperandVisitor visitor;

        public BinaryOperationVisitor(ExpressionEvaluator expressionEvaluator)
        {
            Evaluator = expressionEvaluator;
        }

        internal ExpressionEvaluator Evaluator { get; private set; }

        public Value Evaluate(Value leftOperand, Operations binaryOperation, Value rightOperand)
        {
            // Gets type of left operand and creates appropriate visitor
            Debug.Assert(visitor == null, "Visitor must never be null");
            leftOperand.Accept(this);

            // Sets current operation
            Debug.Assert(visitor != null, "Visitor must be uninitialized at the beginning");
            visitor.Operation = binaryOperation;

            // Gets type of right operand and evaluate expression for given operation
            rightOperand.Accept(visitor);

            // Returns result of binary operation
            Debug.Assert(visitor.Result != null, "Visitor of left operand must return value");
            var result = visitor.Result;
            visitor = null;
            return result;
        }

        #region IValueVisitor Members

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException(
                "The value with its type cannot be left operand of any binary operation");
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            visitor = new LeftNullOperandVisitor(value, Evaluator);
        }

        public override void VisitFloatValue(FloatValue value)
        {
            visitor = new LeftFloatOperandVisitor(value, Evaluator);
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            visitor = new LeftBooleanOperandVisitor(value, Evaluator);
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            visitor = new LeftIntegerOperandVisitor(value, Evaluator);
        }

        #endregion
    }
}
