using System;

namespace Weverca.CodeMetrics.Processing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class MetricAttribute : Attribute
    {
        private ConstructIndicator[] indicators;

        private Rating[] ratings;

        private Quantity[] quantities;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricAttribute" /> class.
        /// </summary>
        /// <param name="indicators"></param>
        public MetricAttribute(params ConstructIndicator[] indicators)
        {
            this.indicators = indicators;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricAttribute" /> class.
        /// </summary>
        /// <param name="ratings"></param>
        public MetricAttribute(params Rating[] ratings)
        {
            this.ratings = ratings;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricAttribute" /> class.
        /// </summary>
        /// <param name="quantities"></param>
        public MetricAttribute(params Quantity[] quantities)
        {
            this.quantities = quantities;
        }
    }
}
