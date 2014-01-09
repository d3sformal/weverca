using System.ComponentModel.DataAnnotations;

using Weverca.Web.Properties;

namespace Weverca.Web.Models
{
    public class AnalysisModel
    {
        [Display(ResourceType = typeof(Resources), Name = "RunVerification")]
        public bool RunVerification { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "RunIndicatorMetrics")]
        public bool RunIndicatorMetrics { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "RunQuantityMetrics")]
        public bool RunQuantityMetrics { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "RunRatingMetrics")]
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