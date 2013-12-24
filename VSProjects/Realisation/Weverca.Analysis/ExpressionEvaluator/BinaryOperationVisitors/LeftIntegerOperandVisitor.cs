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
                        // If aritmetic overflows, result is floating-point number
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
                        // If aritmetic underflows, result is floating-point number
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
                        DivisionByFalse();
                    }
                    break;
                case Operations.Mod:
                    DivisionByBooleanValue(value.Value);
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

                    result = BitwiseOperation.Bitwise(OutSet, operation, leftOperand.Value,
                        TypeConversion.ToInteger(value.Value));
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
        public override void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
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
                    var leftFloat = TypeConversion.ToFloat(leftOperand.Value);
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, leftFloat, value.Value);
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
                        var leftFloat = TypeConversion.ToFloat(leftOperand.Value);
                        result = Comparison.Compare(OutSet, operation, leftFloat, floatValue);
                        if (result != null)
                        {
                            break;
                        }

                        result = ArithmeticOperation.Arithmetic(flow, operation, leftFloat, floatValue);
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
                        // If at least one operand can not be recognized, result can be any integer value.
                        if (BitwiseOperation.IsBitWise(operation))
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
        public override void VisitCompoundValue(CompoundValue value)
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
                    base.VisitCompoundValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow, leftOperand.Value);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by comparison");
                        break;
                    }

                    result = ArithmeticOperation.RightAbstractOperandArithmetic(flow, operation,
                        leftOperand.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitObjectValue(value);
                    break;
            }
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
                    result = Comparison.LeftArrayCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    if (ArithmeticOperation.IsArithmetic(operation))
                    {
                        // TODO: This must be fatal error
                        SetWarning("Unsupported operand type: Arithmetic of array and scalar type");
                        result = OutSet.AnyValue;
                        break;
                    }

                    var leftBoolean = TypeConversion.ToBoolean(leftOperand.Value);
                    var rightBoolean = TypeConversion.ToNativeBoolean(OutSet, value);
                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    var rightInteger = TypeConversion.ToInteger(rightBoolean);
                    result = BitwiseOperation.Bitwise(OutSet, operation, leftOperand.Value, rightInteger);
                    if (result == null)
                    {
                        base.VisitAssociativeArray(value);
                    }

                    break;
            }
        }

        #endregion Compound values

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
            result = BitwiseOperation.Bitwise(OutSet, operation);
            if (result == null)
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
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
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
