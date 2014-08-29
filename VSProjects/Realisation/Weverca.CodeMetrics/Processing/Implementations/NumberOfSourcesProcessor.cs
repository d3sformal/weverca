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

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(Quantity.NumberOfSources)]
    internal class NumberOfSourcesProcessor : QuantityProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, Quantity category,
            Parsers.SyntaxParser parser)
        {
            Debug.Assert(category == Quantity.NumberOfSources,
                "Metric of class must be same as passed metric");

            // Processing is made on single source
            return new Result(1);
        }

        #endregion MetricProcessor overrides
    }
}