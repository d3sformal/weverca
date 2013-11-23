using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    public class BinaryOperationVisitor : AbstractValueVisitor
    {
        /// <summary>
        /// Flow controller of program point providing data for evaluation (output set, position etc.)
        /// </summary>
        private FlowController flow;

        /// <summary>
        /// Visitor of left operand that has concrete boolean value
        /// </summary>
        private LeftBooleanOperandVisitor booleanVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete integer value
        /// </summary>
        private LeftIntegerOperandVisitor integerVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete floating-point value
        /// </summary>
        private LeftFloatOperandVisitor floatVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete string value
        /// </summary>
        private LeftStringOperandVisitor stringVisitor;

        /// <summary>
        /// Visitor of left operand that is null value
        /// </summary>
        private LeftNullOperandVisitor nullVisitor;

        /// <summary>
        /// Selected visitor of left operand that performs binary operations with the given right operand
        /// </summary>
        private LeftOperandVisitor visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperationVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public BinaryOperationVisitor(FlowController flowController)
        {
            booleanVisitor = new LeftBooleanOperandVisitor(flowController);
            integerVisitor = new LeftIntegerOperandVisitor(flowController);
            floatVisitor = new LeftFloatOperandVisitor(flowController);
            stringVisitor = new LeftStringOperandVisitor(flowController);
            nullVisitor = new LeftNullOperandVisitor(flowController);
        }

        /// <summary>
        /// Evaluates binary operation on the given left and right operands
        /// </summary>
        /// <param name="leftOperand">The left operand of binary operation</param>
        /// <param name="binaryOperation">Binary operation to be performed</param>
        /// <param name="rightOperand">The right operand of binary operation</param>
        /// <returns>Result of performing the binary operation on the operands</returns>
        public Value Evaluate(Value leftOperand, Operations binaryOperation, Value rightOperand)
        {
            // Gets visitor of left operand
            leftOperand.Accept(this);
            Debug.Assert(visitor != null, "Visiting of left operand must return its visitor");

            visitor.SetContext(flow);
            return visitor.Evaluate(binaryOperation, rightOperand);
        }

        /// <summary>
        /// Evaluates binary operation on all value combinations of the left and right operands
        /// </summary>
        /// <param name="leftOperand">Entry with all possible left operands of binary operation</param>
        /// <param name="binaryOperation">Binary operation to be performed</param>
        /// <param name="rightOperand">Entry with all possible right operands of binary operation</param>
        /// <returns>Resulting entry after performing the binary operation on all possible operands</returns>
        public MemoryEntry Evaluate(MemoryEntry leftOperand, Operations binaryOperation,
            MemoryEntry rightOperand)
        {
            var values = new HashSet<Value>();

            foreach (var leftValue in leftOperand.PossibleValues)
            {
                // Gets visitor of left operand
                leftValue.Accept(this);
                Debug.Assert(visitor != null, "Visiting of left operand must return its visitor");

                visitor.SetContext(flow);
                var entry = visitor.Evaluate(binaryOperation, rightOperand);
                values.UnionWith(entry.PossibleValues);
            }

            return new MemoryEntry(values);
        }

        /// <summary>
        /// Set current evaluation context.
        /// </summary>
        /// <param name="flowController">Flow controller of program point available for evaluation</param>
        public void SetContext(FlowController flowController)
        {
            flow = flowController;
        }

        #region IValueVisitor Members

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            throw new NotSupportedException(
                "The value with its type cannot be left operand of any binary operation");
        }

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            booleanVisitor.SetLeftOperand(value);
            visitor = booleanVisitor;
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            integerVisitor.SetLeftOperand(value);
            visitor = integerVisitor;
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            floatVisitor.SetLeftOperand(value);
            visitor = floatVisitor;
        }

        #endregion

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            stringVisitor.SetLeftOperand(value);
            visitor = stringVisitor;
        }

        #endregion

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            nullVisitor.SetLeftOperand(value);
            visitor = nullVisitor;
        }

        #endregion

        #endregion
    }
}
