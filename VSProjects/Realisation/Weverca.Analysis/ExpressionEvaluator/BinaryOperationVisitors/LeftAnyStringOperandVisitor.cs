using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with abstract string value as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    public class LeftAnyStringOperandVisitor : LeftAnyScalarOperandVisitor<AnyStringValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyStringOperandVisitor" /> class.
        /// </summary>
        public LeftAnyStringOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyStringOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        public LeftAnyStringOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitScalarValue(ScalarValue value)
        {
            result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitScalarValue(value);
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
                default:
                    result = Comparison.LeftAbstractBooleanCompare(OutSet, operation, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    // If the left operand can not be recognized, result can be any integer value.
                    result = BitwiseOperation.Bitwise(OutSet, operation);
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

                    // If the left operand can not be recognized, result can be any integer value.
                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

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
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation,
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
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation,
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
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
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

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(value.Value));
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
            if (Comparison.IsOperationComparison(operation))
            {
                // TODO: The comparison of string with object depends upon whether the object has
                // the "__toString" magic method implemented. If so, the string comparison is
                // performed. Otherwise, the object is always greater than string.
                result = OutSet.AnyBooleanValue;
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation, TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
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
                    result = ModuloOperation.AbstractModulo(flow,
                        TypeConversion.ToNativeInteger(OutSet, value));
                    break;
                default:
                    result = Comparison.RightAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToNativeBoolean(OutSet, value));
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
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (operation)
            {
                case Operations.Add:
                case Operations.Sub:
                    result = OutSet.AnyFloatValue;
                    break;
                case Operations.Mul:
                    result = OutSet.CreateDouble(0.0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.AnyIntegerValue;
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
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
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
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
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
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    base.VisitIntervalFloatValue(value);
                    break;
            }
        }

        #endregion Interval values

        #region Abstract values

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                return;
            }

            result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
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
                    result = BitwiseOperation.Bitwise(OutSet, operation);
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
                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyNumericValue(value);
                    break;
            }
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

            result = LogicalOperation.AbstractLogical(OutSet, operation, TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
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

            base.VisitAnyArrayValue(value);
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                // Comapring of resource and string makes no sence.
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(value));
            if (result != null)
            {
                return;
            }

            result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyResourceValue(value);
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}
