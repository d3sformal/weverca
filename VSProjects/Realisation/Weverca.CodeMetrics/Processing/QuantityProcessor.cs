using System.Collections.Generic;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing
{
    internal abstract class QuantityProcessor : MetricProcessor<Quantity, int>
    {
        /// <remarks>
        /// Merging of almost all quantities should be easy. Others can override this behavior.
        /// </remarks>
        /// <inheritdoc />
        protected override int Merge(int firstProperty, int secondProperty)
        {
            return firstProperty + secondProperty;
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
