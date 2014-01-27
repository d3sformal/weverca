using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation during the analysis
    /// </summary>
    /// <remarks>
    /// Every evaluator must determine type of the value in the expression. Double dispatch is
    /// the solution of this problem and visitor pattern is the way to achieve it. However,
    /// binary operations need to know a type of two values to perform the calculation. We need
    /// triple dispatch, thus two layers of visitor patterns. The first level visitor finds the type of
    /// the left operand. For every operand type, the proper evaluator of the resolved type is selected.
    /// The second level visitor derived from <see cref="LeftOperandVisitor" /> contains internal left
    /// operand of very known type. This visitor can now perform an evaluation by the same way as
    /// one operand operation, i.e. using double dispatch, because the typed left operand is part of it.
    /// There should exist a left operand visitor for every value type.
    /// </remarks>
    public class BinaryOperationEvaluator : PartialExpressionEvaluator
    {
        /// <summary>
        /// String converter used for concatenation of values to strings
        /// </summary>
        private StringConverter converter;

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
        /// Visitor of left operand that has concrete object value
        /// </summary>
        private LeftObjectOperandVisitor objectVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete array value
        /// </summary>
        private LeftArrayOperandVisitor arrayVisitor;

        /// <summary>
        /// Visitor of left operand that is null value
        /// </summary>
        private LeftNullOperandVisitor nullVisitor;

        /// <summary>
        /// Visitor of left operand that has interval of integer values
        /// </summary>
        private LeftIntegerIntervalOperandVisitor integerIntervalVisitor;

        /// <summary>
        /// Visitor of left operand that has interval of floating-point numbers
        /// </summary>
        private LeftFloatIntervalOperandVisitor floatIntervalVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract boolean value
        /// </summary>
        private LeftAnyBooleanOperandVisitor anyBooleanVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract integer value
        /// </summary>
        private LeftAnyIntegerOperandVisitor anyIntegerVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract floating-point number value
        /// </summary>
        private LeftAnyFloatOperandVisitor anyFloatVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract string value
        /// </summary>
        private LeftAnyStringOperandVisitor anyStringVisitor;

        /// <summary>
        /// Selected visitor of left operand that performs binary operations with the given right operand
        /// </summary>
        private LeftOperandVisitor visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperationEvaluator" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        /// <param name="stringConverter">String converter for concatenation</param>
        public BinaryOperationEvaluator(FlowController flowController, StringConverter stringConverter)
            : base(flowController)
        {
            converter = stringConverter;
            booleanVisitor = new LeftBooleanOperandVisitor(flowController);
            integerVisitor = new LeftIntegerOperandVisitor(flowController);
            floatVisitor = new LeftFloatOperandVisitor(flowController);
            stringVisitor = new LeftStringOperandVisitor(flowController);
            objectVisitor = new LeftObjectOperandVisitor(flowController);
            arrayVisitor = new LeftArrayOperandVisitor(flowController);
            nullVisitor = new LeftNullOperandVisitor(flowController);
            integerIntervalVisitor = new LeftIntegerIntervalOperandVisitor(flowController);
            floatIntervalVisitor = new LeftFloatIntervalOperandVisitor(flowController);
            anyBooleanVisitor = new LeftAnyBooleanOperandVisitor(flowController);
            anyIntegerVisitor = new LeftAnyIntegerOperandVisitor(flowController);
            anyFloatVisitor = new LeftAnyFloatOperandVisitor(flowController);
            anyStringVisitor = new LeftAnyStringOperandVisitor(flowController);
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
            if (binaryOperation == Operations.Concat)
            {
                converter.SetContext(flow);
                return converter.EvaluateConcatenation(leftOperand, rightOperand);
            }

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
            if (binaryOperation == Operations.Concat)
            {
                converter.SetContext(flow);
                return converter.EvaluateConcatenation(leftOperand, rightOperand);
            }

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

        #region AbstractValueVisitor Members

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
        public override void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            floatVisitor.SetLeftOperand(value);
            visitor = floatVisitor;
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            stringVisitor.SetLeftOperand(value);
            visitor = stringVisitor;
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            objectVisitor.SetLeftOperand(value);
            visitor = objectVisitor;
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            arrayVisitor.SetLeftOperand(value);
            visitor = arrayVisitor;
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            nullVisitor.SetLeftOperand(value);
            visitor = nullVisitor;
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            integerIntervalVisitor.SetLeftOperand(value);
            visitor = integerIntervalVisitor;
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            floatIntervalVisitor.SetLeftOperand(value);
            visitor = floatIntervalVisitor;
        }

        #endregion Interval values

        #region Abstract values

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            anyBooleanVisitor.SetLeftOperand(value);
            visitor = anyBooleanVisitor;
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            anyIntegerVisitor.SetLeftOperand(value);
            visitor = anyIntegerVisitor;
        }

        /// <inheritdoc />
        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            anyFloatVisitor.SetLeftOperand(value);
            visitor = anyFloatVisitor;
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            anyStringVisitor.SetLeftOperand(value);
            visitor = anyStringVisitor;
        }

        #endregion Abstract scalar values

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}
