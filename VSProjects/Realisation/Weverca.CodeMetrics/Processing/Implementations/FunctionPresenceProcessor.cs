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

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(ConstructIndicator.Eval, ConstructIndicator.Session, ConstructIndicator.MySql,
        ConstructIndicator.ClassAlias)]
    internal class FunctionPresenceProcessor : IndicatorProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, ConstructIndicator category,
            SyntaxParser parser)
        {
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var functions = MetricRelatedFunctions.Get(category);

            var calls = FindCalls(parser, functions);
            var hasCalls = calls.GetEnumerator().MoveNext();

            return new Result(hasCalls, calls);
        }

        #endregion MetricProcessor overrides
    }
}