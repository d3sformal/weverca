using System;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Represent special kind of value. For example these values can express some non-determinism.
    /// </summary>
    public class SpecialValue : Value
    {
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
            visitor.VisitSpecialValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class AliasValue : SpecialValue
    {
        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAliasValue(this);
        }
    }

    [Obsolete("Use Value.SetInfo() instead")]
    public abstract class InfoValue : SpecialValue
    {
        public readonly object RawData;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoValue" /> class.
        /// </summary>
        /// <param name="rawData"></param>
        internal InfoValue(object rawData)
        {
            RawData = rawData;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitInfoValue(this);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return RawData.GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value obj)
        {
            var infoValue = obj as InfoValue;
            if (infoValue == null)
            {
                return false;
            }

            return infoValue.RawData.Equals(RawData);
        }
    }

    /// <summary>
    /// Stores meta information for variables and values
    /// WARNING:
    ///     Has to be immutable - also generic type T
    /// </summary>
    /// <typeparam name="T">Type of meta information</typeparam>
    public class InfoValue<T> : InfoValue
    {
        public readonly T Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoValue{T}" /> class.
        /// </summary>
        /// <param name="data"></param>
        internal InfoValue(T data)
            : base(data)
        {
            Data = data;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitGenericInfoValue(this);
        }

        public override string ToString()
        {
            return Data.ToString();
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            throw new NotImplementedException();
        }
    }
}
