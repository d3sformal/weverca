using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;


namespace Weverca.AnalysisFramework.UnitTest.InfoLevelPhase
{
    class SimpleBackwardAnalysis : NextPhaseAnalysis
    {
        public SimpleBackwardAnalysis(ProgramPointGraph analyzedGraph)
            : base(analyzedGraph, null, AnalysisDirection.Backward)
        {
        }

        protected override ExpressionEvaluatorBase createExpressionEvaluator()
        {
            return new PropagationEvaluator();
        }

        protected override FlowResolverBase createFlowResolver()
        {
            return null;
        }

        protected override FunctionResolverBase createFunctionResolver()
        {
            return null;
        }

        protected override MemoryAssistantBase createAssistant()
        {
            return null;
        }
    }
}
