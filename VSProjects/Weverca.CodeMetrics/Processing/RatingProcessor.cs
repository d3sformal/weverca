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


using System;
using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing
{
    internal abstract class RatingProcessor : MetricProcessor<Rating, double>
    {
        /// <remarks>
        /// Merging of almost all quantities should be easy. Others can override this behavior.
        /// </remarks>
        /// <inheritdoc />
        protected override double Merge(double firstProperty, double secondProperty)
        {
            return Math.Max(firstProperty, secondProperty);
        }

        /// <remarks>
        /// Merging of almost all indicators should be easy. All occurrences from the first result are just
        /// appended to occurrences from the second result. Derived classes can override this behavior.
        /// </remarks>
        /// <inheritdoc />
        protected override IEnumerable<AstNode> Merge(IEnumerable<AstNode> firstOccurrences,
            IEnumerable<AstNode> secondOccurrences)
        {
            var occurrences = new List<AstNode>(firstOccurrences);
            occurrences.AddRange(secondOccurrences);
            return occurrences;
        }
    }
}