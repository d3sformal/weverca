using System;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Base class for value visitor.
    /// <see cref="AbstractValueVisitor" /> has to implement all methods from <see cref="IValueVisitor" />
    /// interface. Every visit method calls visit method for object's supertype. Last method in the hierarchy
    /// is abstract. Allows that visitor implementations does not have to implement all methods.
    /// NOTE:
    ///     * Simplifies visitor implementations.
    ///     * Provides functionality for adding new visit methods into IValueVisitor.
    ///     * For implementers - implement VisitValue method and then just methods you need.
    /// </summary>
    public abstract class AbstractValueVisitor : IValueVisitor
    {
        /// <inheritdoc />
        public abstract void VisitValue(Value value);

        /// <summary>
        /// For all values in the memory entry calls their accept functions.
        /// </summary>
        /// <param name="entry">Memory entry to visit</param>
        public void VisitMemoryEntry(MemoryEntry entry)
        {
            foreach (var value in entry.PossibleValues)
            {
                value.Accept(this);
            }
        }

        #region Concrete values

        /// <inheritdoc />
        public virtual void VisitConcreteValue(ConcreteValue value)
        {
            VisitValue(value);
        }

        #region Scalar values

        /// <inheritdoc />
        public virtual void VisitScalarValue(ScalarValue value)
        {
            VisitConcreteValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitGenericScalarValue<T>(ScalarValue<T> value)
        {
            VisitScalarValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitBooleanValue(BooleanValue value)
        {
            VisitGenericScalarValue(value);
        }

        #region Numeric values

        /// <inheritdoc />
        public virtual void VisitGenericNumericValue<T>(NumericValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            VisitGenericScalarValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitIntegerValue(IntegerValue value)
        {
            VisitGenericNumericValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitLongintValue(LongintValue value)
        {
            VisitGenericNumericValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitFloatValue(FloatValue value)
        {
            VisitGenericNumericValue(value);
        }

        #endregion

        /// <inheritdoc />
        public virtual void VisitStringValue(StringValue value)
        {
            VisitGenericScalarValue(value);
        }

        #endregion

        #region Compound values

        /// <inheritdoc />
        public virtual void VisitCompoundValue(CompoundValue value)
        {
            VisitConcreteValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitObjectValue(ObjectValue value)
        {
            VisitCompoundValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitAssociativeArray(AssociativeArray value)
        {
            VisitCompoundValue(value);
        }

        #endregion

        /// <inheritdoc />
        public virtual void VisitResourceValue(ResourceValue value)
        {
            VisitConcreteValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitUndefinedValue(UndefinedValue value)
        {
            VisitConcreteValue(value);
        }

        #endregion

        #region Interval values

        /// <inheritdoc />
        public virtual void VisitGenericIntervalValue<T>(IntervalValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            VisitValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            VisitGenericIntervalValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            VisitGenericIntervalValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            VisitGenericIntervalValue(value);
        }

        #endregion

        #region Abstract values

        /// <inheritdoc />
        public virtual void VisitAnyValue(AnyValue value)
        {
            VisitValue(value);
        }

        #region Abstract primitive values

        /// <inheritdoc />
        public virtual void VisitAnyScalarValue(AnyScalarValue value)
        {
            VisitAnyValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            VisitAnyScalarValue(value);
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public virtual void VisitAnyNumericValue(AnyNumericValue value)
        {
            VisitAnyScalarValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            VisitAnyNumericValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitAnyLongintValue(AnyLongintValue value)
        {
            VisitAnyNumericValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitAnyFloatValue(AnyFloatValue value)
        {
            VisitAnyNumericValue(value);
        }

        #endregion

        /// <inheritdoc />
        public virtual void VisitAnyStringValue(AnyStringValue value)
        {
            VisitAnyScalarValue(value);
        }

        #endregion

        #region Abstract compound values

        /// <inheritdoc />
        public virtual void VisitAnyCompoundValue(AnyCompoundValue value)
        {
            VisitAnyValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitAnyObjectValue(AnyObjectValue value)
        {
            VisitAnyCompoundValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitAnyArrayValue(AnyArrayValue value)
        {
            VisitAnyCompoundValue(value);
        }

        #endregion

        /// <inheritdoc />
        public virtual void VisitAnyResourceValue(AnyResourceValue value)
        {
            VisitAnyValue(value);
        }

        #endregion

        #region Function values

        /// <inheritdoc />
        public virtual void VisitFunctionValue(FunctionValue value)
        {
            VisitValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitSourceFunctionValue(SourceFunctionValue value)
        {
            VisitFunctionValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitSourceMethodValue(SourceMethodValue value)
        {
            VisitFunctionValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitNativeAnalyzerValue(NativeAnalyzerValue value)
        {
            VisitFunctionValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitLambdaFunctionValue(LambdaFunctionValue value)
        {
            VisitFunctionValue(value);
        }

        #endregion

        #region Type values

        /// <inheritdoc />
        public virtual void VisitTypeValue(TypeValue value)
        {
            VisitValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitNativeTypeValue(TypeValue value)
        {
            VisitTypeValue(value);
        }

        #endregion

        #region Special values

        /// <inheritdoc />
        public virtual void VisitSpecialValue(SpecialValue value)
        {
            VisitValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitAliasValue(AliasValue value)
        {
            VisitSpecialValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitInfoValue(InfoValue value)
        {
            VisitSpecialValue(value);
        }

        /// <inheritdoc />
        public virtual void VisitInfoValue<T>(InfoValue<T> value)
        {
            VisitInfoValue(value);
        }

        #endregion
    }
}
