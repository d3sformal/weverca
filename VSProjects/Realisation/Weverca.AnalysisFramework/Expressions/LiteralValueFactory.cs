using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Transforms literals of PHP language to equivalent constant concrete value that analysis uses
    /// </summary>
    internal class LiteralValueFactory : TreeVisitor
    {
        /// <summary>
        /// Concrete value derived from PHP literal
        /// </summary>
        private ConcreteValue literal;

        /// <summary>
        /// Output set of a program point
        /// </summary>
        private FlowOutputSet outSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralValueFactory" /> class.
        /// </summary>
        /// <param name="outputSet">Output set of a program point</param>
        public LiteralValueFactory(FlowOutputSet outputSet)
            : base()
        {
            outSet = outputSet;
        }

        /// <summary>
        /// Convert literal of PHP to concrete value of analysis
        /// </summary>
        /// <param name="x">PHP literal</param>
        /// <returns>Concrete value that is created from PHP literal</returns>
        public ConcreteValue EvaluateLiteral(Literal x)
        {
            literal = null;
            VisitElement(x);

            Debug.Assert(literal != null, "Return value must be set after visiting of literal");
            return literal;
        }

        /// <inheritdoc />
        public override void VisitIntLiteral(IntLiteral x)
        {
            literal = outSet.CreateInt((int)x.Value);
        }

        /// <inheritdoc />
        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            literal = outSet.CreateLong((long)x.Value);
        }

        /// <inheritdoc />
        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            literal = outSet.CreateDouble((double)x.Value);
        }

        /// <inheritdoc />
        public override void VisitStringLiteral(StringLiteral x)
        {
            literal = outSet.CreateString((string)x.Value);
        }

        /// <inheritdoc />
        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void VisitBoolLiteral(BoolLiteral x)
        {
            literal = outSet.CreateBool((bool)x.Value);
        }

        /// <inheritdoc />
        public override void VisitNullLiteral(NullLiteral x)
        {
            literal = outSet.UndefinedValue;
        }
    }
}
