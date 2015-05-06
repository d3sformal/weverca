/*
Copyright (c) 2012-2014 Miroslav Vodolan, Matyas Brenner, David Skorvaga, David Hauzar.

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


using System.Collections.Generic;

using PHP.Core.Reflection;

namespace Weverca.CodeMetrics
{
    /// <summary>
    /// Contains PHP functions which are important for metric.
    /// </summary>
    public static class MetricRelatedFunctions
    {
        /// <summary>
        /// Functions that indicates usage of autoload. Phalanger detects __autoload method automatically,
        /// other can be registered by spl_autoload_register.
        /// </summary>
        /// <seealso cref="ConstructIndicator.Autoload" />
        /// <seealso href="http://php.net/manual/en/function.spl-autoload-register.php" />
        private static readonly string[] autoloadRegister = new string[]
        {
            "spl_autoload_register"
        };

        /// <summary>
        /// Functions that indicates usage of magic methods.
        /// </summary>
        private static readonly string[] magicMethods = new string[]
        {
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
        /// One-argument functions that operate with strings (implemented in PHP.Library.PhpStrings).
        /// </summary>
        /// <remarks>
        /// All html* functions are skipped.
        /// </remarks>
        private static readonly string[] stringFunctions = new string[]
        {
            "addslashes",
            "bin2hex",
            "convert_uudecode",
            "convert_uuencode",
            "hebrev",
            "hebrevc",
            "hex2bin",
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
        /// Functions that indicates usage of eval.
        /// </summary>
        private static readonly string[] eval = new string[]
        {
            "eval"
        };

        /// <summary>
        /// Functions that indicates usage of sessions.
        /// </summary>
        private static readonly string[] session = new string[]
        {
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
        /// MySQL function list.
        /// </summary>
        /// <seealso href="http://www.php.net/manual/en/ref.mysql.php" />
        private static readonly string[] mySqlFunctions = new string[]
        {
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
        /// Name of function that creates an alias for a class.
        /// </summary>
        /// <seealso href="http://www.php.net/manual/en/function.class-alias.php" />
        private static readonly string[] classAliasFunction = new string[]
        {
            "class_alias"
        };

        /// <summary>
        /// Indicators to its related functions.
        /// </summary>
        private static readonly Dictionary<ConstructIndicator, string[]> functions
            = new Dictionary<ConstructIndicator, string[]>();

        /// <summary>
        /// Initializes static members of the <see cref="MetricRelatedFunctions" /> class.
        /// </summary>
        static MetricRelatedFunctions()
        {
            functions.Add(ConstructIndicator.Autoload, autoloadRegister);
            functions.Add(ConstructIndicator.MagicMethods, magicMethods);
            functions.Add(ConstructIndicator.DynamicInclude, stringFunctions);
            functions.Add(ConstructIndicator.Eval, eval);
            functions.Add(ConstructIndicator.Session, session);
            functions.Add(ConstructIndicator.MySql, mySqlFunctions);
            functions.Add(ConstructIndicator.ClassAlias, classAliasFunction);
        }

        /// <summary>
        /// Returns functions which presence indicates metric usage.
        /// </summary>
        /// <param name="metric">Metric that looks for presence of returned functions.</param>
        /// <returns>Functions which presence indicates metric usage.</returns>
        public static IEnumerable<string> Get(ConstructIndicator metric)
        {
            string[] output;
            if (!functions.TryGetValue(metric, out output))
            {
                return new string[0];
            }

            return output;
        }
    }
}