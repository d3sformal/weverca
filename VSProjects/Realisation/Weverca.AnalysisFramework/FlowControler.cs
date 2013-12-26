using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Controller used for accessing flow sets and dispatching
    /// </summary>
    public class FlowController
    {
        /// <summary>
        /// Available services obtained from analysis
        /// </summary>
        internal readonly ForwardAnalysisServices Services;

        /// <summary>
        /// Flow resolver from analysis services
        /// </summary>
        internal FlowResolverBase FlowResolver { get { return Services.FlowResolver; } }

        /// <summary>
        /// Currently analyzed program point
        /// </summary>
        public readonly ProgramPointBase ProgramPoint;

        /// <summary>
        /// Currently analyzed partial (elementary part of analyzed statement)
        /// </summary>
        public LangElement CurrentPartial { get { return ProgramPoint.Partial; } }

        public EvaluationLog Log { get; private set; }

        /// <summary>
        /// Input set of program analysis flow
        /// </summary>
        public FlowInputSet InSet { get { return ProgramPoint.InSet; } }

        /// <summary>
        /// Output set of program analysis flow
        /// </summary>
        public FlowOutputSet OutSet { get { return ProgramPoint.OutSet; } }

        /// <summary>
        /// Get/Set arguments used for call branches
        /// NOTE:
        ///     Default arguments are set by framework (you can override them)
        /// </summary>
        public MemoryEntry[] Arguments { get; set; }

        /// <summary>
        /// Keys associated with connected extension branches
        /// </summary>
        public IEnumerable<object> ExtensionKeys { get { return ProgramPoint.Extension.Keys; } }

        /// <summary>
        /// Get/Set this object for call branches
        /// NOTE:
        ///     Default called object is set by framework (you can override it)
        /// </summary>
        public MemoryEntry CalledObject { get; set; }


        /// <summary>
        /// Create flow controller for given input and output set
        /// </summary>
        internal FlowController(ForwardAnalysisServices services, ProgramPointBase programPoint)
        {
            Services = services;
            ProgramPoint = programPoint;
        }

        /// <summary>
        /// Set evaluation log for use by resolvers
        /// </summary>
        /// <param name="log">Evaluation log</param>
        internal void SetLog(EvaluationLog log)
        {
            Log = log;
        }


        public void RemoveExtension(object branchKey)
        {
            ProgramPoint.Extension.Remove(branchKey);
        }

        public void AddExtension(object branchKey, ProgramPointGraph ppGraph, ExtensionType type)
        {
            ProgramPoint.Extension.Add(branchKey, ppGraph, type);
        }
    }
}
