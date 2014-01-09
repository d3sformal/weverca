using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Weverca.Web.Models
{
    public class AnalysisModel
    {
        public bool RunVerification { get; set; }

        public bool RunIndicatorMetrics { get; set; }
        public bool RunQuantityMetrics { get; set; }
        public bool RunRatingMetrics { get; set; }

        public bool RunMetrics
        {
            get
            {
                return RunIndicatorMetrics || RunQuantityMetrics || RunRatingMetrics;
            }
        }

        public AnalysisModel()
        {
            RunVerification = true;
            RunIndicatorMetrics = true;
            RunQuantityMetrics = true;
            RunRatingMetrics = true;
        }
    }
}