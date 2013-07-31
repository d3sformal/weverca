using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;
using PHP.Core.Reflection;

namespace Weverca.Parsers
{
    /// <summary>
    /// Wraps Phalanger syntax parser into one class and provides attributes necessary for the project.
    /// </summary>
    public class SyntaxParser : IReductionsSink, ICommentsSink, IDisposable
    {
        /// <summary>
        /// PHP script compilation unit including advanced constructs (inclusions, global code, etc.)
        /// </summary>
        private readonly ScriptCompilationUnit compilationUnit = new ScriptCompilationUnit();

        /// <summary>
        /// Source code unit emulating a file
        /// </summary>
        private VirtualSourceFileUnit sourceUnit;

        /// <summary>
        /// Text error output of the parser
        /// </summary>
        private StringWriter output;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxParser" /> class.
        /// </summary>
        /// <param name="sourceFile">PHP source file representation</param>
        /// <param name="code">Source code of PHP script</param>
        public SyntaxParser(PhpSourceFile/*!*/ sourceFile, string/*!*/ code)
        {
            sourceUnit = new VirtualSourceFileUnit(compilationUnit, code, sourceFile, Encoding.Default);
            compilationUnit.SourceUnit = sourceUnit;

            // Assembly of the application is used instead of non-existing PHP script assembly.
            // To compile PHP, follow the basic technique of Phalanger PHP compilation.
            // <seealso cref="ScriptContext.CurrentContext" />
            // <seealso cref="ApplicationContext.Default" />
            // <seealso cref="ApplicationContext.AssemblyLoader" />
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var scriptAssembly = new BasicScriptAssembly(assembly, assemblyName, compilationUnit);

            // TODO: It simulates command compilationUnit.module = scriptAssembly.Module;
            // It affects compilationUnit.ScriptModule and compilationUnit.ScriptBuilder too
            var type = compilationUnit.GetType();
            var field = type.GetField("module", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(compilationUnit, scriptAssembly.GetModule());

            IsParsed = false;
            output = new StringWriter(assemblyName.CultureInfo);
            Errors = new ErrorSinkThrowingException();
        }

        /// <summary>
        /// Gets PHP source file
        /// </summary>
        public PhpSourceFile SourceFile
        {
            get { return sourceUnit.SourceFile; }
        }

        /// <summary>
        /// Gets source code of PHP script
        /// </summary>
        public string Code
        {
            get { return sourceUnit.Code; }
        }

        /// <summary>
        /// Gets root of abstract syntax tree representing global code
        /// </summary>
        public GlobalCode Ast
        {
            get { return sourceUnit.Ast; }
        }

        /// <summary>
        /// Gets dictionary of all function declarations
        /// </summary>
        public Dictionary<QualifiedName, ScopedDeclaration<DRoutine>> Functions
        {
            get
            {
                var functions = new Dictionary<QualifiedName, ScopedDeclaration<DRoutine>>();
                var unit = compilationUnit.GetVisibleFunctions();
                foreach (var value in unit)
                {
                    functions.Add(value.Key, value.Value);
                }

                return functions;
            }
        }

        /// <summary>
        /// Gets dictionary of all type declarations
        /// </summary>
        public Dictionary<QualifiedName, PhpType> Types
        {
            get
            {
                var types = new Dictionary<QualifiedName, PhpType>();
                var unit = compilationUnit.GetDeclaredTypes();
                foreach (var value in unit)
                {
                    types.Add(value.QualifiedName, value);
                }

                return types;
            }
        }

        /// <summary>
        /// Gets dictionary of all constant declarations
        /// </summary>
        public Dictionary<QualifiedName, ScopedDeclaration<DConstant>> Constants
        {
            get
            {
                var constants = new Dictionary<QualifiedName, ScopedDeclaration<DConstant>>();
                var unit = compilationUnit.GetVisibleConstants();
                foreach (var value in unit)
                {
                    constants.Add(value.Key, value.Value);
                }

                return constants;
            }
        }

        /// <summary>
        /// Gets list of all inclusions
        /// </summary>
        public IEnumerable<IncludingEx> Inclusions
        {
            get
            {
                return compilationUnit.InclusionExpressions;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether syntactic analysis has been performed
        /// </summary>
        public bool IsParsed { get; protected set; }

        /// <summary>
        /// Gets or sets an information about errors occurred during parsing
        /// </summary>
        public ErrorSink Errors { get; protected set; }

        /// <summary>
        /// Performs syntactic analysis of the source code and creates abstract syntax tree.
        /// </summary>
        /// <remarks>
        /// The entire process of parsing is performed by Phalanger and ability to recognize PHP structures
        /// is limited by its capabilities. The version of PHP is set to 5 by default. The main result of
        /// parsing is abstract syntax tree. In addition, it also creates additional lists with declaration
        /// of types, functions, etc. If parsing fails, details about the syntax error are stored.
        /// </remarks>
        public void Parse()
        {
            // Parser context is unvarying, no need to parse PHP source code multiple times.
            if (!IsParsed)
            {
                // TODO: Emit log messages about result of parsing
                sourceUnit.Parse(Errors, this, Position.Initial, LanguageFeatures.Php5);
                IsParsed = true;
            }
        }

        #region IReductionSink Members

        /// <summary>
        /// Inspects a inclusion statement even during parsing
        /// </summary>
        /// <param name="parser">Parser currently analyzing the source code</param>
        /// <param name="decl">Current inclusion</param>
        public void InclusionReduced(Parser/*!*/ parser, IncludingEx/*!*/ decl)
        {
            compilationUnit.InclusionReduced(parser, decl);
        }

        /// <summary>
        /// Inspects a function declaration even during parsing
        /// </summary>
        /// <param name="parser">Parser currently analyzing the source code</param>
        /// <param name="decl">Current function declaration</param>
        public void FunctionDeclarationReduced(Parser/*!*/ parser, FunctionDecl/*!*/ decl)
        {
            compilationUnit.FunctionDeclarationReduced(parser, decl);
        }

        /// <summary>
        /// Inspects a type declaration even during parsing
        /// </summary>
        /// <param name="parser">Parser currently analyzing the source code</param>
        /// <param name="decl">Current type declaration</param>
        public void TypeDeclarationReduced(Parser/*!*/ parser, TypeDecl/*!*/ decl)
        {
            compilationUnit.TypeDeclarationReduced(parser, decl);
        }

        /// <summary>
        /// Inspects a global constant declaration even during parsing
        /// </summary>
        /// <param name="parser">Parser currently analyzing the source code</param>
        /// <param name="decl">Current global constant declaration</param>
        public void GlobalConstantDeclarationReduced(Parser/*!*/ parser, GlobalConstantDecl/*!*/ decl)
        {
            compilationUnit.GlobalConstantDeclarationReduced(parser, decl);
        }

        #endregion

        // TODO: Handlers called when comment occurs. How to incorporate it to AST? At the end of parsing?
        #region ICommentSink Members

        /// <summary>
        /// Processes an event that the scanner recognizes a line comment
        /// </summary>
        /// <remarks>
        /// The line comment is everything on one line which begins "//" or "#" and continues to the right
        /// </remarks>
        /// <param name="scanner">Scanner currently processing the source code</param>
        /// <param name="position">Position of the line comment in source code</param>
        public void OnLineComment(Scanner/*!*/ scanner, Position position)
        {
        }

        /// <summary>
        /// Processes an event that the scanner recognizes a multiline comment
        /// </summary>
        /// <remarks>
        /// The multiline comment is everything which begins with "/*" and ends with "*/"
        /// </remarks>
        /// <param name="scanner">Scanner currently processing the source code</param>
        /// <param name="position">Position of the comment in source code</param>
        public void OnComment(Scanner/*!*/ scanner, Position position)
        {
        }

        /// <summary>
        /// Processes an event that the scanner recognizes a structuralized PHPDoc DocBlock
        /// </summary>
        /// <remarks>
        /// PHPDoc is formal standard for commenting PHP code. Basic comment block, DocBlock, is structured
        /// multiline comment that begins with "/**" and has an "*" at the beginning of every line
        /// </remarks>
        /// <param name="scanner">Scanner currently processing the source code</param>
        /// <param name="phpDocBlock">PHPDoc DocBlock</param>
        public void OnPhpDocComment(Scanner/*!*/ scanner, PHPDocBlock phpDocBlock)
        {
        }

        /// <summary>
        /// Processes an event that the scanner recognizes a opening PHP tag
        /// </summary>
        /// <remarks>
        /// PHP source code begins after the opening tag "&lt;?php"
        /// </remarks>
        /// <param name="scanner">Scanner currently processing the source code</param>
        /// <param name="position">Position of the opening tag in source code</param>
        public void OnOpenTag(Scanner/*!*/ scanner, Position position)
        {
        }

        /// <summary>
        /// Processes an event that the scanner recognizes a closing PHP tag
        /// </summary>
        /// <remarks>
        /// PHP source code ends before the closing tag "?&gt;"
        /// </remarks>
        /// <param name="scanner">Scanner currently processing the source code</param>
        /// <param name="position">Position of the closing tag in source code</param>
        public void OnCloseTag(Scanner/*!*/ scanner, Position position)
        {
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all disposable object
        /// </summary>
        /// <param name="disposing">
        /// Indicating whether the method was invoked from <see cref="IDisposable.Dispose" />
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (output != null)
                {
                    output.Dispose();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// ErrorSink implementation. Throws exception at every parser error and doesnt allow parser to continue.
    /// </summary>
    public class ErrorSinkThrowingException : ErrorSink
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id of error</param>
        /// <param name="message">Error message</param>
        /// <param name="severity">Error severity</param>
        /// <param name="group"></param>
        /// <param name="fullPath">Full path of parser file</param>
        /// <param name="pos">Error position</param>
        /// <returns>Doesn't return anything ends with excpetion</returns>
        protected override bool Add(int id, string message, ErrorSeverity severity, int group, string fullPath, ErrorPosition pos)
        {
            //for now every parser error ends with excption. I am not really sure if there are errors or parser warning s that doenst have to end with excpetion
            throw new ParserException(String.Format("Parser {4}: {0} at line {1}, char {2}: {3}",fullPath,pos.FirstLine,pos.FirstColumn,message,severity.Value),severity,group,fullPath,pos);
        }
    }

    public class ParserException : Exception
    {
        public string FullPath { get; protected set; }
        public ErrorSeverity Severity { get; protected set; }
        public int Group { get; protected set; }
        public ErrorPosition Position { get; protected set; }
        public ParserException(string message, ErrorSeverity severity, int group, string path, ErrorPosition pos)
            : base(message)
        {
            FullPath = path;
            Severity = severity;
            Group = group;
            Position = pos;
        }
    
    }
}
