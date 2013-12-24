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
    public class LeftArrayOperandVisitor : GenericLeftOperandVisitor<AssociativeArray>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftArrayOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftArrayOperandVisitor(FlowController flowController)
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
                    if (ArithmeticOperation.IsArithmetic(operation))
                    {
                        // TODO: This must be fatal error
                        SetWarning("Unsupported operand type: Arithmetic of array and scalar type");
                        result = OutSet.AnyValue;
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
                    DivisionByBooleanValue(value.Value);
                    break;
                default:
                    var leftBoolean = TypeConversion.ToBoolean(OutSet, leftOperand);
                    result = Comparison.Compare(OutSet, operation, leftBoolean.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean.Value, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    var leftInteger = TypeConversion.ToInteger(leftBoolean.Value);
                    var rightInteger = TypeConversion.ToInteger(value.Value);
                    result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, rightInteger);
                    if (result == null)
                    {
                        base.VisitBooleanValue(value);
                    }

                    break;
            }
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            result = Comparison.LeftArrayCompare(OutSet, operation);
            if (result == null)
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
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToNativeInteger(OutSet, leftOperand), value.Value);
                    break;
                default:
                    var leftBoolean = TypeConversion.ToNativeBoolean(OutSet, leftOperand);
                    var rightBoolean = TypeConversion.ToBoolean(value.Value);
                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    var leftInteger = TypeConversion.ToInteger(leftBoolean);
                    result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, value.Value);
                    if (result == null)
                    {
                        base.VisitIntegerValue(value);
                    }

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
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToNativeInteger(OutSet, leftOperand), value.Value);
                    break;
                default:
                    var leftBoolean = TypeConversion.ToNativeBoolean(OutSet, leftOperand);
                    var rightBoolean = TypeConversion.ToBoolean(value.Value);
                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    var leftInteger = TypeConversion.ToInteger(leftBoolean);
                    result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, value.Value);
                    if (result == null)
                    {
                        base.VisitFloatValue(value);
                    }

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
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToNativeInteger(OutSet, leftOperand), value.Value);
                    break;
                default:
                    result = Comparison.LeftArrayCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    var leftBoolean = TypeConversion.ToNativeBoolean(OutSet, leftOperand);
                    var rightBoolean = TypeConversion.ToBoolean(value.Value);
                    result = LogicalOperation.Logical(OutSet, operation, leftBoolean, rightBoolean);
                    if (result != null)
                    {
                        break;
                    }

                    var leftInteger = TypeConversion.ToInteger(leftBoolean);
                    result = BitwiseOperation.Bitwise(OutSet, operation, leftInteger, value.Value);
                    if (result == null)
                    {
                        base.VisitStringValue(value);
                    }

                    break;
            }
        }

        #endregion Scalar values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (operation)
            {
                case Operations.NotIdentical:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Identical:
                case Operations.LessThan:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.Or:
                case Operations.Xor:
                    result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.Equal:
                case Operations.LessThanOrEqual:
                    result = OutSet.CreateBool(!TypeConversion.ToNativeBoolean(OutSet, leftOperand));
                    break;
                case Operations.BitAnd:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = TypeConversion.ToInteger(OutSet, leftOperand);
                    break;
                case Operations.Mod:
                    DivisionByNull();
                    break;
                default:
                    if (ArithmeticOperation.IsArithmetic(operation))
                    {
                        // TODO: This must be fatal error
                        SetWarning("Unsupported operand type: Arithmetic of array and null type");
                        result = OutSet.AnyValue;
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

                    result = BitwiseOperation.Bitwise(OutSet, operation);
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
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToNativeInteger(OutSet, leftOperand), value);
                    break;
                default:
                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToNativeBoolean(OutSet, leftOperand), value);
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
                    result = ModuloOperation.Modulo(flow,
                        TypeConversion.ToNativeInteger(OutSet, leftOperand), value);
                    break;
                default:
                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.Logical(OutSet, operation,
                        TypeConversion.ToNativeBoolean(OutSet, leftOperand), value);
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
