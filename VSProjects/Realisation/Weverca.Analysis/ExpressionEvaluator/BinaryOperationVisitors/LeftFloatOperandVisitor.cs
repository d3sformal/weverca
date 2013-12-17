using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed floating-point value as the left operand
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    public class LeftFloatOperandVisitor : GenericLeftOperandVisitor<FloatValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftFloatOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftFloatOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            int leftInteger;

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
                        DivisionByFalse();
                    }
                    break;
                default:
                    var leftBoolean = TypeConversion.ToBoolean(leftOperand.Value);
                    result = Comparison.Compare(OutSet, operation, leftBoolean, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftOperand.Value,
                        TypeConversion.ToFloat(value.Value));
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                    {
                        if (BitwiseOperation(leftInteger, TypeConversion.ToInteger(value.Value)))
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

                    base.VisitBooleanValue(value);
                    break;
            }
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            int leftInteger;

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

                    if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                    {
                        if (BitwiseOperation(leftInteger, value.Value))
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

                    base.VisitIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            int leftInteger, rightInteger;

            switch (operation)
            {
                case Operations.Identical:
                    SetWarning("Comparing floating-point numbers directly for equality");
                    result = OutSet.CreateBool(leftOperand.Value == value.Value);
                    break;
                case Operations.NotIdentical:
                    SetWarning("Comparing floating-point numbers directly for non-equality");
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

                    if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger)
                        && TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (BitwiseOperation(leftInteger, rightInteger))
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

                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, floatValue);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftOperand.Value, floatValue);
                    if (result != null)
                    {
                        break;
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isHexadecimal)
                    {
                        integerValue = 0;
                    }

                    int leftInteger;
                    if ((isInteger || (isSuccessful
                        && TypeConversion.TryConvertToInteger(floatValue, out integerValue)))
                        && TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
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
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.Identical:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Add:
                case Operations.Sub:
                    result = OutSet.CreateDouble(leftOperand.Value);
                    break;
                case Operations.Mul:
                    result = OutSet.CreateDouble(0.0);
                    break;
                case Operations.BitAnd:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    int leftInteger;
                    if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                    {
                        result = OutSet.CreateInt(leftInteger);
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.Div:
                case Operations.Mod:
                    DivisionByNull();
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
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value);
                    break;
                default:
                    var floatInterval = TypeConversion.ToFloatInterval(OutSet, value);
                    result = Comparison.IntervalCompare(OutSet, operation, leftOperand.Value, floatInterval);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation,
                        leftOperand.Value, floatInterval);
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

                    base.VisitIntervalFloatValue(value);
                    break;
            }
        }

        #endregion Interval values

        #endregion AbstractValueVisitor Members
    }
}
