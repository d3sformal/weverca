using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed string value as the left operand
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    public class LeftStringOperandVisitor : GenericLeftOperandVisitor<StringValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftStringOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftStringOperandVisitor(FlowController flowController)
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
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    if (value.Value)
                    {
                        result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        SetWarning("Division by zero (converted from boolean false)",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by false returns false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    break;
                default:
                    var leftBoolean = TypeConversion.ToBoolean(leftOperand.Value);
                    result = Comparison.Compare(OutSet, operation, leftBoolean, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    var rightInteger = TypeConversion.ToInteger(value.Value);

                    if (isInteger)
                    {
                        if (ArithmeticOperation(integerValue, rightInteger))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (ArithmeticOperation(floatValue, rightInteger))
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
                        if (BitwiseOperation(integerValue, rightInteger))
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If at least one operand can not be recognized, result can be any integer value.
                        if (IsOperationBitwise())
                        {
                            result = OutSet.AnyIntegerValue;
                            break;
                        }
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
            int integerValue;

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

                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    if (isInteger)
                    {
                        result = Comparison.Compare(OutSet, operation, integerValue, value.Value);
                        if (result != null)
                        {
                            break;
                        }

                        if (ArithmeticOperation(integerValue, value.Value))
                        {
                            break;
                        }
                    }
                    else
                    {
                        result = Comparison.Compare(OutSet, operation, floatValue, value.Value);
                        if (result != null)
                        {
                            break;
                        }

                        if (ArithmeticOperation(floatValue, value.Value))
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
                        if (BitwiseOperation(integerValue, value.Value))
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If at least one operand can not be recognized, result can be any integer value.
                        if (IsOperationBitwise())
                        {
                            result = OutSet.AnyIntegerValue;
                            break;
                        }
                    }

                    base.VisitIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            int integerValue;
            int rightInteger;

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

                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    result = Comparison.Compare(OutSet, operation, floatValue, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    if (ArithmeticOperation(floatValue, value.Value))
                    {
                        break;
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isHexadecimal)
                    {
                        integerValue = 0;
                    }

                    if ((isInteger || (isSuccessful
                        && TypeConversion.TryConvertToInteger(floatValue, out integerValue)))
                        && TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (BitwiseOperation(integerValue, rightInteger))
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If at least one operand can not be recognized, result can be any integer value.
                        if (IsOperationBitwise())
                        {
                            result = OutSet.AnyIntegerValue;
                            break;
                        }
                    }

                    base.VisitFloatValue(value);
                    break;
            }
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            int leftInteger;
            int rightInteger;

            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(leftOperand.Value == leftOperand.Value);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(leftOperand.Value != leftOperand.Value);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value.Value);
                    break;
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                    // Bit operations are defined for every character, not for the entire string
                    // TODO: Implement. PHP string is stored as array of bytes, but printed in UTF8 encoding
                    result = OutSet.AnyStringValue;
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, value.Value);
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

                    double leftFloat;
                    bool isLeftInteger;
                    bool isLeftHexadecimal;
                    var isLeftSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out leftInteger, out leftFloat, out isLeftInteger, out isLeftHexadecimal);

                    double rightFloat;
                    bool isRightInteger;
                    bool isRightHexadecimal;
                    var isRightSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out rightInteger, out rightFloat, out isRightInteger, out isRightHexadecimal);

                    if (isLeftInteger && isRightInteger)
                    {
                        if (ArithmeticOperation(leftInteger, rightInteger))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (ArithmeticOperation(leftFloat, rightFloat))
                        {
                            break;
                        }
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isLeftHexadecimal)
                    {
                        leftInteger = 0;
                    }

                    if (isRightHexadecimal)
                    {
                        rightInteger = 0;
                    }

                    if ((isLeftInteger || (isLeftSuccessful
                        && TypeConversion.TryConvertToInteger(leftFloat, out leftInteger)))
                        && (isRightInteger || (isRightSuccessful
                        && TypeConversion.TryConvertToInteger(rightFloat, out rightInteger))))
                    {
                        if (BitwiseOperation(leftInteger, rightInteger))
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If at least one operand can not be recognized, result can be any integer value.
                        if (IsOperationBitwise())
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

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            int integerValue;
            double floatValue;
            bool isInteger;

            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Add:
                case Operations.Sub:
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true, out integerValue, out floatValue, out isInteger);
                    if (isInteger)
                    {
                        result = OutSet.CreateInt(integerValue);
                    }
                    else
                    {
                        result = OutSet.CreateDouble(floatValue);
                    }
                    break;
                case Operations.Mul:
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true, out integerValue, out floatValue, out isInteger);
                    if (isInteger)
                    {
                        result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        result = OutSet.CreateDouble(0.0);
                    }
                    break;
                case Operations.Div:
                case Operations.Mod:
                    SetWarning("Division by zero (converted from null)",
                        AnalysisWarningCause.DIVISION_BY_ZERO);
                    // Division by null returns false boolean value
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.BitAnd:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = TypeConversion.ToInteger(OutSet, leftOperand);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, string.Empty);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitUndefinedValue(value);
                    break;
            }
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            if (!BitwiseOperation())
            {
                base.VisitGenericIntervalValue(value);
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
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
                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), value);
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger);

                    if (isInteger)
                    {
                        result = Comparison.IntervalCompare(OutSet, operation, integerValue, value);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        result = Comparison.IntervalCompare(OutSet, operation,
                            floatValue, TypeConversion.ToFloatInterval(OutSet, value));
                        if (result != null)
                        {
                            break;
                        }
                    }

                    base.VisitIntervalIntegerValue(value);
                    break;
            }
        }

        #endregion Interval values

        #endregion AbstractValueVisitor Members
    }
}
