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


using System;
using System.Collections.Generic;
using System.Web.Mvc;

using Common.WebDefinitions.Helpers;
using Weverca.CodeMetrics;
using Weverca.Web.Models;
using Weverca.Web.Properties;

namespace Weverca.Web.Controllers
{
    public class ValueHelperController : Controller
    {
        public static IEnumerable<SelectListItem> GetInputValueTypes()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            foreach (WevercaModel.InputType value in Enum.GetValues(typeof(WevercaModel.InputType)))
            {
                selectList.Add(new SelectListItem()
                {
                    Value = value.ToString(),
                    Text = EnumHelper.GetDescription(value)
                });
            }

            return selectList;
        }

        public static IEnumerable<SelectListItem> GetMemoryModelValueTypes()
        {
            List<SelectListItem> selectList = new List<SelectListItem>();
            foreach (AnalysisModel.MemoryModelType value in Enum.GetValues(typeof(AnalysisModel.MemoryModelType)))
            {
                selectList.Add(new SelectListItem()
                {
                    Value = value.ToString(),
                    Text = EnumHelper.GetDescription(value)
                });
            }

            return selectList;
        }

        public static string FormatMetricsResult(Quantity quantity, MetricResult<int> metricResult)
        {
            return string.Format(MetricsResources.ResourceManager.GetString(quantity.ToString()), string.Format(Resources.IntegerFormat, metricResult.Result));
        }

        public static string FormatMetricsResult(Rating quantity, MetricResult<double> metricResult)
        {
            return string.Format(MetricsResources.ResourceManager.GetString(quantity.ToString()), string.Format(Resources.DoubleFormat, metricResult.Result));
        }

        public static string FormatMetricsResult(ConstructIndicator quantity, MetricResult<bool> metricResult)
        {
            return string.Format(MetricsResources.ResourceManager.GetString(quantity.ToString()), metricResult.Result ? Resources.Yes : Resources.No);
        }
    }
}