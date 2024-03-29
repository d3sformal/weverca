/*
Copyright (c) 2012-2014 David Skorvaga and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation during the analysis.
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
    /// <seealso cref="LeftOperandVisitor" />
    /// <seealso cref="UnaryOperationEvaluator" />
    public class BinaryOperationEvaluator : PartialExpressionEvaluator
    {
        /// <summary>
        /// Boolean converter used for evaluation of logical operations.
        /// </summary>
        private BooleanConverter booleanConverter;

        /// <summary>
        /// String converter used for concatenation of values to strings.
        /// </summary>
        private StringConverter stringConverter;

        /// <summary>
        /// Visitor of left operand that has concrete boolean value.
        /// </summary>
        private LeftBooleanOperandVisitor booleanVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete integer value.
        /// </summary>
        private LeftIntegerOperandVisitor integerVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete floating-point value.
        /// </summary>
        private LeftFloatOperandVisitor floatVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete string value.
        /// </summary>
        private LeftStringOperandVisitor stringVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete object value.
        /// </summary>
        private LeftObjectOperandVisitor objectVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete array value.
        /// </summary>
        private LeftArrayOperandVisitor arrayVisitor;

        /// <summary>
        /// Visitor of left operand that is null value.
        /// </summary>
        private LeftNullOperandVisitor nullVisitor;

        /// <summary>
        /// Visitor of left operand that has interval of integer values.
        /// </summary>
        private LeftIntegerIntervalOperandVisitor integerIntervalVisitor;

        /// <summary>
        /// Visitor of left operand that has interval of floating-point numbers.
        /// </summary>
        private LeftFloatIntervalOperandVisitor floatIntervalVisitor;

        /// <summary>
        /// Visitor of left operand that has any abstract value.
        /// </summary>
        private LeftAnyValueOperandVisitor anyValueVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract boolean value.
        /// </summary>
        private LeftAnyBooleanOperandVisitor anyBooleanVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract integer value.
        /// </summary>
        private LeftAnyIntegerOperandVisitor anyIntegerVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract floating-point number value.
        /// </summary>
        private LeftAnyFloatOperandVisitor anyFloatVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract string value.
        /// </summary>
        private LeftAnyStringOperandVisitor anyStringVisitor;

        /// <summary>
        /// Visitor of left operand that has abstract array value.
        /// </summary>
        private LeftAnyArrayOperandVisitor anyArrayVisitor;

        /// <summary>
        /// Visitor of left operand that has concrete or abstract resource value.
        /// </summary>
        private LeftResourceOperandVisitor resourceVisitor;

        /// <summary>
        /// Selected visitor of left operand that performs binary operations with the given right operand.
        /// </summary>
        private LeftOperandVisitor visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperationEvaluator" /> class.
        /// </summary>
        /// <param name="booleanEvaluator">Boolean converter for logical operations.</param>
        /// <param name="stringEvaluator">String converter for concatenation.</param>
        public BinaryOperationEvaluator(BooleanConverter booleanEvaluator, StringConverter stringEvaluator)
            : this(null, booleanEvaluator, stringEvaluator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperationEvaluator" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        /// <param name="booleanEvaluator">Boolean converter for logical operations.</param>
        /// <param name="stringEvaluator">String converter for concatenation.</param>
        public BinaryOperationEvaluator(FlowController flowController, BooleanConverter booleanEvaluator,
            StringConverter stringEvaluator)
            : base(flowController)
        {
            booleanConverter = booleanEvaluator;
            stringConverter = stringEvaluator;
            booleanVisitor = new LeftBooleanOperandVisitor();
            integerVisitor = new LeftIntegerOperandVisitor();
            floatVisitor = new LeftFloatOperandVisitor();
            stringVisitor = new LeftStringOperandVisitor();
            objectVisitor = new LeftObjectOperandVisitor();
            arrayVisitor = new LeftArrayOperandVisitor();
            resourceVisitor = new LeftResourceOperandVisitor();
            nullVisitor = new LeftNullOperandVisitor();
            integerIntervalVisitor = new LeftIntegerIntervalOperandVisitor();
            floatIntervalVisitor = new LeftFloatIntervalOperandVisitor();
            anyValueVisitor = new LeftAnyValueOperandVisitor();
            anyBooleanVisitor = new LeftAnyBooleanOperandVisitor();
            anyIntegerVisitor = new LeftAnyIntegerOperandVisitor();
            anyFloatVisitor = new LeftAnyFloatOperandVisitor();
            anyStringVisitor = new LeftAnyStringOperandVisitor();
            anyArrayVisitor = new LeftAnyArrayOperandVisitor();
        }

        /// <summary>
        /// Evaluates binary operation on all value combinations of the left and right operands.
        /// </summary>
        /// <param name="leftOperand">Entry with all possible left operands of binary operation.</param>
        /// <param name="binaryOperation">Binary operation to be performed.</param>
        /// <param name="rightOperand">Entry with all possible right operands of binary operation.</param>
        /// <returns>Resulting entry of performing the binary operation on all possible operands.</returns>
        public MemoryEntry Evaluate(MemoryEntry leftOperand, Operations binaryOperation,
            MemoryEntry rightOperand)
        {
            if (binaryOperation == Operations.Concat)
            {
                stringConverter.SetContext(flow);
                return stringConverter.EvaluateConcatenation(leftOperand, rightOperand);
            }

            /* TODO: Replace logical operations in binary operation visitors with boolean converter
            booleanConverter.SetContext(OutSet);
            var booleanValue = booleanConverter.EvaluateLogicalOperation(leftOperand,
                binaryOperation, rightOperand);
            if (booleanValue != null)
            {
                return new MemoryEntry(booleanValue);
            }
             */

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

            postprocessValues(values);

            return new MemoryEntry(values);
        }

        private void postprocessValues(HashSet<Value> values)
        {
            // If the result contains AnyValue, remove all other values except of UndefinedValue
            var isUndefined = postprocessAnyValues(values);

            // If the result contains both true and false, replace it with AnyBoolean value
            if ((!isUndefined && values.Count == 2) || (isUndefined && values.Count == 3))
            {
                bool isTrue = false;
                bool isFalse = false;
                foreach (var value in values)
                {
                    var booleanValue = value as ScalarValue<Boolean>;
                    if (booleanValue == null) continue;
                    if (!isTrue) isTrue = booleanValue.Value;
                    if (!isFalse) isFalse = !booleanValue.Value;

                }
                if (isTrue && isFalse)
                {
                    values.Clear();
                    values.Add(OutSet.AnyBooleanValue);
                    if (isUndefined)
                    {
                        values.Add(OutSet.UndefinedValue);
                    }
                }
            }
        }

        private bool postprocessAnyValues(HashSet<Value> values)
        {
            bool isUndefinedValue = false;
            var anyValues = new List<AnyValue>();
            foreach (var value in values)
            {
                if (value is AnyValue)
                {
                    anyValues.Add((AnyValue)value);
                }
                else
                {
                    if (!isUndefinedValue)
                    {
                        isUndefinedValue = value is UndefinedValue;
                    }
                }
            }

            if (anyValues.Count > 0)
            {
                values.Clear();
                values.UnionWith(anyValues);
                if (isUndefinedValue) values.Add(OutSet.UndefinedValue);
            }

            return isUndefinedValue;
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
        /// <exception cref="System.NotSupportedException">Thrown always</exception>
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
            // Almost every binary operation converts object to another type. If object is converted
            // into boolean, it is always true. Exact comparing of objects is complicated to achieve.
            // And comparison into string calls "__toString" magic method. that is not currently supported.
            // Thou we do not need concrete object.
            objectVisitor.SetLeftOperand(OutSet.AnyObjectValue);
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
        public override void VisitResourceValue(ResourceValue value)
        {
            // Every binary operation converts resource to another type, but conversion to another type
            // makes no sence expect boolean that is always true. Thou we do not need concrete resource.
            resourceVisitor.SetLeftOperand(OutSet.AnyResourceValue);
            visitor = resourceVisitor;
        }

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
        /// <exception cref="System.NotSupportedException">Thrown always</exception>
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

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            anyValueVisitor.SetLeftOperand(value);
            visitor = anyValueVisitor;
        }

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
        /// <exception cref="System.NotSupportedException">Thrown always</exception>
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

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            objectVisitor.SetLeftOperand(value);
            visitor = objectVisitor;
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            anyArrayVisitor.SetLeftOperand(value);
            visitor = anyArrayVisitor;
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            resourceVisitor.SetLeftOperand(value);
            visitor = resourceVisitor;
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}