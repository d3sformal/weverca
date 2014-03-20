using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

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
        /// Expression evaluator instance
        /// </summary>
        public ExpressionEvaluatorBase ExpressionEvaluator
        {
            get
            {
                return Services.Evaluator;
            }
        }

        /// <summary>
        /// Flow resolver from analysis services
        /// </summary>
        internal FlowResolverBase FlowResolver { get { return Services.FlowResolver; } }

        /// <summary>
        /// Currently analyzed program point
        /// </summary>
        public readonly ProgramPointBase CurrentProgramPoint;

        /// <summary>
        /// Info about entry file where analysis has started
        /// </summary>
        public FileInfo EntryScript { get { return ProgramEnd.OwningPPGraph.OwningScript; } }

        /// <summary>
        /// Info about current file where program point comes from
        /// </summary>
        public FileInfo CurrentScript { get { return CurrentPPG.OwningScript; } }

        /// <summary>
        /// Currently processed program point graph
        /// </summary>
        public ProgramPointGraph CurrentPPG { get { return CurrentProgramPoint.OwningPPGraph; } }

        /// <summary>
        /// Currently analyzed partial (elementary part of analyzed statement)
        /// </summary>
        public LangElement CurrentPartial { get { return CurrentProgramPoint.Partial; } }

        /// <summary>
        /// Log holding all subresults hashed according to AST partials
        /// </summary>
        public EvaluationLog Log { get; private set; }

        /// <summary>
        /// Input set of program analysis flow
        /// </summary>
        public FlowInputSet InSet { get { return CurrentProgramPoint.InSet; } }

        /// <summary>
        /// Output set of program analysis flow
        /// </summary>
        public FlowOutputSet OutSet { get { return CurrentProgramPoint.OutSet; } }

        /// <summary>
        /// Get/Set arguments used for call branches
        /// NOTE:
        ///     Default arguments are set by framework (you can override them)
        /// </summary>
        public MemoryEntry[] Arguments { get; set; }

        /// <summary>
        /// Keys associated with connected extension branches. Returns copy so it can be modified during iteration
        /// </summary>
        public object[] ExtensionKeys { get { return  CurrentProgramPoint.Extension.Keys.ToArray(); } }

        /// <summary>
        /// Get/Set this object for call branches
        /// NOTE:
        ///     Default called object is set by framework (you can override it)
        /// </summary>
        public MemoryEntry CalledObject { get; set; }

        /// <summary>
        /// End point of program point graph
        /// </summary>
        public ProgramPointBase ProgramEnd { get { return Services.ProgramEnd; } }

        /// <summary>
        /// Create flow controller for given input and output set
        /// </summary>
        internal FlowController(ForwardAnalysisServices services, ProgramPointBase programPoint)
        {
            Services = services;
            CurrentProgramPoint = programPoint;
        }

        /// <summary>
        /// Set evaluation log for use by resolvers
        /// </summary>
        /// <param name="log">Evaluation log</param>
        internal void SetLog(EvaluationLog log)
        {
            Log = log;
        }

        /// <summary>
        /// Remove extension branch with given branchKey from Point
        /// </summary>
        /// <param name="branchKey">Key of removed branch</param>
        public void RemoveExtension(object branchKey)
        {
            CurrentProgramPoint.Extension.Remove(branchKey);
        }

        /// <summary>
        /// Add extension branch into Point
        /// </summary>
        /// <param name="branchKey">Key of added branch</param>
        /// <param name="ppGraph">Extending program point used as branch</param>
        /// <param name="type">Type of extension</param>
        public void AddExtension(object branchKey, ProgramPointGraph ppGraph, ExtensionType type)
        {
            CurrentProgramPoint.Extension.Add(branchKey, ppGraph, type);
        }

        /// <summary>
        /// Set throw branches from current point into specified program points. These branches contain
        /// catch point and handles given ThrowInfo values.
        /// </summary>
        /// <param name="branches">ThrowInfo values specifiing udpates/creations/deletions of throw branches</param>
        /// <param name="removeFlowChildren">If true flow children will be removed, otherwise no other than catch point children are affected</param>
        public void SetThrowBranching(IEnumerable<ThrowInfo> branches, bool removeFlowChildren = false)
        {
            //create indexed structure for branches
            var indexed = new Dictionary<CatchBlockDescription, ThrowInfo>();
            foreach (var branch in branches)
            {
                indexed.Add(branch.Catch, branch);
            }

            //update already existing branches
            var childrenCopy = CurrentProgramPoint.FlowChildren.ToArray();
            foreach (var child in childrenCopy)
            {
                var catchChild = child as CatchPoint;
                if (catchChild == null)
                {
                    if (removeFlowChildren)
                        CurrentProgramPoint.RemoveFlowChild(child);

                    continue;
                }

                ThrowInfo info;
                if (indexed.TryGetValue(catchChild.CatchDescription, out info))
                {
                    catchChild.ReThrow(info);

                    //remove branch from index because it is already complete
                    indexed.Remove(catchChild.CatchDescription);
                }
                else
                {
                    //no more it contains branch for this catch child
                    //disconnect it from graph
                    CurrentProgramPoint.RemoveFlowChild(catchChild);
                    catchChild.RemoveFlowChild(catchChild.TargetPoint);
                }
            }

            //add new branches
            foreach (var throwInfo in indexed.Values)
            {
                //create catch point according to specified throw info
                var catchPoint = new CatchPoint(CurrentProgramPoint, throwInfo.Catch);
                InitializeNewPoint(catchPoint);

                catchPoint.ReThrow(throwInfo);

                //connect branch into graph
                CurrentProgramPoint.AddFlowChild(catchPoint);
                catchPoint.AddFlowChild(catchPoint.TargetPoint);
            }
        }


        internal void InitializeNewPoint(ProgramPointBase point)
        {
            point.Initialize(Services.CreateEmptySet(), Services.CreateEmptySet());
            point.SetServices(Services);
            point.SetOwningGraph(CurrentPPG);
        }
    }
}
