namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Class is representing a value of primitive PHP type
    /// </summary>
    public abstract class ConcreteValue : Value
    {
        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitConcreteValue(this);
        }
    }

    /// <summary>
    /// Class is representing reference to an PHP external resource, identified by special internal ID
    /// </summary>
    public class ResourceValue : ConcreteValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceValue" /> class.
        /// It prevents creating resource from outside
        /// </summary>
        internal ResourceValue() { }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitResourceValue(this);
        }
    }

    /// <summary>
    /// Class is representing PHP null type with the only one possible value: <c>NULL</c>
    /// </summary>
    public class UndefinedValue : ConcreteValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UndefinedValue" /> class.
        /// It prevents creating undefined value from outside
        /// </summary>
        internal UndefinedValue() { }

        /// <summary>
        /// Returns hash code of the type, thou all instances of undefined (or null) type are the same
        /// </summary>
        /// <returns>Hash code of undefined value</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        /// <summary>
        /// Determines whether type of the compared object is <see cref="UndefinedValue"/>
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns><c>true</c> if object has same type as the current one, otherwise <c>false</c></returns>
        public override bool Equals(object obj)
        {
            return GetType() == obj.GetType();
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitUndefinedValue(this);
        }
    }
}
