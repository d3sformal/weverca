using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    internal class LiteralValueFactory : TreeVisitor
    {
        private Value literal = null;
        private FlowOutputSet outSet;

        public LiteralValueFactory(FlowOutputSet outSet)
            : base()
        {
            this.outSet = outSet;
        }

        public Value EvaluateLiteral(Literal x)
        {
            try
            {
                VisitElement(x);
                Debug.Assert(literal != null, "Return value must be set after visiting of literal");
                return literal;
            }
            finally
            {
                literal = null;
            }
        }

        public override void VisitIntLiteral(IntLiteral x)
        {
            literal = outSet.CreateInt((int)x.Value);
        }

        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            literal = outSet.CreateLong((long)x.Value);
        }

        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            literal = outSet.CreateDouble((double)x.Value);
        }

        public override void VisitStringLiteral(StringLiteral x)
        {
            literal = outSet.CreateString((string)x.Value);
        }

        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            throw new NotSupportedException();
        }

        public override void VisitBoolLiteral(BoolLiteral x)
        {
            literal = outSet.CreateBool((bool)x.Value);
        }

        public override void VisitNullLiteral(NullLiteral x)
        {
            literal = outSet.UndefinedValue;
        }
    }
}
