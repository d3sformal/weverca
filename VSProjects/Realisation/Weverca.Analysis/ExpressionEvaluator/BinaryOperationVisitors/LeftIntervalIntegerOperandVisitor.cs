using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with interval of integer values as the left operand
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    public class LeftIntegerIntervalOperandVisitor : GenericLeftOperandVisitor<IntegerIntervalValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftIntegerIntervalOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftIntegerIntervalOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitScalarValue(ScalarValue value)
        {
            if (!BitwiseOperation())
            {
                base.VisitScalarValue(value);
            }
        }

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
                    result = LogicalOperation.Logical(OutSet, operation, leftOperand, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    var rightInteger = TypeConversion.ToInteger(value.Value);
                    result = Comparison.IntervalCompare(OutSet, operation, leftOperand, rightInteger);
                    if (result != null)
                    {
                        break;
                    }

                    if (ArithmeticOperation(leftOperand, rightInteger))
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
                    result = Comparison.Equal(OutSet, leftOperand, value.Value);
                    break;
                case Operations.NotIdentical:
                    result = Comparison.NotEqual(OutSet, leftOperand, value.Value);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand, value.Value);
                    break;
                default:
                    result = Comparison.IntervalCompare(OutSet, operation, leftOperand, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    if (ArithmeticOperation(leftOperand, value.Value))
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                        TypeConversion.ToBoolean(value.Value));
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
                    result = ModuloOperation.Modulo(flow, leftOperand, value.Value);
                    break;
                default:
                    result = Comparison.IntervalCompare(OutSet, operation,
                        TypeConversion.ToFloatInterval(OutSet, leftOperand), value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                        TypeConversion.ToBoolean(value.Value));
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
                    result = ModuloOperation.Modulo(flow, leftOperand, value.Value);
                    break;
                default:
                    result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                        TypeConversion.ToBoolean(value.Value));
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    var isSuccessful = TypeConversion.TryConvertToNumber(value.Value, true,
                        out integerValue, out floatValue, out isInteger);

                    if (isInteger)
                    {
                        result = Comparison.IntervalCompare(OutSet, operation, leftOperand, integerValue);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        result = Comparison.IntervalCompare(OutSet, operation,
                            TypeConversion.ToFloatInterval(OutSet, leftOperand), floatValue);
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
                    result = Comparison.IntervalCompare(OutSet, operation,
                        leftOperand, TypeConversion.ToInteger(value));
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
                    result = Comparison.Equal(OutSet, leftOperand, value);
                    break;
                case Operations.NotIdentical:
                    result = Comparison.NotEqual(OutSet, leftOperand, value);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand, value);
                    break;
                default:
                    result = Comparison.IntervalCompare(OutSet, operation, leftOperand, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand, value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitIntervalIntegerValue(value);
                    break;
            }
        }

        #endregion Interval values

        #endregion AbstractValueVisitor Members
    }
}
