using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Weverca.Web.Models;

namespace Weverca.Web.Controllers
{
    public class HomeController : Controller
    {
        [ValidateInput(false)]
        public ActionResult Index(WevercaModel model)
        {
            if (model == null)
            {
                model = new WevercaModel();
            }
            model.Verify = true;
            return View(model);
        }
    }
}
