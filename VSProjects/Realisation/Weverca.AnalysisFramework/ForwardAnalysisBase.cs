using System;
using System.Collections.Generic;
using System.Linq;

using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Provide forward CFG analysis API.
    /// !!UNDER CONSTRUCTION, API CAN BE HEAVILY CHANGED!!
    /// </summary>
    public abstract class ForwardAnalysisBase
    {
        #region Private members


        /// <summary>
        /// Available services provided by analysis
        /// </summary>
        private AnalysisServices _services;

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

        private Queue<ProgramPointBase> _workQueue = new Queue<ProgramPointBase>();

        #endregion

        #region Analysis result API

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
        /// Create snapshot used during analysis
        /// NOTE:
        ///     * Is called whenever new snapshot is needed (every time new snapshot has to be created)
        /// </summary>
        /// <returns>Created snapshot</returns>
        protected abstract SnapshotBase createSnapshot();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardAnalysisBase" /> class.
        /// Create forward analysis object for given entry method graph.
        /// </summary>
        /// <param name="entryMethodGraph">Control flow graph of method which is entry point of analysis</param>
        public ForwardAnalysisBase(Weverca.ControlFlowGraph.ControlFlowGraph entryMethodGraph)
        {
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
            _services.SetServices(ProgramPointGraph);

            var output=_services.CreateEmptySet();            
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
            _services.UnSetServices(ProgramPointGraph);
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
        /// Initialize all resolvers and services
        /// </summary>
        private void initialize()
        {
            _expressionEvaluator = createExpressionEvaluator();
            _flowResolver = createFlowResolver();
            _functionResolver = createFunctionResolver();

            _services = new AnalysisServices(_workQueue,_functionResolver,_expressionEvaluator,createEmptySet,  _flowResolver);
        }

        /// <summary>
        /// Creates empty output set
        /// </summary>
        /// <returns>Created output set</returns>
        private FlowOutputSet createEmptySet()
        {
            return new FlowOutputSet(createSnapshot());
        }

      
        /// <summary>
        /// Throws exception when analyze has been already proceeded
        /// </summary>
        private void checkAlreadyAnalysed()
        {
            if (IsAnalysed)
            {
                throw new NotSupportedException("Analyze has already been proceeded");
            }
        }

        #endregion
    }
}
