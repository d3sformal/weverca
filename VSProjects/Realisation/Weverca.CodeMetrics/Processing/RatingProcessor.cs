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
