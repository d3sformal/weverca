using System.Diagnostics;
using System.Web.Mvc;

using Weverca.Web.Definitions;
using Weverca.Web.Models;

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
                
                ResultModel result = Analyzer.Run(model.PhpCode, model.AnalysisModel);

                stopwatch.Stop();
                Debug.WriteLine("Analysis took: {0}", stopwatch.Elapsed);
                
                return View("Result", result);
            }
        }
    }
}
