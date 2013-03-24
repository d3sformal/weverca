using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.CodeMetrics
{
    /// <summary>
    /// Contains php functions which are important for metric
    /// </summary>
    public static class MetricRelatedFunctions
    {
        /// <summary>
        /// Functions that indicates usage of autoload
        /// </summary>
        static readonly string[] Autoload_Register = new string[] { "spl_autoload_register" };
        /// <summary>
        /// Functions that indicates usage of eval
        /// </summary>
        static readonly string[] Eval = new string[] { "eval" };
        /// <summary>
        /// Functions that indicates usage of sessiosn
        /// </summary>
        static readonly string[] Session = new string[]{
            "session_cache_expire",
            "session_cache_limiter",
            "session_commit",
            "session_decode",
            "session_destroy",
            "session_encode",
            "session_get_cookie_params",
            "session_id",
            "session_is_registered",
            "session_module_name",
            "session_name",
            "session_regenerate_id",
            "session_register_shutdown",
            "session_register",
            "session_save_path",
            "session_set_cookie_params",
            "session_set_save_handler",
            "session_start",
            "session_status",
            "session_unregister",
            "session_unset",
            "session_write_close",
        };

        /// <summary>
        /// Indicators to its related functions
        /// </summary>
        static readonly Dictionary<ConstructIndicator, string[]> functions = new Dictionary<ConstructIndicator, string[]>();

        static MetricRelatedFunctions()
        {
            functions.Add(ConstructIndicator.Session, Session);
            functions.Add(ConstructIndicator.Autoload, Autoload_Register);
            functions.Add(ConstructIndicator.Eval, Eval);
            //TODO add other metric related functions
        }

        /// <summary>
        /// Returns functions which presence indicates category usage
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        internal static IReadOnlyCollection<string> Get(ConstructIndicator category)
        {
            string[] output;
            if (!functions.TryGetValue(category, out output))
            {
                return new string[0];
            }

            return output;
        }
    }
}
