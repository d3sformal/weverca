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
    public class LeftIntegerOperandVisitor : GenericLeftOperandVisitor<IntegerValue>
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
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Add:
                    // Result of addition can overflow
                    if ((leftOperand.Value >= int.MaxValue) && value.Value)
                    {
                        // If aritmetic overflows, result is double
                        result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            + TypeConversion.ToFloat(value.Value));
                    }
                    else
                    {
                        result = OutSet.CreateInt(leftOperand.Value + TypeConversion.ToInteger(value.Value));
                    }
                    break;
                case Operations.Sub:
                    // Result of addition can underflow
                    if ((leftOperand.Value <= int.MinValue) && value.Value)
                    {
                        // If aritmetic underflows, result is double
                        result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            - TypeConversion.ToFloat(value.Value));
                    }
                    else
                    {
                        result = OutSet.CreateInt(leftOperand.Value - TypeConversion.ToInteger(value.Value));
                    }
                    break;
                case Operations.Mul:
                    if (value.Value)
                    {
                        result = OutSet.CreateInt(leftOperand.Value);
                    }
                    else
                    {
                        result = OutSet.CreateInt(0);
                    }
                    break;
                case Operations.Div:
                    if (value.Value)
                    {
                        result = OutSet.CreateInt(leftOperand.Value);
                    }
                    else
                    {
                        SetWarning("Division by zero (converted from boolean false)",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by false returns false boolean value
                        result = OutSet.CreateBool(false);
                    }
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

                    if (BitwiseOperation(leftOperand.Value, TypeConversion.ToInteger(value.Value)))
                    {
                        break;
                    }

                    base.VisitBooleanValue(value);
                    break;
            }
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

                    if (ArithmeticOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (BitwiseOperation(leftOperand.Value, value.Value))
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

                    if (ArithmeticOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    int rightInteger;
                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (BitwiseOperation(leftOperand.Value, rightInteger))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (IsOperationBitwise())
                        {
                            result = OutSet.AnyIntegerValue;
                            break;
                        }
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
                        result = Comparison.Compare(OutSet, operation,
                            leftOperand.Value, integerValue);
                        if (result != null)
                        {
                            break;
                        }

                        if (ArithmeticOperation(leftOperand.Value, integerValue))
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

                        if (ArithmeticOperation(leftOperand.Value, floatValue))
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
                        if (BitwiseOperation(leftOperand.Value, integerValue))
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
            switch (operation)
            {
                case Operations.Identical:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.Mul:
                case Operations.BitAnd:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.Sub:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = leftOperand;
                    break;
                case Operations.Div:
                case Operations.Mod:
                    SetWarning("Division by zero (converted from null)",
                        AnalysisWarningCause.DIVISION_BY_ZERO);
                    // Division by null returns false boolean value
                    result = OutSet.CreateBool(false);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation,
                        leftOperand.Value, TypeConversion.ToInteger(value));
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

        #endregion

        #endregion AbstractValueVisitor Members
    }
}
