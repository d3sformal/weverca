using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using PHP.Core;

using Weverca.Analysis;
using Weverca.TaintedAnalysis;

using Weverca.Parsers;
using Weverca.ControlFlowGraph;



namespace Weverca
{
    static class Analyzer
    {
        internal static ProgramPointGraph Run(string entryFile)
        {
            var cfg = GenerateCFG(entryFile);
            return Run(cfg);
        }

        internal static ProgramPointGraph Run(ControlFlowGraph.ControlFlowGraph entryMethod)
        {
            var analysis = new ForwardAnalysis(entryMethod);

            analysis.Analyse();

            return analysis.ProgramPointGraph;
        }

        /// <summary>
        /// Using the phalanger parser generates ControlFlowGraph for a given file.
        /// </summary>
        /// <param name="fileName">Name of the file with php source.</param>
        /// <returns>Created ControlFlowGraph</returns>
        internal static ControlFlowGraph.ControlFlowGraph GenerateCFG(string fileName)
        {
            string code;
            using (StreamReader reader = new StreamReader(fileName))
            {
                code = reader.ReadToEnd();
            }

            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            var parser = new SyntaxParser(source_file, code);
            parser.Parse();


            return new Weverca.ControlFlowGraph.ControlFlowGraph(parser.Ast);
        }
    }
}
