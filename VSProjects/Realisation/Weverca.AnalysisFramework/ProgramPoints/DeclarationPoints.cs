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

        public override LangElement Partial { get { return Declaration; } }

        internal TypeDeclPoint(TypeDecl declaration)
        {
            NeedsFunctionResolver = true;

            Declaration = declaration;
        }

        protected override void flowThrough()
        {
            Services.FunctionResolver.DeclareGlobal(Declaration);
        }
    }

    /// <summary>
    /// Function declaration representation
    /// </summary>
    public class FunctionDeclPoint : ProgramPointBase
    {
        public readonly FunctionDecl Declaration;

        public override LangElement Partial { get { return Declaration; } }

        internal FunctionDeclPoint(FunctionDecl declaration)
        {
            NeedsFunctionResolver = true;
            Declaration = declaration;
        }

        protected override void flowThrough()
        {
            Services.FunctionResolver.DeclareGlobal(Declaration);
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
            NeedsExpressionEvaluator = true;

            Declaration = declaration;
            Initializer = initializer;
        }

        protected override void flowThrough()
        {
            Services.Evaluator.ConstantDeclaration(Declaration, Initializer.Value.ReadMemory(InSnapshot));

            Value = Initializer.Value;
        }
    }
}
