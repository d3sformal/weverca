using System;
using System.Collections.Generic;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates values during the analysis so that they can be used as index of array
    /// </summary>
    /// <remarks>
    /// An array in PHP is actually an ordered map with strings as indices. However in fact, PHP can
    /// distinguishes between string and integer indices. That is useful when array is created, because
    /// if index is not stated with an element, implicit integer index is used. It is incremented after
    /// every use unless it is rewritten by explicit integer index.
    /// Analysis distinguishes five basic groups of values: integers, strings, abstract integers, abstract
    /// non-integers and compounds values. Integers and strings are concrete values that can address elements
    /// directly. Abstract values cannot represent concrete value and therefore they are synonym for index
    /// called unknown index which represent possibly every index. Compound values are reported as illegal
    /// values for indices and they are ignored by PHP with warning.
    /// </remarks>
    public class ArrayIndexEvaluator : PartialExpressionEvaluator
    {
        /// <summary>
        /// Value which can be converted from any value into integer index
        /// </summary>
        private IntegerValue integerIndex;

        /// <summary>
        /// Value which cannot be converted into integer index and is represented as string
        /// </summary>
        private StringValue stringIndex;

        /// <summary>
        /// Indicates that the given value cannot be converted to an integer value
        /// </summary>
        private bool isNotConvertibleToInteger;

        /// <summary>
        /// Indicates that the given value is compound and cannot be used as index
        /// </summary>
        private bool isCompoundValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayIndexEvaluator" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        public ArrayIndexEvaluator(FlowController flowController)
            : base(flowController) { }

        /// <summary>
        /// Evaluates all values to integer or string index of array if it is possible
        /// </summary>
        /// <param name="entry">Memory entry with all possible values to convert to index</param>
        /// <param name="integerValues">Integer indices converted from values that are suited for it</param>
        /// <param name="stringValues">String indices that cannot be converted to integer values</param>
        /// <param name="isAlwaysConcrete">
        /// Indicates that there are combination of values that can be used as concrete index or
        /// they are compound values. Then element does not need to be stored into unknown index
        /// </param>
        /// <param name="isAlwaysInteger">Indicates that all values can be converted to integer</param>
        /// <param name="isAlwaysLegal">Indicates that there is no compound (forbidden) value</param>
        public void Evaluate(MemoryEntry entry, ref HashSet<IntegerValue> integerValues,
            ref HashSet<StringValue> stringValues, out bool isAlwaysConcrete,
            out bool isAlwaysInteger, out bool isAlwaysLegal)
        {
            integerIndex = null;
            stringIndex = null;
            isNotConvertibleToInteger = false;
            isCompoundValue = false;

            isAlwaysConcrete = true;
            isAlwaysInteger = true;
            isAlwaysLegal = true;

            foreach (var value in entry.PossibleValues)
            {
                value.Accept(this);

                Debug.Assert(!isCompoundValue || (isNotConvertibleToInteger && (stringIndex == null)),
                    "When value is compound, it cannot be string or integer in the same time");
                Debug.Assert(isNotConvertibleToInteger || (stringIndex == null),
                    "Value cannot be converted both to a number and a string.");
                Debug.Assert((integerIndex == null) || !isNotConvertibleToInteger,
                    "Value that is converted to concrete integer index is always convertible to integer");

                if (isCompoundValue)
                {
                    if (isAlwaysLegal)
                    {
                        isAlwaysLegal = false;
                    }

                    isCompoundValue = false;

                    if (isAlwaysInteger)
                    {
                        isAlwaysInteger = false;
                    }

                    isNotConvertibleToInteger = false;
                }
                else if (stringIndex != null)
                {
                    stringValues.Add(stringIndex);
                    stringIndex = null;

                    if (isAlwaysInteger)
                    {
                        isAlwaysInteger = false;
                    }

                    isNotConvertibleToInteger = false;
                }
                else if (integerIndex != null)
                {
                    integerValues.Add(integerIndex);
                    integerIndex = null;
                }
                else
                {
                    if (isAlwaysConcrete)
                    {
                        isAlwaysConcrete = false;
                    }

                    if (isNotConvertibleToInteger)
                    {
                        if (isAlwaysInteger)
                        {
                            isAlwaysInteger = false;
                        }

                        isNotConvertibleToInteger = false;
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates all values to string if it is possible and creates index identifier
        /// </summary>
        /// <param name="entry">Memory entry with all possible values to convert to index</param>
        /// <param name="isAlwaysLegal">Indicates that there is no compound (forbidden) value</param>
        /// <returns>Member identifier created from all values of the memory entry</returns>
        public MemberIdentifier EvaluateToIdentifiers(MemoryEntry entry, out bool isAlwaysLegal)
        {
            var integerValues = new HashSet<IntegerValue>();
            var stringValues = new HashSet<StringValue>();

            bool isAlwaysConcrete;
            bool isAlwaysInteger;

            Evaluate(entry, ref integerValues, ref stringValues, out isAlwaysConcrete,
                out isAlwaysInteger, out isAlwaysLegal);

            if (isAlwaysConcrete)
            {
                var indices = new HashSet<string>();

                foreach (var integerValue in integerValues)
                {
                    indices.Add(TypeConversion.ToString(integerValue.Value));
                }

                foreach (var stringValue in stringValues)
                {
                    indices.Add(stringValue.Value);
                }

                return new MemberIdentifier(indices);
            }
            else
            {
                return new MemberIdentifier();
            }
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitScalarValue(ScalarValue value)
        {
            isCompoundValue = false;
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            integerIndex = TypeConversion.ToInteger(OutSet, value);
            stringIndex = null;
            isNotConvertibleToInteger = false;
            VisitGenericScalarValue(value);
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            stringIndex = null;
            isNotConvertibleToInteger = false;
            VisitGenericScalarValue(value);
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            integerIndex = value;
            VisitGenericNumericValue(value);
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            IntegerValue integerValue;
            if (TypeConversion.TryConvertToInteger(OutSet, value, out integerValue))
            {
                integerIndex = integerValue;
            }
            else
            {
                integerIndex = null;
            }

            VisitGenericNumericValue(value);
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            IntegerValue integerValue;
            if (TypeConversion.TryConvertToInteger(OutSet, value, out integerValue))
            {
                integerIndex = integerValue;
            }
            else
            {
                integerIndex = null;
            }

            VisitGenericNumericValue(value);
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            integerIndex = null;
            stringIndex = value;
            isNotConvertibleToInteger = true;
            VisitGenericScalarValue(value);
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitCompoundValue(CompoundValue value)
        {
            integerIndex = null;
            stringIndex = null;
            isNotConvertibleToInteger = true;
            isCompoundValue = true;
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitResourceValue(ResourceValue value)
        {
            integerIndex = null;
            stringIndex = null;
            isNotConvertibleToInteger = true;
            isCompoundValue = false;
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            integerIndex = null;
            stringIndex = TypeConversion.ToString(OutSet, value);
            isNotConvertibleToInteger = true;
            isCompoundValue = false;
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            integerIndex = null;
            stringIndex = null;
            isNotConvertibleToInteger = false;
            isCompoundValue = false;
        }

        #endregion Interval values

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            integerIndex = null;
            stringIndex = null;

            // It does not possibly be convertible to integer
            isNotConvertibleToInteger = true;

            // It can possibly be a compound value
            isCompoundValue = true;
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            integerIndex = null;
            stringIndex = null;
            isCompoundValue = false;
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            isNotConvertibleToInteger = false;
            VisitAnyScalarValue(value);
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyNumericValue(AnyNumericValue value)
        {
            isNotConvertibleToInteger = false;
            VisitAnyScalarValue(value);
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            isNotConvertibleToInteger = true;
            VisitAnyScalarValue(value);
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyCompoundValue(AnyCompoundValue value)
        {
            integerIndex = null;
            stringIndex = null;
            isCompoundValue = true;
            isNotConvertibleToInteger = true;
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            integerIndex = null;
            stringIndex = null;
            isCompoundValue = false;
            isNotConvertibleToInteger = true;
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}
