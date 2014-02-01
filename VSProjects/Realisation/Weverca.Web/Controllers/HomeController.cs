using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Mvc;

using Weverca.Web.Definitions;
using Weverca.Web.Models;
using Weverca.Web.Properties;

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
                
                ResultModel result;
                bool completed = TryExecute(() => Analyzer.Run(model.PhpCode, model.AnalysisModel), Settings.Default.AnalysisTimeout, out result);
                if (completed)
                {
                    return View("Result", result);
                }
                else
                {
                    return View("Message", new MessageModel(Resources.Error, Resources.AnalysisTimeouted));
                }
            }
        }

        bool TryExecute<T>(Func<T> func, int timeout, out T result)
        {
            var t = default(T);
            //TODO: creating a new thread for each client is not really a good idea
            //The best way for solid solution would be to create a stand-alone WCF windows service (with dual-http-binding) doing the analysis and use AJAX to display the state of the queue/result here.
            var thread = new Thread(() => t = func());
            thread.Start();
            var completed = thread.Join(timeout);
            if (!completed) thread.Abort();
            result = t;
            return completed;
        }
    }
}
