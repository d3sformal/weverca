using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    public abstract class FunctionValue : Value
    {
        /// <summary>
        /// Name of function, which can be used for function declaring
        /// <remarks>Not all function types can be declared e.g. LambdaExpression</remarks>
        /// </summary>
        public readonly Name Name;
        /// <summary>
        /// Element, which caused creating this function
        /// </summary>
        public readonly LangElement DeclaringElement;

        internal FunctionValue(LangElement declaringElement,Name name)
        {
            if (declaringElement == null)
                throw new ArgumentNullException("declaringElement");
            DeclaringElement = declaringElement;
            Name = name;
        }

        public override abstract void Accept(IValueVisitor visitor);
    }

    public class NativeAnalyzerValue : FunctionValue
    {
        public readonly NativeAnalyzer Analyzer;

        internal NativeAnalyzerValue(Name name, NativeAnalyzer analyzer)
            : base(analyzer,name)
        {
            Analyzer = analyzer;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitNativeAnalyzerValue(this);
        }

        protected override int getHashCode()
        {
            return Analyzer.GetHashCode();
        }

        protected override bool equals(Value other)
        {
            var o = other as NativeAnalyzerValue;
            if (o == null)
                return false;

            return Analyzer.Equals(o.Analyzer);
        }

        /// <inheritdoc />
        protected override Value cloneWithStorage(InfoDataStorage storage)
        {
            throw new System.NotImplementedException();
        }
    }

    public class SourceFunctionValue : FunctionValue
    {
        public readonly FunctionDecl Declaration;

        internal SourceFunctionValue(FunctionDecl declaration)
            : base(declaration,declaration.Name)
        {
            Declaration = declaration;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitSourceFunctionValue(this);
        }

        protected override int getHashCode()
        {
            return Declaration.GetHashCode();
        }

        protected override bool equals(Value other)
        {
            var o = other as SourceFunctionValue;
            if (o == null)
                return false;

            return Declaration.Equals(o.Declaration);
        }

        /// <inheritdoc />
        protected override Value cloneWithStorage(InfoDataStorage storage)
        {
            throw new System.NotImplementedException();
        }
    }

    public class SourceMethodValue : FunctionValue
    {
        public readonly MethodDecl Declaration;

        internal SourceMethodValue(MethodDecl declaration)
            : base(declaration, declaration.Name)
        {
            Declaration = declaration;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitSourceMethodValue(this); 
        }

        protected override int getHashCode()
        {
            return Declaration.GetHashCode();
        }

        protected override bool equals(Value other)
        {
            var o = other as SourceMethodValue;
            if (o == null)
                return false;

            return Declaration.Equals(o.Declaration);
        }

        /// <inheritdoc />
        protected override Value cloneWithStorage(InfoDataStorage storage)
        {
            throw new System.NotImplementedException();
        }
    }

    public class LambdaFunctionValue : FunctionValue
    {
        public readonly LambdaFunctionExpr Declaration;

        internal LambdaFunctionValue(LambdaFunctionExpr declaration)
            : base(declaration, new Name(".LambdaExpression"))
        {
            Declaration = declaration;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitLambdaFunctionValue(this); 
        }

        protected override int getHashCode()
        {
            return Declaration.GetHashCode();
        }

        protected override bool equals(Value other)
        {
            var o = other as LambdaFunctionValue;
            if (o == null)
                return false;

            return Declaration.Equals(o.Declaration);
        }

        /// <inheritdoc />
        protected override Value cloneWithStorage(InfoDataStorage storage)
        {
            throw new System.NotImplementedException();
        }
    }
}
