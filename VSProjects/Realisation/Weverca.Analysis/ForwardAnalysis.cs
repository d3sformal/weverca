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
        /// Currently analyzed call stack.
        /// </summary>
        private AnalysisDispatchStack _dispatchStack;

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

        #region Analysis routines

        /// <summary>
        /// Run analysis starting at EntryMethodGraph
        /// </summary>
        private void analyse()
        {
            checkAlreadyAnalysed();
            EntryInput.CommitTransaction();

            ProgramPointGraph = new ProgramPointGraph(EntryCFG.start);

            // create analysis entry point from given graph
            var entryDispatch = new DispatchInfo(ProgramPointGraph, EntryInput);
            var entryLevel = new DispatchLevel(entryDispatch, _services, DispatchType.ParallelCall);

            runCallStackAnalysis(entryLevel);
        }

        /// <summary>
        /// Run analysis on call stack from given entryLevel
        /// </summary>
        /// <param name="entryLevel">Entry level, where analysis starts</param>
        private void runCallStackAnalysis(DispatchLevel entryLevel)
        {
            _dispatchStack = new AnalysisDispatchStack(_services);
            _dispatchStack.Push(entryLevel);

            while (!_dispatchStack.IsEmpty)
            {
                var currentContext = _dispatchStack.CurrentContext;
                if (currentContext.IsComplete)
                {
                    if (!_dispatchStack.CurrentLevel.ShiftToNextDispatch())
                    {
                        // we can't move to next context in current level
                        popCallStack();
                    }

                    continue;
                }

                // NOTE: Can modify callStack - use currentContext for moving to nextPartial
                flowThroughCurrentPartial(currentContext);
                currentContext.NextPartial();
            }
        }

        /// <summary>
        /// Flows through current partial in context
        /// NOTE:
        ///     Can emit call dispatches
        /// </summary>
        /// <param name="context">Context where partial will be flown through</param>
        private void flowThroughCurrentPartial(AnalysisDispatchContext context)
        {
            var controller = new FlowController(_services, context.CurrentProgramPoint, context.CurrentPartial);
            context.CurrentWalker.Eval(controller);

            DispatchLevel level = null;
            if (controller.HasCallExtension)
            {
                level = createCallLevel(controller);
            }
            else if (controller.HasIncludeExtension)
            {
                level = createIncludeLevel(controller);
            }
            else
            {
                // there is no dispatching
                return;
            }

            _dispatchStack.Push(level);
        }

        private DispatchLevel createCallLevel(FlowController controller)
        {
            var dispatches = new List<DispatchInfo>();
            var currentExtension = controller.CurrentCallExtension;
            foreach (var branchKey in currentExtension.BranchingKeys)
            {
                var branch = currentExtension.GetBranch(branchKey);

                // get input for branch so it could be initialized
                var currentInput = currentExtension.GetInput(branch);

                currentInput.StartTransaction();
                currentInput.ExtendAsCall(controller.OutSet, controller.CalledObject, controller.Arguments);
                _functionResolver.InitializeCall(currentInput, branchKey, controller.Arguments);
                currentInput.CommitTransaction();

                // get inputs from all containing extensions
                var inputs = getExtensionInputs(branch);

                dispatches.Add(new DispatchInfo(branch, inputs));
            }

            return new DispatchLevel(dispatches, _services, DispatchType.ParallelCall);
        }

        private DispatchLevel createIncludeLevel(FlowController controller)
        {
            var dispatches = new List<DispatchInfo>();
            var currentExtension = controller.CurrentIncludeExtension;
            foreach (var branchKey in currentExtension.BranchingKeys)
            {
                var branch = currentExtension.GetBranch(branchKey);
                var currentInput = currentExtension.GetInput(branch);

                currentInput.StartTransaction();
                currentInput.Extend(controller.OutSet);
                currentInput.CommitTransaction();

                var inputs = getExtensionInputs(branch);

                dispatches.Add(new DispatchInfo(branch, inputs));
            }

            return new DispatchLevel(dispatches, _services, DispatchType.ParallelInclude);
        }

        private FlowInputSet[] getExtensionInputs(ProgramPointGraph branch)
        {
            var result = new List<FlowInputSet>();

            foreach (var extension in branch.ContainingCallExtensions)
            {
                result.Add(extension.GetInput(branch));
            }

            foreach (var extension in branch.ContainingIncludeExtensions)
            {
                result.Add(extension.GetInput(branch));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Correctly pops context from _callstack (with all handling routines)
        /// </summary>
        private void popCallStack()
        {
            var poppedContext = _dispatchStack.CurrentLevel;
            _dispatchStack.Pop();

            if (!_dispatchStack.IsEmpty)
            {
                mergeCallResult(_dispatchStack.CurrentContext, poppedContext);
            }
        }

        /// <summary>
        /// Merge dispatched call results into callers context
        /// </summary>
        /// <param name="callerContext">Context of caller that invokes calls</param>
        /// <param name="callResults">Results of invoked calls</param>
        private void mergeCallResult(AnalysisDispatchContext callerContext, DispatchLevel calledContext)
        {
            var callResults = calledContext.GetResult();
            var callPPGraphs = from callResult in callResults select callResult.ProgramPointGraph;

            _flowResolver.CallDispatchMerge(callerContext.CurrentOutputSet, callPPGraphs.ToArray(), calledContext.DispatchType);

            // push return value into walker
            var returnValue = getReturnValue(callResults);
            _dispatchStack.CurrentContext.CurrentWalker.InsertReturnValue(returnValue);
        }

        /// <summary>
        /// Resolve return value from given callResults
        /// </summary>
        /// <param name="callResults">Results of call dispatches</param>
        /// <returns>Resolved return value</returns>
        private MemoryEntry getReturnValue(AnalysisDispatchContext[] callResults)
        {
            var calls = from callResult in callResults select callResult.ProgramPointGraph;

            var returnValue = _functionResolver.ResolveReturnValue(calls.ToArray());

            return returnValue;
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

            _services = new AnalysisServices(createEmptySet, createWalker, _flowResolver);
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
        /// Creates partial walker containing available resolvers
        /// </summary>
        /// <returns>Created partial walker</returns>
        private PartialWalker createWalker()
        {
            return new PartialWalker(_expressionEvaluator, _functionResolver);
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
