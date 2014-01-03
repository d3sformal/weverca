using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Weverca.Web.Models;
using Weverca.Web.Definitions;
using System.Diagnostics;

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
                Stopwatch stopwatch = Stopwatch.StartNew();

                model.AssignInputType();
                
                var ppGraph = Analyzer.Run(model.PhpCode);

                var output = new WebOutput();
                var graphWalker = new CallGraphPrinter(ppGraph);

                graphWalker.Run(output);

                stopwatch.Stop();
                Debug.WriteLine("Analysis took: {0}", stopwatch.Elapsed);
                
                return View("Result", new ResultModel(model.PhpCode, output.Output));
            }
        }
    }
}
