/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{

    /// <summary>
    /// Type of values which stroes infromation about declared functions and method
    /// </summary>
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
        /// <summary>
        /// The script in that this function is declared
        /// </summary>
        public readonly FileInfo DeclaringScript;
        /// <summary>
        /// Method declaration
        /// Note that it works only if the function value is SourceMethodValue
        /// Added for convenient coding of FunctionResolver, not nice
        /// </summary>
        public MethodDecl MethodDecl
        {
            get { return DeclaringElement as MethodDecl; }
        }

        internal FunctionValue(LangElement declaringElement, Name name, FileInfo declaringScript)
        {
            if (declaringElement == null)
                throw new ArgumentNullException("declaringElement");

            if (declaringScript == null)
                throw new ArgumentNullException("declaringScript");

            DeclaringScript = declaringScript;
            DeclaringElement = declaringElement;
            Name = name;
        }

        /// <inheritdoc />
        public override abstract void Accept(IValueVisitor visitor);

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + " " + Name;
        }
    }

    /// <summary>
    /// Function value, which stores information about native function
    /// </summary>
    public class NativeAnalyzerValue : FunctionValue
    {
        /// <summary>
        /// Native analyzer delegated used for analysis
        /// </summary>
        public readonly NativeAnalyzer Analyzer;

        internal NativeAnalyzerValue(Name name, NativeAnalyzer analyzer)
            : base(analyzer, name, new FileInfo("native_functions.virtual"))
        {
            Analyzer = analyzer;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitNativeAnalyzerValue(this);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return Analyzer.GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value other)
        {
            var o = other as NativeAnalyzerValue;
            if (o == null)
                return false;

            return Analyzer.Equals(o.Analyzer);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new NativeAnalyzerValue(Name, Analyzer);
        }
    }

    /// <summary>
    /// Function value which stroes information about function defined in source code
    /// </summary>
    public class SourceFunctionValue : FunctionValue
    {
        /// <summary>
        /// Ast element of function declaration
        /// </summary>
        public readonly FunctionDecl Declaration;

        internal SourceFunctionValue(FunctionDecl declaration, FileInfo declaringScript)
            : base(declaration, declaration.Name, declaringScript)
        {
            Declaration = declaration;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitSourceFunctionValue(this);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return Declaration.GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value other)
        {
            var o = other as SourceFunctionValue;
            if (o == null)
                return false;

            return Declaration.Equals(o.Declaration);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new SourceFunctionValue(Declaration, DeclaringScript);
        }
    }

    /// <summary>
    /// Method value which stroes information about method defined in source code
    /// </summary>
    public class SourceMethodValue : FunctionValue
    {
        /// <summary>
        /// Ast element of method declaration
        /// </summary>
        public readonly MethodDecl Declaration;

        internal SourceMethodValue(MethodDecl declaration, FileInfo declaringScript)
            : base(declaration, declaration.Name, declaringScript)
        {
            Declaration = declaration;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitSourceMethodValue(this);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return Declaration.GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value other)
        {
            var o = other as SourceMethodValue;
            if (o == null)
                return false;

            return Declaration.Equals(o.Declaration);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new SourceMethodValue(MethodDecl, DeclaringScript);
        }
    }

    /// <summary>
    /// Represents lambda function created as value
    /// </summary>
    public class LambdaFunctionValue : FunctionValue
    {
        /// <summary>
        /// Declaration of represented lambda function
        /// </summary>
        public readonly LambdaFunctionExpr Declaration;

        internal LambdaFunctionValue(LambdaFunctionExpr declaration, FileInfo declaringScript)
            : base(declaration, new Name(".LambdaExpression"), declaringScript)
        {
            Declaration = declaration;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitLambdaFunctionValue(this);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return Declaration.GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value other)
        {
            var o = other as LambdaFunctionValue;
            if (o == null)
                return false;

            return Declaration.Equals(o.Declaration);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new LambdaFunctionValue(Declaration, DeclaringScript);
        }
    }
}