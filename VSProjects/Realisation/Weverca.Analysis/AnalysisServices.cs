using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
   
    /// <summary>
    /// Represents method which creates empty flow info set.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    /// <returns>Empty flow info set</returns>
    delegate FlowOutputSet EmptySetDelegate();

    delegate PartialWalker WalkerCreatorDelegate();

    /// <summary>
    /// Group of services that are provided by analysis object.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    class AnalysisServices
    {
        internal readonly FlowResolverBase FlowResolver;

        internal readonly EmptySetDelegate CreateEmptySet;        
        internal readonly WalkerCreatorDelegate CreateWalker;

        public AnalysisServices(EmptySetDelegate emptySet,WalkerCreatorDelegate createWalker, FlowResolverBase flowResolver)
        {
            CreateEmptySet = emptySet;            
            CreateWalker = createWalker;

            FlowResolver = flowResolver;
        }

        internal bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, MemoryEntry[] expressionParts)
        {            
            return FlowResolver.ConfirmAssumption(outSet,condition, expressionParts);
        }

        internal void FlowThrough(ProgramPoint programPoint)
        {            
            FlowResolver.FlowThrough(programPoint);
        }
    }
}
