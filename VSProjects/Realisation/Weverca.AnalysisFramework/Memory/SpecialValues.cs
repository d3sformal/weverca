using System;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Represent special kind of value. For example these values can express some non-determinism.
    /// </summary>
    public class SpecialValue : Value
    {
        protected override int getHashCode()
        {
            return GetType().GetHashCode();
        }

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
            throw new System.NotImplementedException();
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

        internal InfoValue(object rawData)
        {
            RawData = rawData;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitInfoValue(this);
        }

        protected override int getHashCode()
        {
            return RawData.GetHashCode();
        }

        protected override bool equals(Value obj)
        {
            var o = obj as InfoValue;
            if (o == null)
            {
                return false;
            }

            return o.RawData.Equals(RawData);
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

        internal InfoValue(T data)
            : base(data)
        {
            Data = data;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitInfoValue<T>(this);
        }

        public override string ToString()
        {
            return Data.ToString();
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            throw new System.NotImplementedException();
        }
    }
}
