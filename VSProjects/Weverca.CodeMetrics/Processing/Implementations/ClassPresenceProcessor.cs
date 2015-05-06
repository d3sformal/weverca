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
using System.Diagnostics;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Determines whether there is class presence.
    /// </summary>
    [Metric(ConstructIndicator.ClassOrInterface)]
    internal class ClassPresenceProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.ClassOrInterface,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var types = new Queue<TypeDecl>();

            foreach (var type in parser.Types)
            {
                var node = type.Value.Declaration.GetNode();
                var typeDeclaration = node as TypeDecl;
                Debug.Assert(typeDeclaration != null, "PhpType is always in type declaration node");

                types.Enqueue(typeDeclaration);
            }

            var hasTypes = types.GetEnumerator().MoveNext();

            return new Result(hasTypes, types);
        }

        #endregion MetricProcessor overrides
    }
}