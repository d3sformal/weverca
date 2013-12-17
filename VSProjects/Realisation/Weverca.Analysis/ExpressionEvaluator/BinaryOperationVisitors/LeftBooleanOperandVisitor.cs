using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed boolean value as the left operand
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    public class LeftBooleanOperandVisitor : GenericLeftOperandVisitor<BooleanValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftBooleanOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
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
                case Operations.Mod:
                    if (value.Value)
                    {
                        // Modulo by 1 (true) is always 0
                        result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        DivisionByFalse();
                    }
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

                    if (BitwiseOperation(leftInteger, rightInteger))
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
                case Operations.Add:
                    // Result of addition can overflow
                    if (leftOperand.Value && (value.Value >= int.MaxValue))
                    {
                        // If aritmetic overflows, result is double
                        result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            + TypeConversion.ToFloat(value.Value));
                    }
                    else
                    {
                        result = OutSet.CreateInt(TypeConversion.ToInteger(leftOperand.Value) + value.Value);
                    }
                    break;
                case Operations.Sub:
                    // Result of subtraction can overflow
                    if (value.Value > (int.MinValue + TypeConversion.ToInteger(leftOperand.Value)))
                    {
                        result = OutSet.CreateInt(TypeConversion.ToInteger(leftOperand.Value) - value.Value);
                    }
                    else
                    {
                        // If aritmetic overflows, result is double
                        result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            - TypeConversion.ToFloat(value.Value));
                    }
                    break;
                case Operations.Mul:
                    if (leftOperand.Value)
                    {
                        result = OutSet.CreateInt(value.Value);
                    }
                    else
                    {
                        result = OutSet.CreateInt(0);
                    }
                    break;
                case Operations.Div:
                    if (value.Value != 0)
                    {
                        if (leftOperand.Value)
                        {
                            if ((value.Value == 1) || (value.Value == -1))
                            {
                                result = value;
                            }
                            else
                            {
                                result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                                    / TypeConversion.ToFloat(value.Value));
                            }
                        }
                        else
                        {
                            result = OutSet.CreateInt(0);
                        }
                    }
                    else
                    {
                        DivisionByZero();
                    }
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

                    if (BitwiseOperation(TypeConversion.ToInteger(leftOperand.Value), value.Value))
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
            int rightInteger;

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

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation,
                        TypeConversion.ToFloat(leftOperand.Value), value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (BitwiseOperation(TypeConversion.ToInteger(leftOperand.Value), rightInteger))
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
                        if (BitwiseOperation(leftInteger, integerValue))
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
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.CreateBool(leftOperand.Value);
                    break;
                case Operations.Equal:
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(!leftOperand.Value);
                    break;
                case Operations.Identical:
                case Operations.LessThan:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(true);
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
                    result = TypeConversion.ToInteger(OutSet, leftOperand);
                    break;
                case Operations.Div:
                case Operations.Mod:
                    DivisionByNull();
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
                    if (BitwiseOperation())
                    {
                        break;
                    }

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
                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, value);
                    if (result != null)
                    {
                        break;
                    }

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
                    result = LogicalOperation.Logical(OutSet, operation, leftOperand.Value, value);
                    if (result != null)
                    {
                        break;
                    }

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

                    base.VisitIntervalFloatValue(value);
                    break;
            }
        }

        #endregion Interval values

        #endregion AbstractValueVisitor Members
    }
}
