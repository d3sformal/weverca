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
    /// Group of services that are provided by analysis object.
    /// </summary>    
    class AnalysisServices
    {
        private readonly Queue<ProgramPointBase> _workListQueue;

        /// <summary>
        /// Available flow resolver obtained from analysis
        /// </summary>
        internal readonly FlowResolverBase FlowResolver;

        internal readonly ExpressionEvaluatorBase Evaluator;

        internal readonly FunctionResolverBase FunctionResolver;

        /// <summary>
        /// Available empty set creator obtained from analysis
        /// </summary>
        internal readonly EmptySetDelegate CreateEmptySet;

        public AnalysisServices(Queue<ProgramPointBase> workListQueue, FunctionResolverBase functionResolver, ExpressionEvaluatorBase evaluator, EmptySetDelegate emptySet, FlowResolverBase flowResolver)
        {
            _workListQueue = workListQueue;
            CreateEmptySet = emptySet;
            FlowResolver = flowResolver;
            FunctionResolver = functionResolver;
            Evaluator = evaluator;
        }

        internal bool ConfirmAssumption(FlowController flow, AssumptionCondition condition)
        {
            return FlowResolver.ConfirmAssumption(flow.OutSet, condition, flow.Log);
        }

        internal void FlowThrough(ProgramPointBase programPoint)
        {
            FlowResolver.FlowThrough(programPoint);
        }

        internal void Enqueue(ProgramPointBase programPoint)
        {
            _workListQueue.Enqueue(programPoint);
        }

        internal void SetServices(ProgramPointGraph ppGraph)
        {
            foreach (var point in ppGraph.Points)
            {
                point.SetServices(this);
            }
        }

        internal void UnSetServices(ProgramPointGraph ppGraph)
        {
            foreach (var point in ppGraph.Points)
            {
                point.SetServices(null);
            }
        }
    }
}
