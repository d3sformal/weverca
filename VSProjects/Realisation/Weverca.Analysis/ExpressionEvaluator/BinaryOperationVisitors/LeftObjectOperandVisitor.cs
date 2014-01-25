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
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation");
                        break;
                    }

                    base.VisitScalarValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
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
                // with other number. However, since this conversion is undefined, we are comparing
                // concrete number with abstract number that can result into both true and false
                SetWarning("Object cannot be converted to integer by comparison");
                return;
            }

            VisitGenericScalarValue(value);
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
                        // TODO: The comparison of string with object depends upon whether the object has
                        // the "__toString" magic method implemented. If so, the string comparison is
                        // performed. Otherwise, the object is always greater than string.
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

                    result = isInteger
                        ? ArithmeticOperation.LeftAbstractArithmetic(flow, operation, integerValue)
                        : ArithmeticOperation.LeftAbstractArithmetic(flow, operation, floatValue);

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
                        // TODO: Two object instances are equal if they have the same attributes
                        // and their values, and are instances of the same class.
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

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand),
                        TypeConversion.ToNativeBoolean(OutSet, value));
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
        public override void VisitResourceValue(ResourceValue value)
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
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.LeftAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.AbstractIntegerArithmetic(flow, operation);
                    if (result != null)
                    {
                        // Arithmetic objects and resources is nonsence
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
                        // Bitwise operation with resource can give any integer
                        break;
                    }

                    base.VisitResourceValue(value);
                    break;
            }
        }

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
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    switch (operation)
                    {
                        case Operations.Mul:
                        case Operations.BitAnd:
                            SetWarning("Object cannot be converted to integer");
                            result = OutSet.CreateInt(0);
                            break;
                        case Operations.Add:
                        case Operations.Sub:
                        case Operations.BitOr:
                        case Operations.BitXor:
                        case Operations.ShiftLeft:
                        case Operations.ShiftRight:
                            SetWarning("Object cannot be converted to integer");
                            result = OutSet.AnyIntegerValue;
                            break;
                        case Operations.Div:
                        case Operations.Mod:
                            SetWarning("Object cannot be converted to integer");
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
                        // with other number. However, since this conversion is undefined, we are comparing
                        // concrete number with abstract number that can result into both true and false
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
                    result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation, value);
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
                    result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation, value);
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

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    // Ommitted warnings messages that objects cannot be converted to integers
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        // Ommitted warning message that object cannot be converted to integer
                        break;
                    }

                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
                    {
                        // Ommitted warning message that object cannot be converted to integer
                        // Ommitted error report that array is unsupported operand in arithmetic operation
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand));
                    if (result != null)
                    {
                        return;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        // Ommitted warnings messages that objects cannot be converted to integers
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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand));
                    if (result != null)
                    {
                        return;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation");
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
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    DivisionByAnyBooleanValue();
                    break;
                default:
                    result = Comparison.RightAbstractBooleanCompare(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand));
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.RightAbstractBooleanArithmetic(flow, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by arithmetic operation");
                        break;
                    }

                    base.VisitAnyBooleanValue(value);
                    break;
            }
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyNumericValue(AnyNumericValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        // Probably since PHP version 5.1.5, Object converts into a number when comparing
                        // with other number. However, since this conversion is undefined, we are comparing
                        // concrete number with abstract number that can result into both true and false
                        SetWarning("Object cannot be converted to integer by comparison");
                        break;
                    }

                    base.VisitAnyNumericValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            result = ArithmeticOperation.AbstractIntegerArithmetic(flow, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyIntegerValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            throw new NotSupportedException("Long integer is not currently supported");
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyFloatValue(value);
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    if (Comparison.IsOperationComparison(operation))
                    {
                        // TODO: The comparison of string with object depends upon whether the object has
                        // the "__toString" magic method implemented. If so, the string comparison is
                        // performed. Otherwise, the object is always greater than string.
                        result = OutSet.AnyBooleanValue;
                        break;
                    }

                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
                    {
                        // A string can be converted into floating point number too.
                        break;
                    }

                    base.VisitAnyStringValue(value);
                    break;
            }
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    // It cannot be decided if they are identical or not.
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation");
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by comparison");
                        break;
                    }

                    result = ArithmeticOperation.AbstractIntegerArithmetic(flow, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by arithmetic operation");
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
                        SetWarning("Object cannot be converted to integer by bitwise operation");
                        break;
                    }

                    base.VisitAnyObjectValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
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
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.RightAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
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

                    base.VisitAnyArrayValue(value);
                    break;
            }
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
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
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.LeftAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.AbstractIntegerArithmetic(flow, operation);
                    if (result != null)
                    {
                        // Arithmetic objects and resources is nonsence
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
                        // Bitwise operation with resource can give any integer
                        break;
                    }

                    base.VisitAnyResourceValue(value);
                    break;
            }
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}
