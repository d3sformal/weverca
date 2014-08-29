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
using PHP.Core.Reflection;

using Weverca.CodeMetrics.Processing.AstVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Calculates how many user-defined functions a single function calls on average.
    /// </summary>
    [Metric(Rating.PhpFunctionsCoupling)]
    internal class FunctionCouplingProcessor : RatingProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, Rating category,
            SyntaxParser parser)
        {
            Debug.Assert(category == Rating.PhpFunctionsCoupling,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            if (parser.Functions == null)
            {
                // No function is declared
                if (resolveOccurances)
                {
                    return new Result(0.0, new DirectFcnCall[0]);
                }
                else
                {
                    return new Result(0.0);
                }
            }

            var functionReferences = new Queue<KeyValuePair<FunctionDecl, DirectFcnCall[]>>();
            foreach (var routine in parser.Functions)
            {
                var phpFunction = routine.Value.Member as PhpFunction;
                Debug.Assert(phpFunction != null);

                var declaration = phpFunction.Declaration.GetNode() as FunctionDecl;
                Debug.Assert(declaration != null);

                var visitor = new FunctionCouplingVisitor(declaration.Function);
                declaration.VisitMe(visitor);
                var references = visitor.GetReferences();
                functionReferences.Enqueue(new KeyValuePair<FunctionDecl, DirectFcnCall[]>(
                    declaration, references));
            }

            var result = CalculateRating(functionReferences.ToArray());

            if (resolveOccurances)
            {
                var allFunctionReferences = new Queue<DirectFcnCall>();
                foreach (var functionReference in functionReferences)
                {
                    foreach (var reference in functionReference.Value)
                    {
                        allFunctionReferences.Enqueue(reference);
                    }
                }

                // Return all function calls (DirectFcnCall) used inside a function body
                return new Result(result, allFunctionReferences.ToArray());
            }
            else
            {
                return new Result(result);
            }
        }

        #endregion MetricProcessor overrides

        /// <summary>
        /// Calculate average number of unique function references that a function contains.
        /// </summary>
        /// <param name="functionCouplings">List of function with connections to another function.s</param>
        /// <returns>
        /// Measurement of average number of unique function reference inside a function implementation.
        /// </returns>
        private static double CalculateRating(KeyValuePair<FunctionDecl, DirectFcnCall[]>[] functionCouplings)
        {
            if (functionCouplings.Length <= 0)
            {
                return 0.0;
            }

            var numberOfReferences = 0;
            foreach (var functionDeclaration in functionCouplings)
            {
                Debug.Assert(functionDeclaration.Value != null);
                numberOfReferences += functionDeclaration.Value.Length;
            }

            return System.Convert.ToDouble(numberOfReferences) / functionCouplings.Length;
        }
    }
}