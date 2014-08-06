using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Describes computation directoin of fixpoint
    /// </summary>
    public enum AnalysisDirection
    {
        /// <summary>
        /// After point change its children are processed
        /// </summary>
        Forward,

        /// <summary>
        /// After point change its parents are processed
        /// </summary>
        Backward
    }


    /// <summary>
    /// TODO: How to resolve steps (it cause many duplications with forward resolvers)
    /// </summary>
    public abstract class NextPhaseAnalysis
    {

        #region Private members

        /// <summary>
        /// Visitor used for visiting analyzed graph
        /// </summary>
        private readonly NextPhaseAnalyzer _analyzer;

		/// <summary>
		/// List of program points that should be processed
		/// </summary>
		private readonly WorkList _workList = WorkList.GetInstance(true);

        #endregion

        #region Analysis result API

        /// <summary>
        /// Determine direction of analysis, that can be
        /// determined while analysis is constructed
        /// </summary>
        public readonly AnalysisDirection Direction;

        /// <summary>
        /// Gets a value indicating whether analysis has already finished.
        /// </summary>
        public bool IsAnalysed { get; private set; }

        /// <summary>
        /// Gets root output from analysis
        /// </summary>
        public ProgramPointGraph AnalyzedProgramPointGraph { get; private set; }

        /// <summary>
        /// Determine count of commits on single flow set that is needed to start widening
        /// </summary>
        public int WideningLimit { get; protected set; }

        /// <summary>
        /// Entry input of the analysis.
        /// </summary>
        public FlowOutputSet EntryInput { get { return GetInSet(GetEntryPoint(AnalyzedProgramPointGraph)); } }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardAnalysisBase" /> class.
        /// Create forward analysis object for given entry method graph.
        /// </summary>
        /// <param name="analyzedPPG">The analyzed PPG.</param>
        /// <param name="direction">The direction of analysis.</param>
        /// <param name="analyzer">The analyzer.</param>
        public NextPhaseAnalysis(ProgramPointGraph analyzedPPG, AnalysisDirection direction, NextPhaseAnalyzer analyzer)
        {
            _analyzer = analyzer;

            Direction = direction;
            AnalyzedProgramPointGraph = analyzedPPG;
            WideningLimit = int.MaxValue;
        }

        /// <summary>
        /// Run analysis on EntryMethodGraph
        /// </summary>
        public virtual void Analyse()
        {
            checkAlreadyAnalysed();
            initialize();
            analyse();
            IsAnalysed = true;
        }

        public NextPhaseAnalyzer getNextPhaseAnalyzer()
        {
            return _analyzer;
        }

        #region Analysis routines

        /// <summary>
        /// Run analysis starting at EntryMethodGraph
        /// </summary>
        private void analyse()
        {
            var entryPoint = GetEntryPoint(AnalyzedProgramPointGraph);

            _workList.AddEntryPoint(entryPoint);

            //fix point computation
            while (_workList.HasWork)
            {
                var point = _workList.GetWork();

                if (point.InSet == null) continue;

                extendInput(point);

                flowThrough(point);

                if (hasChanges(point))
                {
                    _workList.AddChildren(point);
                }
            }
        }

        private bool hasChanges(ProgramPointBase point)
        {
            var outSet = GetOutSet(point);
            return outSet.HasChanges;
        }

        private void flowThrough(ProgramPointBase point)
        {
            prepare(point);
            _analyzer.FlowThrough(point);
            commit(point);
        }

        private void prepare(ProgramPointBase point)
        {
            point.SetMode(SnapshotMode.InfoLevel);

            var outSet = GetOutSet(point);
            var inSet = GetInSet(point);
            outSet.StartTransaction();
            //default extending
            outSet.Extend(inSet);
        }

        private void commit(ProgramPointBase point)
        {
            point.SetMode(SnapshotMode.InfoLevel);

            var outSet = GetOutSet(point);
            outSet.CommitTransaction();
        }

        private void extendInput(ProgramPointBase point)
        {
            var inputs = GetInputPoints(point);
            if (!inputs.Any()) return;

            point.SetMode(SnapshotMode.InfoLevel);

            var inSet = GetInSet(point);

            switch (Direction)
            {
                case AnalysisDirection.Forward:
                    forwardExtend(point, inputs, inSet);
                    break;
                case AnalysisDirection.Backward:
                    standardExtend(inputs, inSet);
                    break;
                default:
                    throwUnknownDirection();
                    break;
            }
        }

        #region Extending strategies

        private void forwardExtend(ProgramPointBase point, IEnumerable<ProgramPointBase> inputs, FlowOutputSet inSet)
        {
            var extensionPoint = point as ExtensionPoint;
            var sinkPoint = point as ExtensionSinkPoint;
            if (extensionPoint != null)
            {
                extendCallExtension(extensionPoint, inSet);
            }
            else if (sinkPoint != null)
            {
                extendCallSink(sinkPoint, inSet);
            }
            else
            {
                standardExtend(inputs, inSet);
            }
        }

        private void standardExtend(IEnumerable<ProgramPointBase> inputs, FlowOutputSet inSet)
        {
            var inputSets = new List<FlowInputSet>();
            foreach (var input in inputs)
            {
                var outSet = GetOutSet(input);
				var assumeParent = input as AssumePoint;

				if (outSet == null || (assumeParent != null && !assumeParent.Assumed))
					continue;

                inputSets.Add(outSet);
            }

            inSet.StartTransaction();
            inSet.Extend(inputSets.ToArray());
            inSet.CommitTransaction();
        }

        private void extendCallSink(ExtensionSinkPoint point, FlowOutputSet inSet)
        {
            inSet.StartTransaction();
            inSet.Extend(point.OwningExtension.Owner.OutSet);
            //Services.FlowResolver.CallDispatchMerge(_inSet, OwningExtension.Branches);
            inSet.CommitTransaction();
        }

        private void extendCallExtension(ExtensionPoint point, FlowOutputSet inSet)
        {
            inSet.StartTransaction();

            if (point.Type == ExtensionType.ParallelCall)
            {
                inSet.ExtendAsCall(point.Caller.OutSet, point.Flow.CalledObject, point.Flow.Arguments);
            }
            else
            {
                inSet.Extend(point.Caller.OutSet);
            }

            inSet.CommitTransaction();
        }

        #endregion

        #region Analysis direction handling

        internal ProgramPointBase GetEntryPoint(ProgramPointGraph ppg)
        {
            switch (Direction)
            {
                case AnalysisDirection.Forward:
                    return ppg.Start;
                case AnalysisDirection.Backward:
                    return ppg.End;
                default:
                    throwUnknownDirection();
                    return null;
            }
        }

        internal FlowOutputSet GetInSet(ProgramPointBase point)
        {
            switch (Direction)
            {
                case AnalysisDirection.Forward:
                    return point.InSet as FlowOutputSet;
                case AnalysisDirection.Backward:
                    return point.OutSet;
                default:
                    throwUnknownDirection();
                    return null;
            }
        }

        internal FlowOutputSet GetOutSet(ProgramPointBase point)
        {
            switch (Direction)
            {
                case AnalysisDirection.Forward:
                    return point.OutSet;
                case AnalysisDirection.Backward:
                    return point.InSet as FlowOutputSet;
                default:
                    throwUnknownDirection();
                    return null;
            }
        }

        internal IEnumerable<ProgramPointBase> GetInputPoints(ProgramPointBase point)
        {
            switch (Direction)
            {
                case AnalysisDirection.Forward:
                    return point.FlowParents;
                case AnalysisDirection.Backward:
                    return point.FlowChildren;
                default:
                    throwUnknownDirection();
                    return null;
            }
        }

        private void throwUnknownDirection()
        {
            throw new NotSupportedException("Analysis doesn't support: " + Direction);
        }

        #endregion

        #endregion

        #region Private utilities

        /// <summary>
        /// Initialize all resolvers and services
        /// </summary>
        private void initialize()
        {
            _analyzer.Initialize(this);
            resetPoints(new HashSet<ProgramPointGraph>(), AnalyzedProgramPointGraph);
        }

        private void resetPoints(HashSet<ProgramPointGraph> processedGraphs, ProgramPointGraph ppg)
        {
            processedGraphs.Add(ppg);
            foreach (var point in ppg.Points)
            {

                point.ResetInitialization();

                foreach (var branch in point.Extension.Branches) 
                {
                    branch.ResetInitialization();
                    point.Extension.Sink.ResetInitialization();
                    if (!processedGraphs.Contains (branch.Graph)) {
                        resetPoints(processedGraphs, branch.Graph);
                    }
                }
            }
        }

        /// <summary>
        /// Throws exception when analyze has been already proceeded
        /// </summary>
        private void checkAlreadyAnalysed()
        {
            if (IsAnalysed)
            {
                throw new NotSupportedException("Analyse has already been proceeded");
            }
        }

        #endregion
    }
}
