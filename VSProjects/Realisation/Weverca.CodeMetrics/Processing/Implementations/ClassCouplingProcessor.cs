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
using Weverca.CodeMetrics.Processing.AstVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Calculates how many user-defined classes a single class uses on average.
    /// </summary>
    [Metric(Rating.ClassCoupling)]
    internal class ClassCouplingProcessor : RatingProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, Rating category,
            SyntaxParser parser)
        {
            Debug.Assert(category == Rating.ClassCoupling,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            if (parser.Types == null)
            {
                // No type is declared
                if (resolveOccurances)
                {
                    return new Result(0.0, new DirectTypeRef[0]);
                }
                else
                {
                    return new Result(0.0);
                }
            }

            var types = new Queue<TypeDecl>();
            foreach (var type in parser.Types)
            {
                var node = type.Value.Declaration.GetNode();
                var typeNode = node as TypeDecl;
                Debug.Assert(typeNode != null);

                // As a type, we consider class and interface too
                if ((typeNode.AttributeTarget & PhpAttributeTargets.Types) != 0)
                {
                    types.Enqueue(typeNode);
                }
            }

            var typeReferences = new Queue<KeyValuePair<TypeDecl, DirectTypeRef[]>>();
            foreach (var type in types)
            {
                var visitor = new ClassCouplingVisitor(type.Type);
                type.VisitMe(visitor);
                var references = visitor.GetReferences();
                typeReferences.Enqueue(new KeyValuePair<TypeDecl, DirectTypeRef[]>(
                    type, references));
            }

            var result = CalculateRating(typeReferences.ToArray());

            if (resolveOccurances)
            {
                var allTypeReferences = new Queue<DirectTypeRef>();
                foreach (var typeReference in typeReferences)
                {
                    foreach (var reference in typeReference.Value)
                    {
                        allTypeReferences.Enqueue(reference);
                    }
                }

                // Return all class and interface references (DirectTypeRef) used inside a class declaration
                return new Result(result, allTypeReferences.ToArray());
            }
            else
            {
                return new Result(result);
            }
        }

        #endregion MetricProcessor overrides

        /// <summary>
        /// Calculate average number of unique type references that a type contains.
        /// </summary>
        /// <param name="classCouplings">List of types with connections to another types.</param>
        /// <returns>Measurement of average number of unique type references inside a type.</returns>
        private static double CalculateRating(KeyValuePair<TypeDecl, DirectTypeRef[]>[] classCouplings)
        {
            if (classCouplings.Length <= 0)
            {
                return 0.0;
            }

            var numberOfReferences = 0;
            foreach (var classDeclaration in classCouplings)
            {
                Debug.Assert(classDeclaration.Value != null);
                numberOfReferences += classDeclaration.Value.Length;
            }

            return System.Convert.ToDouble(numberOfReferences) / classCouplings.Length;
        }
    }
}