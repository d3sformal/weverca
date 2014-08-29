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
                try
                {
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
                catch (Exception e)
                {
                    return View("Message", new MessageModel(Resources.Error, e.Message));
                }

            }
        }

        public ActionResult About()
        {
            return View();
        }

        bool TryExecute<T>(Func<T> func, int timeout, out T result)
        {
            object t = default(T);
            //TODO: creating a new thread for each client is not really a good idea
            //The best way for solid solution would be to create a stand-alone WCF windows service (with dual-http-binding) doing the analysis and use AJAX to display the state of the queue/result here.
            var thread = new Thread(() => { try { t = func(); } catch (Exception e) { t=e; } });
            thread.Start();
            var completed = thread.Join(timeout);
            if (!completed)
            {
                thread.Abort();
            }
            if(t is Exception)
                throw (t as Exception);

            result = (T)t;
            return completed;
            
        }
    }
}