using System.ComponentModel.DataAnnotations;

using Common.WebDefinitions.Localization;
using Weverca.Web.Properties;


namespace Weverca.Web.Models
{
    public class AnalysisModel
    {
        #region Enums
        public enum MemoryModelType
        {
            [LocalizedDescription(typeof(Resources), "VirtualReference")]
            VirtualReference,

            [LocalizedDescription(typeof(Resources), "Copy")]
            Copy
        }
        #endregion

        [Display(ResourceType = typeof(Resources), Name = "MemoryModelType")]
        public MemoryModelType MemoryModel { get; set; }

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
            MemoryModel = MemoryModelType.Copy;
        }

        public MemoryModels.MemoryModels GetMemoryModel()
        {
            if (MemoryModel == MemoryModelType.Copy) return MemoryModels.MemoryModels.CopyMM;
            return MemoryModels.MemoryModels.VirtualReferenceMM;
        }
    }
}