using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with abstract integer value as the left operand
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    public class LeftAnyIntegerOperandVisitor : GenericLeftOperandVisitor<AnyIntegerValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyIntegerOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftAnyIntegerOperandVisitor(FlowController flowController)
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
            if (result == null)
            {
                base.VisitScalarValue(value);
            }
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
                    DivisionByBooleanValue(value.Value);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = Comparison.LeftAbstractBooleanCompare(OutSet, operation, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = ArithmeticOperation.LeftAbstractOperandArithmetic(flow, operation,
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
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
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
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = ArithmeticOperation.LeftAbstractOperandArithmetic(flow, operation, value.Value);
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
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
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

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);

                    if (isInteger)
                    {
                        result = ArithmeticOperation.LeftAbstractOperandArithmetic(flow,
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
                    result = OutSet.AnyBooleanValue;
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
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result == null)
                    {
                        VisitUndefinedValue(value);
                    }

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

            result = LogicalOperation.AbstractLogical(OutSet, operation, value);
            if (result != null)
            {
                return;
            }

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
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    result = ArithmeticOperation.LeftAbstractOperandArithmetic(flow, operation, value);
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
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    result = ArithmeticOperation.AbstractFloatArithmetic(OutSet, operation);
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
