﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

using Weverca.Analysis.Memory;
using Weverca.Analysis.Expressions;

namespace Weverca.Analysis
{
    /// <summary>
    /// Provide forward CFG analysis API.
    /// !!UNDER CONSTRUCTION, API CAN BE HEAVILY CHANGED!!
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    public abstract class ForwardAnalysis
    {
        #region Private members

        /// <summary>
        /// Currently analyzed call stack.
        /// </summary>
        AnalysisCallStack _callStack;

        AnalysisServices _services;
        ExpressionEvaluator _expressionEvaluator;
        FunctionResolver _functionResolver;
        FlowResolver _flowResolver;


        #endregion


        #region Analysis result API

        /// <summary>
        /// Determine that analysis has been already runned.
        /// </summary>
        public bool IsAnalysed { get; private set; }

        /// <summary>
        /// Root output from analysis
        /// </summary>
        public ProgramPointGraph ProgramPointGraph { get; private set; }
        /// <summary>
        /// Control flow graph of method which is entry point of analysis.
        /// </summary>
        public Weverca.ControlFlowGraph.ControlFlowGraph EntryCFG { get; private set; }

        #endregion

        /// <summary>
        /// Create forward analysis object for given entry method graph.
        /// </summary>
        /// <param name="entryMethodGraph">Control flow graph of method which is entry point of analysis</param>
        public ForwardAnalysis(ControlFlowGraph.ControlFlowGraph entryMethodGraph)
        {
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

        protected abstract ExpressionEvaluator createExpressionEvaluator();

        protected abstract FlowResolver createFlowResolver();

        protected abstract FunctionResolver createFunctionResolver();

        protected abstract AbstractSnapshot createSnapshot();

        #endregion

        #region Analysis routines

        /// <summary>
        /// Run analyzis starting at EntryMethodGraph
        /// </summary>
        private void analyse()
        {
            checkAlreadyAnalysed();

            //TODO API for creating input
            var input = createEmptySet() as FlowInputSet;
            ProgramPointGraph = new ProgramPointGraph(EntryCFG.start);

            //create analysis entry point from given graph 
            var entryDispatch = new CallInfo(input, ProgramPointGraph);
            var entryLevel = new CallDispatchLevel(entryDispatch, _services);

            runCallStackAnalysis(entryLevel);
        }

        /// <summary>
        /// Run analysis on callstack from given entryLevel
        /// </summary>
        /// <param name="entryLevel">Entry level, where analysis starts</param>
        private void runCallStackAnalysis(CallDispatchLevel entryLevel)
        {
            _callStack = new AnalysisCallStack(_services);
            _callStack.Push(entryLevel);

            while (!_callStack.IsEmpty)
            {
                var currentContext = _callStack.CurrentContext;
                if (currentContext.IsComplete)
                {
                    if (!_callStack.CurrentLevel.ShiftToNextDispatch())
                    {
                        //we can't move to next context in current level
                        popCallStack();
                    }
                    continue;
                }

                //NOTE: Can modify callStack - use currentContext for moving to nextPartial
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
        private void flowThroughCurrentPartial(AnalysisCallContext context)
        {
            var controller = new FlowController(context.CurrentInputSet, context.CurrentOutputSet);
            context.CurrentWalker.Eval(controller, context.CurrentPartial);

            if (controller.HasCallDispatch)
            {
                var dispatchLevel = new CallDispatchLevel(controller.CallDispatches, _services);
                _callStack.Push(dispatchLevel);
            }
        }

        /// <summary>
        /// Correctly pops context from _callstack (with all handling routines)
        /// </summary>
        private void popCallStack()
        {
            var callResult = _callStack.CurrentLevel.GetResult();
            _callStack.Pop();

            if (!_callStack.IsEmpty)
            {
                mergeCallResult(_callStack.CurrentContext, callResult);

                //push return value into walker
                var returnValue = getReturnValue(callResult);
                _callStack.CurrentContext.CurrentWalker.InsertReturnValue(returnValue);
            }
        }

        /// <summary>
        /// Merge dispatched call results into callers context
        /// </summary>
        /// <param name="callerContext">Context of caller that invokes calls</param>
        /// <param name="callResults">Results of invoked calls</param>
        private void mergeCallResult(AnalysisCallContext callerContext, AnalysisCallContext[] callResults)
        {
            var callPPGraphs = from callResult in callResults select callResult.ProgramPointGraph;

            foreach (var callPPGraph in callPPGraphs)
            {
                callPPGraph.AddInvocationPoint(callerContext.CurrentProgramPoint);
            }

            _flowResolver.CallDispatchMerge(callerContext.CurrentOutputSet, callPPGraphs.ToArray());
        }
        
        /// <summary>
        /// Resolve return value from given callResults
        /// </summary>
        /// <param name="callResults">Resolts of call dispatches</param>
        /// <returns>Resolved return value</returns>
        private MemoryEntry getReturnValue(AnalysisCallContext[] callResults)
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
