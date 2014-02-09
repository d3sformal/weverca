namespace Weverca.CodeMetrics
{
    /// <summary>
    /// Enumeration of metrics that indicates construct presence in source code.
    /// </summary>
    public enum ConstructIndicator
    {
        /// <summary>
        /// Indicator of __autoload redeclaration presence or spl_autoload_register call. The function
        /// tries to load currently used type that has not been declared yet. It makes easier the situation
        /// when code uses many classes outside the file. It usually finds the correct file with the name
        /// depending on name of the class and includes it. Function spl_autoload_register extends autoload
        /// mechanism by registering a custom function.
        /// </summary>
        /// <seealso cref="MetricRelatedFunctions.autoloadRegister" />
        /// <seealso href="http://www.php.net/manual/en/language.oop5.autoload.php" />
        Autoload,

        /// <summary>
        /// Indicator of any magic methods presence. All these method starts with double underscore
        /// (e.g. __invoke()) and have special significance. They are called not as ordinary methods,
        /// but through some language construct. If the method is not declared, PHP either reports
        /// an error (e.g. __toString) or performs default operation (e.g. __construct).
        /// </summary>
        /// <seealso href="http://www.php.net/manual/en/language.oop5.magic.php" />
        MagicMethods,

        /// <summary>
        /// Indicator of class or interface construct presence. This is mechanism to create a custom
        /// user-defined type. Classes and interfaces serves as basic building block of object-oriented
        /// programming that was added to PHP 5 as the main new feature.
        /// </summary>
        /// <seealso href="http://www.php.net/manual/en/language.oop5.php" />
        ClassOrInterface,

        /// <summary>
        /// Indicator of include based on dynamic variable. Include statement embeds a file, which is passed
        /// as a parameter, to the source code. Included file can essentially change the context of running
        /// program. If the parameter of the statement is an expression whose evaluation depends on
        /// a variable or method return value, it is very hard to predict program's behavior.
        /// </summary>
        /// <seealso href="http://www.php.net/manual/en/function.include.php" />
        DynamicInclude,

        /// <summary>
        /// Indicator of references presence. References (or aliases) in PHP are used for access the same
        /// memory content by different names. A change made to one variable will reflect also to others.
        /// They can also be passed as parameter to or return from method. A variable used once as
        /// reference cannot be changed to other type other than by unsetting. References generally make
        /// any code analysis difficult.
        /// </summary>
        /// <seealso href="http://www.php.net/manual/en/language.references.php" />
        References,

        /// <summary>
        /// Indicator of session function and $_SESSION usage presence.
        /// <seealso href="http://www.php.net/manual/en/book.session.php" />
        /// </summary>
        Session,

        /// <summary>
        /// Indicator of declaration of class/function inside another function presence.
        /// </summary>
        InsideFunctionDeclaration,

        /// <summary>
        /// Indicator of super global variable usage. The super global variables represents kind of input
        /// that can make trouble in the analysis. Their content is not known when the program starts
        /// and it can contain virtually every value
        /// <seealso href="http://www.php.net/manual/en/language.variables.superglobals.php" />
        /// </summary>
        SuperGlobalVariable,

        /// <summary>
        /// Indicator of eval function usage. Eval function includes another program in a string.
        /// Constant string can be evaluated, but evaluation of dynamically created strings can be
        /// very difficult or almost impossible.
        /// <seealso href="http://www.php.net/manual/en/function.eval.php" />
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
        /// </summary>
        /// <example>MyFunction(parameter1, &amp;parameter2);.</example>
        PassingByReferenceAtCallSide,

        /// <summary>
        /// Indicator of MySQL functions presence.
        /// </summary>
        /// <seealso href="http://www.php.net/manual/en/book.mysql.php" />
        MySql,

        /// <summary>
        /// Indicator of class alias construction.
        /// </summary>
        /// <seealso href="http://php.net/manual/en/function.class-alias.php" />
        ClassAlias,
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
        /// Rating of class coupling. It is a measure of how many user-defined classes a single class uses,
        /// for all classes on average. The higher quotient is, the more design suffer. Highly coupled code
        /// indicates problems to reuse and maintain because of the many dependencies between each other.
        /// </summary>
        ClassCoupling,

        /// <summary>
        /// Rating of php standard functions coupling.
        /// </summary>
        PhpFunctionsCoupling,
    }

    /// <summary>
    /// Enumeration of metrics that has quantitative value (unsigned integer range).
    /// </summary>
    public enum Quantity
    {
        /// <summary>
        /// Maximal depth of inheritance (via extends) detected.
        /// </summary>
        MaxInheritanceDepth,

        /// <summary>
        /// Total number of source lines. It can tell an information about source code size.
        /// </summary>
        NumberOfLines,

        /// <summary>
        /// Total number of sources. It simply counts the number of PHP source code files.
        /// </summary>
        NumberOfSources,

        /// <summary>
        /// The maximal depth of method overriding.
        /// </summary>
        MaxMethodOverridingDepth,
    }
}
