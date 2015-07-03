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

        public MemoryModels.MemoryModelFactory GetMemoryModel()
        {
            if (MemoryModel == MemoryModelType.Copy) return MemoryModels.MemoryModels.ModularCopyMM;
            return MemoryModels.MemoryModels.VirtualReferenceMM;
        }
    }
}