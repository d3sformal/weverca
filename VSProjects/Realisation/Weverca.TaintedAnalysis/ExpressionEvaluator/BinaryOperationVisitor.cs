using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    internal abstract class LeftOperandVisitor : AbstractValueVisitor
    {
    }

    internal class LeftOperandBooleanValueVisitor : LeftOperandVisitor
    {
        private BooleanValue leftOperand;
        private BinaryOperationVisitor visitor;

        internal LeftOperandBooleanValueVisitor(BooleanValue value, BinaryOperationVisitor binaryVisitor)
        {
            leftOperand = value;
            visitor = binaryVisitor;
        }

        #region IValueVisitor Members

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        public override void VisitFloatValue(FloatValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        #endregion
    }

    internal class LeftOperandIntegerValueVisitor : LeftOperandVisitor
    {
        private IntegerValue leftOperand;
        private BinaryOperationVisitor visitor;

        internal LeftOperandIntegerValueVisitor(IntegerValue value, BinaryOperationVisitor binaryVisitor)
        {
            leftOperand = value;
            visitor = binaryVisitor;
        }

        #region IValueVisitor Members

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        public override void VisitFloatValue(FloatValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        #endregion
    }

    internal class LeftOperandFloatValueVisitor : LeftOperandVisitor
    {
        private FloatValue leftOperand;
        private BinaryOperationVisitor visitor;

        internal LeftOperandFloatValueVisitor(FloatValue value, BinaryOperationVisitor binaryVisitor)
        {
            leftOperand = value;
            visitor = binaryVisitor;
        }

        #region IValueVisitor Members

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        public override void VisitFloatValue(FloatValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        #endregion
    }

    public class BinaryOperationVisitor : AbstractValueVisitor
    {
        private LeftOperandVisitor visitor;

        public BinaryOperationVisitor(ExpressionEvaluator expressionEvaluator)
        {
            Evaluator = expressionEvaluator;
        }

        public Value Evaluate(Value leftOperand, Operations binaryOperation, Value rightOperand)
        {
            // Sets current operation
            Operation = binaryOperation;

            // Gets type of left operand and creates appropriate visitor
            Debug.Assert(visitor == null, "Visitor must never be null");
            leftOperand.Accept(this);

            // Gets type of right operand and evaluate expression for given operation
            Debug.Assert(visitor != null, "Visitor must never be null");
            rightOperand.Accept(visitor);
            visitor = null;

            // Returns result of binary operation
            Debug.Assert(Result != null, "Visitor of left operand must return value");
            var result = Result;
            Result = null;
            return result;
        }

        internal ExpressionEvaluator Evaluator { get; private set; }

        internal Operations Operation { get; private set; }

        internal Value Result { get; set; }

        #region IValueVisitor Members

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        public override void VisitFloatValue(FloatValue value)
        {
            visitor = new LeftOperandFloatValueVisitor(value, this);
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            visitor = new LeftOperandBooleanValueVisitor(value, this);
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            visitor = new LeftOperandIntegerValueVisitor(value, this);
        }

        #endregion
    }
}
