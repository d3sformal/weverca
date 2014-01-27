using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed string value as the left operand
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    public class LeftStringOperandVisitor : LeftScalarOperandVisitor<StringValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftStringOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftStringOperandVisitor(FlowController flowController)
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

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    var rightInteger = TypeConversion.ToInteger(value.Value);

                    result = isInteger
                        ? ArithmeticOperation.Arithmetic(flow, operation, integerValue, rightInteger)
                        : ArithmeticOperation.Arithmetic(flow, operation, floatValue, rightInteger);

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
                        result = BitwiseOperation.Bitwise(OutSet, operation, integerValue, rightInteger);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If the left operand can not be recognized, result can be any integer value.
                        result = BitwiseOperation.Bitwise(OutSet, operation);
                        if (result != null)
                        {
                            break;
                        }
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
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    if (isInteger)
                    {
                        result = Comparison.Compare(OutSet, operation, integerValue, value.Value);
                        if (result != null)
                        {
                            break;
                        }

                        result = ArithmeticOperation.Arithmetic(flow, operation, integerValue, value.Value);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        result = Comparison.Compare(OutSet, operation, floatValue, value.Value);
                        if (result != null)
                        {
                            break;
                        }

                        result = ArithmeticOperation.Arithmetic(flow, operation, floatValue, value.Value);
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
                        result = BitwiseOperation.Bitwise(OutSet, operation, integerValue, value.Value);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If the left operand can not be recognized, result can be any integer value.
                        result = BitwiseOperation.Bitwise(OutSet, operation);
                        if (result != null)
                        {
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
            switch (operation)
            {
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
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    result = Comparison.Compare(OutSet, operation, floatValue, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, floatValue, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isHexadecimal)
                    {
                        integerValue = 0;
                    }

                    int rightInteger;
                    if ((isInteger || (isSuccessful
                        && TypeConversion.TryConvertToInteger(floatValue, out integerValue)))
                        && TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        result = BitwiseOperation.Bitwise(OutSet, operation, integerValue, rightInteger);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If at least one operand can not be recognized, result can be any integer value.
                        result = BitwiseOperation.Bitwise(OutSet, operation);
                        if (result != null)
                        {
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
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(string.Equals(leftOperand.Value, value.Value,
                        StringComparison.Ordinal));
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(!string.Equals(leftOperand.Value, value.Value,
                        StringComparison.Ordinal));
                    break;
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value.Value);
                    break;
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                    // Bit operations are defined for every character, not for the entire string
                    // TODO: PHP string is stored as array of bytes, but printed in UTF8 encoding
                    result = OutSet.AnyStringValue;
                    break;
                default:
                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value.Value));
                    if (result != null)
                    {
                        break;
                    }

                    int leftInteger;
                    double leftFloat;
                    bool isLeftInteger;
                    bool isLeftHexadecimal;
                    var isLeftSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out leftInteger, out leftFloat, out isLeftInteger, out isLeftHexadecimal);

                    int rightInteger;
                    double rightFloat;
                    bool isRightInteger;
                    bool isRightHexadecimal;
                    var isRightSuccessful = TypeConversion.TryConvertToNumber(value.Value, true,
                        out rightInteger, out rightFloat, out isRightInteger, out isRightHexadecimal);

                    // If both strings are convertible to number, they are conpared as numbers
                    result = (isLeftSuccessful && isRightSuccessful)
                        ? ((isLeftInteger && isRightInteger)
                            ? Comparison.Compare(OutSet, operation, leftInteger, rightInteger)
                            : Comparison.Compare(OutSet, operation, leftFloat, rightFloat))
                        : Comparison.Compare(OutSet, operation, leftOperand.Value, value.Value);

                    if (result != null)
                    {
                        break;
                    }

                    result = (isLeftInteger && isRightInteger)
                        ? ArithmeticOperation.Arithmetic(flow, operation, leftInteger, rightInteger)
                        : ArithmeticOperation.Arithmetic(flow, operation, leftFloat, rightFloat);

                    if (result != null)
                    {
                        break;
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isLeftHexadecimal)
                    {
                        leftInteger = 0;
                    }

                    if (isRightHexadecimal)
                    {
                        rightInteger = 0;
                    }

                    // Only shifting operations can happen
                    if ((isLeftInteger || (isLeftSuccessful
                        && TypeConversion.TryConvertToInteger(leftFloat, out leftInteger)))
                        && (isRightInteger || (isRightSuccessful
                        && TypeConversion.TryConvertToInteger(rightFloat, out rightInteger))))
                    {
                        result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, rightInteger);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If at least one operand can not be recognized, result can be any integer value.
                        result = BitwiseOperation.Bitwise(OutSet, operation);
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
            if (Comparison.IsOperationComparison(operation))
            {
                // TODO: The comparison of string with object depends upon whether the object has
                // the "__toString" magic method implemented. If so, the string comparison is
                // performed. Otherwise, the object is always greater than string.
                result = OutSet.AnyBooleanValue;
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            result = BitwiseOperation.Bitwise(OutSet, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by bitwise operation");
                return;
            }

            int integerValue;
            double floatValue;
            bool isInteger;
            TypeConversion.TryConvertToNumber(leftOperand.Value, true, out integerValue,
                out floatValue, out isInteger);

            result = isInteger
                ? ArithmeticOperation.RightAbstractArithmetic(flow, operation, integerValue)
                : ArithmeticOperation.RightAbstractArithmetic(flow, operation, floatValue);

            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation");
                return;
            }

            base.VisitObjectValue(value);
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
                    result = Comparison.RightAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToNativeBoolean(OutSet, value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation, leftOperand.Value,
                        TypeConversion.ToNativeInteger(OutSet, value));
                    if (result != null)
                    {
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
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        // Comapring of resource and string makes no sence.
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger);

                    result = isInteger
                        ? ArithmeticOperation.RightAbstractArithmetic(flow, operation, integerValue)
                        : ArithmeticOperation.RightAbstractArithmetic(flow, operation, floatValue);

                    if (result != null)
                    {
                        // Arithmetic with resources is nonsence
                        break;
                    }

                    base.VisitResourceValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            int integerValue;
            double floatValue;
            bool isInteger;

            switch (operation)
            {
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.Add:
                case Operations.Sub:
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true, out integerValue,
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
                case Operations.Mul:
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true, out integerValue,
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
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = TypeConversion.ToInteger(OutSet, leftOperand);
                    break;
                default:
                    result = Comparison.Compare(OutSet, operation, leftOperand.Value, string.Empty);
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
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
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
                case Operations.Mod:
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value);
                    break;
                default:
                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), value);
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger);

                    if (isInteger)
                    {
                        result = Comparison.IntervalCompare(OutSet, operation, integerValue, value);
                        if (result != null)
                        {
                            break;
                        }

                        result = ArithmeticOperation.Arithmetic(flow, operation, integerValue, value);
                        if (result != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        var floatInterval = TypeConversion.ToFloatInterval(OutSet, value);
                        result = Comparison.IntervalCompare(OutSet, operation, floatValue, floatInterval);
                        if (result != null)
                        {
                            break;
                        }

                        result = ArithmeticOperation.Arithmetic(flow, operation, floatValue, floatInterval);
                        if (result != null)
                        {
                            break;
                        }
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
                    result = ModuloOperation.Modulo(flow, leftOperand.Value, value);
                    break;
                default:
                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), value);
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger);

                    result = Comparison.IntervalCompare(OutSet, operation, floatValue, value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.Arithmetic(flow, operation, floatValue, value);
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
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                // Ommitted warning message that object cannot be converted to integer
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value));
            if (result != null)
            {
                return;
            }

            base.VisitAnyValue(value);
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value));
            if (result != null)
            {
                return;
            }

            base.VisitAnyScalarValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
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
                    result = Comparison.RightAbstractBooleanCompare(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger);

                    result = isInteger
                        ? ArithmeticOperation.RightAbstractBooleanArithmetic(flow, operation, integerValue)
                        : ArithmeticOperation.RightAbstractBooleanArithmetic(flow, operation, floatValue);

                    if (result != null)
                    {
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
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyNumericValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            int integerValue;
            double floatValue;
            bool isInteger;
            TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                out integerValue, out floatValue, out isInteger);

            result = isInteger
                ? ArithmeticOperation.RightAbstractArithmetic(flow, operation, integerValue)
                : ArithmeticOperation.RightAbstractArithmetic(flow, operation, floatValue);

            if (result != null)
            {
                return;
            }

            base.VisitAnyIntegerValue(value);
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
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                    // Bit operations are defined for every character, not for the entire string
                    result = OutSet.AnyStringValue;
                    break;
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.AnyIntegerValue;
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
                    {
                        // Strings can be converted into floating point number too.
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
            if (Comparison.IsOperationComparison(operation))
            {
                // The comparison of string with object depends upon whether the object has
                // the "__toString" magic method implemented. If so, the string comparison is
                // performed. Otherwise, the object is always greater than string. Since we cannot
                // determine whether the abstract object has or has not the method,
                // we must return indeterminate boolean value.
                result = OutSet.AnyBooleanValue;
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            int integerValue;
            double floatValue;
            bool isInteger;
            TypeConversion.TryConvertToNumber(leftOperand.Value, true, out integerValue,
                out floatValue, out isInteger);

            result = isInteger
                ? ArithmeticOperation.RightAbstractArithmetic(flow, operation, integerValue)
                : ArithmeticOperation.RightAbstractArithmetic(flow, operation, floatValue);

            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation");
                return;
            }

            base.VisitAnyObjectValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            result = Comparison.RightAlwaysGreater(OutSet, operation);
            if (result != null)
            {
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(leftOperand.Value));
            if (result != null)
            {
                return;
            }

            base.VisitAnyArrayValue(value);
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            switch (operation)
            {
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        // Comapring of resource and string makes no sence.
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToBoolean(leftOperand.Value), TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger);

                    result = isInteger
                        ? ArithmeticOperation.RightAbstractArithmetic(flow, operation, integerValue)
                        : ArithmeticOperation.RightAbstractArithmetic(flow, operation, floatValue);

                    if (result != null)
                    {
                        // Arithmetic with resources is nonsence
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
