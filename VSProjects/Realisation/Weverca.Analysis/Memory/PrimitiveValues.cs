using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.Analysis.Memory
{

    /// <summary>
    /// Abstract class for primitive values
    /// </summary>
    public abstract class PrimitiveValue : Value
    {
        /// <summary>
        /// Value that is stored in PrimitiveValue casted as object
        /// </summary>
        public abstract object RawValue { get; }


        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitPrimitiveValue(this);
        }
    }

    public class PrimitiveValue<T> : PrimitiveValue
    {
        /// <summary>
        /// Strong typed value stored in PrimitiveValue
        /// </summary>
        public readonly T Value;

        public override object RawValue { get { return Value; } }

        internal PrimitiveValue(T value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var o = obj as PrimitiveValue<T>;
            if (o == null)
            {
                return false;
            }
            return Value.Equals(o.Value);
        }

        public override string ToString()
        {
            return string.Format("'{0}', Type: {1}", Value, typeof(T).Name);
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitGenericPrimitiveValue(this);
        }
    }

    public class IntegerValue : PrimitiveValue<int>
    {
        internal IntegerValue(int value) : base(value) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntegerValue(this);
        }
    }

    public class LongintValue : PrimitiveValue<long>
    {
        internal LongintValue(long value) : base(value) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitLongintValue(this);
        }
    }

    public class StringValue : PrimitiveValue<string>
    {
        internal StringValue(string value) : base(value) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitStringValue(this);
        }
    }

    public class BooleanValue : PrimitiveValue<bool>
    {
        internal BooleanValue(bool value) : base(value) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitBooleanValue(this);
        }
    }

    public class FloatValue : PrimitiveValue<double>
    {
        internal FloatValue(double value) : base(value) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitFloatValue(this);
        }
    }

     


}
