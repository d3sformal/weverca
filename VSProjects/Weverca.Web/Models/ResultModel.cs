/*
Copyright (c) 2012-2014 Matyas Brenner and David Hauzar

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