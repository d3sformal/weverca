using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with abstract number of integer values as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    /// <typeparam name="TNumeric">Type of left abstract number operand.</typeparam>
    public abstract class LeftAnyNumericOperandVisitor<TNumeric> : LeftAnyScalarOperandVisitor<TNumeric>
        where TNumeric : AnyNumericValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyNumericOperandVisitor{TNumeric}" /> class.
        /// </summary>
        protected LeftAnyNumericOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyNumericOperandVisitor{TNumeric}" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        protected LeftAnyNumericOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitScalarValue(ScalarValue value)
        {
            result = BitwiseOperation.Bitwise(OutSet, operation);
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
                return;
            }

            base.VisitGenericNumericValue(value);
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
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
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

                    base.VisitStringValue(value);
                    break;
            }
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by comparison",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(value));
            if (result != null)
            {
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
                    result = leftOperand;
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
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitGenericIntervalValue(value);
        }

        #endregion Interval values

        #region Abstract values

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            result = BitwiseOperation.Bitwise(OutSet, operation);
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
                    result = Comparison.AbstractCompare(OutSet, operation);
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
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyNumericValue(value);
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
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

                    base.VisitAnyStringValue(value);
                    break;
            }
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by comparison",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(value));
            if (result != null)
            {
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
                // Comapring of resource and number makes no sence.
                return;
            }

            result = LogicalOperation.AbstractLogical(OutSet, operation,
                TypeConversion.ToBoolean(value));
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
