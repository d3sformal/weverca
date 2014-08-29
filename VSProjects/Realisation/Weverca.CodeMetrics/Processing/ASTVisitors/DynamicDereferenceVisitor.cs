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
    /// Checks AST for Dynamic dereference like $$a.
    /// </summary>
    internal class DynamicDereferenceVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        /// <remarks>
        /// The method checks indirect variable use for dynamic dereference.
        /// </remarks>
        /// <inheritdoc />
        public override void VisitIndirectVarUse(IndirectVarUse x)
        {
            // variable is dynamicaly dereferenced, if it has a variable use in it's name.
            if (x.VarNameEx is VariableUse)
            {
                occurrenceNodes.Enqueue(x);
            }

            base.VisitIndirectVarUse(x);
        }

        #endregion TreeVisitor overrides
    }
}