using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.Taint
{
    public class TaintForwardAnalysis : NextPhaseAnalysis
    {
        public TaintForwardAnalysis(ProgramPointGraph analyzedGraph)
            : base(analyzedGraph, AnalysisDirection.Forward, new TaintAnalyzer())
        {
        }
    }
}
