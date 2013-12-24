using System;

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
    public class LeftNullOperandVisitor : GenericLeftOperandVisitor<UndefinedValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftNullOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftNullOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitScalarValue(ScalarValue value)
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
                    base.VisitScalarValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (operation)
            {
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.CreateBool(value.Value);
                    break;
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(!value.Value);
                    break;
                case Operations.GreaterThan:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Sub:
                    result = OutSet.CreateInt(-TypeConversion.ToInteger(value.Value));
                    break;
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.BitOr:
                case Operations.BitXor:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.Div:
                case Operations.Mod:
                    DivisionByBooleanValue(value.Value);
                    break;
                default:
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
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Sub:
                    // Result of subtraction can overflow
                    if ((value.Value == 0) || ((-value.Value) != 0))
                    {
                        result = OutSet.CreateInt(-value.Value);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationEvaluator.VisitIntegerValue" />
                        result = OutSet.CreateDouble(-(TypeConversion.ToFloat(value.Value)));
                    }
                    break;
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.BitOr:
                case Operations.BitXor:
                    result = value;
                    break;
                case Operations.Div:
                case Operations.Mod:
                    if (value.Value != 0)
                    {
                        // 0 (null) divided or modulo by anything is always 0
                        result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        DivisionByZero();
                    }
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation,
                        TypeConversion.ToInteger(leftOperand), value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            int rightInteger;

            switch (operation)
            {
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Add:
                    result = OutSet.CreateDouble(value.Value);
                    break;
                case Operations.Sub:
                    result = OutSet.CreateDouble(-value.Value);
                    break;
                case Operations.Mul:
                    result = OutSet.CreateDouble((value.Value > 0.0) ? 0.0
                        : ((value.Value < -0.0) ? -0.0 : value.Value));
                    break;
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        result = OutSet.CreateInt(rightInteger);
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.Div:
                    if (value.Value != 0.0)
                    {
                        // 0 (null) divided or modulo by anything is always 0
                        result = OutSet.CreateDouble(0.0);
                    }
                    else
                    {
                        DivisionByFloatingPointZero();
                    }
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand), value.Value);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation,
                        TypeConversion.ToInteger(leftOperand), value.Value);
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
            double floatValue;
            bool isInteger;

            switch (operation)
            {
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Add:
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);
                    if (isInteger)
                    {
                        result = OutSet.CreateInt(integerValue);
                    }
                    else
                    {
                        result = OutSet.CreateDouble(floatValue);
                    }
                    break;
                case Operations.Sub:
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);
                    if (isInteger)
                    {
                        // Result of subtraction can overflow
                        if ((integerValue == 0) || ((-integerValue) != 0))
                        {
                            result = OutSet.CreateInt(-integerValue);
                        }
                        else
                        {
                            // <seealso cref="UnaryOperationEvaluator.VisitIntegerValue" />
                            result = OutSet.CreateDouble(-TypeConversion.ToFloat(integerValue));
                        }
                    }
                    else
                    {
                        result = OutSet.CreateDouble(-floatValue);
                    }
                    break;
                case Operations.Mul:
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);
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
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);
                    if (isInteger)
                    {
                        if (integerValue != 0)
                        {
                            // 0 (null) divided or modulo by anything is always 0
                            result = OutSet.CreateInt(0);
                        }
                        else
                        {
                            DivisionByZero();
                        }
                    }
                    else
                    {
                        if (floatValue != 0.0)
                        {
                            // 0 (null) divided or modulo by anything is always 0
                            result = OutSet.CreateDouble(0.0);
                        }
                        else
                        {
                            DivisionByFloatingPointZero();
                        }
                    }
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand), value.Value);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation, string.Empty, value.Value);
                    if (result != null)
                    {
                        break;
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
                case Operations.Equal:
                case Operations.Identical:
                case Operations.LessThanOrEqual:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.NotEqual:
                case Operations.NotIdentical:
                case Operations.LessThan:
                case Operations.GreaterThan:
                case Operations.And:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Add:
                case Operations.Sub:
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
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
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
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
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    result = value;
                    break;
                case Operations.Mod:
                    if ((value.Start > 0) || (value.End < 0))
                    {
                        result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        // As right operant can be range of values, can possibly be 0 too
                        // That causes division by zero and returns false
                        SetWarning("Division by any integer, possible division by zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        result = OutSet.AnyValue;
                    }
                    break;
                default:
                    var leftInteger = TypeConversion.ToInteger(leftOperand);
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
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            switch (operation)
            {
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    IntegerIntervalValue integerInterval;
                    if (TypeConversion.TryConvertToIntegerInterval(OutSet, value, out integerInterval))
                    {
                        result = integerInterval;
                    }
                    else
                    {
                        result = OutSet.AnyValue;
                    }
                    break;
                case Operations.Mod:
                    if ((value.Start > 0.0) || (value.End < 0.0))
                    {
                        result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        // As right operant can be range of values, can possibly be 0 too
                        // That causes division by zero and returns false
                        SetWarning("Division by any integer, possible division by zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        result = OutSet.AnyValue;
                    }
                    break;
                default:
                    var leftFloat = TypeConversion.ToFloat(leftOperand);
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
