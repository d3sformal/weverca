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


using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.AstVisitors
{
    /// <summary>
    /// Enriched tree with a place to store AST nodes which represent some wanted language construct.
    /// </summary>
    internal class OccurrenceVisitor : TreeVisitor
    {
        /// <summary>
        /// All occurrences of searched language constructs.
        /// </summary>
        protected Queue<AstNode> occurrenceNodes = new Queue<AstNode>();

        /// <summary>
        /// Return all nodes with occurrences of searched language constructs.
        /// </summary>
        /// <returns>Occurrences of appropriate nodes.</returns>
        internal IEnumerable<AstNode> GetOccurrences()
        {
            // Copy result to an array to make it immutable
            return occurrenceNodes.ToArray();
        }

        /// <summary>
        /// Remove all stored occurrences.
        /// </summary>
        internal void ResetContent()
        {
            occurrenceNodes.Clear();
        }
    }
}