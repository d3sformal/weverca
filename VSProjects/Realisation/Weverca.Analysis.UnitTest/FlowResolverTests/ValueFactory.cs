using System;

using PHP.Core.AST;
using PHP.Core.Parsers;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.UnitTest.FlowResolverTests
{
    static class LiteralFactory
    {
        public static IntLiteral Create(int value)
        {
            return new IntLiteral(new Position(), value);
        }

        public static LongIntLiteral Create(long value)
        {
            return new LongIntLiteral(new Position(), value);
        }

        public static DoubleLiteral Create(double value)
        {
            return new DoubleLiteral(new Position(), value);
        }

        public static StringLiteral Create(string value)
        {
            return new StringLiteral(new Position(), value);
        }

        public static BoolLiteral Create(bool value)
        {
            return new BoolLiteral(new Position(), value);
        }
    }

    static class LiteralValueFactory
    {
        public static Tuple<Literal, Value> Create(int value)
        {
            return new Tuple<Literal, Value>(LiteralFactory.Create(value), new IntegerValue(value));
        }

        public static Tuple<Literal, Value> Create(long value)
        {
            return new Tuple<Literal, Value>(LiteralFactory.Create(value), new LongintValue(value));
        }

        public static Tuple<Literal, Value> Create(double value)
        {
            return new Tuple<Literal, Value>(LiteralFactory.Create(value), new FloatValue(value));
        }

        public static Tuple<Literal, Value> Create(string value)
        {
            return new Tuple<Literal, Value>(LiteralFactory.Create(value), new StringValue(value));
        }

        public static Tuple<Literal, Value> Create(bool value)
        {
            return new Tuple<Literal, Value>(LiteralFactory.Create(value), new BooleanValue(value));
        }
    }
}
