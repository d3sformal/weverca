using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Represents method which creates empty flow info set.
    /// </summary>    
    /// <returns>Empty flow info set</returns>
    delegate FlowOutputSet EmptySetDelegate();

    /// <summary>
    /// Group of services that are provided by analysis object.
    /// </summary>    
    public class AnalysisServices
    {
        private readonly Queue<ProgramPointBase> _workListQueue;

        /// <summary>
        /// The entry script of the analysis
        /// </summary>
        public static FileInfo EntryScript
        {
            get;
            private set;
        }
        /// <summary>
        /// The script in that elements of currently created program point graph are defined
        /// </summary>
        public static FileInfo CurrentScript
        {
            get;
            internal set;
        }

        /// <summary>
        /// Available flow resolver obtained from analysis
        /// </summary>
        internal readonly FlowResolverBase FlowResolver;

        /// <summary>
        /// Available expression evaluator obtained from analysis
        /// </summary>
        internal readonly ExpressionEvaluatorBase Evaluator;

        /// <summary>
        /// Available function resolver obtained from analysis
        /// </summary>
        internal readonly FunctionResolverBase FunctionResolver;

        /// <summary>
        /// Available empty set creator obtained from analysis
        /// </summary>
        internal readonly EmptySetDelegate CreateEmptySet;

        internal AnalysisServices(Queue<ProgramPointBase> workListQueue, FunctionResolverBase functionResolver, ExpressionEvaluatorBase evaluator, EmptySetDelegate emptySet, FlowResolverBase flowResolver, FileInfo entryScript)
        {
            _workListQueue = workListQueue;
            CreateEmptySet = emptySet;
            FlowResolver = flowResolver;
            FunctionResolver = functionResolver;
            Evaluator = evaluator;
            EntryScript = entryScript;
            CurrentScript = entryScript;
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

        /// <summary>
        /// Set services for all points in given graph
        /// </summary>
        /// <param name="ppGraph">Graph which program points will be set</param>
        internal void SetServices(ProgramPointGraph ppGraph)
        {
            foreach (var point in ppGraph.Points)
            {
                SetServices(point);
            }
        }

        internal void SetServices(ProgramPointBase point)
        {
            point.SetServices(this);
        }

        /// <summary>
        /// Unset services for all points in given graph
        /// </summary>
        /// <param name="ppGraph">Graph which program points will be unset</param>
        internal void UnSetServices(ProgramPointGraph ppGraph)
        {
            foreach (var point in ppGraph.Points)
            {
                point.SetServices(null);
            }
        }
    }
}
