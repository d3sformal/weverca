using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with interval of numbers as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    /// <typeparam name="TComparable">Native type of values in left operand interval.</typeparam>
    public abstract class LeftIntervalOperandVisitor<TComparable>
        : GenericLeftOperandVisitor<IntervalValue<TComparable>>
        where TComparable : IComparable, IComparable<TComparable>, IEquatable<TComparable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftIntervalOperandVisitor{TComparable}" /> class.
        /// </summary>
        protected LeftIntervalOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftIntervalOperandVisitor{TComparable}" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        protected LeftIntervalOperandVisitor(FlowController flowController)
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
                case Operations.Mod:
                    result = ModuloOperation.ModuloByBooleanValue(flow, value.Value);
                    break;
                default:
                    result = Comparison.IntervalCompare(OutSet, operation, leftOperand, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand, value.Value);
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
            result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                TypeConversion.ToBoolean(value.Value));
            if (result != null)
            {
                return;
            }

            base.VisitIntegerValue(value);
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                TypeConversion.ToBoolean(value.Value));
            if (result != null)
            {
                return;
            }

            base.VisitFloatValue(value);
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
                default:
                    result = LogicalOperation.Logical(OutSet, operation, leftOperand,
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
                    SetWarning("Object cannot be converted to integer by modulo operation",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by comparison",
                            AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                        TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation",
                            AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                        break;
                    }

                    base.VisitObjectValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            result = Comparison.RightAlwaysGreater(OutSet, operation);
            if (result != null)
            {
                return;
            }

            result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                TypeConversion.ToNativeBoolean(Snapshot, value));
            if (result != null)
            {
                return;
            }

            result = BitwiseOperation.Bitwise(OutSet, operation);
            if (result != null)
            {
                return;
            }

            if (ArithmeticOperation.IsArithmetic(operation))
            {
                // TODO: This must be fatal error
                SetWarning("Unsupported operand type: Arithmetic of array and numeric type");
                result = OutSet.AnyValue;
                return;
            }

            base.VisitAssociativeArray(value);
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.Identical:
                case Operations.LessThan:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Equal:
                case Operations.LessThanOrEqual:
                    bool convertedValue;
                    if (TypeConversion.TryConvertToBoolean(leftOperand, out convertedValue))
                    {
                        result = OutSet.CreateBool(!convertedValue);
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
                    break;
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.Add:
                case Operations.Sub:
                    result = leftOperand;
                    break;
                case Operations.BitAnd:
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
            result = BitwiseOperation.Bitwise(OutSet, operation);
            if (result != null)
            {
                // It is too complicated to represend result of bitwise operation with intervals
                return;
            }

            base.VisitGenericIntervalValue(value);
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            result = LogicalOperation.Logical(OutSet, operation, leftOperand, value);
            if (result != null)
            {
                return;
            }

            base.VisitIntervalIntegerValue(value);
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            result = LogicalOperation.Logical(OutSet, operation, leftOperand, value);
            if (result != null)
            {
                return;
            }

            base.VisitIntervalFloatValue(value);
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
                    // Ommitted warning message that object cannot be converted to integer
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        // Ommitted warning message that object cannot be converted to integer
                        break;
                    }

                    result = ArithmeticOperation.AbstractFloatArithmetic(Snapshot, operation);
                    if (result != null)
                    {
                        // Ommitted error report that array is unsupported operand in arithmetic operation
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation, leftOperand);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        // Ommitted warning message that object cannot be converted to integer
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
            result = LogicalOperation.AbstractLogical(OutSet, operation, leftOperand);
            if (result != null)
            {
                return;
            }

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
                case Operations.Mod:
                    result = ModuloOperation.ModuloByAnyBooleanValue(flow);
                    break;
                default:
                    result = Comparison.RightAbstractBooleanCompare(OutSet, operation, leftOperand);
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
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyNumericValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            result = ArithmeticOperation.AbstractFloatArithmetic(Snapshot, operation);
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
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.AbstractFloatArithmetic(Snapshot, operation);
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
        public override void VisitAnyCompoundValue(AnyCompoundValue value)
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
                    base.VisitAnyCompoundValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by comparison",
                            AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                        TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation",
                            AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
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
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.RightAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation, leftOperand);
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
                        SetWarning("Unsupported operand type: Arithmetic of array and numeric type");
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
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        // Comapring of resource and number makes no sence.
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftOperand,
                        TypeConversion.ToBoolean(value));
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
