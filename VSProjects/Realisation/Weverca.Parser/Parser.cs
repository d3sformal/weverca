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
  
    public class ErrorSinkImpl : ErrorSink
    {
        protected override bool Add(int id, string message, ErrorSeverity severity, int group, string fullPath, ErrorPosition pos) {
            Console.WriteLine("Line: " + pos.FirstLine + ":\n" + message+" in "+fullPath);
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
        private ScriptCompilationUnit compilationUnit;
        private SourceUnit sourceUnit;
        private ErrorSink errors;

        public GlobalCode Ast { get { return sourceUnit.Ast; } }
        public Dictionary<QualifiedName, Declaration> Functions { get { return (Dictionary<QualifiedName, Declaration>)compilationUnit.GetVisibleFunctions(); } }
        public Dictionary<QualifiedName, Declaration> Types { get { return (Dictionary<QualifiedName, Declaration>)compilationUnit.GetVisibleTypes(); } }
        public Dictionary<QualifiedName, Declaration> Constants { get { return (Dictionary<QualifiedName, Declaration>)compilationUnit.GetVisibleConstants(); } }
        public bool IsParsed { protected set; get; }

        public SyntaxParser(PhpSourceFile source_file, string code)
        {
            // TODO: Complete member initialization
            this.source_file = source_file;
            this.code = code;
            this.errors = new ErrorSinkImpl();
            this.compilationUnit = new ScriptCompilationUnit();
            this.sourceUnit = new PHP.Core.Reflection.VirtualSourceFileUnit(compilationUnit, code, source_file, Encoding.Default);
            compilationUnit.SourceUnit = sourceUnit;
            this.IsParsed=false;
                      
        }

        public void Parse(){
            compilationUnit.ParseSourceFile(sourceUnit, errors, LanguageFeatures.Php5);
            this.IsParsed=true;
        }
    }
}
