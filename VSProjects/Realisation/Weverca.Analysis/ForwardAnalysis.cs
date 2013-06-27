using System;
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
    /// Is used for creating snapshot during Analysis
    /// </summary>
    /// <returns>Created snapshot</returns>
    public delegate AbstractSnapshot SnapshotProvider();

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

            _callStack = new AnalysisCallStack(_services);

            var input = createEmptySet() as FlowInputSet;
            ProgramPointGraph = new ProgramPointGraph(EntryCFG.start);

            var entryDispatch = new CallInfo(input,ProgramPointGraph);
            var entryLevel = new CallDispatchLevel(entryDispatch,_services);
            
            _callStack.Push(entryLevel);

            while (!_callStack.IsEmpty)
            {
                var currentContext = _callStack.CurrentContext;
                if (currentContext.IsComplete)
                {                                                   
                    if (!_callStack.CurrentLevel.ShiftToNext())
                    {
                        //we can't move to next context in current level
                        popCallStack();
                    }
                    continue;
                }
                
                //NOTE: Can modify callStack - use currentContext for moving to nextPartial
                flowThroughNextPartial(currentContext);
                currentContext.NextPartial();
            }
        }

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

        private void mergeCallResult(AnalysisCallContext currentContext, AnalysisCallContext[] callResults)
        {
            var callPPGraphs=from callResult in callResults select callResult.ProgramPointGraph;

            foreach (var callPPGraph in callPPGraphs)
            {
                callPPGraph.AddInvocationPoint(currentContext.CurrentProgramPoint);
            }

            _flowResolver.SetContext(new FlowControler());
            _flowResolver.CallDispatchMerge(currentContext.CurrentOutputSet, callPPGraphs.ToArray());
        }

        private void flowThroughNextPartial(AnalysisCallContext currentContext)
        {            
            var controller = new FlowControler(currentContext.CurrentInputSet, currentContext.CurrentOutputSet);
            currentContext.CurrentWalker.Eval(controller, currentContext.CurrentPartial);

            if (controller.HasCallDispatch)
            {
                var dispatchLevel = new CallDispatchLevel(controller.CallDispatches, _services);
                _callStack.Push(dispatchLevel);
            }
        }

        private MemoryEntry getReturnValue(AnalysisCallContext[] callResults)
        {
            var calls = from callResult in callResults select callResult.ProgramPointGraph;

            var returnValue = _functionResolver.ResolveReturnValue(calls.ToArray());

            return returnValue;
        }

        private FlowOutputSet createEmptySet()
        {
            return new FlowOutputSet(createSnapshot());
        }

        private PartialWalker createWalker()
        {
            return new PartialWalker(_expressionEvaluator,_functionResolver);
        }

   

        #endregion

        #region Private utilities

        /// <summary>
        /// Initialize all resolvers and services
        /// </summary>
        private void initialize()
        {
            _expressionEvaluator=createExpressionEvaluator();
            _flowResolver=createFlowResolver();
            _functionResolver=createFunctionResolver();

            _services = new AnalysisServices(createEmptySet,  createWalker,_flowResolver);
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
