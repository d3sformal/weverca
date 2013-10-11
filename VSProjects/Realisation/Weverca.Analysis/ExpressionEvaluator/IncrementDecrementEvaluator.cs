using System;
using System.Collections.Generic;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates prefix or postfix increment or decrement during the analysis
    /// </summary>
    /// <remarks>
    /// Increment or decrement is defined by operation <see cref="Operations.IncDec" />
    /// </remarks>
    public class IncrementDecrementEvaluator : AbstractValueVisitor
    {
        /// <summary>
        /// Flow controller of program point providing data for evaluation (output set, position etc.)
        /// </summary>
        private FlowController flow;

        /// <summary>
        /// Determines whether operation is increment, otherwise it is decrement
        /// </summary>
        private bool isIncrement;

        /// <summary>
        /// Result of performing the unary operation on the given value
        /// </summary>
        private Value result;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementDecrementEvaluator" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public IncrementDecrementEvaluator(FlowController flowController)
        {
            SetContext(flowController);
        }

        /// <summary>
        /// Gets output set of a program point
        /// </summary>
        public FlowOutputSet OutSet
        {
            get { return flow.OutSet; }
        }

        /// <summary>
        /// Evaluates prefix or postfix increment or decrement of the value
        /// </summary>
        /// <param name="isIncrementOperation">Determines whether to perform increment operation</param>
        /// <param name="operand">One operand of increment or decrement</param>
        /// <returns>Result of operand increment or decrement</returns>
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
        /// Evaluates prefix or postfix increment or decrement of all possible values in memory entry
        /// </summary>
        /// <param name="isIncrementOperation">Determines whether to perform increment operation</param>
        /// <param name="entry">Memory entry with all possible operands of increment or decrement</param>
        /// <returns>Result of performing the increment or decrement on all possible operands</returns>
        public MemoryEntry Evaluate(bool isIncrementOperation, MemoryEntry entry)
        {
            var values = new HashSet<Value>();

            foreach (var value in entry.PossibleValues)
            {
                var result = Evaluate(isIncrementOperation, value);
                values.Add(result);
            }

            return new MemoryEntry(values);
        }

        /// <summary>
        /// Set current evaluation context.
        /// </summary>
        /// <param name="flowController">Flow controller of program point available for evaluation</param>
        public void SetContext(FlowController flowController)
        {
            flow = flowController;
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        /// <remarks>
        /// Since it is the last visitor method in the hierarchy, operation is performing on wrong operand.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown always</exception>
        public override void VisitValue(Value value)
        {
            throw new ArgumentException("Increment or decrement operation is not supported for this type");
        }

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

        #endregion

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

        #endregion

        #region Compound values

        /// <inheritdoc />
        public override void VisitCompoundValue(CompoundValue value)
        {
            result = value;
        }

        #endregion

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

        #endregion

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

        #endregion

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            result = value;
        }

        #region Abstract primitive values

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

        #endregion

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            result = value;
        }

        #endregion

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyCompoundValue(AnyCompoundValue value)
        {
            result = value;
        }

        #endregion

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            result = value;
        }

        #endregion

        #region Function values

        /// <inheritdoc />
        public override void VisitLambdaFunctionValue(LambdaFunctionValue value)
        {
            // TODO: There is no special lambda type, it is implemented as Closure object with __invoke()
            throw new NotImplementedException();
        }

        #endregion

        #endregion
    }
}
