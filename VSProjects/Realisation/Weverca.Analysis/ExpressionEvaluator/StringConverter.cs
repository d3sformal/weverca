using System;
using System.Collections.Generic;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Converts a value to string during the analysis
    /// </summary>
    /// <remarks>
    /// String casting is defined by operations <see cref="Operations.StringCast" /> and
    /// <see cref="Operations.UnicodeCast" />
    /// </remarks>
    public class StringConverter : AbstractValueVisitor
    {
        /// <summary>
        /// Flow controller of program point providing data for evaluation (output set, position etc.)
        /// </summary>
        private FlowController flow;

        /// <summary>
        /// Result of string conversion of the given value
        /// </summary>
        private StringValue result;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringConverter" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public StringConverter(FlowController flowController)
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
        /// Converts the value to string representation
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>String value if conversion is successful, otherwise <c>null</c></returns>
        public StringValue Evaluate(Value value)
        {
            // Gets type of value and evaluate expression for given operation
            result = null;
            value.Accept(this);

            // Returns result of string conversion. Null value indicates any possible string
            return result;
        }

        /// <summary>
        /// Converts all possible values in memory entry to string representation
        /// </summary>
        /// <param name="entry">Memory entry with all possible values to convert</param>
        /// <param name="isAlwaysConcrete">Indicates whether every value converts to concrete string</param>
        /// <returns>List of strings after conversion of all possible values</returns>
        public IEnumerable<StringValue> Evaluate(MemoryEntry entry, out bool isAlwaysConcrete)
        {
            var values = new HashSet<StringValue>();
            isAlwaysConcrete = true;

            foreach (var value in entry.PossibleValues)
            {
                var result = Evaluate(value);

                if (result != null)
                {
                    values.Add(result);
                }
                else
                {
                    if (isAlwaysConcrete)
                    {
                        isAlwaysConcrete = false;
                    }
                }
            }

            return values;
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
        /// Since it is the last visitor method in the hierarchy, conversion is performing on wrong operand.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown always</exception>
        public override void VisitValue(Value value)
        {
            throw new ArgumentException("Type of value does not support casting to string");
        }

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
        }

        #endregion

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            result = value;
        }

        #endregion

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            // TODO: Object can by converted only if it has __toString magic method implemented
            // No result indicates possibly any string value
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            result = TypeConversion.ToString(OutSet, value);
        }

        #endregion

        /// <inheritdoc />
        public override void VisitResourceValue(ResourceValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
        }

        #endregion

        #region Interval values

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            // No result indicates possibly any string value
        }

        #endregion

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            // TODO: This is possible fatal error
            // No result indicates possibly any string value
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            // No result indicates possibly any string value
        }

        #endregion

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            // TODO: Object can by converted only if it has __toString magic method implemented
            // No result indicates possibly any string value
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
        }

        #endregion

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            // No result indicates possibly any string value
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
