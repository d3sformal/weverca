namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Class is abstract interpretation of all PHP types. It is used as unknown value
    /// </summary>
    public class AnyValue : Value
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyValue" /> class.
        /// It prevents creating abstract values from outside
        /// </summary>
        internal AnyValue() { }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return GetType().GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value obj)
        {
            return GetType() == obj.GetType();
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new AnyValue();
        }
    }

    /// <summary>
    /// Class is abstract interpretation of all PHP scalar types
    /// </summary>
    public abstract class AnyScalarValue : AnyValue
    {
        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyScalarValue(this);
        }

    }

    /// <summary>
    /// Class is representing abstract interpretation of PHP boolean type
    /// </summary>
    public class AnyBooleanValue : AnyScalarValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyBooleanValue" /> class.
        /// It prevents creating abstract booleans from outside
        /// </summary>
        internal AnyBooleanValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyBooleanValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AnyBooleanValue value = new AnyBooleanValue();
            value.setStorage(getStorage());
            return value;
        }
    }

    /// <summary>
    /// Class is abstract interpretation of all PHP numeric types
    /// </summary>
    public abstract class AnyNumericValue : AnyScalarValue
    {
        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyNumericValue(this);
        }
    }

    /// <summary>
    /// Class is representing abstract interpretation of PHP integer type
    /// </summary>
    public class AnyIntegerValue : AnyNumericValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyIntegerValue" /> class.
        /// It prevents creating abstract integers from outside
        /// </summary>
        internal AnyIntegerValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyIntegerValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AnyIntegerValue value = new AnyIntegerValue();
            value.setStorage(getStorage());
            return value;
        }
    }

    /// <summary>
    /// Class is representing abstract interpretation of PHP long integer type
    /// </summary>
    public class AnyLongintValue : AnyNumericValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyLongintValue" /> class.
        /// It prevents creating abstract long integers from outside
        /// </summary>
        internal AnyLongintValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyLongintValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AnyLongintValue value = new AnyLongintValue();
            value.setStorage(getStorage());
            return value;
        }
    }

    /// <summary>
    /// Class is representing abstract interpretation of PHP floating-point number type
    /// </summary>
    public class AnyFloatValue : AnyNumericValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyFloatValue" /> class.
        /// It prevents creating abstract floating-point numbers from outside
        /// </summary>
        internal AnyFloatValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyFloatValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AnyFloatValue value = new AnyFloatValue();
            value.setStorage(getStorage());
            return value;
        }
    }

    /// <summary>
    /// Class is representing abstract interpretation of PHP string type
    /// </summary>
    public class AnyStringValue : AnyScalarValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyStringValue" /> class.
        /// It prevents creating abstract strings from outside
        /// </summary>
        internal AnyStringValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyStringValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AnyStringValue value = new AnyStringValue();
            value.setStorage(getStorage());
            return value;
        }
    }

    /// <summary>
    /// Class is representing any possible composition of PHP compound value, i.e. objects and arrays
    /// </summary>
    public abstract class AnyCompoundValue : AnyValue
    {
        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyCompoundValue(this);
        }
    }

    /// <summary>
    /// Class is representing abstract interpretation of PHP objects, thus all compositions of their fields
    /// </summary>
    public class AnyObjectValue : AnyCompoundValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyObjectValue" /> class.
        /// It prevents creating abstract objects from outside
        /// </summary>
        internal AnyObjectValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyObjectValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AnyObjectValue value = new AnyObjectValue();
            value.setStorage(getStorage());
            return value;
        }
    }

    /// <summary>
    /// Class is representing abstract interpretation of PHP arrays, thus all compositions of their elements
    /// </summary>
    public class AnyArrayValue : AnyCompoundValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyArrayValue" /> class.
        /// It prevents creating abstract arrays from outside
        /// </summary>
        internal AnyArrayValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyArrayValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AnyArrayValue value = new AnyArrayValue();
            value.setStorage(getStorage());
            return value;
        }
    }

    /// <summary>
    /// Class is representing abstract interpretation of PHP resource type
    /// </summary>
    public class AnyResourceValue : AnyValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnyResourceValue" /> class.
        /// It prevents creating abstract resources from outside
        /// </summary>
        internal AnyResourceValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyResourceValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AnyResourceValue value = new AnyResourceValue();
            value.setStorage(getStorage());
            return value;
        }
    }
}
