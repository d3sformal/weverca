using System.Collections.Generic;

using PHP.Core.AST;
using Weverca.CodeMetrics;

namespace Weverca.Web.Models
{
    public class ResultModel
    {
        public string PhpCode { get; private set; }

        public string Output { get; set; }

        public Dictionary<ConstructIndicator, MetricResult<bool>> IndicatorMetricsResult { get; private set; }

        public Dictionary<Quantity, MetricResult<int>> QuantityMetricsResult { get; private set; }

        public Dictionary<Rating, MetricResult<double>> RatingMetricsResult { get; private set; }

        public ResultModel(string phpCode)
        {
            PhpCode = phpCode;

            IndicatorMetricsResult = new Dictionary<ConstructIndicator, MetricResult<bool>>();
            QuantityMetricsResult = new Dictionary<Quantity, MetricResult<int>>();
            RatingMetricsResult = new Dictionary<Rating, MetricResult<double>>();            
        }
    }

    public class MetricResult<T>
    {
        public T Result { get; set; }

        public IEnumerable<AstNode> Occurences { get; set; }
    }
}