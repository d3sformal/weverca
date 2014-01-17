using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    class SimpleTaintForwardAnalysis : NextPhaseAnalysis 
    {
        public SimpleTaintForwardAnalysis(ProgramPointGraph analyzedGraph)
            : base(analyzedGraph, AnalysisDirection.Forward, new TaintAnalyzer())
        {
        }
    }
}
