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
using PHP.Core.Parsers;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Delegate for native methods called during analysis in context of ProgramPointGraph
    /// </summary>
    /// <param name="flow">Flow controller available for method</param>
    public delegate void NativeAnalyzerMethod(FlowController flow);

    /// <summary>
    /// Lang element used for storing native analyzers in ProgramPointGraphs
    /// </summary>
    public class NativeAnalyzer : LangElement
    {
        /// <summary>
        /// Stored native analyzer
        /// </summary>
        public readonly NativeAnalyzerMethod Method;

        /// <summary>
        /// Element which caused invoking of this analyzer - is used for sharing position
        /// </summary>
        public readonly LangElement InvokingElement;

        /// <summary>
        /// Create NativeAnalyzer with specified method
        /// </summary>
        /// <param name="method">Method which is invoked via native analyzer</param>
        /// <param name="invokingElement">Element which caused invoking of this analyzer - is used for sharing position</param>
        public NativeAnalyzer(NativeAnalyzerMethod method,LangElement invokingElement)
            :base(invokingElement.Position)
        {
            InvokingElement = invokingElement;
            Method = method;
        }

        /// <summary>
        /// Override for VisitNative in PartialWalker
        /// </summary>
        /// <param name="visitor">Visitor</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            var expander = visitor as Expressions.ElementExpander;

            if (expander == null)
            {
            }
            else
            {
                expander.VisitNative(this);
            }
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Method.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var o= obj as NativeAnalyzer;

            if (o == null)
                return false;

            return o.Method.Equals(Method);
        }
    }
}