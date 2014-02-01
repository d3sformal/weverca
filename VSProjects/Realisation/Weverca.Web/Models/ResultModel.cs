using System.Collections.Generic;

using PHP.Core.AST;
using Weverca.Analysis;
using Weverca.CodeMetrics;

namespace Weverca.Web.Models
{
    public class ResultModel
    {
        #region Properties

        public string PhpCode { get; private set; }

        public string Output { get; set; }

        public Dictionary<ConstructIndicator, MetricResult<bool>> IndicatorMetricsResult { get; private set; }

        public Dictionary<Quantity, MetricResult<int>> QuantityMetricsResult { get; private set; }

        public Dictionary<Rating, MetricResult<double>> RatingMetricsResult { get; private set; }

        public List<AnalysisWarning> Warnings { get; private set; }
        public List<AnalysisSecurityWarning> SecurityWarnings { get; private set; }

        #endregion

        #region Constructor

        public ResultModel(string phpCode)
        {
            PhpCode = phpCode;

            IndicatorMetricsResult = new Dictionary<ConstructIndicator, MetricResult<bool>>();
            QuantityMetricsResult = new Dictionary<Quantity, MetricResult<int>>();
            RatingMetricsResult = new Dictionary<Rating, MetricResult<double>>();            
        }

        #endregion

        #region Methods

        public void LoadWarnings()
        {
            Warnings = AnalysisWarningHandler.GetWarnings();
            SecurityWarnings = AnalysisWarningHandler.GetSecurityWarnings();
        }

        #endregion
    }

    public class MetricResult<T>
    {
        public T Result { get; set; }

        public IEnumerable<AstNode> Occurences { get; set; }
    }
}