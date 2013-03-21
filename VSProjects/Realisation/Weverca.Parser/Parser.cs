using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHP.Core.AST;
using PHP.Core.Reflection;
using PHP.Core;
using PHP.Core.Parsers;

namespace Weverca.Parsers
{
   public class CompilationUnit : CompilationUnitBase, IReductionsSink
    {
        public sealed class PureAssembly : PhpAssembly
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


        public sealed class PureModule : PhpModule
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
        public override bool IsPure { get { return is_pure; } }
        private bool is_pure=true;
        public override bool IsTransient { get { return false; } }
        public CompilationUnit(){
        
            PureAssembly a = new PureAssembly(ApplicationContext.Default);
            this.module = a.Module = new PureModule(a);
        }
        public override  DType GetVisibleType(QualifiedName qualifiedName, ref string fullName, Scope currentScope,
            bool mustResolve) {
                throw new NotImplementedException();
        }
        public override  DRoutine GetVisibleFunction(QualifiedName qualifiedName, ref string fullName, Scope currentScope)
        {
            throw new NotImplementedException();
        }
        public override  DConstant GetVisibleConstant(QualifiedName qualifiedName, ref string fullName, Scope currentScope) {
            throw new NotImplementedException();
        }

        public override  IEnumerable<PhpType> GetDeclaredTypes()
        {
            throw new NotImplementedException();
        }
        public override  IEnumerable<PhpFunction> GetDeclaredFunctions()
        {
            throw new NotImplementedException();
        }
        public override  IEnumerable<GlobalConstant> GetDeclaredConstants()
        {
            throw new NotImplementedException();
        }


        public Dictionary<QualifiedName, Declaration> Functions;
        public Dictionary<QualifiedName, Declaration> Types;
        public Dictionary<QualifiedName, Declaration> Constants;

        public void InclusionReduced(Parser/*!*/ parser, IncludingEx/*!*/ node)
        {
            // make all inclusions dynamic:
        }

        public void FunctionDeclarationReduced(Parser/*!*/ parser, FunctionDecl/*!*/ node)
        {
            if (Functions == null) Functions = new Dictionary<QualifiedName, Declaration>();
            AddDeclaration(parser.ErrorSink, node.Function, Functions);
        }

        public void TypeDeclarationReduced(Parser/*!*/ parser, TypeDecl/*!*/ node)
        {
            if (Types == null) Types = new Dictionary<QualifiedName, Declaration>();
            AddDeclaration(parser.ErrorSink, node.Type, Types);
        }

        public void GlobalConstantDeclarationReduced(Parser/*!*/ parser, GlobalConstantDecl/*!*/ node)
        {
            if (Constants == null) Constants = new Dictionary<QualifiedName, Declaration>();
            AddDeclaration(parser.ErrorSink, (GlobalConstant)node.Constant, Constants);
        }

        private void AddDeclaration(ErrorSink/*!*/ errors, IDeclaree/*!*/ member, Dictionary<QualifiedName, Declaration>/*!*/ table)
        {
            Declaration existing;
            Declaration current = member.Declaration;

            if (table.TryGetValue(member.QualifiedName, out existing))
            {
                // partial declarations are not allowed in transient code => nothing to check;
                if (CheckDeclaration(errors, member, existing))
                    AddVersionToGroup(current, existing);
            }
            else
            {
                // add a new declaration to the table:
                table.Add(member.QualifiedName, current);
            }
        }
    }
   
    public class ErrorSinkImpl : ErrorSink
    {
        protected override bool Add(int id, string message, ErrorSeverity severity, int group, string fullPath, ErrorPosition pos) {
            Console.WriteLine(message);
            return true;
        }
    }

    /// <summary>
    /// Wraps phalanger syntax parser in one class and provides attributes neccessary for our project. If someone needs more data from praser fell free to add metods or getters and setters.
    /// </summary>
    public class SyntaxParser
    {
        private PhpSourceFile source_file;
        private string code;
        private CompilationUnit compilationUnit;
        private SourceUnit sourceUnit;
        private ErrorSink errors;

        public GlobalCode Ast { get { return sourceUnit.Ast; } }
        public Dictionary<QualifiedName, Declaration> Functions { get { return compilationUnit.Functions; } }
        public Dictionary<QualifiedName, Declaration> Types { get { return compilationUnit.Types; } }
        public Dictionary<QualifiedName, Declaration> Constants { get { return compilationUnit.Constants; } }


        public SyntaxParser(PhpSourceFile source_file, string code)
        {
            // TODO: Complete member initialization
            this.source_file = source_file;
            this.code = code;
            this.errors = new ErrorSinkImpl();
            this.compilationUnit = new CompilationUnit();
            this.sourceUnit = new PHP.Core.Reflection.VirtualSourceFileUnit(compilationUnit, code, source_file, Encoding.Default);
        }

        public void Parse(){
            sourceUnit.Parse(errors, compilationUnit, Position.Initial, LanguageFeatures.Php5);
        }
    }
}
