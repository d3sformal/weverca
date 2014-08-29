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
    /// Identifies all inclusions which can be evaluated statically.
    /// </summary>
    [Metric(ConstructIndicator.DynamicInclude)]
    internal class DynamicIncludeProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(category == ConstructIndicator.DynamicInclude,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            if (parser.Inclusions == null)
            {
                // No type is inclusions
                if (resolveOccurances)
                {
                    return new Result(false, new IncludingEx[0]);
                }
                else
                {
                    return new Result(false);
                }
            }

            var occurrences = new Queue<IncludingEx>();
            var stringFunctions = MetricRelatedFunctions.Get(category);
            Debug.Assert(stringFunctions.GetEnumerator().MoveNext());
            var functions = new HashSet<string>(stringFunctions);

            foreach (var inclusion in parser.Inclusions)
            {
                var expressions = new Queue<Expression>();
                expressions.Enqueue(inclusion.Target);
                var isDynamic = false;

                while (expressions.Count > 0)
                {
                    var expression = expressions.Dequeue();

                    // Note that the strings beginning with quotes are automatically broken down by variables
                    var concatenation = expression as ConcatEx;
                    if (concatenation != null)
                    {
                        foreach (var operand in concatenation.Expressions)
                        {
                            expressions.Enqueue(operand);
                        }
                    }
                    else
                    {
                        var functionCall = expression as DirectFcnCall;
                        if (functionCall != null)
                        {
                            // The subroutine must be function, i.e. it must not be member of a class
                            if (functionCall.IsMemberOf == null
                                // The number of parameters must be exactly 1
                                && functionCall.CallSignature.Parameters.Count == 1
                                // Function names are case-insensitive
                                && functions.Contains(functionCall.QualifiedName.Name.LowercaseValue))
                            {
                                expressions.Enqueue(functionCall.CallSignature.Parameters[0].Expression);
                            }
                            else
                            {
                                isDynamic = true;
                                break;
                            }
                        }
                        else if (!(expression is StringLiteral))
                        {
                            // It is not correct terminal symbol of the expression, a string
                            isDynamic = true;
                            break;
                        }
                    }
                }

                if (isDynamic)
                {
                    occurrences.Enqueue(inclusion);
                }
            }

            var hasOccurrence = occurrences.GetEnumerator().MoveNext();

            // Return inclusions (IncludingEx) that cannot be evaluated statically
            return new Result(hasOccurrence, occurrences);
        }

        #endregion MetricProcessor overrides
    }
}