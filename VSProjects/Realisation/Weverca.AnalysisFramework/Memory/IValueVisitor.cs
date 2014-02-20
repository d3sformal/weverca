using System;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Provides visitor functionality for Value class and its descendants.
    /// If there will be any change in this interface, <see cref="AbstractValueVisitor"/>
    /// has to be changed with it.
    /// </summary>
    public interface IValueVisitor
    {
        /// <summary>
        /// Visits <see cref="Value"/>. It is the root of hierarchy
        /// </summary>
        /// <param name="value">The value to visit</param>
        void VisitValue(Value value);

        #region Concrete values

        /// <summary>
        /// Visits <see cref="ConcreteValue"/>
        /// </summary>
        /// <param name="value">The concrete PHP primitive value to visit</param>
        void VisitConcreteValue(ConcreteValue value);

        #region Scalar values

        /// <summary>
        /// Visits <see cref="ScalarValue"/>
        /// </summary>
        /// <param name="value">The scalar value to visit</param>
        void VisitScalarValue(ScalarValue value);

        /// <summary>
        /// Visits <see cref="ScalarValue{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of the scalar value</typeparam>
        /// <param name="value">The typed scalar value to visit</param>
        void VisitGenericScalarValue<T>(ScalarValue<T> value);

        /// <summary>
        /// Visits <see cref="BooleanValue"/>
        /// </summary>
        /// <param name="value">The boolean value to visit</param>
        void VisitBooleanValue(BooleanValue value);

        #region Numeric values

        /// <summary>
        /// Visits <see cref="NumericValue{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of number representation</typeparam>
        /// <param name="value">The numeric value to visit</param>
        void VisitGenericNumericValue<T>(NumericValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>;

        /// <summary>
        /// Visits <see cref="IntegerValue"/>
        /// </summary>
        /// <param name="value">The integral value to visit</param>
        void VisitIntegerValue(IntegerValue value);

        /// <summary>
        /// Visits <see cref="LongintValue"/>
        /// </summary>
        /// <param name="value">The long integral value to visit</param>
        void VisitLongintValue(LongintValue value);

        /// <summary>
        /// Visits <see cref="FloatValue"/>
        /// </summary>
        /// <param name="value">The floating-point number to visit</param>
        void VisitFloatValue(FloatValue value);

        #endregion Numeric values

        /// <summary>
        /// Visits <see cref="StringValue"/>
        /// </summary>
        /// <param name="value">The Unicode string representation to visit</param>
        void VisitStringValue(StringValue value);

        #endregion Scalar values

        #region Compound values

        /// <summary>
        /// Visits <see cref="CompoundValue"/>
        /// </summary>
        /// <param name="value">The compound structure (i.e. object or array) to visit</param>
        void VisitCompoundValue(CompoundValue value);

        /// <summary>
        /// Visits <see cref="ObjectValue"/>
        /// </summary>
        /// <param name="value">The object to visit</param>
        void VisitObjectValue(ObjectValue value);

        /// <summary>
        /// Visits <see cref="AssociativeArray"/>
        /// </summary>
        /// <param name="value">The array to visit</param>
        void VisitAssociativeArray(AssociativeArray value);

        #endregion Compound values

        /// <summary>
        /// Visits <see cref="ResourceValue"/>
        /// </summary>
        /// <param name="value">The resource reference to visit</param>
        void VisitResourceValue(ResourceValue value);

        /// <summary>
        /// Visits <see cref="UndefinedValue"/>
        /// </summary>
        /// <param name="value">The undefined/null value to visit</param>
        void VisitUndefinedValue(UndefinedValue value);

        #endregion Concrete values

        #region Interval values

        /// <summary>
        /// Visits <see cref="IntervalValue{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of interval domain</typeparam>
        /// <param name="value">The interval of numbers to visit</param>
        void VisitGenericIntervalValue<T>(IntervalValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>;

        /// <summary>
        /// Visits <see cref="IntegerIntervalValue"/>
        /// </summary>
        /// <param name="value">The interval of integer values to visit</param>
        void VisitIntervalIntegerValue(IntegerIntervalValue value);

        /// <summary>
        /// Visits <see cref="LongintIntervalValue"/>
        /// </summary>
        /// <param name="value">The interval of long integer values to visit</param>
        void VisitIntervalLongintValue(LongintIntervalValue value);

        /// <summary>
        /// Visits <see cref="FloatIntervalValue"/>
        /// </summary>
        /// <param name="value">The interval of floating-point numbers to visit</param>
        void VisitIntervalFloatValue(FloatIntervalValue value);

        #endregion Interval values

        #region Abstract values

        /// <summary>
        /// Visits <see cref="AnyValue"/>
        /// </summary>
        /// <param name="value">The abstract PHP primitive value to visit</param>
        void VisitAnyValue(AnyValue value);

        #region Abstract scalar values

        /// <summary>
        /// Visits <see cref="AnyScalarValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of scalar type to visit</param>
        void VisitAnyScalarValue(AnyScalarValue value);

        /// <summary>
        /// Visits <see cref="AnyBooleanValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of boolean type to visit</param>
        void VisitAnyBooleanValue(AnyBooleanValue value);

        #region Abstract numeric values

        /// <summary>
        /// Visits <see cref="AnyNumericValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of a numeric type to visit</param>
        void VisitAnyNumericValue(AnyNumericValue value);

        /// <summary>
        /// Visits <see cref="AnyIntegerValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of integer type to visit</param>
        void VisitAnyIntegerValue(AnyIntegerValue value);

        /// <summary>
        /// Visits <see cref="AnyLongintValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of long integer type to visit</param>
        void VisitAnyLongintValue(AnyLongintValue value);

        /// <summary>
        /// Visits <see cref="AnyFloatValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of floating-point numbers type to visit</param>
        void VisitAnyFloatValue(AnyFloatValue value);

        #endregion Abstract numeric values

        /// <summary>
        /// Visits <see cref="AnyStringValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of string type to visit</param>
        void VisitAnyStringValue(AnyStringValue value);

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <summary>
        /// Visits <see cref="AnyCompoundValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of all compound values to visit</param>
        void VisitAnyCompoundValue(AnyCompoundValue value);

        /// <summary>
        /// Visits <see cref="AnyObjectValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of all objects to visit</param>
        void VisitAnyObjectValue(AnyObjectValue value);

        /// <summary>
        /// Visits <see cref="AnyArrayValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of all arrays to visit</param>
        void VisitAnyArrayValue(AnyArrayValue value);

        #endregion Abstract compound values

        /// <summary>
        /// Visits <see cref="AnyResourceValue"/>
        /// </summary>
        /// <param name="value">The abstract interpretation of resource reference to visit</param>
        void VisitAnyResourceValue(AnyResourceValue value);

        #endregion Abstract values

        #region Function values

        /// <summary>
        /// Visits <see cref="FunctionValue"/>
        /// </summary>
        /// <param name="value"></param>
        void VisitFunctionValue(FunctionValue value);

        /// <summary>
        /// Visits <see cref="SourceFunctionValue"/>
        /// </summary>
        /// <param name="value"></param>
        void VisitSourceFunctionValue(SourceFunctionValue value);

        /// <summary>
        /// Visits <see cref="SourceMethodValue"/>
        /// </summary>
        /// <param name="value"></param>
        void VisitSourceMethodValue(SourceMethodValue value);

        /// <summary>
        /// Visits <see cref="NativeAnalyzerValue"/>
        /// </summary>
        /// <param name="value"></param>
        void VisitNativeAnalyzerValue(NativeAnalyzerValue value);

        /// <summary>
        /// Visits <see cref="LambdaFunctionValue"/>
        /// </summary>
        /// <param name="value"></param>
        void VisitLambdaFunctionValue(LambdaFunctionValue value);

        #endregion Function values

        #region Type values

        /// <summary>
        /// Visits <see cref="TypeValue"/>
        /// </summary>
        /// <param name="value">Visited value</param>
        void VisitTypeValue(TypeValue value);

        /// <summary>
        /// Visits native <see cref="TypeValue"/>
        /// </summary>
        /// <param name="value">Visited value</param>
        void VisitNativeTypeValue(TypeValue value);

        #endregion Type values

        #region Special values

        /// <summary>
        /// Visits <see cref="SpecialValue"/>
        /// </summary>
        /// <param name="value">Visited value</param>
        void VisitSpecialValue(SpecialValue value);
                
        /// <summary>
        /// Visits <see cref="InfoValue"/>
        /// </summary>
        /// <param name="value">Visited value</param>
        void VisitInfoValue(InfoValue value);

        /// <summary>
        /// Visits <see cref="InfoValue{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of meta information</typeparam>
        /// <param name="value">Visited value</param>
        void VisitGenericInfoValue<T>(InfoValue<T> value);

        #endregion Special values
    }
}
