using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Weverca.Web.Models;
using Weverca.Web.Definitions;

namespace Weverca.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new WevercaModel());
        }
        
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Index(WevercaModel model)
        {
            if (model == null)
            {
                model = new WevercaModel();
            }

            if (model.ChangeInput)
            {
                model.AssignInput();
                model.ChangeInput = false;

                ModelState.Clear();
                return View(model);
            }
            else
            {
                model.AssignInputType();
                //TODO: run analysis
                var ppGraph = Analyzer.Run(model.PhpCode);
                
                return View("Result", new ResultModel(model.PhpCode));
            }
        }
    }
}
