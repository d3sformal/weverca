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


using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed boolean value as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    public class LeftBooleanOperandVisitor : LeftScalarOperandVisitor<BooleanValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftBooleanOperandVisitor" /> class.
        /// </summary>
        public LeftBooleanOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftBooleanOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        public LeftBooleanOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(leftOperand.Value == value.Value);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(leftOperand.Value != value.Value);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    var leftInteger = TypeConversion.ToInteger(leftOperand.Value);
                    var rightInteger = TypeConversion.ToInteger(value.Value);
                    result = ArithmeticOperation.Arithmetic(flow, operation, leftInteger, rightInteger);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, rightInteger);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitBooleanValue(value);
                    break;
            }
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    base.VisitGenericNumericValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    // When dividend is true and divisor != +-1, result is 1, otherwise 0
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand.Value), value.Value);
                    break;
                default:
                    var rightBoolean = TypeConversion.ToBoolean(value.Value);
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    var leftInteger = TypeConversion.ToInteger(leftOperand.Value);
                    result = ArithmeticOperation.Arithmetic(flow, operation, leftInteger, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    // When dividend is true and divisor != +-1, result is 1, otherwise 0
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand.Value), value.Value);
                    break;
                default:
                    var rightBoolean = TypeConversion.ToBoolean(value.Value);
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    var leftInteger = TypeConversion.ToInteger(leftOperand.Value);
                    result = ArithmeticOperation.Arithmetic(flow, operation, leftInteger, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitFloatValue(value);
                    break;
            }
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            int integerValue;

            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    // When dividend is true and divisor != +-1, result is 1, otherwise 0
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand.Value), value.Value);
                    break;
                default:
                    var rightBoolean = TypeConversion.ToBoolean(value.Value);
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    var leftInteger = TypeConversion.ToInteger(leftOperand.Value);

                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(value.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    result = isInteger
                        ? ArithmeticOperation.Arithmetic(flow, operation, leftInteger, integerValue)
                        : ArithmeticOperation.Arithmetic(flow, operation, leftInteger, floatValue);

                    if (result != null)
                    {
                        break;
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isHexadecimal)
                    {
                        integerValue = 0;
                    }

                    if (isInteger || (isSuccessful
                        && TypeConversion.TryConvertToInteger(floatValue, out integerValue)))
                    {
                        result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, integerValue);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If the right operand can not be recognized, result can be any integer value.
                        result = BitwiseOperation.Bitwise(OutSet, operation);
                        if (result != null)
                        {
                            break;
                        }
                    }

                    base.VisitStringValue(value);
                    break;
            }
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            var rightBoolean = TypeConversion.ToBoolean(value);
            result = Comparison.Compare(OutSet, operation, leftOperand.Value, rightBoolean);
            if (result != null)
            {
                return;
            }

            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation,
                TypeConversion.ToInteger(leftOperand.Value));
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, rightBoolean);
            if (result != null)
            {
                return;
            }

            base.VisitObjectValue(value);
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, TypeConversion.ToInteger(leftOperand.Value),
                        TypeConversion.ToNativeInteger(Snapshot, value));
                    break;
                default:
                    var rightBoolean = TypeConversion.ToNativeBoolean(Snapshot, value);
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation,
                        TypeConversion.ToInteger(leftOperand.Value),
                        TypeConversion.ToNativeInteger(Snapshot, value));
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAssociativeArray(value);
                    break;
            }
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (operation)
            {
                case Operations.LessThan:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Equal:
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(!leftOperand.Value);
                    break;
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.Or:
                case Operations.Xor:
                    result = leftOperand;
                    break;
                case Operations.Add:
                case Operations.Sub:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = TypeConversion.ToInteger(OutSet, leftOperand);
                    break;
                case Operations.Mul:
                    result = OutSet.CreateInt(0);
                    break;
                default:
                    base.VisitUndefinedValue(value);
                    break;
            }
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    base.VisitGenericIntervalValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand.Value), value);
                    break;
                default:
                    var leftInteger = TypeConversion.ToInteger(leftOperand.Value);
                    result = Comparison.IntervalCompare(OutSet, operation, leftInteger, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftInteger, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitIntervalIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand.Value), value);
                    break;
                default:
                    var leftFloat = TypeConversion.ToFloat(leftOperand.Value);
                    result = Comparison.IntervalCompare(OutSet, operation, leftFloat, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftFloat, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitIntervalFloatValue(value);
                    break;
            }
        }

        #endregion Interval values

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            result = Comparison.RightAbstractBooleanCompare(OutSet, operation, leftOperand.Value);
            if (result != null)
            {
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation, leftOperand.Value);
            if (result != null)
            {
                return;
            }

            base.VisitAnyValue(value);
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            result = Comparison.RightAbstractBooleanCompare(OutSet, operation, leftOperand.Value);
            if (result != null)
            {
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation, leftOperand.Value);
            if (result != null)
            {
                return;
            }

            result = BitwiseOperation.Bitwise(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyScalarValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                default:
                    result = ArithmeticOperation.RightAbstractBooleanArithmetic(flow,
                        operation, TypeConversion.ToInteger(leftOperand.Value));
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyBooleanValue(value);
                    break;
            }
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyNumericValue(AnyNumericValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    base.VisitAnyNumericValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation,
                TypeConversion.ToInteger(leftOperand.Value));
            if (result != null)
            {
                return;
            }

            base.VisitAnyIntegerValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            result = ArithmeticOperation.AbstractFloatArithmetic(Snapshot, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyFloatValue(value);
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    result = ArithmeticOperation.AbstractFloatArithmetic(Snapshot, operation);
                    if (result != null)
                    {
                        // A string can be converted into floating point number too.
                        break;
                    }

                    base.VisitAnyStringValue(value);
                    break;
            }
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            var rightBoolean = TypeConversion.ToBoolean(value);
            result = Comparison.Compare(OutSet, operation, leftOperand.Value, rightBoolean);
            if (result != null)
            {
                return;
            }

            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation,
                TypeConversion.ToInteger(leftOperand.Value));
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, rightBoolean);
            if (result != null)
            {
                return;
            }

            base.VisitAnyObjectValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            result = Comparison.RightAbstractBooleanCompare(OutSet, operation, leftOperand.Value);
            if (result != null)
            {
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation, leftOperand.Value);
            if (result != null)
            {
                return;
            }

            base.VisitAnyArrayValue(value);
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            var rightBoolean = TypeConversion.ToBoolean(value);
            result = Comparison.Compare(OutSet, operation, leftOperand.Value, rightBoolean);
            if (result != null)
            {
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, rightBoolean);
            if (result != null)
            {
                return;
            }

            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation,
                TypeConversion.ToInteger(leftOperand.Value));
            if (result != null)
            {
                // Arithmetic with resources is nonsence
                return;
            }

            base.VisitAnyResourceValue(value);
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}