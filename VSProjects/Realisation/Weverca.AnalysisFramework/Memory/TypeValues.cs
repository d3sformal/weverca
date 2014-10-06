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

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Value storing information about class declaration
    /// </summary>
    public class TypeValue : Value
    {
        /// <summary>
        /// Stores information about class, linke ancestors, fields, methods and constants
        /// </summary>
        public readonly ClassDecl Declaration;

        /// <summary>
        /// Class name
        /// </summary>
        public readonly QualifiedName QualifiedName;

        internal TypeValue(ClassDecl declaration)
        {
            QualifiedName = declaration.QualifiedName;
            Declaration = declaration;
        }

        /// <inheritdoc />
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitNativeTypeValue(this);
        }

        /// <inheritdoc />
        protected override Value cloneValue()
        {
            return new TypeValue(Declaration);
        }

        /// <inheritdoc />
        protected override int getHashCode()
        {
            return Declaration.GetHashCode();
        }

        /// <inheritdoc />
        protected override bool equals(Value other)
        {
            return this == other;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + " type: " + QualifiedName;
        }
    }
}