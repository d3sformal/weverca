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
    public class LeftObjectOperandVisitor : GenericLeftOperandVisitor<ObjectValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftObjectOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftObjectOperandVisitor(FlowController flowController)
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
                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result == null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation");
                        base.VisitScalarValue(value);
                    }

                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    DivisionByBooleanValue(value.Value);
                    break;
                default:
                    var leftBoolean = TypeConversion.ToBoolean(leftOperand);
                    result = Comparison.Compare(OutSet, operation, leftBoolean, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation,
                        TypeConversion.ToInteger(value.Value));
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by arithmetic operation");
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean, value.Value);
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
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                // Probably since PHP version 5.1.5, Object converts into a number when comparing
                // with other number. However, since this conversion is undefined, we comparing
                // concrete number with abstract number that can result in true and false too
                SetWarning("Object cannot be converted to integer by comparison");
            }
            else
            {
                VisitGenericScalarValue(value);
            }
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand), TypeConversion.ToBoolean(value.Value));
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
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand), TypeConversion.ToBoolean(value.Value));
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
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    if (Comparison.IsOperationComparison(operation))
                    {
                        // TODO: Object can be converted only if it has __toString magic method implemented
                        // TODO: If there is no __toString magic method, the object is always greater
                        result = OutSet.AnyBooleanValue;
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand), TypeConversion.ToBoolean(value.Value));
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);

                    if (isInteger)
                    {
                        result = ArithmeticOperation.LeftAbstractArithmetic(flow,
                            operation, integerValue);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
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

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    // TODO: Compare if two objects are the same instances of the same class
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    SetWarning("Both objects cannot be converted to integers by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    if (Comparison.IsOperationComparison(operation))
                    {
                        /*
                         * TODO: Two object instances are equal if they have the same attributes and values,
                         * and are instances of the same class.
                         */
                        result = OutSet.AnyBooleanValue;
                        break;
                    }

                    if (ArithmeticOperation.IsArithmetic(operation))
                    {
                        SetWarning("Both objects cannot be converted to integers by arithmetic operation");
                        result = OutSet.AnyIntegerValue;
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand), TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Both objects cannot be converted to integers by bitwise operation");
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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.LeftAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    if (ArithmeticOperation.IsArithmetic(operation))
                    {
                        SetWarning("Object cannot be converted to integer by arithmetic operation");
                        // TODO: This must be fatal error
                        SetWarning("Unsupported operand type: Arithmetic of array and scalar type");
                        result = OutSet.AnyValue;
                        break;
                    }

                    var leftBoolean = TypeConversion.ToBoolean(leftOperand);
                    var rightBoolean = TypeConversion.ToNativeBoolean(OutSet, value);
                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation");
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
                case Operations.LessThan:
                case Operations.LessThanOrEqual:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotEqual:
                case Operations.NotIdentical:
                case Operations.GreaterThan:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.AnyBooleanValue;
                    break;
                default:
                    SetWarning("Object cannot be converted to integer");
                    switch (operation)
                    {
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
                            result = OutSet.AnyIntegerValue;
                            break;
                        case Operations.Div:
                        case Operations.Mod:
                            DivisionByNull();
                            break;
                        default:
                            base.VisitUndefinedValue(value);
                            break;
                    }
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
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        // Probably since PHP version 5.1.5, Object converts into a number when comparing
                        // with other number. However, since this conversion is undefined, we comparing
                        // concrete number with abstract number that can result in true and false too
                        SetWarning("Object cannot be converted to number by comparison");
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation");
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
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    result = ArithmeticOperation.LeftAbstractOperandArithmetic(flow, operation, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand), value);
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
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand), value);
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
