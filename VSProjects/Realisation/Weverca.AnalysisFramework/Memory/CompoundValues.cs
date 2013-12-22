namespace Weverca.AnalysisFramework.Memory
{
    public abstract class CompoundValue : ConcreteValue
    {
        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitCompoundValue(this);
        }

        protected override int getHashCode()
        {
            return UID+this.GetType().GetHashCode();
        }

        protected override bool equals(Value other)
        {
            return this == other;
        }

        public override string ToString()
        {
            return string.Format("{0} UID: {1}", base.ToString(), UID);
        }
    }

    /// <summary>
    /// ObjectValue is used as "ticket" that allows snapshot API to operate on represented object
    /// NOTE:
    ///     Is supposed to be used as Hash key for getting stored info in snapshot
    /// </summary>
    public sealed class ObjectValue : CompoundValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValue" /> class.
        /// It prevents creating objects from outside
        /// </summary>
        internal ObjectValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitObjectValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            ObjectValue value = new ObjectValue();
            value.setStorage(getStorage());
            return value;
        }
    }

    /// <summary>
    /// AssociativeArray is used as "ticket" that allows snapshot API to operate on represented array
    /// NOTE:
    ///     * Is supposed to be used as Hash key for getting stored info in snapshot
    /// </summary>
    public sealed class AssociativeArray : CompoundValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssociativeArray" /> class.
        /// It prevents creating arrays from outside
        /// </summary>
        internal AssociativeArray() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAssociativeArray(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            AssociativeArray value = new AssociativeArray();
            value.setStorage(getStorage());
            return value;
        }
    }
}
