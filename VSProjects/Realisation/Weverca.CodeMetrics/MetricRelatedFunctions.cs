using System;
using System.Collections.Generic;

using PHP.Core.Reflection;

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
        /// Names of magic methods
        /// </summary>
        static readonly string[] Magic_Methods = new string[]{
            DObject.SpecialMethodNames.Call.LowercaseValue,
            DObject.SpecialMethodNames.CallStatic.LowercaseValue,
            DObject.SpecialMethodNames.Clone.LowercaseValue,
            DObject.SpecialMethodNames.Construct.LowercaseValue,
            DObject.SpecialMethodNames.Destruct.LowercaseValue,
            DObject.SpecialMethodNames.Get.LowercaseValue,
            DObject.SpecialMethodNames.Invoke.LowercaseValue,
            DObject.SpecialMethodNames.Isset.LowercaseValue,
            DObject.SpecialMethodNames.Set.LowercaseValue,
            "__set_state",
            DObject.SpecialMethodNames.Sleep.LowercaseValue,
            DObject.SpecialMethodNames.Tostring.LowercaseValue,
            DObject.SpecialMethodNames.Unset.LowercaseValue,
            DObject.SpecialMethodNames.Wakeup.LowercaseValue,
        };
        /// <summary>
        /// One-argument functions that operate with strings (implemented in PHP.Library.PhpStrings)
        /// </summary>
        static readonly string[] String_Functions = new string[]{
            "addslashes",
            "bin2hex",
            "convert_uudecode",
            "convert_uuencode",
            "hebrev",
            "hebrevc",
            "hex2bin",
            // All html* functions are skipped
            "lcfirst",
            "md5",
            "metaphone",
            "nl2br",
            "number_format",
            "quotemeta",
            "sha1",
            "soundex",
            "str_rot13",
            "strip_tags",
            "stripcslashes",
            "stripslashes",
            "strrev",
            "strtolower",
            "strtoupper",
            "ucfirst",
            "ucwords",
        };
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
        /// MySQL function list
        /// <seealso cref="http://www.php.net/manual/en/ref.mysql.php"/>
        /// </summary>
        static readonly string[] mySqlFunctions = new string[] {
            "mysql_affected_rows",
            "mysql_client_encoding",
            "mysql_close",
            "mysql_connect",
            "mysql_create_db",
            "mysql_data_seek",
            "mysql_db_name",
            "mysql_db_query",
            "mysql_drop_db",
            "mysql_errno",
            "mysql_error",
            "mysql_escape_string",
            "mysql_fetch_array",
            "mysql_fetch_assoc",
            "mysql_fetch_field",
            "mysql_fetch_lengths",
            "mysql_fetch_object",
            "mysql_fetch_row",
            "mysql_field_flags",
            "mysql_field_len",
            "mysql_field_name",
            "mysql_field_seek",
            "mysql_field_table",
            "mysql_field_type",
            "mysql_free_result",
            "mysql_get_client_info",
            "mysql_get_host_info",
            "mysql_get_proto_info",
            "mysql_get_server_info",
            "mysql_info",
            "mysql_insert_id",
            "mysql_list_dbs",
            "mysql_list_fields",
            "mysql_list_processes",
            "mysql_list_tables",
            "mysql_num_fields",
            "mysql_num_rows",
            "mysql_pconnect",
            "mysql_ping",
            "mysql_query",
            "mysql_real_escape_string",
            "mysql_result",
            "mysql_select_db",
            "mysql_set_charset",
            "mysql_stat",
            "mysql_tablename",
            "mysql_thread_id",
            "mysql_unbuffered_query"
        };

        /// <summary>
        /// Indicators to its related functions
        /// </summary>
        static readonly Dictionary<ConstructIndicator, string[]> functions = new Dictionary<ConstructIndicator, string[]>();

        static MetricRelatedFunctions()
        {
            functions.Add(ConstructIndicator.MagicMethod, Magic_Methods);
            functions.Add(ConstructIndicator.DynamicInclude, String_Functions);
            functions.Add(ConstructIndicator.Session, Session);
            functions.Add(ConstructIndicator.Autoload, Autoload_Register);
            functions.Add(ConstructIndicator.Eval, Eval);
            functions.Add(ConstructIndicator.MySQL, mySqlFunctions);
            // TODO: Add other metric related functions
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
