using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed integer value as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    public class LeftNullOperandVisitor : GenericLeftOperandVisitor<UndefinedValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftNullOperandVisitor" /> class.
        /// </summary>
        public LeftNullOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftNullOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
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
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
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
                case Operations.GreaterThan:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(!value.Value);
                    break;
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = value;
                    break;
                case Operations.Add:
                case Operations.BitOr:
                case Operations.BitXor:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.Sub:
                    result = OutSet.CreateInt(-TypeConversion.ToInteger(value.Value));
                    break;
                case Operations.Mul:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Div:
                    if (value.Value)
                    {
                        // Division of 0 (null) by non-null is always 0
                        result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        SetWarning("Division by zero (converted from boolean false)",
                            AnalysisWarningCause.DIVISION_BY_ZERO);

                        // Division or modulo by false returns false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Mod:
                    result = ModuloOperation.ModuloByBooleanValue(flow, value.Value);
                    break;
                default:
                    base.VisitBooleanValue(value);
                    break;
            }
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.GreaterThan:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.LessThanOrEqual:
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
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value.Value));
                    break;
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Add:
                case Operations.BitOr:
                case Operations.BitXor:
                    result = value;
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
                    result = OutSet.CreateInt(0);
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
                        SetWarning("Division by zero", AnalysisWarningCause.DIVISION_BY_ZERO);

                        // Division or modulo by zero returns false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    break;
                default:
                    base.VisitIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value.Value));
                    break;
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Add:
                    result = OutSet.CreateDouble(value.Value);
                    break;
                case Operations.Sub:
                    result = OutSet.CreateDouble(-value.Value);
                    break;
                case Operations.Mul:
                    result = OutSet.CreateDouble(0.0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    int rightInteger;
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
                        SetWarning("Division by floating-point zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);

                        // Division by floating-point zero does not return NaN
                        // or infinite, but false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand), value.Value);
                    break;
                default:
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
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToInteger(leftOperand), value.Value);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation, string.Empty, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);

                    var leftInteger = TypeConversion.ToInteger(leftOperand);
                    result = isInteger
                        ? ArithmeticOperation.Arithmetic(flow, operation, leftInteger, integerValue)
                        : ArithmeticOperation.Arithmetic(flow, operation, leftInteger, floatValue);

                    if (result != null)
                    {
                        break;
                    }

                    base.VisitStringValue(value);
                    break;
            }
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitCompoundValue(CompoundValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.GreaterThan:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    base.VisitCompoundValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            // An object is always greater then null value
            switch (operation)
            {
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    SetWarning("Object cannot be converted to integer",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.Sub:
                case Operations.BitOr:
                case Operations.BitXor:
                    SetWarning("Object cannot be converted to integer",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Div:
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    // We can assume that object is not zero, because null is zero
                    result = OutSet.CreateInt(0);
                    break;
                default:
                    base.VisitObjectValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(!TypeConversion.ToNativeBoolean(OutSet, value));
                    break;
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.Mod:
                    if (TypeConversion.ToNativeBoolean(OutSet, value))
                    {
                        // 0 (null) divided or modulo by anything is always 0
                        result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        SetWarning("Division by zero (converted from array)",
                            AnalysisWarningCause.DIVISION_BY_ZERO);

                        // Division or modulo by zero returns false boolean value
                        result = OutSet.CreateBool(false);
                    }
                    break;
                default:
                    if (ArithmeticOperation.IsArithmetic(operation))
                    {
                        // TODO: This must be fatal error
                        SetWarning("Unsupported operand type: Arithmetic of array and null type");
                        result = OutSet.AnyValue;
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
                    result = ArithmeticOperation.DivisionByNull(flow);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.ModuloByNull(flow);
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
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.Identical:
                case Operations.GreaterThan:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    bool booleanValue;
                    if (TypeConversion.TryConvertToBoolean(value, out booleanValue))
                    {
                        result = OutSet.CreateBool(!booleanValue);
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
                    break;
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    BooleanValue convertedValue;
                    if (TypeConversion.TryConvertToBoolean(OutSet, value, out convertedValue))
                    {
                        result = convertedValue;
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
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
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.BitOr:
                case Operations.BitXor:
                    result = value;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, TypeConversion.ToInteger(leftOperand), value);
                    break;
                default:
                    result = ArithmeticOperation.Arithmetic(flow, operation,
                        TypeConversion.ToInteger(leftOperand), value);
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
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.BitOr:
                case Operations.BitXor:
                    IntervalValue<int> integerInterval;
                    if (TypeConversion.TryConvertToIntegerInterval(OutSet, value, out integerInterval))
                    {
                        result = integerInterval;
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, TypeConversion.ToInteger(leftOperand), value);
                    break;
                default:
                    result = ArithmeticOperation.Arithmetic(flow, operation,
                        TypeConversion.ToFloat(leftOperand), value);
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
            switch (operation)
            {
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.GreaterThanOrEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Add:
                case Operations.Sub:
                    // Ommitted warning message that object cannot be converted to integer
                    // Ommitted error report that array is unsupported operand in arithmetic operation
                    result = OutSet.AnyFloatValue;
                    break;
                case Operations.Mul:
                    // Ommitted warning message that object cannot be converted to integer
                    // Ommitted error report that array is unsupported operand in arithmetic operation
                    result = OutSet.CreateDouble(0.0);
                    break;
                case Operations.Div:
                    // Ommitted warning message that object cannot be converted to integer
                    // Ommitted warning message of division by zero
                    // Ommitted error report that array is unsupported operand in arithmetic operation
                    result = OutSet.AnyValue;
                    break;
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    // Ommitted warning message that object cannot be converted to integer
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    // Ommitted warning message that object cannot be converted to integer
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Mod:
                    // Ommitted warning message that object cannot be converted to integer
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    if (PerformCommonAnyOperandOperations())
                    {
                        break;
                    }

                    base.VisitAnyValue(value);
                    break;
            }
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            switch (operation)
            {
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
                    break;
                default:
                    if (PerformCommonAnyOperandOperations())
                    {
                        break;
                    }

                    base.VisitAnyScalarValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.GreaterThanOrEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = value;
                    break;
                case Operations.Add:
                case Operations.BitOr:
                case Operations.BitXor:
                    result = TypeConversion.AnyBooleanToIntegerInterval(OutSet);
                    break;
                case Operations.Sub:
                    var booleanInterval = TypeConversion.AnyBooleanToIntegerInterval(OutSet);
                    result = OutSet.CreateIntegerInterval(-booleanInterval.End, -booleanInterval.Start);
                    break;
                case Operations.Mul:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Div:
                    SetWarning("Possible division by zero (converted from boolean false)",
                        AnalysisWarningCause.DIVISION_BY_ZERO);

                    // Division or modulo by false returns false boolean value
                    result = OutSet.AnyValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.ModuloByAnyBooleanValue(flow);
                    break;
                default:
                    base.VisitAnyBooleanValue(value);
                    break;
            }
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyNumericValue(AnyNumericValue value)
        {
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.GreaterThanOrEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    base.VisitAnyNumericValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.BitOr:
                case Operations.BitXor:
                    result = value;
                    break;
                default:
                    result = ArithmeticOperation.RightAbstractArithmetic(flow, operation,
                        TypeConversion.ToInteger(leftOperand));
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.BitOr:
                case Operations.BitXor:
                    result = OutSet.AnyIntegerValue;
                    break;
                default:
                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyFloatValue(value);
                    break;
            }
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.GreaterThanOrEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    result = OutSet.AnyIntegerValue;
                    break;
                default:
                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyStringValue(value);
                    break;
            }
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyCompoundValue(AnyCompoundValue value)
        {
            if (PerformCommonAnyOperandOperations())
            {
                return;
            }

            base.VisitAnyCompoundValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            // An object is always greater then null value
            switch (operation)
            {
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    SetWarning("Object cannot be converted to integer",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.Sub:
                case Operations.BitOr:
                case Operations.BitXor:
                    SetWarning("Object cannot be converted to integer",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Div:
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    // We can assume that object is not zero, because null is zero
                    result = OutSet.CreateInt(0);
                    break;
                default:
                    base.VisitAnyObjectValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.GreaterThanOrEqual:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    result = TypeConversion.AnyArrayToIntegerInterval(OutSet);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    if (ArithmeticOperation.IsArithmetic(operation))
                    {
                        // TODO: This must be fatal error
                        SetWarning("Unsupported operand type: Arithmetic of array and null type");
                        result = OutSet.AnyValue;
                        break;
                    }

                    base.VisitAnyArrayValue(value);
                    break;
            }
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            // Resource is always greater than null
            switch (operation)
            {
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.Sub:
                case Operations.BitOr:
                case Operations.BitXor:
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Div:
                case Operations.Mod:
                    // We can assume that resource is not zero, because it is always true
                    result = OutSet.CreateInt(0);
                    break;
                default:
                    if (PerformCommonAnyOperandOperations())
                    {
                        break;
                    }

                    base.VisitAnyResourceValue(value);
                    break;
            }
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members

        #region Helper methods

        /// <summary>
        /// Perform operation that is common for all abstract right operands or do nothing.
        /// </summary>
        /// <returns><c>True</c> whether operation has been performed, otherwise <c>false</c></returns>
        private bool PerformCommonAnyOperandOperations()
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.GreaterThan:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    return true;
                case Operations.NotIdentical:
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(true);
                    return true;
                default:
                    return false;
            }
        }

        #endregion Helper methods
    }
}
