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
    /// Checks the AST for <see cref="RefAssignEx"/>.
    /// </summary>
    /// <remarks>
    /// If right side of the expression is <see cref="DirectVarUse"/> or an access
    /// to the array <see cref="ItemUse"/>, it is an alias.
    /// </remarks>
    internal class AliasVisitor : OccurrenceVisitor
    {
        #region TreeVisitor overrides

        /// <inheritdoc />
        public override void VisitRefAssignEx(RefAssignEx x)
        {
            if ((x.RValue is DirectVarUse) || (x.RValue is ItemUse) || (x.RValue is IndirectVarUse))
            {
                occurrenceNodes.Enqueue(x);
            }

            base.VisitRefAssignEx(x);
        }

        #endregion TreeVisitor overrides
    }
}