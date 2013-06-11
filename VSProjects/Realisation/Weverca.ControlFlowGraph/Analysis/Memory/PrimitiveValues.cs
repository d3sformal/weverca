using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    public class PrimitiveValue<T> : Value
    {
        public readonly T Value;

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
            return Value.Equals(obj);
        }
    }

    public class IntegerValue : PrimitiveValue<int>
    {
        internal IntegerValue(int value) : base(value) { }
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
