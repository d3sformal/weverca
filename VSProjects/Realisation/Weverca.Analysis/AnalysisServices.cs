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
    /// <returns>Empty flow info set</returns>
    delegate FlowOutputSet EmptySetDelegate();

    /// <summary>
    /// Represents method which creates new partial walker.
    /// </summary>
    /// <returns>Created walker</returns>
    delegate PartialWalker WalkerCreatorDelegate();

    /// <summary>
    /// Group of services that are provided by analysis object.
    /// </summary>    
    class AnalysisServices
    {
        /// <summary>
        /// Available flow resolver obtained from analysis
        /// </summary>
        internal readonly FlowResolverBase FlowResolver;

        /// <summary>
        /// Available empty set creator obtained from analysis
        /// </summary>
        internal readonly EmptySetDelegate CreateEmptySet;    
    
        /// <summary>
        /// 
        /// </summary>
        internal readonly WalkerCreatorDelegate CreateWalker;

        public AnalysisServices(EmptySetDelegate emptySet,WalkerCreatorDelegate createWalker, FlowResolverBase flowResolver)
        {
            CreateEmptySet = emptySet;            
            CreateWalker = createWalker;
            FlowResolver = flowResolver;
        }

        internal bool ConfirmAssumption(FlowController flow, AssumptionCondition condition)
        {            
            return FlowResolver.ConfirmAssumption(flow.OutSet,condition, flow.Log);
        }

        internal void FlowThrough(ProgramPoint programPoint)
        {            
            FlowResolver.FlowThrough(programPoint);
        }
    }
}
