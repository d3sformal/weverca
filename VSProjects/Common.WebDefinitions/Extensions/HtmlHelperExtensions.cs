using System;
using System.Reflection;
using System.Web.Mvc;

namespace Common.WebDefinitions.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Gets currents version of the apllication.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <returns>Current version</returns>
        public static string CurrentVersion(this HtmlHelper helper, Type assemblyType)
        {
            return Assembly.GetAssembly(assemblyType).GetName().Version.ToString();
        }
    }
}
