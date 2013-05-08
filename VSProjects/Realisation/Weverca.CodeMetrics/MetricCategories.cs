using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.CodeMetrics
{
    /// <summary>
    /// Enumeration of metrics that inidicates construct presence in sources.
    /// </summary>
    public enum ConstructIndicator
    {
        /// <summary>
        /// Indicator of __autoload redeclaration presence or spl_autoload_register call.
        /// <seealso cref="http://php.net/manual/en/language.oop5.autoload.php"/>
        /// </summary>
        Autoload,
        /// <summary>
        /// Indicator of any magic function presence.
        /// There is lot of magic functions in php. 
        /// <seealso cref="http://www.php.net/manual/en/language.oop5.magic.php"/>
        /// </summary>
        MagicMethod,
        /// <summary>
        /// Indicator of class construct presence.
        /// </summary>
        Class,
        /// <summary>
        /// Include based on dynamic variable.
        /// </summary>
        DynamicInclude,
        /// <summary>
        /// Indicator of alias presence.
        /// </summary>
        Alias,
        /// <summary>
        /// Indicator of session function usage/ $_SESSION usage presence.
        /// <seealso cref="http://php.net/manual/en/ref.session.php"/>
        /// </summary>
        Session,
        /// <summary>
        /// Indicator of declaration of class/function inside another function presence.
        /// </summary>
        InsideFunctionDeclaration,
        /// <summary>
        /// Indicator of super global variable usage.
        /// <seealso cref="http://php.net/manual/en/language.variables.superglobals.php"/>
        /// </summary>
        SuperGlobalVariable,
        /// <summary>
        /// Indicator of eval function usage.          
        /// </summary>
        Eval,
        /// <summary>
        /// Indicator of dynamic call usage.
        /// </summary>
        DynamicCall,
        /// <summary>
        /// Indicator of dynamic dereference usage.
        /// </summary>
        DynamicDereference,
        /// <summary>
        /// Indicator of duck typing usage.
        /// </summary>
        DuckTyping,
        /// <summary>
        /// Indicator of passing variable by reference at call side.
        /// EXAMPLE: my_function(&parameter)
        /// </summary>
        PassingByReferenceAtCallSide,

        /// <summary>
        /// Indicator of My SQL functions presence
        /// </summary>
        MySQL
    }

    /// <summary>
    /// Enumeration of metrics that has numeric rating (double range).
    /// </summary>
    public enum Rating
    {
        /// <summary>
        /// Rating of cyclomatic complexity.
        /// </summary>
        Cyclomacity,
        /// <summary>
        /// Rating of class coupling.
        /// </summary>
        ClassCoupling,
        /// <summary>
        /// Rating of php standard functions coupling.
        /// </summary>
        PhpFunctionsCoupling
    }

    /// <summary>
    /// Enumeration of metrics that has quantitative value (uint range)
    /// </summary>
    public enum Quantity
    {
        /// <summary>
        /// Maximal depth of inheritance (via extends) detected.
        /// </summary>
        MaxInheritanceDepth,
        /// <summary>
        /// Total number of source lines.
        /// </summary>
        NumberOfLines,
        /// <summary>
        /// Total number of sources.
        /// </summary>
        NumberOfSources,
    }
}
