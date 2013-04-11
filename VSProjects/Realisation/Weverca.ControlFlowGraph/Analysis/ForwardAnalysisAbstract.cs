using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Provide forward CFG analysis API.
    /// !!UNDER CONSTRUCTION, API CAN BE HEAVILY CHANGED!!
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    public abstract class ForwardAnalysisAbstract<FlowInfo>
    {
        #region Private members

        /// <summary>
        /// Currently analyzed call stack.
        /// </summary>
        AnalysisCallStack<FlowInfo> _callStack;
        /// <summary>
        /// Analysis services provider. Is used for hiding some API.
        /// </summary>
        AnalysisServices<FlowInfo> _services;

        #endregion

        /// <summary>
        /// Determine that analysis has been already runned.
        /// </summary>
        public bool IsAnalysed { get; private set; }

        /// <summary>
        /// Root output from analysis
        /// </summary>
        public ProgramPointGraph<FlowInfo> ProgramPointGraph { get; private set; }
        /// <summary>
        /// Control flow graph of method which is entry point of analysis.
        /// </summary>
        public ControlFlowGraph EntryMethodGraph { get; private set; }

        /// <summary>
        /// Create forward analysis object for given entry method graph.
        /// </summary>
        /// <param name="entryMethodGraph">Control flow graph of method which is entry point of analysis</param>
        public ForwardAnalysisAbstract(ControlFlowGraph entryMethodGraph)
        {
            _services = new AnalysisServices<FlowInfo>(BlockMerge, NewEmptySet, ConfirmAssumption);
            EntryMethodGraph = entryMethodGraph;
        }

        /// <summary>
        /// Run analysis on EntryMethodGraph
        /// </summary>
        public void Analyse()
        {
            checkAlreadyAnalysed();
            analyse();
            IsAnalysed = true;
        }

        #region Available API for analysis handling

        public FlowInputSet<FlowInfo> LastCallOutput { get; private set; }

        /// <summary>
        /// Analyze given statement. 
        /// </summary>
        /// <param name="inSet">Information available before given statement.</param>
        /// <param name="statement">Statement which can add new information.</param>
        /// <param name="outSet">Output information which is known after statement analysis.</param>
        protected abstract void FlowThrough(FlowControler<FlowInfo> flow, LangElement statement);
        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>
        /// <typeparam name="FlowInfo"></typeparam>
        /// <param name="inSet">Input set which is available before assumption</param>
        /// <param name="condition">Assumption condition.</param>
        /// <param name="outSet">Output set after assumption.</param>
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        protected abstract bool ConfirmAssumption(FlowControler<FlowInfo> flow, AssumptionCondition condition);

        /// <summary>
        /// Represents method which merges inSets into outSet
        /// </summary>
        /// <typeparam name="FlowInfo"></typeparam>
        /// <param name="inSet1">Input set to be merged into output</param>
        /// <param name="inSet2">Input set to be merged into output</param>
        /// <param name="outSet">Result of merging</param>
        protected abstract void BlockMerge(FlowInputSet<FlowInfo> inSet1, FlowInputSet<FlowInfo> inSet2, FlowOutputSet<FlowInfo> outSet);
        /// <summary>
        /// All dispatched includes are merged via this call.        
        /// </summary>
        /// <param name="inSets">Input sets from all dispatched includes.</param>
        /// <param name="outSet">Output after merging.</param>
        protected abstract void IncludeMerge(IEnumerable<FlowInputSet<FlowInfo>> inSets, FlowOutputSet<FlowInfo> outSet);
        /// <summary>
        /// All dispatched calls are merged via this call.        
        /// </summary>
        /// <param name="inSets">Input sets from all dispatched includes.</param>
        /// <param name="outSet">Output after merging.</param>
        protected abstract void CallMerge(FlowInputSet<FlowInfo> inSet1, FlowInputSet<FlowInfo> inSet2, FlowOutputSet<FlowInfo> outSet);

        /// <summary>
        /// When call dispatch is poped from stack, all dispatches are merged into one. Then ReturnFromCall 
        /// </summary>
        /// <param name="callerInSet"></param>
        /// <param name="callOutput"></param>
        /// <param name="outSet"></param>
        /// <returns></returns>
        protected abstract void ReturnedFromCall(FlowInputSet<FlowInfo> callerInSet, FlowInputSet<FlowInfo> callOutput, FlowOutputSet<FlowInfo> outSet);

        /// <summary>
        /// Creates set without any stored information.
        /// </summary>
        /// <returns>New empty set.</returns>
        protected virtual FlowOutputSet<FlowInfo> NewEmptySet()
        {
            return new FlowOutputSet<FlowInfo>();
        }

        #endregion

        #region Analysis routines

        /// <summary>
        /// Run analyzis starting at EntryMethodGraph
        /// </summary>
        private void analyse()
        {
            //TODO input data handling
            var input = NewEmptySet();

            _callStack = new AnalysisCallStack<FlowInfo>();

            var entryDispatch = new CallDispatch<FlowInfo>(EntryMethodGraph.start, input);
            var entryLevel = new CallDispatchLevel<FlowInfo>(entryDispatch, _services);

            ProgramPointGraph = entryLevel.CurrentContext.ProgramPointGraph;

            _callStack.Push(entryLevel);

            while (!_callStack.IsEmpty)
            {
                var currentContext = _callStack.Peek;
                if (currentContext.IsComplete)
                {
                    //pop out empty context
                    //TODO collect result of analysis 

                    if (!_callStack.CurrentLevel.ShiftToNext())
                    {
                        var callResult = _callStack.CurrentLevel.GetResult();
                        _callStack.Pop();
                        handleCompletedCallDispatch(callResult);
                    }
                    continue;
                }

                flowThroughNextStmt(currentContext);
            }
        }

        private void handleCompletedCallDispatch(AnalysisCallContext<FlowInfo>[] contexts)
        {
            var mergedCallContext = contexts[0].ProgramPointGraph.End.OutSet;
            if (contexts.Length > 1)
            {
                for (int i = 1; i < contexts.Length; ++i)
                {
                    var contextResult = contexts[i].ProgramPointGraph.End.OutSet;
                    var callMergeOutSet = NewEmptySet();
                    CallMerge(mergedCallContext, contextResult, callMergeOutSet);
                    mergedCallContext = callMergeOutSet;
                }
            }

            LastCallOutput = mergedCallContext;

            if (!_callStack.IsEmpty)
            {
                var currentInSet = _callStack.Peek.CurrentInputSet;
                var currentOutSet = _callStack.Peek.CurrentOutputSetUpdate;
                ReturnedFromCall(currentInSet, LastCallOutput, currentOutSet);
                _callStack.Peek.UpdateOutputSet(currentOutSet);
            }
        }

        /// <summary>
        /// Flow through next statement in given context.
        /// NOTE: Can add dispatching into callStack
        /// </summary>
        /// <param name="context"></param>
        private void flowThroughNextStmt(AnalysisCallContext<FlowInfo> context)
        {
            var inSet = context.CurrentInputSet;
            var outSet = context.CurrentOutputSetUpdate;
            var statement = context.CurrentStatement;
            
            var flow = new FlowControler<FlowInfo>(inSet,outSet);
            FlowThrough(flow, statement);
            context.UpdateOutputSet(flow.OutSet);

            //Call output is available only once
            LastCallOutput = null;

            if (flow.HasCallDispatch)
            {
                var level = new CallDispatchLevel<FlowInfo>(flow.CallDispatches, _services);
                _callStack.Push(level);
                return;
            }

            //we will skip to next statement only if there is no dispatch
            context.SkipToNextStatement();          
        }

        #endregion

        #region Private utilities

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
