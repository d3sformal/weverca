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
        public readonly ConstantDecl Declaration;

        public readonly ValuePoint Initializer;

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
