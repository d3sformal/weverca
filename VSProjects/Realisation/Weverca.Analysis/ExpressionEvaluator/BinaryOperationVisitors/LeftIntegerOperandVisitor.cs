using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed integer value as the left operand
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    public class LeftIntegerOperandVisitor : LeftNumericOperandVisitor<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftIntegerOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftIntegerOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            var leftBoolean = TypeConversion.ToBoolean(leftOperand.Value);
            result = Comparison.Compare(OutSet, operation, leftBoolean, value.Value);
            if (result != null)
            {
                return;
            }

            var rightInteger = TypeConversion.ToInteger(value.Value);
            result = ArithmeticOperation.Arithmetic(flow, operation,
                leftOperand.Value, rightInteger);
            if (result != null)
            {
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation, leftBoolean, value.Value);
            if (result != null)
            {
                return;
            }

            result = BitwiseOperation.Bitwise(OutSet, operation, leftOperand.Value, rightInteger);
            if (result != null)
            {
                return;
            }

            base.VisitBooleanValue(value);
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(leftOperand.Value == value.Value);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(leftOperand.Value != value.Value);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value.Value);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value.Value));
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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value.Value);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value.Value));
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
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value.Value);
                    break;
                default:
                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value.Value));
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(value.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    if (isInteger)
                    {
                        result = Comparison.Compare(OutSet, operation, leftOperand.Value, integerValue);
                        if (result != null)
                        {
                            break;
                        }

                        result = ArithmeticOperation.Arithmetic(flow, operation,
                            leftOperand.Value, integerValue);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        result = Comparison.Compare(OutSet, operation, leftOperand.Value, floatValue);
                        if (result != null)
                        {
                            break;
                        }

                        result = ArithmeticOperation.Arithmetic(flow, operation,
                            leftOperand.Value, floatValue);
                        if (result != null)
                        {
                            break;
                        }
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isHexadecimal)
                    {
                        integerValue = 0;
                    }

                    if (isInteger || (isSuccessful
                        && TypeConversion.TryConvertToInteger(floatValue, out integerValue)))
                    {
                        result = BitwiseOperation.Bitwise(OutSet, operation,
                            leftOperand.Value, integerValue);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If the right operand can not be recognized, result can be any integer value.
                        if (BitwiseOperation.IsBitwise(operation))
                        {
                            result = OutSet.AnyIntegerValue;
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
            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation, leftOperand.Value);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation");
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            result = BitwiseOperation.Bitwise(OutSet, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by bitwise operation");
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
                    result = ModuloOperation.Modulo(flow, leftOperand.Value,
                        TypeConversion.ToNativeInteger(OutSet, value));
                    break;
                default:
                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToNativeBoolean(OutSet, value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation, leftOperand.Value,
                        TypeConversion.ToNativeInteger(OutSet, value));
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
        public override void VisitResourceValue(ResourceValue value)
        {
            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation, leftOperand.Value);
            if (result != null)
            {
                // Arithmetic with resources is nonsence
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            base.VisitResourceValue(value);
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.Equal:
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(leftOperand.Value));
                    break;
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = leftOperand;
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
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = Comparison.Equal(OutSet, leftOperand.Value, value);
                    break;
                case Operations.NotIdentical:
                    result = Comparison.NotEqual(OutSet, leftOperand.Value, value);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value);
                    break;
                default:
                    result = Comparison.IntervalCompare(OutSet, operation, leftOperand.Value, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftOperand.Value, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), value);
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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value);
                    break;
                default:
                    result = Comparison.IntervalCompare(OutSet, operation, leftOperand.Value, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftOperand.Value, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), value);
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
            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value));
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
            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value));
            if (result != null)
            {
                return;
            }

            base.VisitAnyScalarValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            result = Comparison.RightAbstractBooleanCompare(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value));
            if (result != null)
            {
                return;
            }

            result = ArithmeticOperation.RightAbstractBooleanArithmetic(flow,
                operation, leftOperand.Value);
            if (result != null)
            {
                return;
            }

            base.VisitAnyBooleanValue(value);
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyNumericValue(AnyNumericValue value)
        {
            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation, leftOperand.Value);
            if (result != null)
            {
                return;
            }

            base.VisitAnyNumericValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                default:
                    base.VisitAnyIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
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
                    base.VisitAnyFloatValue(value);
                    break;
            }
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation, leftOperand.Value);
            if (result != null)
            {
                // A string can be converted into floating point number too.
                return;
            }

            base.VisitAnyStringValue(value);
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation,
                leftOperand.Value);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation");
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            base.VisitAnyObjectValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value));
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
            result = ArithmeticOperation.RightAbstractArithmetic(flow, operation, leftOperand.Value);
            if (result != null)
            {
                // Arithmetic with resources is nonsence
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            base.VisitAnyResourceValue(value);
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}
