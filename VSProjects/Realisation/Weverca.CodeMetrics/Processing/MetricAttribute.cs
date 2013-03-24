using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.CodeMetrics.Processing
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple=false)]
    class MetricAttribute:Attribute
    {
        ConstructIndicator[] indicators;
        Rating[] ratings;
        Quantity[] quantities;

        public MetricAttribute(params ConstructIndicator[] indicators){
            this.indicators = indicators;
        }

        public MetricAttribute(params Rating[] ratings)
        {
            this.ratings = ratings;
        }

        public MetricAttribute(params Quantity[] quantities)
        {
            this.quantities = quantities;
        }
    }
}
