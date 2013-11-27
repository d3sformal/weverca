using System;
using System.Collections.Generic;

using PHP.Core;

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
    public class StringConverter : PartialExpressionEvaluator
    {
        /// <summary>
        /// Result of conversion when the value can be converted to a concrete string
        /// </summary>
        private StringValue result;

        /// <summary>
        /// Result of conversion that gives no concrete string value
        /// </summary>
        private AnyStringValue abstractResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringConverter" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public StringConverter(FlowController flowController)
            : base(flowController) { }

        /// <summary>
        /// Converts the value to string, concrete or abstract, depending on success of conversion
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Concrete value if conversion succeeds, otherwise <see cref="AnyStringValue" /></returns>
        public Value Evaluate(Value value)
        {
            // Gets type of value and convert to string
            value.Accept(this);

            // Returns result of string conversion
            if (result != null)
            {
                Debug.Assert(abstractResult == null, "If the string is concrete, cannot be abstract");
                return result;
            }
            else
            {
                Debug.Assert(abstractResult != null, "If the string is abstract, cannot be concrete");
                return abstractResult;
            }
        }

        /// <summary>
        /// Converts the value to concrete string representation
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Concrete string value if conversion is successful, otherwise <c>null</c></returns>
        public StringValue EvaluateToString(Value value)
        {
            // Gets type of value and convert to string
            value.Accept(this);

            // Returns result of string conversion. Null value indicates any possible string
            Debug.Assert((result != null) != (abstractResult != null),
                "Result can be either concrete or abstract string value");
            return result;
        }

        /// <summary>
        /// Converts the value to abstract string representation
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Abstract string value if conversion is successful, otherwise <c>null</c></returns>
        public AnyStringValue EvaluateToAnyString(Value value)
        {
            // Gets type of value and convert to string
            value.Accept(this);

            // Returns result of string conversion. Null value indicates that it is a concrete string
            Debug.Assert((result != null) != (abstractResult != null),
                "Result can be either concrete or abstract string value");
            return abstractResult;
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
                // Gets type of value and convert to string
                value.Accept(this);

                Debug.Assert((result != null) != (abstractResult != null),
                    "Result can be either concrete or abstract string value");

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
        /// Converts all possible values in memory entry to concrete or abstract strings
        /// </summary>
        /// <param name="entry">Memory entry with all possible values to convert</param>
        /// <returns>Resulting entry after converting of all possible values to strings</returns>
        public MemoryEntry Evaluate(MemoryEntry entry)
        {
            var values = new HashSet<Value>();

            foreach (var value in entry.PossibleValues)
            {
                values.Add(Evaluate(value));
            }

            return new MemoryEntry(values);
        }

        /// <summary>
        /// Convert values to string representation and concatenate them
        /// </summary>
        /// <param name="leftOperand">The left operand of concatenation</param>
        /// <param name="rightOperand">The right operand of concatenation</param>
        /// <returns>Concatenated string of both operands</returns>
        public Value EvaluateConcatenation(Value leftOperand, Value rightOperand)
        {
            // Gets type of left operand and convert to string
            leftOperand.Accept(this);

            var leftString = result;
            var leftAnyString = abstractResult;

            // Gets type of right operand and convert to string
            rightOperand.Accept(this);

            // Check whether it is concrete or abstract value
            if ((leftString != null) && (result != null))
            {
                return OutSet.CreateString(string.Concat(leftString.Value, result.Value));
            }
            else
            {
                return OutSet.AnyStringValue;
            }
        }

        /// <summary>
        /// Convert all possible values to string representation and concatenate every combination of them
        /// </summary>
        /// <param name="leftOperand">The left operand of concatenation</param>
        /// <param name="rightOperand">The right operand of concatenation</param>
        /// <returns>All string ​​resulting from the combination of left and right operand values</returns>
        public MemoryEntry EvaluateConcatenation(MemoryEntry leftOperand, MemoryEntry rightOperand)
        {
            var values = new HashSet<Value>();

            bool isLeftAlwaysConcrete;
            var leftStrings = Evaluate(leftOperand, out isLeftAlwaysConcrete);

            bool isRightAlwaysConcrete;
            var rightStrings = Evaluate(rightOperand, out isRightAlwaysConcrete);

            if ((!isLeftAlwaysConcrete) || (!isRightAlwaysConcrete))
            {
                values.Add(OutSet.AnyStringValue);
            }

            foreach (var leftValue in leftStrings)
            {
                foreach (var rightValue in rightStrings)
                {
                    values.Add(OutSet.CreateString(string.Concat(leftValue.Value, rightValue.Value)));
                }
            }

            return new MemoryEntry(values);
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
            abstractResult = null;
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
            abstractResult = null;
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
            abstractResult = null;
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
            abstractResult = null;
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            result = value;
            abstractResult = null;
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            // TODO: Object can by converted only if it has __toString magic method implemented
            result = null;
            abstractResult = OutSet.AnyStringValue;
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            result = TypeConversion.ToString(OutSet, value);
            abstractResult = null;
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitResourceValue(ResourceValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
            abstractResult = null;
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
            abstractResult = null;
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            result = null;
            abstractResult = OutSet.AnyStringValue;
        }

        #endregion Interval values

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            // TODO: This is possible fatal error
            result = null;
            abstractResult = OutSet.AnyStringValue;
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            result = null;
            abstractResult = OutSet.AnyStringValue;
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            // TODO: Object can by converted only if it has __toString magic method implemented
            result = null;
            abstractResult = OutSet.AnyStringValue;
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            result = TypeConversion.ToString(OutSet, value);
            abstractResult = null;
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            result = null;
            abstractResult = OutSet.AnyStringValue;
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}
