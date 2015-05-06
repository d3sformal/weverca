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


using System.Diagnostics;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(Quantity.NumberOfLines)]
    internal class NumberOfLinesProcessor : QuantityProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, Quantity category,
            Parsers.SyntaxParser parser)
        {
            Debug.Assert(category == Quantity.NumberOfLines,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var statements = parser.Ast.Statements;
            if (statements.Count > 0)
            {
                var lastStatement = statements[statements.Count - 1];
                var occurrences = new Statement[] { lastStatement };
                return new Result(lastStatement.Position.LastLine, occurrences);
            }
            else
            {
                return new Result(0);
            }
        }

        #endregion MetricProcessor overrides
    }
}