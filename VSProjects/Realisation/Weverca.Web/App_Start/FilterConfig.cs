using System.Web;
using System.Web.Mvc;
using Common.WebDefinitions.Debug;

namespace Weverca.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorLogAttribute());
        }
    }
}