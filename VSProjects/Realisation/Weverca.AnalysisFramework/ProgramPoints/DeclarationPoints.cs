using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST; 

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Type declaration representation
    /// </summary>
    public class TypeDeclPoint : ProgramPointBase
    {
        /// <summary>
        /// Declaration element represented by current point
        /// </summary>
        public readonly TypeDecl Declaration;
        
        /// <inheritdoc />
        public override LangElement Partial { get { return Declaration; } }

        internal TypeDeclPoint(TypeDecl declaration)
        {
            Declaration = declaration;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Services.Evaluator.DeclareGlobal(Declaration);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitTypeDecl(this);
        }
    }

    /// <summary>
    /// Function declaration representation
    /// </summary>
    public class FunctionDeclPoint : ProgramPointBase
    {
        /// <summary>
        /// function declaration ast node
        /// </summary>
        public readonly FunctionDecl Declaration;

        /// <inheritdoc />
        public override LangElement Partial { get { return Declaration; } }

        internal FunctionDeclPoint(FunctionDecl declaration)
        {
            Declaration = declaration;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Services.FunctionResolver.DeclareGlobal(Declaration);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitFunctionDecl(this);
        }
    }

    /// <summary>
    /// Constant declaration representation
    /// </summary>
    public class ConstantDeclPoint : ValuePoint
    {
        /// <summary>
        /// Constant declaration represented by current point
        /// </summary>
        public readonly ConstantDecl Declaration;

        /// <summary>
        /// Initializer that creates value from declaration
        /// </summary>
        public readonly ValuePoint Initializer;

        /// <inheritdoc />
        public override LangElement Partial { get { return Declaration; } }
        
        internal ConstantDeclPoint(ConstantDecl declaration, ValuePoint initializer)
        {
            Declaration = declaration;
            Initializer = initializer;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Services.Evaluator.ConstantDeclaration(Declaration, Initializer.Value.ReadMemory(OutSnapshot));

            Value = Initializer.Value;
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitConstantDecl(this);
        }
    }
}
