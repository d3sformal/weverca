using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;

namespace Weverca.AnalysisFramework
{
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
        /// Queue of program points that should be processed
        /// </summary>
        private Queue<ProgramPointBase> _workQueue = new Queue<ProgramPointBase>();

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

        /// Determine count of commits on single flow set that is needed to start widening
        /// </summary>
        public int WideningLimit { get; protected set; }

        #endregion
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardAnalysisBase" /> class.
        /// Create forward analysis object for given entry method graph.
        /// </summary>
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
        public void Analyse()
        {
            checkAlreadyAnalysed();
            initialize();
            analyse();
            IsAnalysed = true;
        }

        #region Analysis routines

        /// <summary>
        /// Run analysis starting at EntryMethodGraph
        /// </summary>
        private void analyse()
        {
            var entryPoint = GetEntryPoint(AnalyzedProgramPointGraph);
            enqueue(entryPoint);

            //fix point computation
            while (_workQueue.Count > 0)
            {
                var point = _workQueue.Dequeue();

                var inputs = GetInputPoints(point);
                extendInput(point, inputs);

                flowThrough(point);

                if (hasChanges(point))
                {
                    enqueueChildren(point);
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

        private void extendInput(ProgramPointBase point, IEnumerable<ProgramPointBase> inputs)
        {
            point.SetMode(SnapshotMode.InfoLevel);

            var inSet = GetInSet(point);

            var inputSets = new List<FlowInputSet>();
            foreach (var input in inputs)
            {
                var outSet = GetOutSet(input);
                inputSets.Add(outSet);
            }

            inSet.StartTransaction();
            inSet.Extend(inputSets.ToArray());
            inSet.CommitTransaction();
        }

        private void enqueueChildren(ProgramPointBase point)
        {
            var children = GetOutputPoints(point);
            foreach (var child in children)
            {
                enqueue(child);
            }
        }

        private void enqueue(ProgramPointBase point)
        {
            if (!_workQueue.Contains(point))
            {
                _workQueue.Enqueue(point);
            }
        }

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

        internal IEnumerable<ProgramPointBase> GetOutputPoints(ProgramPointBase point)
        {
            switch (Direction)
            {
                case AnalysisDirection.Forward:
                    return point.FlowChildren;
                case AnalysisDirection.Backward:
                    return point.FlowParents;
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

            resetPoints(AnalyzedProgramPointGraph);
        }

        private void resetPoints(ProgramPointGraph ppg)
        {
            //TODO traverse all subgraphs

            foreach (var point in ppg.Points)
            {
                point.ResetInitialization();
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
