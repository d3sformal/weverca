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


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

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