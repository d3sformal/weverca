using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with interval of integer values as the left operand
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />
    /// </remarks>
    public class LeftIntegerIntervalOperandVisitor : GenericLeftOperandVisitor<IntegerIntervalValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftIntegerIntervalOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public LeftIntegerIntervalOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            switch (operation)
            {
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    // Realize that objects cannot be converted to integer and we suppress warning
                    result = OutSet.AnyIntegerValue;
                    break;
                default:
                    base.VisitValue(value);
                    break;
            }
        }

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            // TODO: Implement

            switch (operation)
            {
                default:
                    if (ComparisonOperation(leftOperand, TypeConversion.ToInteger(value.Value)))
                    {
                        break;
                    }

                    if (LogicalOperation<int>(value.Value, leftOperand))
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
            // TODO: Implement

            switch (operation)
            {
                default:
                    if (ComparisonOperation(leftOperand, value.Value))
                    {
                        break;
                    }

                    if (LogicalOperation<int>(TypeConversion.ToBoolean(value.Value), leftOperand))
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
            // TODO: Implement

            switch (operation)
            {
                default:
                    if (LogicalOperation<int>(TypeConversion.ToBoolean(value.Value), leftOperand))
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
            // TODO: Implement

            switch (operation)
            {
                default:
                    if (LogicalOperation<int>(TypeConversion.ToBoolean(value.Value), leftOperand))
                    {
                        break;
                    }

                    base.VisitStringValue(value);
                    break;
            }
        }

        #endregion Scalar values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            // TODO: Implement

            switch (operation)
            {
                default:
                    if (ComparisonOperation(leftOperand, TypeConversion.ToInteger(value)))
                    {
                        break;
                    }

                    if (LogicalOperation<int>(TypeConversion.ToBoolean(value), leftOperand))
                    {
                        break;
                    }

                    base.VisitUndefinedValue(value);
                    break;
            }
        }

        #endregion Concrete values

        #endregion AbstractValueVisitor Members
    }
}
