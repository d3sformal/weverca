using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Emit;
using PHP.Core.Parsers;
using PHP.Core.Reflection;

namespace Weverca.Parsers
{
    public class BasicScriptAssembly : ScriptAssembly
    {
        public ScriptBuilder Builder { get; private set; }
        public ScriptModule Module { get; private set; }

        public BasicScriptAssembly(Assembly/*!*/ assembly, AssemblyName assemblyName,
            PhpSourceFile/*!*/ file, ScriptCompilationUnit/*!*/ compilationUnit)
            : base(ApplicationContext.Default, assembly)
        {
            var assemblyBuilder = new BasicScriptAssemblyBuilder(this, assemblyName, file);
            var subNamespace = ScriptModule.GetSubnamespace(
                compilationUnit.SourceUnit.SourceFile.RelativePath, true);
            Module = new ScriptModule(compilationUnit, this, subNamespace);
            Builder = new ScriptBuilder(compilationUnit, assemblyBuilder, subNamespace);
        }

        public BasicScriptAssembly(Module/*!*/ module, AssemblyName assemblyName,
            PhpSourceFile/*!*/ file, ScriptCompilationUnit/*!*/ compilationUnit)
            : this(module.Assembly, assemblyName, file, compilationUnit)
        {
        }

        #region ScriptAssembly

        public override bool IsMultiScript
        {
            get
            {
                return false;
            }
        }

        public override IEnumerable<ScriptModule> GetModules()
        {
            yield return Module;
        }

        public override PhpModule GetModule(PhpSourceFile name)
        {
            return Module;
        }

        #endregion
    }


    public class BasicScriptAssemblyBuilder : ScriptAssemblyBuilder
    {
        public BasicScriptAssemblyBuilder(BasicScriptAssembly/*!*/ assembly,
            AssemblyName assemblyName, PhpSourceFile/*!*/ file)
            : base(assembly, assemblyName, file.Directory.FullFileName, file.FullPath.FileName,
            AssemblyKinds.Library, new List<ResourceFileReference>(0), false, false, true, null)
        {
        }

        #region PhpAssemblyBuilder overrides

        public override IPhpModuleBuilder DefineModule(CompilationUnitBase/*!*/ compilationUnit)
        {
            return compilationUnit.ModuleBuilder;
        }

        #endregion

        #region ScriptAssemblyBuilder overrides

        protected override ScriptBuilder GetEntryScriptBuilder()
        {
            Debug.Assert(assembly is BasicScriptAssembly);
            var scriptAssembly = assembly as BasicScriptAssembly;
            return scriptAssembly.Builder;
        }

        #endregion
    }


    /// <summary>
    /// Wraps Phalanger syntax parser into one class and provides attributes neccessary for the project.
    /// If someone needs more data from parser, feel free to add methods, getters and setters.
    /// </summary>
    public class SyntaxParser :IReductionsSink, ICommentsSink
    {
        private PhpSourceFile sourceFile;
        private string code;
        private ScriptCompilationUnit compilationUnit = new ScriptCompilationUnit();
        private SourceUnit sourceUnit;

        public GlobalCode Ast { get { return sourceUnit.Ast; } }
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
        public IncludingEx[] Inclusions
        {
            get
            {
                return compilationUnit.InclusionExpressions.ToArray();
            }
        }
        public bool IsParsed { get; protected set; }
        public TextErrorSink Errors { get; protected set; }

        public SyntaxParser(PhpSourceFile/*!*/ sourceFile, string/*!*/ code)
        {
            this.sourceFile = sourceFile;
            this.code = code;

            sourceUnit = new PHP.Core.Reflection.VirtualSourceFileUnit(compilationUnit,
                code, sourceFile, Encoding.Default);
            compilationUnit.SourceUnit = sourceUnit;

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var scriptAssembly = new BasicScriptAssembly(assembly, assemblyName, sourceFile, compilationUnit);

            // TODO: It simulates command compilationUnit.module = scriptAssembly.Module;
            // It affects compilationUnit.ScriptModule and compilationUnit.ScriptBuilder too
            var type = compilationUnit.GetType();
            var field = type.GetField("module", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(compilationUnit, scriptAssembly.Module);

            IsParsed = false;
            Errors = new TextErrorSink(new StringWriter());
        }

        public void Parse()
        {
            sourceUnit.Parse(Errors, this, Position.Initial, LanguageFeatures.Php5);
            this.IsParsed = true;
        }


        #region forwarding IReductionSink

        public void InclusionReduced(Parser parser, IncludingEx decl)
        {
            compilationUnit.InclusionReduced(parser, decl);
        }

        public void FunctionDeclarationReduced(Parser parser, FunctionDecl decl)
        {
            compilationUnit.FunctionDeclarationReduced(parser, decl);
        }

        public void TypeDeclarationReduced(Parser parser, TypeDecl decl)
        {
            compilationUnit.TypeDeclarationReduced(parser, decl);
        }

        public void GlobalConstantDeclarationReduced(Parser parser, GlobalConstantDecl decl)
        {
            compilationUnit.GlobalConstantDeclarationReduced(parser, decl);
        }
        #endregion

        /*
         tieto funckie sa volaju pri nastaveni komentarov. odporucam ulozit niekam a na na konci parsovania nejakym spsobom zakomponovat do ast.
         */
        #region ICommentSink

        public void OnLineComment(Scanner scanner, Position position)
        {
                          
        }

        public void OnComment(Scanner scanner, Position position)
        {

        }

        public void OnPhpDocComment(Scanner scanner, PHPDocBlock phpDocBlock)
        {

        }

        public void OnOpenTag(Scanner scanner, Position position)
        {

        }

        public void OnCloseTag(Scanner scanner, Position position)
        {

        }

        #endregion
    }
    
}
