/*
Copyright (c) 2012-2014 Miroslav Vodolan, Matyas Brenner, David Skorvaga, David Hauzar.

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


using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Checks the tree for dynamic function call.
    /// </summary>
    internal class DynamicCallVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        /// <inheritdoc />
        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            // x.NameExpr is VariableUse -- cannot be done. NameExpr is internal.
            // Is it enough that we have IndirectFunctionCall?
            // Could there be something different that var use in nameExpr?
            occurrenceNodes.Enqueue(x);
            base.VisitIndirectFcnCall(x);
        }

        /// <inheritdoc />
        public override void VisitNewEx(NewEx x)
        {
            if (x.ClassNameRef is IndirectTypeRef)
            {
                occurrenceNodes.Enqueue(x);
            }

            base.VisitNewEx(x);
        }

        #endregion TreeVisitor overrides
    }
}