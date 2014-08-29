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

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Processor for analyzing maximum inheritance depth.
    /// </summary>
    [Metric(Quantity.MaxInheritanceDepth)]
    internal class MaxInheritanceDepthProcessor : QuantityProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, Quantity category,
            SyntaxParser parser)
        {
            Debug.Assert(category == Quantity.MaxInheritanceDepth,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var occurrences = new Queue<AstNode>();
            var maxInheritanceDepth = FindMaxInheritanceDepth(parser, occurrences);

            return new Result(maxInheritanceDepth, occurrences.ToArray());
        }

        #endregion MetricProcessor overrides

        /// <summary>
        /// Find max inheritance depth and fill occurrences with type declarations where this depth appeared.
        /// </summary>
        /// <param name="parser">Parser which contains analyzed AST.</param>
        /// <param name="occurrences">Type declarations where max depth appeared.</param>
        /// <returns>Maximum inheritance depth that has been found.</returns>
        private int FindMaxInheritanceDepth(SyntaxParser parser, Queue<AstNode> occurrences)
        {
            Debug.Assert(parser.IsParsed);
            Debug.Assert(!parser.Errors.AnyError);

            if (parser.Types == null)
            {
                // No type is declared, there is no inheritance
                return 0;
            }

            var maxInheritanceDepth = 0;

            // Find max inheritance depth in any types
            foreach (var qualifiedName in parser.Types.Keys)
            {
                // If inheritance depth is equal to maximum, no preprocesing is needed
                var inheritanceDepth = GetInheritanceDepth(qualifiedName, parser);
                if (inheritanceDepth > maxInheritanceDepth)
                {
                    // We have found greater inheritance chain - clear old entries
                    occurrences.Clear();
                    maxInheritanceDepth = inheritanceDepth;
                }
                else if (inheritanceDepth < maxInheritanceDepth)
                {
                    // Inheritance depth is lower skip it
                    continue;
                }

                var node = parser.Types[qualifiedName].Declaration.GetNode();
                Debug.Assert(node != null, "Node of type declarations must not been null");

                var astNode = node as AstNode;
                Debug.Assert(astNode != null);

                occurrences.Enqueue(astNode);
            }

            return maxInheritanceDepth;
        }

        /// <summary>
        /// Get inheritance depth for for given typeName.
        /// </summary>
        /// <param name="typeName">Name of analyzed type.</param>
        /// <param name="parser">Parser that has analyzed type.</param>
        /// <returns>Depth of inheritance of given typeName.</returns>
        private int GetInheritanceDepth(QualifiedName typeName, SyntaxParser parser)
        {
            PhpType type;
            if (!parser.Types.TryGetValue(typeName, out type))
            {
                // TODO: There is possible an error in parsed source
                return 1;
            }

            var node = type.Declaration.GetNode() as TypeDecl;
            Debug.Assert(node != null);

            var maxInheritanceDepth = GetInheritanceDepth(node.BaseClassName, parser);
            foreach (var implemented in node.ImplementsList)
            {
                var implementedName = implemented.QualifiedName;
                var inheritanceDepth = GetInheritanceDepth(implementedName, parser);

                if (inheritanceDepth > maxInheritanceDepth)
                {
                    maxInheritanceDepth = inheritanceDepth;
                }
            }

            return maxInheritanceDepth + 1;
        }

        /// <summary>
        /// Get inheritance depth for for given qualifiedName.
        /// </summary>
        /// <param name="qualifiedName">Name of analyzed type.</param>
        /// <param name="parser">Parser that has analyzed type.</param>
        /// <returns>Depth of inheritance of given qualifiedName or null if qualifiedName is null.</returns>
        private int GetInheritanceDepth(GenericQualifiedName? qualifiedName, SyntaxParser parser)
        {
            if (qualifiedName == null)
            {
                return 0;
            }

            return GetInheritanceDepth(qualifiedName.Value.QualifiedName, parser);
        }
    }
}