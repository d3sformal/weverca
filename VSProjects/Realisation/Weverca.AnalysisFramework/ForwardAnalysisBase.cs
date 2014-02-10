using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Create snapshot used during analysis
    /// NOTE:
    ///     * Is called whenever new snapshot is needed (every time new snapshot has to be created)
    /// </summary>
    /// <returns>Created snapshot</returns>
    public delegate SnapshotBase CreateSnapshot();

    /// <summary>
    /// Provide forward CFG analysis API.
    /// </summary>
    public abstract class ForwardAnalysisBase
    {

        #region Private members

        /// <summary>
        /// Create snapshot used during analysis
        /// </summary>
        private readonly CreateSnapshot _createSnapshotDelegate;

        /// <summary>
        /// Available services provided by analysis
        /// </summary>
        private ForwardAnalysisServices _services;

        /// <summary>
        /// Available expression evaluator
        /// </summary>
        private ExpressionEvaluatorBase _expressionEvaluator;

        /// <summary>
        /// Available function resolver
        /// </summary>
        private FunctionResolverBase _functionResolver;

        /// <summary>
        /// Available flow resolver
        /// </summary>
        private FlowResolverBase _flowResolver;

        /// <summary>
        /// Queue of program points that should be processed
        /// </summary>
        private Queue<ProgramPointBase> _workQueue = new Queue<ProgramPointBase>();

        #endregion

        #region Analysis result API

        /// <summary>
        /// Forward analysis has always forward direction
        /// </summary>
        public const AnalysisDirection Direction = AnalysisDirection.Forward;

        /// <summary>
        /// Gets a value indicating whether analysis has already finished.
        /// </summary>
        public bool IsAnalysed { get; private set; }

        /// <summary>
        /// Gets root output from analysis
        /// </summary>
        public ProgramPointGraph ProgramPointGraph { get; private set; }

        /// <summary>
        /// Gets control flow graph of method which is entry point of analysis.
        /// </summary>
        public Weverca.ControlFlowGraph.ControlFlowGraph EntryCFG { get; private set; }

        /// <summary>
        /// Input which is used only once when starting analysis - can be modified only before analysing
        /// </summary>
        public readonly FlowOutputSet EntryInput;

        /// <summary>
        /// Determine count of commits on single flow set that is needed to start widening
        /// </summary>
        public int WideningLimit { get; protected set; }

        #endregion

        #region Template methods for obtaining resolvers

        /// <summary>
        /// Create expression evaluator which is used during analysis
        /// NOTE:
        ///     * Is created only once
        /// </summary>
        /// <returns>Created evaluator</returns>
        protected abstract ExpressionEvaluatorBase createExpressionEvaluator();

        /// <summary>
        /// Create flow resolver which is used during analysis
        /// NOTE:
        ///     * Is created only once
        /// </summary>
        /// <returns>Created resolver</returns>
        protected abstract FlowResolverBase createFlowResolver();

        /// <summary>
        /// Create function resolver which is used during analysis
        /// NOTE:
        ///     * Is created only once
        /// </summary>
        /// <returns>Created resolver</returns>
        protected abstract FunctionResolverBase createFunctionResolver();

        /// <summary>
        /// Create memory assistant, that will be used for initializing created snapshots
        /// NOTE:
        ///     * Is called whenever new assistant is needed (every time new assistant has to be created)
        /// </summary>
        /// <returns>Created memory assistant</returns>
        protected abstract MemoryAssistantBase createAssistant();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardAnalysisBase" /> class.
        /// Create forward analysis object for given entry method graph.
        /// </summary>
        /// <param name="entryMethodGraph">Control flow graph of method which is entry point of analysis</param>
        /// <param name="createSnapshotDelegate">Method that creates a snapshot used during analysis</param>
        /// <param name="entryScript">The entry script of the analysis</param>
        public ForwardAnalysisBase(Weverca.ControlFlowGraph.ControlFlowGraph entryMethodGraph, CreateSnapshot createSnapshotDelegate)
        {
            _createSnapshotDelegate = createSnapshotDelegate;
            WideningLimit = int.MaxValue;
            EntryInput = createEmptySet();
            EntryInput.StartTransaction();
            EntryCFG = entryMethodGraph;
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
            EntryInput.CommitTransaction();

            ProgramPointGraph = ProgramPointGraph.FromSource(EntryCFG);
            _services.SetProgramEnd(ProgramPointGraph.End);
            _services.SetServices(ProgramPointGraph);

            var output = _services.CreateEmptySet();
            ProgramPointGraph.Start.Initialize(EntryInput, output);

            enqueue(ProgramPointGraph.Start);


            //fix point computation
            while (_workQueue.Count > 0)
            {
                var point = _workQueue.Dequeue();

                //during flow through are enqueued all needed flow children
                point.FlowThrough();
            }

            //because of avoid incorrect use
            //_services.UnSetServices(ProgramPointGraph);
        }

        private void enqueue(ProgramPointBase point)
        {
            if (!_workQueue.Contains(point))
            {
                _workQueue.Enqueue(point);
            }
        }


        #endregion

        #region Private utilities

        /// <summary>
        /// Create snapshot used during analysis
        /// NOTE:
        ///     * Is called whenever new snapshot is needed (every time new snapshot has to be created)
        /// </summary>
        /// <returns>Created snapshot</returns>
        private SnapshotBase createSnapshot()
        {
            return _createSnapshotDelegate();
        }

        /// <summary>
        /// Initialize all resolvers and services
        /// </summary>
        private void initialize()
        {
            _expressionEvaluator = createExpressionEvaluator();
            _flowResolver = createFlowResolver();
            _functionResolver = createFunctionResolver();

            _services = new ForwardAnalysisServices(
                _workQueue,
                _functionResolver, _expressionEvaluator, createEmptySet, _flowResolver

                );
        }

        /// <summary>
        /// Creates empty output set
        /// </summary>
        /// <returns>Created output set</returns>
        private FlowOutputSet createEmptySet()
        {
            var snapshot = createSnapshot();
            var assistant = createAssistant();

            snapshot.InitAssistant(assistant);
            assistant.InitContext(snapshot);

            return new FlowOutputSet(snapshot, WideningLimit);
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
