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
