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
                        if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                        {
                            result = OutSet.CreateInt(0);
                        }
                        else
                        {
                            result = OutSet.AnyIntegerValue;
                        }
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
                    if (ComparisonOperation(leftBoolean, value.Value))
                    {
                        break;
                    }

                    if (ArithmeticOperation(leftOperand.Value, TypeConversion.ToFloat(value.Value)))
                    {
                        break;
                    }

                    if (LogicalOperation(leftBoolean, value.Value))
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
                    if (value.Value != 0)
                    {
                        if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                        {
                            // Value has the same sign as dividend
                            result = OutSet.CreateInt(leftInteger % value.Value);
                        }
                        else
                        {
                            result = OutSet.AnyIntegerValue;
                        }
                    }
                    else
                    {
                        SetWarning("Division by floating-point zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by floating-point zero does not return NaN, but false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    break;
                default:
                    if (ComparisonOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (ArithmeticOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToBoolean(value.Value)))
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
                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (rightInteger != 0)
                        {
                            if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                            {
                                // Value has the same sign as dividend
                                result = OutSet.CreateInt(leftInteger % rightInteger);
                            }
                            else
                            {
                                result = OutSet.AnyIntegerValue;
                            }
                        }
                        else
                        {
                            SetWarning("Division by floating-point zero",
                                AnalysisWarningCause.DIVISION_BY_ZERO);
                            // Division by floating-point zero does not return NaN, but false boolean value
                            result = OutSet.CreateBool(false);
                        }
                    }
                    else
                    {
                        // As right operant can has any value, can be 0 too
                        // That causes division by zero and returns false
                        SetWarning("Division by any integer, possible division by zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        result = OutSet.AnyValue;
                    }
                    break;
                default:
                    if (ComparisonOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (ArithmeticOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToBoolean(value.Value)))
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

                    base.VisitFloatValue(value);
                    break;
            }
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            int integerValue;
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
                    TypeConversion.TryConvertToInteger(value.Value, out integerValue);
                    if (integerValue != 0)
                    {
                        if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                        {
                            // Value has the same sign as dividend
                            result = OutSet.CreateInt(leftInteger % integerValue);
                        }
                        else
                        {
                            result = OutSet.AnyIntegerValue;
                        }
                    }
                    else
                    {
                        SetWarning("Division by zero (converted from string)",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by zero returns false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    break;
                default:
                    if (LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToBoolean(value.Value)))
                    {
                        break;
                    }

                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(value.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    if (ComparisonOperation(leftOperand.Value, floatValue))
                    {
                        break;
                    }

                    if (ArithmeticOperation(leftOperand.Value, floatValue))
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
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.Equal:
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(leftOperand.Value));
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
                case Operations.Add:
                case Operations.Sub:
                    result = OutSet.CreateDouble(leftOperand.Value);
                    break;
                case Operations.Mul:
                    result = OutSet.CreateDouble((leftOperand.Value >= 0.0) ? 0.0 : -0.0);
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
                    SetWarning("Division by zero (converted from null)",
                        AnalysisWarningCause.DIVISION_BY_ZERO);
                    // Division by null returns false boolean value
                    result = OutSet.CreateBool(false);
                    break;
                default:
                    base.VisitUndefinedValue(value);
                    break;
            }
        }

        #endregion Concrete values

        #endregion AbstractValueVisitor Members
    }
}
