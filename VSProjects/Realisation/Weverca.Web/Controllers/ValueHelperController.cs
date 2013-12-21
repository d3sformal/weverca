using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.WebDefinitions.Helpers;
using Weverca.Web.Models;

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
    }
}
