using System;

using PHP.Core;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Abstract class for scalar value representation
    /// </summary>
    /// <remarks>
    /// Every value represents any data in memory and it is accessible as raw object
    /// </remarks>
    public abstract class ScalarValue : ConcreteValue
    {
        /// <summary>
        /// Gets value that is stored in <see cref="ScalarValue" /> casted as object
        /// </summary>
        public abstract object RawValue { get; }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitScalarValue(this);
        }
    }

    /// <summary>
    /// Abstract class for scalar value representation specifying the concrete type
    /// </summary>
    /// <typeparam name="T">Type of the scalar value</typeparam>
    public abstract class ScalarValue<T> : ScalarValue
    {
        /// <summary>
        /// Strong typed value stored in <see cref="ScalarValue"/>
        /// </summary>
        public readonly T Value;

        /// <inheritdoc />
        public override object RawValue
        {
            get { return Value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarValue{T}" /> class.
        /// </summary>
        /// <param name="value">Typed value representing data in memory</param>
        internal ScalarValue(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns hash code of scalar value, so the class behaves as the scalar type
        /// </summary>
        /// <returns>Hash code of scalar value</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified object has the same scalar value as current object
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns><c>true</c> whether objects have the same scalar value, otherwise <c>false</c></returns>
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var o = obj as ScalarValue<T>;
            if (o == null)
            {
                return false;
            }

            return Value.Equals(o.Value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("'{0}' Type: {1}", Value, typeof(T).Name);
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitGenericScalarValue(this);
        }
    }

    /// <summary>
    /// Class is representing PHP boolean type with only two possible values: <c>true</c> and <c>false</c>
    /// </summary>
    public class BooleanValue : ScalarValue<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanValue" /> class.
        /// </summary>
        /// <param name="value">Boolean value representing the object</param>
        internal BooleanValue(bool value) : base(value) { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitBooleanValue(this);
        }
    }

    /// <summary>
    /// Class is representing PHP number of arbitrary type
    /// </summary>
    /// <typeparam name="T">Type of number representation</typeparam>
    public abstract class NumericValue<T> : ScalarValue<T>
        where T : IComparable, IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Gets representation of native zero value. This value has nothing to do with the value of class
        /// </summary>
        public abstract T Zero { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericValue{T}" /> class.
        /// </summary>
        /// <param name="value">Numeric value representing the object</param>
        internal NumericValue(T value) : base(value) { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitGenericNumericValue(this);
        }
    }

    /// <summary>
    /// Class is representing PHP integral number
    /// </summary>
    public class IntegerValue : NumericValue<int>
    {
        /// <inheritdoc />
        public override int Zero
        {
            get { return 0; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerValue" /> class.
        /// </summary>
        /// <param name="value">Integer value representing the object</param>
        internal IntegerValue(int value) : base(value) { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntegerValue(this);
        }
    }

    /// <summary>
    /// Class is representing PHP long integral number
    /// </summary>
    public class LongintValue : NumericValue<long>
    {
        /// <inheritdoc />
        public override long Zero
        {
            get { return 0L; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LongintValue" /> class.
        /// </summary>
        /// <param name="value">Long integer value representing the object</param>
        internal LongintValue(long value) : base(value) { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitLongintValue(this);
        }
    }

    /// <summary>
    /// Class is representing PHP floating-point number
    /// </summary>
    public class FloatValue : NumericValue<double>
    {
        /// <inheritdoc />
        public override double Zero
        {
            get { return 0.0; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatValue" /> class.
        /// </summary>
        /// <param name="value">Floating-point number representing the object</param>
        internal FloatValue(double value) : base(value) { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitFloatValue(this);
        }
    }

    /// <summary>
    /// Class is representing PHP Unicode string constant
    /// </summary>
    public class StringValue : ScalarValue<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringValue" /> class.
        /// </summary>
        /// <param name="value">String value representing the object</param>
        internal StringValue(string value)
            : base(value)
        {
            Debug.Assert(value != null, "String must always have a value, otherwise it is undefined value");
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitStringValue(this);
        }
    }
}
