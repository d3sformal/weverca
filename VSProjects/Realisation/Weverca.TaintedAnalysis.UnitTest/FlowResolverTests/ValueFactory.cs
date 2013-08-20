using PHP.Core.AST;
using PHP.Core.Parsers;

namespace Weverca.TaintedAnalysis.UnitTest.FlowResolverTests
{
    static class ValueFactory
    {
        public static IntLiteral Create(int value)
        {
            return new IntLiteral(new Position(), value);
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
}
