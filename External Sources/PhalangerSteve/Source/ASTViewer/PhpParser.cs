using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

using PHP.Core.Reflection;
using PHP.Core.Parsers;
using PHP.Core;
using PHP.Core.AST;

namespace PhpRefactoring.Utils
{
    /// <summary>
    /// Empty Error sink.
    /// </summary>
    public class EmptyErrorSink : ErrorSink
    {
        public EmptyErrorSink()
        {
        }

        protected override bool Add(int id, string message, ErrorSeverity severity, int group, string fullPath, ErrorPosition pos)
        {
            return true;
        }
    }

    #region nested interface: ICodeComments

    /// <summary>
    /// Gets additional information about source code, such as comments position.
    /// </summary>
    internal interface ICodeComments
    {
        /// <summary>
        /// Gets list of open/close PHP tags position.
        /// </summary>
        List<Tuple<Position, Position>> PhpOpenTags { get; }

        /// <summary>
        /// Gets list of PHPDoc blocks.
        /// </summary>
        List<PHPDocBlock> PhpDocBlocks { get; }

        /// <summary>
        /// Gets list of line comments position.
        /// </summary>
        List<Position> PhpLineComments { get; }

        /// <summary>
        /// Gets list of block comments position.
        /// </summary>
        List<Position> PhpBlockComments { get; }
    }

    #endregion

    #region nested class: CustomCompilationUnit

    internal sealed class CustomCompilationUnit : CompilationUnitBase, IReductionsSink, ICommentsSink, ICodeComments
    {
        #region Nested class: PureAssembly

        private sealed class PureAssembly : PhpAssembly
        {
            public PureModule/*!*/ Module { get { return module; } internal /* friend PAB */ set { module = value; } }
            private PureModule/*!*/ module;

            #region Construction


            /// <summary>
            /// Used by the builder.
            /// </summary>
            internal PureAssembly(ApplicationContext/*!*/ applicationContext)
                : base(applicationContext)
            {
                // to be written-up
            }

            #endregion

            public override PhpModule GetModule(PhpSourceFile name)
            {
                return module;
            }

        }

        #endregion

        #region Nested class: PureModule

        private sealed class PureModule : PhpModule
        {
            #region Construction

            /// <summary>
            /// Called by the loader. The module can be loaded to <see cref="PureAssembly"/> or 
            /// <see cref="PhpLibraryAssembly"/>.
            /// </summary>
            internal PureModule(DAssembly/*!*/ assembly)
                : base(assembly)
            {
            }


            #endregion

            protected override CompilationUnitBase CreateCompilationUnit()
            {
                throw new NotImplementedException();
            }

            public override void Reflect(bool full, Dictionary<string, DTypeDesc> types, Dictionary<string, DRoutineDesc> functions, DualDictionary<string, DConstantDesc> constants)
            {
                throw new NotImplementedException();
            }

        }

        #endregion

        #region Fields & Properties

        public override bool IsPure { get { return is_pure; } }
        private bool is_pure;
        public override bool IsTransient { get { return false; } }

        /// <summary>
        /// List of positions of open/close php tags. Cannot be <c>null</c>.
        /// </summary>
        private readonly List<Tuple<Position, Position>>/*!*/phpOpenTags = new List<Tuple<Position, Position>>(1);

        /// <summary>
        /// Last PHP open tag position. Will be paired with next close tag, and stored within <see cref="phpOpenTags"/>.
        /// </summary>
        private Position openPhpTag = Position.Invalid;

        /// <summary>
        /// List of PHPDoc blocks.
        /// </summary>
        private readonly List<PHPDocBlock>/*!*/phpDocBlocks = new List<PHPDocBlock>();

        /// <summary>
        /// List of line comments position.
        /// </summary>
        private readonly List<Position>/*!*/phpLineComments = new List<Position>();

        /// <summary>
        /// List of block comments position.
        /// </summary>
        private readonly List<Position>/*!*/phpBlockComments = new List<Position>();

        #endregion

        #region Construction

        private CustomCompilationUnit(bool is_pure)
            : base()
        {
            this.is_pure = is_pure;

            PureAssembly a = new PureAssembly(ApplicationContext.Default);
            this.module = a.Module = new PureModule(a);
        }

        /// <summary>
        /// Parses given <paramref name="code"/>.
        /// </summary>
        /// <param name="fileName">File name of the <paramref name="code"/>.</param>
        /// <param name="code">Source code to parse.</param>
        /// <returns>An instance of <see cref="GlobalCode"/> or <c>null</c> reference if <paramref name="code"/> could not be parsed.</returns>
        public static GlobalCode ParseCode(string/*!*/fileName, string/*!*/code, out ICodeComments codeComments)
        {
            codeComments = null;

            if (fileName == null || code == null)
                return null;

            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));

