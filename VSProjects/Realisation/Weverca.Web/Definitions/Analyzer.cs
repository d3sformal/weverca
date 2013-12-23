using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Weverca.Analysis;
using Weverca.AnalysisFramework;

namespace Weverca.Web.Definitions
{
    static class Analyzer
    {
        public static ProgramPointGraph Run(string phpCode)
        {
            string fileName = @"\UserInput\userInput.php";

            var cfg = GenerateCfg(phpCode, fileName);

            return Analyze(cfg, fileName);
        }

        static ControlFlowGraph.ControlFlowGraph GenerateCfg(string phpCode, string fileName)
        {
            return ControlFlowGraph.ControlFlowGraph.FromSource(phpCode, fileName);
        }

        static ProgramPointGraph Analyze(ControlFlowGraph.ControlFlowGraph entryMethod, string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            var analysis = new ForwardAnalysis(entryMethod, MemoryModels.MemoryModels.VirtualReferenceMM, fileInfo);

            analysis.Analyse();

            return analysis.ProgramPointGraph;
        }
    }
}