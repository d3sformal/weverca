using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using PHP.Core;

using Weverca.Analysis.ExpressionEvaluator;
using Weverca.Analysis.Properties;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.NativeAnalyzers
{

    /// <summary>
    /// Represents information about native function argument
    /// </summary>
    public class NativeFunctionArgument
    {
        /// <summary>
        /// Type of argument
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Indicates if argument is passed by reference
        /// </summary>
        public bool ByReference { get; private set; }

        /// <summary>
        /// Indicates if argument is optional
        /// </summary>
        public bool Optional { get; private set; }

        /// <summary>
        /// Dots means that function takes more than one argument:
        /// Example printf("",...);
        /// </summary>
        public bool Dots { get; private set; }

        /// <summary>
        /// Creates the instance of NativeFunctionArgument
        /// </summary>
        /// <param name="type">Type of argument</param>
        /// <param name="optional">optional flag</param>
        /// <param name="byReference">reference flag</param>
        /// <param name="dots">dots flag</param>
        public NativeFunctionArgument(string type, bool optional, bool byReference, bool dots)
        {
            this.Type = type;
            this.ByReference = byReference;
            this.Optional = optional;
            this.Dots = dots;
        }
    }

    /// <summary>
    /// Stores information about native function
    /// </summary>
    public class NativeFunction
    {
        /// <summary>
        /// Delegate called during analysis to model this function
        /// </summary>
        public NativeAnalyzerMethod Analyzer { get; set; }

        /// <summary>
        /// Function name
        /// </summary>
        public QualifiedName Name { get; protected set; }

        /// <summary>
        /// Function arguments
        /// </summary>
        public List<NativeFunctionArgument> Arguments { get; protected set; }

        /// <summary>
        /// Represents return ttype of function
        /// </summary>
        public string ReturnType { get; protected set; }

        /// <summary>
        /// Minimal number of arguments, which function takes
        /// </summary>
        public int MinArgumentCount = -1;

        /// <summary>
        /// Maximal number of arguments, which function takes
        /// </summary>
        public int MaxArgumentCount = -1;

        /// <summary>
        /// Creates instance of NativeFunction
        /// </summary>
        /// <param name="name">Name of function</param>
        /// <param name="returnType">Return type</param>
        /// <param name="arguments">Arguments of function</param>
        public NativeFunction(QualifiedName name, string returnType, List<NativeFunctionArgument> arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
            this.ReturnType = returnType;
            this.Analyzer = null;
        }

        /// <summary>
        /// Default contructino for NativeFunction
        /// </summary>
        public NativeFunction()
        { }
    }


    /// <summary>
    /// Singleton class which stores information about native functinos and their arguments.
    /// Provides delegates for modeling native functions during analysis 
    /// </summary>
    public class NativeFunctionAnalyzer
    {
        /// <summary>
        /// Type-modeling implementations of functions.
        /// All PHP native functions are modeled by type.
        /// </summary>
        private Dictionary<QualifiedName, List<NativeFunction>> typeModeledFunctions = new Dictionary<QualifiedName, List<NativeFunction>>();

        /// <summary>
        /// Concrete implementations of functions.
        /// </summary>
        private Dictionary<QualifiedName, NativeAnalyzerMethod> concreteFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();

        /// <summary>
        /// Special implementations of functions.
        /// If a special implementation of function exists, it should be called and any other implementation
        /// should not be called.
        /// </summary>
        private Dictionary<QualifiedName, NativeAnalyzerMethod> specialFunctions = new Dictionary<QualifiedName, NativeAnalyzerMethod>();

        /// <summary>
        /// Structure storing information about function which clean dirty flag from values
        /// </summary>
        public Dictionary<QualifiedName, List<FlagType>> SanitizingFunctions;

        /// <summary>
        /// Structure storing information about function which report security warning, when, argument contains drity flag
        /// </summary>
        public Dictionary<QualifiedName, List<FlagType>> ReportingFunctions;

        private HashSet<string> types = new HashSet<string>();

        private HashSet<string> returnTypes = new HashSet<string>();

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static NativeFunctionAnalyzer instance = null;

        #region consructor, xml parser

        /// <summary>
        /// Creates new instance of NativeFunctionAnalyzer.
        /// Parses XML file containing information about native functions.
        /// </summary>
        private NativeFunctionAnalyzer()
        {

            initCleaningFunctions();
            initReportingFunctions();

            string function = "";
            string returnType = "";
            string functionAlias = "";
            List<NativeFunctionArgument> arguments = new List<NativeFunctionArgument>();

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.php_functions)))
            using (XmlReader reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "function")
                            {
                                arguments = new List<NativeFunctionArgument>();
                                function = reader.GetAttribute("name");
                                returnType = reader.GetAttribute("returnType");
                                functionAlias = reader.GetAttribute("alias");
                                QualifiedName functionName = new QualifiedName(new Name(function));
                                if (functionAlias != null)
                                {
                                    typeModeledFunctions[functionName] = typeModeledFunctions[new QualifiedName(new Name(functionAlias))];
                                }
                                returnTypes.Add(reader.GetAttribute("returnType"));
                            }
                            else if (reader.Name == "arg")
                            {
                                types.Add(reader.GetAttribute("type"));
                                bool optional = false;
                                bool byReference = false;
                                bool dots = false;
                                if (reader.GetAttribute("optional") == "true")
                                {
                                    optional = true;
                                }
                                if (reader.GetAttribute("byReference") == "true")
                                {
                                    byReference = true;
                                }
                                if (reader.GetAttribute("dots") == "true")
                                {
                                    dots = true;
                                }
                                NativeFunctionArgument argument = new NativeFunctionArgument(reader.GetAttribute("type"), optional, byReference, dots);
                                arguments.Add(argument);
                            }
                            break;
                        case XmlNodeType.Text:
                            break;
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                            break;
                        case XmlNodeType.Comment:
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "function")
                            {
                                QualifiedName functionName = new QualifiedName(new Name(function));
                                if (!typeModeledFunctions.ContainsKey(functionName))
                                {
                                    typeModeledFunctions[functionName] = new List<NativeFunction>();
                                }
                                typeModeledFunctions[functionName].Add(new NativeFunction(functionName, returnType, arguments));

                            }
                            break;
                    }
                }
            }

            /*
            var it = instance.types.GetEnumerator();
            while (it.MoveNext())
            {
                Console.WriteLine(it.Current);
               
            }
            Console.WriteLine();
            it = instance.returnTypes.GetEnumerator();
            while (it.MoveNext())
            {
                Console.WriteLine(it.Current);
                
            }*/


            /*foreach(var fnc in instance.allNativeFunctions)
            {
                checkFunctionsArguments(fnc.Value, null);
                for (int i = 0; i < fnc.Value.Count; i++)
                {
                    for (int j = i + 1; j < fnc.Value.Count; j++)
                    {
                        if (false==areIntervalsDisjuct(fnc.Value.ElementAt(i).MinArgumentCount, fnc.Value.ElementAt(i).MaxArgumentCount,fnc.Value.ElementAt(j).MinArgumentCount, fnc.Value.ElementAt(j).MaxArgumentCount))
                        {
                        Console.WriteLine("function: {0}", fnc.Value.ElementAt(0).Name);
                        Console.WriteLine("{0} {1} {2} {3}",fnc.Value.ElementAt(i).MinArgumentCount, fnc.Value.ElementAt(i).MaxArgumentCount,fnc.Value.ElementAt(j).MinArgumentCount, fnc.Value.ElementAt(j).MaxArgumentCount);
                        }
                    }
                }
            }*/


            QualifiedName defineName = new QualifiedName(new Name("define"));
            SpecialFunctionsImplementations analyzer = new SpecialFunctionsImplementations(typeModeledFunctions[defineName]);
            specialFunctions.Add(defineName, new NativeAnalyzerMethod(analyzer._define));

            QualifiedName constantName = new QualifiedName(new Name("constant"));
            SpecialFunctionsImplementations constantAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[constantName]);
            specialFunctions.Add(constantName, new NativeAnalyzerMethod(constantAnalyzer._constant));

            QualifiedName is_arrayName = new QualifiedName(new Name("is_array"));
            SpecialFunctionsImplementations is_arrayAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_arrayName]);
            specialFunctions.Add(is_arrayName, new NativeAnalyzerMethod(is_arrayAnalyzer._is_array));

            QualifiedName is_boolName = new QualifiedName(new Name("is_bool"));
            SpecialFunctionsImplementations is_boolAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_boolName]);
            specialFunctions.Add(is_boolName, new NativeAnalyzerMethod(is_boolAnalyzer._is_bool));

            QualifiedName is_doubleName = new QualifiedName(new Name("is_double"));
            SpecialFunctionsImplementations is_doubleAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_doubleName]);
            specialFunctions.Add(is_doubleName, new NativeAnalyzerMethod(is_doubleAnalyzer._is_double));

            QualifiedName is_floatName = new QualifiedName(new Name("is_float"));
            SpecialFunctionsImplementations is_floatAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_floatName]);
            specialFunctions.Add(is_floatName, new NativeAnalyzerMethod(is_floatAnalyzer._is_double));

            QualifiedName is_intName = new QualifiedName(new Name("is_int"));
            SpecialFunctionsImplementations is_intAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_intName]);
            specialFunctions.Add(is_intName, new NativeAnalyzerMethod(is_intAnalyzer._is_int));

            QualifiedName is_integerName = new QualifiedName(new Name("is_integer"));
            SpecialFunctionsImplementations is_integerAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_integerName]);
            specialFunctions.Add(is_integerName, new NativeAnalyzerMethod(is_integerAnalyzer._is_int));

            QualifiedName is_longName = new QualifiedName(new Name("is_long"));
            SpecialFunctionsImplementations is_longAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_longName]);
            specialFunctions.Add(is_longName, new NativeAnalyzerMethod(is_longAnalyzer._is_int));

            QualifiedName is_nullName = new QualifiedName(new Name("is_null"));
            SpecialFunctionsImplementations is_nullAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_nullName]);
            specialFunctions.Add(is_nullName, new NativeAnalyzerMethod(is_nullAnalyzer._is_null));

            QualifiedName is_numericName = new QualifiedName(new Name("is_numeric"));
            SpecialFunctionsImplementations is_numericAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_numericName]);
            specialFunctions.Add(is_numericName, new NativeAnalyzerMethod(is_numericAnalyzer._is_numeric));

            QualifiedName is_objectName = new QualifiedName(new Name("is_object"));
            SpecialFunctionsImplementations is_objectAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_objectName]);
            specialFunctions.Add(is_objectName, new NativeAnalyzerMethod(is_objectAnalyzer._is_object));

            QualifiedName is_realName = new QualifiedName(new Name("is_real"));
            SpecialFunctionsImplementations is_realAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_realName]);
            specialFunctions.Add(is_realName, new NativeAnalyzerMethod(is_realAnalyzer._is_double));

            QualifiedName is_resourceName = new QualifiedName(new Name("is_resource"));
            SpecialFunctionsImplementations is_resourceAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_resourceName]);
            specialFunctions.Add(is_resourceName, new NativeAnalyzerMethod(is_resourceAnalyzer._is_resource));

            QualifiedName is_scalarName = new QualifiedName(new Name("is_scalar"));
            SpecialFunctionsImplementations is_scalarAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_scalarName]);
            specialFunctions.Add(is_scalarName, new NativeAnalyzerMethod(is_scalarAnalyzer._is_scalar));

            QualifiedName is_stringName = new QualifiedName(new Name("is_string"));
            SpecialFunctionsImplementations is_stringAnalyzer = new SpecialFunctionsImplementations(typeModeledFunctions[is_stringName]);
            specialFunctions.Add(is_stringName, new NativeAnalyzerMethod(is_stringAnalyzer._is_string));

            NativeFunctionsConcreteImplementations.AddConcreteFunctions(typeModeledFunctions, concreteFunctions);

        }

        private QualifiedName getQualifiedName(string s)
        {
            return new QualifiedName(new Name(s));
        }

        private List<FlagType> getList(params FlagType[] types)
        {
            var result = new List<FlagType>();
            foreach (var type in types)
            {
                result.Add(type);
            }
            return result;

        }

        private void initCleaningFunctions()
        {
            SanitizingFunctions = new Dictionary<QualifiedName, List<FlagType>>();

            SanitizingFunctions.Add(getQualifiedName("htmlentities"), getList(FlagType.HTMLDirty));
            SanitizingFunctions.Add(getQualifiedName("htmlspecialchars"), getList(FlagType.HTMLDirty));

            SanitizingFunctions.Add(getQualifiedName("mysql_escape_string"), getList(FlagType.SQLDirty));
            SanitizingFunctions.Add(getQualifiedName("mysql_real_escape_string"), getList(FlagType.SQLDirty));
            SanitizingFunctions.Add(getQualifiedName("sqlite_escape_string"), getList(FlagType.SQLDirty));
            SanitizingFunctions.Add(getQualifiedName("mysqli_real_escape_string"), getList(FlagType.SQLDirty));
            SanitizingFunctions.Add(getQualifiedName("mysqli_escape_string"), getList(FlagType.SQLDirty));

            SanitizingFunctions.Add(getQualifiedName("md5"), getList(FlagType.FilePathDirty,FlagType.HTMLDirty,FlagType.SQLDirty));
            SanitizingFunctions.Add(getQualifiedName("sha1"), getList(FlagType.FilePathDirty, FlagType.HTMLDirty, FlagType.SQLDirty));

            SanitizingFunctions.Add(getQualifiedName("intval"), getList(FlagType.FilePathDirty, FlagType.HTMLDirty, FlagType.SQLDirty));
        }

        private void initReportingFunctions()
        {
            ReportingFunctions = new Dictionary<QualifiedName, List<FlagType>>();
            ReportingFunctions.Add(getQualifiedName("print_r"), getList(FlagType.HTMLDirty));
            ReportingFunctions.Add(getQualifiedName("printf"), getList(FlagType.HTMLDirty));
            ReportingFunctions.Add(getQualifiedName("print"), getList(FlagType.HTMLDirty));

            ReportingFunctions.Add(getQualifiedName("fopen"), getList(FlagType.FilePathDirty));
            //includy

            ReportingFunctions.Add(getQualifiedName("dbplus_sql"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_blob_size"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_change_user"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_clob_size"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_connect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_create_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_database_password"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_database"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_db_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_db_status"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_drop_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_hostname"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_list_fields"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_list_tables"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_password"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_pconnect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_select_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_set_password"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_start_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_stop_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("fbsql_username"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_connect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_create_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_db_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_drop_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_list_fields"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_list_tables"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_pconnect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_select_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mssql_bind"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mssql_connect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mssql_guid_string"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mssql_init"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mssql_pconnect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mssql_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mssql_select_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_connect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_create_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_db_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_drop_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_fetch_object"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_list_fields"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_list_tables"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_pconnect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_select_db"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_set_charset"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysql_unbuffered_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_master_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_slave_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqlnd_memcache_set"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqlnd_ms_match_wild"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqlnd_ms_query_is_select"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqlnd_ms_set_user_pick_server"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqlnd_qc_set_is_select"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqlnd_qc_set_storage_handler"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqlnd_qc_set_user_handlers"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_execute"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_stmt_execute"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("session_pgsql_add_error"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("session_pgsql_set_field"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sql_regcase"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_array_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_create_aggregate"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_create_function"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_exec"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_factory"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_fetch_column_types"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_fetch_object"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_open"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_popen"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_single_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_udf_decode_binary"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_udf_encode_binary"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlite_unbuffered_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlsrv_configure"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlsrv_connect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlsrv_fetch_object"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlsrv_get_config"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlsrv_prepare"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("sqlsrv_query"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_connect"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_stmt_bind_param"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_stmt_send_long_data"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_createdb"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql_regcase"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("msql"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_bind_param"), getList(FlagType.SQLDirty));
            ReportingFunctions.Add(getQualifiedName("mysqli_send_long_data"), getList(FlagType.SQLDirty));

        }

        #endregion

        /// <summary>
        /// Return singleton instance. If instance was not created yet, it call constructor.
        /// </summary>
        /// <returns>Singleton instance</returns>
        static public NativeFunctionAnalyzer CreateInstance()
        {
            if (instance != null)
            {
                return instance;
            }
            instance = new NativeFunctionAnalyzer();
            return instance;
        }

        /// <summary>
        /// Indicteas if native functino exist
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>True if function exist, false otherwise</returns>
        public bool existNativeFunction(QualifiedName name)
        {
            return typeModeledFunctions.ContainsKey(name);
        }

        /// <summary>
        /// Return array of function names
        /// </summary>
        /// <returns>array of function names</returns>
        public QualifiedName[] getNativeFunctions()
        {
            return typeModeledFunctions.Keys.ToArray();
        }

        
        /// <summary>
        /// Return signleton instance of NativeAnalyzerMethod
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>delegate which models this function</returns>
        public NativeAnalyzerMethod GetInstance(QualifiedName name)
        {
          
            if (!existNativeFunction(name))
            {
                return null;
            }

            if (specialFunctions.Keys.Contains(name))
            {
                return specialFunctions[name];
            }

            if (concreteFunctions.Keys.Contains(name))
            {
                return concreteFunctions[name];
            }

            // default: model function by type
            InitTypeModeledFunction(name);
            return typeModeledFunctions[name][0].Analyzer;
        }

        /// <summary>
        /// Inits analyzer for given function
        /// </summary>
        /// <param name="name">Function name</param>
        private void InitTypeModeledFunction(QualifiedName name)
        {
            if (typeModeledFunctions[name][0].Analyzer == null)
            {
                TypeModeledFunctionAnalyzerHelper analyzer = new TypeModeledFunctionAnalyzerHelper(typeModeledFunctions[name]);
                typeModeledFunctions[name][0].Analyzer = new NativeAnalyzerMethod(analyzer.analyze);
            }
        }
    }

    /// <summary>
    /// Abstract class, which provides implemetation of native functions
    /// </summary>
    abstract class NativeFunctionAnalyzerHelper
    {
        /// <summary>
        /// Information about native function with all "overloads"
        /// </summary>
        protected List<NativeFunction> nativeFunctions;

        /// <summary>
        /// Creates new instance of NativeFunctionAnalyzerHelper
        /// </summary>
        /// <param name="nativeFunctions">native functions</param>
        public NativeFunctionAnalyzerHelper(List<NativeFunction> nativeFunctions)
        {
            this.nativeFunctions = nativeFunctions;
        }

        /// <summary>
        /// Computes return values of function
        /// </summary>
        /// <param name="flow">FlowController</param>
        /// <param name="arguments">Function arguments</param>
        /// <returns>computed values</returns>
        protected abstract List<Value> ComputeResult(FlowController flow, List<MemoryEntry> arguments);


        /// <summary>
        /// Models given function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void analyze(FlowController flow)
        {
            // Check arguments
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                // TODO: what to do if some argument value does not match (it is not possible to call the function with value of such type)?
                // a) Remove this value?
                // b) If we keep the value there is a problem what to do if the functon is evaluated concretely using this value
                //      We can perform this check again before evaluationg the function and detect that the function should not be evaluated
                //          This is not good - dupolicates the check
                //      We can try to convert this value to the type supported by concrete function. If the conversion is not possible, do not call the function.
                //          Not very good - gives weird semantics.
                // Overall, a) is better choice, but it is not done yet.
                NativeAnalyzerUtils.checkArgumentTypes(flow, nativeFunctions);
            }

            // Get function arguments
            MemoryEntry argc = flow.InSet.ReadVariable(new VariableIdentifier(".argument_count")).ReadMemory(flow.OutSet.Snapshot);
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;
            List<MemoryEntry> arguments = new List<MemoryEntry>();
            List<Value> argumentValues = new List<Value>();
            for (int i = 0; i < argumentCount; i++)
            {
                arguments.Add(flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(i)).ReadMemory(flow.OutSet.Snapshot));
                argumentValues.AddRange(arguments.Last().PossibleValues);
            }

            // Compute result
            MemoryEntry functionResult = new MemoryEntry(ComputeResult(flow, arguments).ToArray());
            functionResult = new MemoryEntry(FlagsHandler.CopyFlags(argumentValues, functionResult.PossibleValues));
            NativeFunctionAnalyzer analyzer = NativeFunctionAnalyzer.CreateInstance();
            if (analyzer.SanitizingFunctions.ContainsKey(nativeFunctions[0].Name))
            {
                List<Value> values = new List<Value>(functionResult.PossibleValues);
                foreach (var flag in analyzer.SanitizingFunctions[nativeFunctions[0].Name])
                {
                    values = new List<Value>(FlagsHandler.Clean(values, flag));
                }

                functionResult = new MemoryEntry(values);
            }
            if (analyzer.ReportingFunctions.ContainsKey(nativeFunctions[0].Name))
            {
                foreach (FlagType type in analyzer.ReportingFunctions[nativeFunctions[0].Name])
                {
                    if (FlagsHandler.GetFlags(argumentValues).isDirty(type))
                    {
                        AnalysisWarningHandler.SetWarning(flow.OutSet, new AnalysisSecurityWarning(NativeAnalyzerUtils.GetCallerScript(flow.OutSet), flow.CurrentPartial, flow.CurrentProgramPoint, type));
                        break;
                    }
                }
            }
            flow.OutSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(flow.OutSet.Snapshot, functionResult);
            List<Value> assigned_aliases = NativeAnalyzerUtils.ResolveAliasArguments(flow, argumentValues, nativeFunctions);

        }

        /// <summary>
        /// Computes return values of function
        /// </summary>
        /// <param name="flow">FlowController</param>
        /// <param name="arguments">Method arguments</param>
        /// <returns>computed list of values</returns>
        protected List<Value> ComputeResultType(FlowController flow, List<MemoryEntry> arguments)
        {
            var argumentCount = arguments.Count;
            var possibleValues = new List<Value>();
            foreach (var nativeFunction in nativeFunctions)
            {
                if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
                {
                    foreach (var value in NativeAnalyzerUtils.ResolveReturnValue(nativeFunction.ReturnType, flow).PossibleValues)
                    {
                        possibleValues.Add(value);
                    }
                }
            }

            if (possibleValues.Count == 0)
            {
                foreach (var nativeFunction in nativeFunctions)
                {
                    foreach (var value in NativeAnalyzerUtils.ResolveReturnValue(nativeFunction.ReturnType, flow).PossibleValues)
                    {
                        possibleValues.Add(value);
                    }
                }
            }

            return possibleValues;
        }
    }

    /// <summary>
    /// Delegate used for concrete function implemetations
    /// </summary>
    /// <param name="flow">FlowController</param>
    /// <param name="arguments">Value</param>
    /// <returns>computed value</returns>
    delegate Value ConcreteFunctionDelegate(FlowController flow, Value[] arguments);

    /// <summary>
    /// Hepler used for concrete functions implementations or for functions implemented by Phalanger
    /// </summary>
    class ConcreteFunctionAnalyzerHelper : NativeFunctionAnalyzerHelper
    {
        private bool containsAbstractValue;
        private ConcreteFunctionDelegate concreteFunction;

        /// <summary>
        /// Create new instance of ConcreteFunctionAnalyzerHelper
        /// </summary>
        /// <param name="nativeFunctions">List of native functions</param>
        /// <param name="concreteFunction">Delegate of function implementation</param>
        public ConcreteFunctionAnalyzerHelper(List<NativeFunction> nativeFunctions, ConcreteFunctionDelegate concreteFunction)
            : base(nativeFunctions)
        {
            this.concreteFunction = concreteFunction;
        }

        /// <inheritdoc />
        protected override List<Value> ComputeResult(FlowController flow, List<MemoryEntry> arguments)
        {
            var result = new List<Value>();
            containsAbstractValue = false;

            combination(flow, new List<Value>(), arguments, 0, result);

            if (containsAbstractValue)
            {
                result.AddRange(ComputeResultType(flow, arguments));
            }

            return result;
        }

        private void combination(FlowController flow, List<Value> argsValues, List<MemoryEntry> args, int pos, List<Value> result)
        {
            if (pos >= args.Count)
            {
                result.Add(concreteFunction(flow, argsValues.ToArray()));
                return;
            }

            foreach (var argValue in args.ElementAt(pos).PossibleValues)
            {
                if (argValue is AnyValue)
                {
                    containsAbstractValue = true;
                    continue;
                }

                var newArgsValues = new List<Value>(argsValues);
                newArgsValues.Add(argValue);
                combination(flow, newArgsValues, args, pos + 1, result);
            }
            return;
        }
    }

    /// <summary>
    /// Helper for type modeling of native functions
    /// </summary>
    class TypeModeledFunctionAnalyzerHelper : NativeFunctionAnalyzerHelper
    {
        /// <summary>
        /// Create new instance of TypeModeledFunctionAnalyzerHelper 
        /// </summary>
        /// <param name="nativeFunctions"></param>
        public TypeModeledFunctionAnalyzerHelper(List<NativeFunction> nativeFunctions) : base(nativeFunctions) { }

        /// <inheritdoc />
        protected override List<Value> ComputeResult(FlowController flow, List<MemoryEntry> arguments)
        {
            return ComputeResultType(flow, arguments);
        }


    }

    /// <summary>
    /// Helper for special implementations of functions
    /// </summary>
    class SpecialFunctionsImplementations
    {
        /// <summary>
        /// Information about function
        /// </summary>
        private List<NativeFunction> nativeFunctions;

        /// <summary>
        /// Creates new instance of SpecialFunctionsImplementations
        /// </summary>
        /// <param name="nativeFunctions"></param>
        public SpecialFunctionsImplementations(List<NativeFunction> nativeFunctions)
        {
            this.nativeFunctions = nativeFunctions;
        }

        #region implementation of native php functions

        /// <summary>
        /// Implementation of define function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _define(FlowController flow)
        {

            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                NativeAnalyzerUtils.checkArgumentTypes(flow, nativeFunctions);
            }

            MemoryEntry argc = flow.InSet.ReadVariable(new VariableIdentifier(".argument_count")).ReadMemory(flow.OutSet.Snapshot);
            int argumentCount = ((IntegerValue)argc.PossibleValues.ElementAt(0)).Value;

            var nativeFunction = nativeFunctions.ElementAt(0);
            List<Value> possibleValues = new List<Value>();
            bool canBeTrue = false;
            bool canBeFalse = false;
            if (nativeFunction.MinArgumentCount <= argumentCount && nativeFunction.MaxArgumentCount >= argumentCount)
            {
                bool canBeCaseSensitive = false, canBeCaseInsensitive = false;
                if (argumentCount == 2)
                {
                    canBeCaseSensitive = true;
                }
                else
                {
                    foreach (var arg2 in flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(2)).ReadMemory(flow.OutSet.Snapshot).PossibleValues)
                    {
                        var booleanConverter = new BooleanConverter(flow.OutSet.Snapshot);
                        var result = booleanConverter.EvaluateToBoolean(arg2);

                        if (result != null)
                        {
                            if (result.Value)
                            {
                                canBeCaseInsensitive = true;
                            }
                            else
                            {
                                canBeCaseSensitive = true;
                            }
                        }
                        else
                        {
                            canBeCaseSensitive = true;
                            canBeCaseInsensitive = true;
                        }
                    }
                }

                var stringConverter = new StringConverter(flow);
                bool isAllwayConcrete=false;
                IEnumerable<StringValue> arg0Strings = stringConverter.Evaluate(flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(0)).ReadMemory(flow.OutSet.Snapshot), out isAllwayConcrete);

                foreach (StringValue arg0 in arg0Strings)
                {                  
                    QualifiedName qConstantName = new QualifiedName(new Name(arg0.Value));
                    List<Value> result = new List<Value>();
                    foreach (var arg1 in flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(1)).ReadMemory(flow.OutSet.Snapshot).PossibleValues)
                    {
                        if (ValueTypeResolver.IsArray(arg1) || ValueTypeResolver.IsObject(arg1))
                        {
                            canBeFalse = true;
                        }
                        else
                        {
                            result.Add(arg1);
                            canBeTrue = true;
                        }
                    }
                    if (canBeCaseSensitive)
                    {
                        UserDefinedConstantHandler.insertConstant(flow.OutSet, qConstantName, new MemoryEntry(result.ToArray()), false);
                    }
                    if (canBeCaseInsensitive)
                    {
                        UserDefinedConstantHandler.insertConstant(flow.OutSet, qConstantName, new MemoryEntry(result.ToArray()), true);
                    }
                }
                if (canBeTrue)
                {
                    possibleValues.Add(flow.OutSet.CreateBool(true));
                }
                if (canBeFalse)
                {
                    possibleValues.Add(flow.OutSet.CreateBool(false));
                }
            }
            else
            {
                possibleValues.Add(flow.OutSet.CreateBool(false));
            }
            flow.OutSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(possibleValues));
        }

        /// <summary>
        /// Implementation of constant function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _constant(FlowController flow)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                var stringConverter = new StringConverter(flow);
                bool isAllwaysConcrete=false;
                IEnumerable<StringValue> arg0strings=stringConverter.Evaluate(flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(0)).ReadMemory(flow.OutSet.Snapshot),out isAllwaysConcrete);

                foreach (StringValue arg0 in arg0strings)
                {
                    List<Value> values = new List<Value>();
                    NativeConstantAnalyzer constantAnalyzer = NativeConstantAnalyzer.Create(flow.OutSet);
                    QualifiedName name = new QualifiedName(new Name(arg0.Value));

                    if (constantAnalyzer.ExistContant(name))
                    {
                        values.Add(constantAnalyzer.GetConstantValue(name));
                    }
                    else
                    {
                        values = UserDefinedConstantHandler.GetConstant(flow.OutSet, name).PossibleValues.ToList();
                    }
                    flow.OutSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(values));
                }
            }
            else
            {
                flow.OutSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(flow.OutSet.UndefinedValue));
            }
        }

        /// <summary>
        /// Delegate for implemetation of is_"something" native functions
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        delegate bool Typedelegate(Value value);

        /// <summary>
        /// Implementation of is_array function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_array(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsArray(value);
            }));
        }

        /// <summary>
        /// Implementation of is_bool function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_bool(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsBool(value);
            }));
        }

        /// <summary>
        /// Implementation of is_double function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_double(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsFloat(value);
            }));
        }

        /// <summary>
        /// Implementation of is_int function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_int(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsInt(value) || ValueTypeResolver.IsLong(value);
            }));
        }

        /// <summary>
        /// Implementation of is_null function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_null(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return (value is UndefinedValue);
            }));
        }

        /// <summary>
        /// Implementation of is_numeric function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_numeric(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsInt(value) || ValueTypeResolver.IsLong(value) || ValueTypeResolver.IsFloat(value);
            }));
        }

        /// <summary>
        /// Implementation of is_object function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_object(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsObject(value);
            }));
        }

        /// <summary>
        /// Implementation of is_resource function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_resource(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return (value is AnyResourceValue);
            }));
        }

        /// <summary>
        /// Implementation of is_scalar function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_scalar(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsInt(value) || ValueTypeResolver.IsLong(value) || ValueTypeResolver.IsFloat(value) || ValueTypeResolver.IsBool(value) || ValueTypeResolver.IsString(value); ;
            }));
        }

        /// <summary>
        /// Implementation of is_string function
        /// </summary>
        /// <param name="flow">FlowController</param>
        public void _is_string(FlowController flow)
        {
            processIsFunctions(flow, new Typedelegate(value =>
            {
                return ValueTypeResolver.IsString(value);
            }));
        }

        /// <summary>
        /// Implementation method for all is_something functions
        /// </summary>
        /// <param name="flow">FlowController</param>
        /// <param name="del">Typedelegate</param>
        private void processIsFunctions(FlowController flow, Typedelegate del)
        {
            if (NativeAnalyzerUtils.checkArgumentsCount(flow, nativeFunctions))
            {
                bool canBeTrue = false;
                bool canBeFalse = false;
                foreach (var arg0 in flow.OutSet.ReadVariable(NativeAnalyzerUtils.Argument(0)).ReadMemory(flow.OutSet.Snapshot).PossibleValues)
                {
                    if (del(arg0))
                    {
                        canBeTrue = true;
                    }
                    else if (arg0 is AnyValue)
                    {
                        canBeTrue = true;
                        canBeFalse = true;
                        break;
                    }
                    else
                    {
                        canBeFalse = true;
                    }
                }
                List<Value> result = new List<Value>();
                if (canBeTrue)
                {
                    result.Add(flow.OutSet.CreateBool(true));
                }
                if (canBeFalse)
                {
                    result.Add(flow.OutSet.CreateBool(false));
                }

                flow.OutSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(result));
            }
            else
            {
                flow.OutSet.GetLocalControlVariable(SnapshotBase.ReturnValue).WriteMemory(flow.OutSet.Snapshot, new MemoryEntry(flow.OutSet.AnyBooleanValue));
            }
        }

        #endregion
    }

}
