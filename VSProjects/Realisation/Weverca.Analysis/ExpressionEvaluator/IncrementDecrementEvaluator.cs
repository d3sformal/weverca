using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates prefix or postfix increment or decrement during the analysis.
    /// </summary>
    /// <remarks>
    /// Increment or decrement is defined by operation <see cref="Operations.IncDec" />.
    /// </remarks>
    /// <seealso cref="UnaryOperationEvaluator" />
    public class IncrementDecrementEvaluator : PartialExpressionEvaluator
    {
        /// <summary>
        /// Determines whether operation is increment, otherwise it is decrement.
        /// </summary>
        private bool isIncrement;

        /// <summary>
        /// Result of performing the unary operation on the given value.
        /// </summary>
        private Value result;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementDecrementEvaluator" /> class.
        /// </summary>
        public IncrementDecrementEvaluator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementDecrementEvaluator" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        public IncrementDecrementEvaluator(FlowController flowController)
            : base(flowController)
        {
        }

        /// <summary>
        /// Evaluates prefix or postfix increment or decrement of the value.
        /// </summary>
        /// <param name="isIncrementOperation">Determines whether to perform increment operation.</param>
        /// <param name="operand">One operand of increment or decrement.</param>
        /// <returns>Result of operand increment or decrement.</returns>
        public Value Evaluate(bool isIncrementOperation, Value operand)
        {
            // Sets current operation
            isIncrement = isIncrementOperation;

            // Gets type of operand and evaluate expression for given operation
            result = null;
            operand.Accept(this);

            // Returns result of increment or decrement
            Debug.Assert(result != null, "The result must be assigned after visiting the value");
            return result;
        }

        /// <summary>
        /// Evaluates prefix or postfix increment or decrement of all possible values in memory entry.
        /// </summary>
        /// <param name="isIncrementOperation">Determines whether to perform increment operation.</param>
        /// <param name="entry">Memory entry with all possible operands of increment or decrement.</param>
        /// <returns>Result of performing the increment or decrement on all possible operands.</returns>
        public MemoryEntry Evaluate(bool isIncrementOperation, MemoryEntry entry)
        {
            // Sets current operation
            isIncrement = isIncrementOperation;

            var values = new HashSet<Value>();
            foreach (var value in entry.PossibleValues)
            {
                // Gets type of operand and evaluate expression for given operation
                result = null;
                value.Accept(this);

                // Returns result of increment or decrement
                Debug.Assert(result != null, "The result must be assigned after visiting the value");
                values.Add(result);
            }

            return new MemoryEntry(values);
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            result = value;
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            if (isIncrement)
            {
                if (value.Value < int.MaxValue)
                {
                    result = OutSet.CreateInt(value.Value + 1);
                }
                else
                {
                    result = OutSet.CreateDouble(TypeConversion.ToFloat(value.Value) + 1.0);
                }
            }
            else
            {
                if (value.Value > int.MinValue)
                {
                    result = OutSet.CreateInt(value.Value - 1);
                }
                else
                {
                    result = OutSet.CreateDouble(TypeConversion.ToFloat(value.Value) - 1.0);
                }
            }
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            if (isIncrement)
            {
                if (value.Value < long.MaxValue)
                {
                    result = OutSet.CreateLong(value.Value + 1);
                }
                else
                {
                    result = OutSet.CreateDouble(TypeConversion.ToFloat(value.Value) + 1.0);
                }
            }
            else
            {
                if (value.Value > long.MinValue)
                {
                    result = OutSet.CreateLong(value.Value - 1);
                }
                else
                {
                    result = OutSet.CreateDouble(TypeConversion.ToFloat(value.Value) - 1.0);
                }
            }
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            if (isIncrement)
            {
                result = OutSet.CreateDouble(value.Value + 1.0);
            }
            else
            {
                result = OutSet.CreateDouble(value.Value - 1.0);
            }
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            if (isIncrement)
            {
                // TODO: Implement. PHP follows Perl's convention
                result = OutSet.AnyStringValue;
            }
            else
            {
                result = value;
            }
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitCompoundValue(CompoundValue value)
        {
            result = value;
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitResourceValue(ResourceValue value)
        {
            result = value;
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            if (isIncrement)
            {
                result = OutSet.CreateInt(1);
            }
            else
            {
                result = value;
            }
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            if (isIncrement)
            {
                if (value.End < int.MaxValue)
                {
                    result = OutSet.CreateIntegerInterval(value.Start + 1, value.End + 1);
                }
                else
                {
                    result = OutSet.CreateFloatInterval(TypeConversion.ToFloat(value.Start) + 1.0,
                        TypeConversion.ToFloat(value.End) + 1.0);
                }
            }
            else
            {
                if (value.Start > int.MinValue)
                {
                    result = OutSet.CreateIntegerInterval(value.Start - 1, value.End - 1);
                }
                else
                {
                    result = OutSet.CreateFloatInterval(TypeConversion.ToFloat(value.Start) - 1.0,
                        TypeConversion.ToFloat(value.End) - 1.0);
                }
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            if (isIncrement)
            {
                if (value.End < long.MaxValue)
                {
                    result = OutSet.CreateLongintInterval(value.Start + 1, value.End + 1);
                }
                else
                {
                    result = OutSet.CreateFloatInterval(TypeConversion.ToFloat(value.Start) + 1.0,
                        TypeConversion.ToFloat(value.End) + 1.0);
                }
            }
            else
            {
                if (value.Start > long.MinValue)
                {
                    result = OutSet.CreateLongintInterval(value.Start - 1, value.End - 1);
                }
                else
                {
                    result = OutSet.CreateFloatInterval(TypeConversion.ToFloat(value.Start) - 1.0,
                        TypeConversion.ToFloat(value.End) - 1.0);
                }
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            if (isIncrement)
            {
                result = OutSet.CreateFloatInterval(value.Start + 1.0, value.End + 1.0);
            }
            else
            {
                result = OutSet.CreateFloatInterval(value.Start - 1.0, value.End - 1.0);
            }
        }

        #endregion Interval values

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            result = value;
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            result = value;
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            result = OutSet.AnyValue;
        }

        /// <inheritdoc />
        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            result = OutSet.AnyValue;
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            result = value;
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            result = value;
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyCompoundValue(AnyCompoundValue value)
        {
            result = value;
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            result = value;
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}
