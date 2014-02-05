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
        public static IEnumerable<SelectListItem> GetValueTypes()
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
