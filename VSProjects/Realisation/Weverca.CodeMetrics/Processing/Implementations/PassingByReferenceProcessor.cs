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

using Weverca.CodeMetrics.Processing.AstVisitors;
using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Identifies all function and static method calls with a parameter passed by reference.
    /// </summary>
    [Metric(ConstructIndicator.PassingByReferenceAtCallSide)]
    internal class PassingByReferenceProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            System.Diagnostics.Debug.Assert(category == ConstructIndicator.PassingByReferenceAtCallSide,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            if ((parser.Functions == null) && (parser.Types == null))
            {
                // No type is declared
                if (resolveOccurances)
                {
                    return new Result(false, new FunctionCall[0]);
                }
                else
                {
                    return new Result(false);
                }
            }

            var functions = (parser.Functions != null) ? parser.Functions
                : new Dictionary<QualifiedName, ScopedDeclaration<DRoutine>>();
            var types = (parser.Types != null) ? parser.Types
                : new Dictionary<QualifiedName, PhpType>();

            var visitor = new PassingByReferenceVisitor(functions, types);
            parser.Ast.VisitMe(visitor);

            var occurrences = visitor.GetOccurrences();
            var hasOccurrence = occurrences.GetEnumerator().MoveNext();

            // Return function (DirectFcnCall) or static method (DirectStMtdCall) calls
            // (both are subtypes of FunctionCall) which pass at least one parameter by reference
            return new Result(hasOccurrence, occurrences);
        }

        #endregion MetricProcessor overrides
    }
}