using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Common.WebDefinitions.Debug
{
    /// <summary>
    /// Extendes <see cref="HandleErrorAttribute"/>. Errors are sent to email specified in settings.
    /// </summary>
    public class HandleErrorLogAttribute : HandleErrorAttribute
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="filterContext">The action-filter context.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="filterContext"/> parameter is null.</exception>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new NullReferenceException("filterContext parameter cannot be null");
            }

            logger.ErrorException(string.Format("Unhandled exception was raised in {0}", filterContext.Controller), filterContext.Exception);

            base.OnException(filterContext);
        }
    }
}
