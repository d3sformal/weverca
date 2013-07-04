using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.Analysis.Memory
{
    public abstract class PrimitiveValue : Value
    {
        public abstract object RawValue { get; }
    }

    public class PrimitiveValue<T> : PrimitiveValue
    {
        public readonly T Value;

        public override object RawValue { get { return Value; } }

        public PrimitiveValue(T value)
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

            var o=obj as PrimitiveValue<T>;
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
    }

    public class IntegerValue : PrimitiveValue<int>
    {
        internal IntegerValue(int value) : base(value) { }
    }

    public class LongintValue : PrimitiveValue<long>
    {
        internal LongintValue(long value) : base(value) { }
    }

    public class StringValue : PrimitiveValue<string>
    {
        internal StringValue(string value) : base(value) { }
    }

    public class BooleanValue : PrimitiveValue<bool>
    {
        internal BooleanValue(bool value) : base(value) { }
    }

    public class FloatValue : PrimitiveValue<double>
    {
        internal FloatValue(double value) : base(value) { }
    }

    public class FunctionValue : Value
    {
        public readonly FunctionDecl Declaration;
        internal FunctionValue(FunctionDecl declaration)
        {
            Declaration = declaration;
        }
    }
}