            ErrorSink errors = new EmptyErrorSink();
            CustomCompilationUnit compilation_unit = new CustomCompilationUnit(/*project.IsPure*/false);
            VirtualSourceFileUnit source_unit = new VirtualSourceFileUnit(compilation_unit, /*!*/code, source_file, Encoding.Default)
            {
                AllowGlobalCode = !compilation_unit.IsPure
            };

            // perform parsing
            var success = compilation_unit.ParseSourceFiles(
                        source_unit,    // units to be parsed
                        errors,  // errors forwarded to VS's Error List
                /*project.LanguageFeatures*/
                        LanguageFeatures.Php5);

            //
            if (success)
                codeComments = (ICodeComments)compilation_unit;

            return source_unit.Ast;
        }

        #endregion

        #region Declarations: look-up and enumeration

        public override DRoutine GetVisibleFunction(QualifiedName qualifiedName, ref string fullName/*!*/, Scope currentScope)
        {
            throw new NotImplementedException();
        }

        public override DType GetVisibleType(QualifiedName qualifiedName, ref string fullName/*!*/, Scope currentScope,
            bool mustResolve)
        {
            throw new NotImplementedException();
        }

        public override DConstant GetVisibleConstant(QualifiedName qualifiedName, ref string fullName/*!*/, Scope currentScope)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<PhpType>/*!*/ GetDeclaredTypes()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<PhpFunction>/*!*/ GetDeclaredFunctions()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<GlobalConstant>/*!*/ GetDeclaredConstants()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Parsing

        public bool ParseSourceFiles<S>(S/*!*/ source_unit, ErrorSink/*!*/ errors,
            LanguageFeatures languageFeatures)
            where S : SourceUnit
        {
            if (source_unit == null)
                throw new ArgumentNullException("sourceFiles");
            if (errors == null)
                throw new ArgumentNullException("errors");

            //Dictionary<PhpSourceFile, S> files = new Dictionary<PhpSourceFile, S>();

            bool success = true;
            PhpSourceFile source_file = source_unit.SourceFile;

            try
            {
                source_unit.Parse(errors, this, Position.Initial, languageFeatures);
            }
            catch// (CompilerException)
            {
                //files[source_file] = null;
                success = false;
            }
            finally
            {
                // do not close opened source units now as their source might be used later by analyzer
            }

            return success;
        }

        #endregion

        #region IReductionsSink Members

        public void InclusionReduced(Parser/*!*/ parser, PHP.Core.AST.IncludingEx/*!*/ node)
        {

        }

        public void FunctionDeclarationReduced(Parser/*!*/ parser, PHP.Core.AST.FunctionDecl/*!*/ node)
        {

        }

        public void TypeDeclarationReduced(Parser/*!*/ parser, PHP.Core.AST.TypeDecl/*!*/ node)
        {

        }

        public void GlobalConstantDeclarationReduced(Parser/*!*/ parser, PHP.Core.AST.GlobalConstantDecl/*!*/ node)
        {

        }

        private void AddDeclaration(ErrorSink/*!*/ errors, IDeclaree/*!*/ member, Dictionary<QualifiedName, Declaration>/*!*/ table)
        {
            Declaration existing;
            Declaration current = member.Declaration;

            if (table.TryGetValue(member.QualifiedName, out existing))
            {
                if (CheckDeclaration(errors, member, existing))
                    AddVersionToGroup(current, existing);
            }
            else
            {
                if (current.IsConditional)
                    member.Version = new VersionInfo(1, null);

                // add a new declaration to the table:
                table.Add(member.QualifiedName, current);
            }
        }

        #endregion

        #region ICommentsSink Members

        void ICommentsSink.OnOpenTag(Scanner scanner, Position position)
        {
            this.openPhpTag = position;
        }

        void ICommentsSink.OnCloseTag(Scanner scanner, Position position)
        {
            this.phpOpenTags.Add(new Tuple<Position, Position>(this.openPhpTag, position));

#if DEBUG
            this.openPhpTag = Position.Invalid;
#endif
        }

        void ICommentsSink.OnLineComment(Scanner scanner, Position position)
        {
            this.phpLineComments.Add(position);
        }

        void ICommentsSink.OnComment(Scanner scanner, Position position)
        {
            this.phpBlockComments.Add(position);
        }

        void ICommentsSink.OnPhpDocComment(Scanner scanner, PHPDocBlock phpDocBlock)
        {
            this.phpDocBlocks.Add(phpDocBlock);
        }

        #endregion

        #region ICodeComments Members

        List<Tuple<Position, Position>> ICodeComments.PhpOpenTags
        {
            get { return this.phpOpenTags; }
        }

        List<PHPDocBlock> ICodeComments.PhpDocBlocks
        {
            get { return this.phpDocBlocks; }
        }

        List<Position> ICodeComments.PhpLineComments
        {
            get { return this.phpLineComments; }
        }

        List<Position> ICodeComments.PhpBlockComments
        {
            get { return this.phpBlockComments; }
        }

        #endregion
    }

    #endregion

}