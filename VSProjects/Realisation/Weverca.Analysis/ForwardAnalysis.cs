using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

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

        PartialWalker _walker;
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
        public Weverca.ControlFlowGraph.ControlFlowGraph EntryMethodGraph { get; private set; }

        /// <summary>
        /// Create forward analysis object for given entry method graph.
        /// </summary>
        /// <param name="entryMethodGraph">Control flow graph of method which is entry point of analysis</param>
        public ForwardAnalysis(Weverca.ControlFlowGraph.ControlFlowGraph entryMethodGraph)
        {
            _services = new AnalysisServices(BlockMerge, createEmptySet, ConfirmAssumption);
            EntryMethodGraph = entryMethodGraph;
            _walker = new PartialWalker(null, null);
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

        public FlowInputSet LastCallOutput { get; private set; }

        /// <summary>
        /// Analyze given statement. 
        /// </summary>
        /// <param name="inSet">Information available before given statement.</param>
        /// <param name="statement">Statement which can add new information.</param>
        /// <param name="outSet">Output information which is known after statement analysis.</param>
        protected abstract void FlowThrough(FlowControler flow, LangElement statement);
        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>
        /// <typeparam name="FlowInfo"></typeparam>
        /// <param name="inSet">Input set which is available before assumption</param>
        /// <param name="condition">Assumption condition.</param>
        /// <param name="outSet">Output set after assumption.</param>
        /// <returns>False if you can prove that condition cannot be ever satisfied, true otherwise.</returns>
        protected abstract bool ConfirmAssumption(FlowControler flow, AssumptionCondition condition);

        /// <summary>
        /// Represents method which merges inSets into outSet
        /// </summary>
        /// <typeparam name="FlowInfo"></typeparam>
        /// <param name="inSet1">Input set to be merged into output</param>
        /// <param name="inSet2">Input set to be merged into output</param>
        /// <param name="outSet">Result of merging</param>
        protected abstract void BlockMerge(FlowInputSet inSet1, FlowInputSet inSet2, FlowOutputSet outSet);
        /// <summary>
        /// All dispatched includes are merged via this call.        
        /// </summary>
        /// <param name="inSets">Input sets from all dispatched includes.</param>
        /// <param name="outSet">Output after merging.</param>
        protected abstract void IncludeMerge(IEnumerable<FlowInputSet> inSets, FlowOutputSet outSet);
        /// <summary>
        /// All dispatched calls are merged via this call.        
        /// </summary>
        /// <param name="inSets">Input sets from all dispatched includes.</param>
        /// <param name="outSet">Output after merging.</param>
        protected abstract void CallMerge(FlowInputSet inSet1, FlowInputSet inSet2, FlowOutputSet outSet);

        /// <summary>
        /// When call dispatch is poped from stack, all dispatches are merged into one. Then ReturnFromCall 
        /// </summary>
        /// <param name="callerInSet"></param>
        /// <param name="callOutput"></param>
        /// <param name="outSet"></param>
        /// <returns></returns>
        protected abstract void ReturnedFromCall(FlowInputSet callerInSet, FlowInputSet callOutput, FlowOutputSet outSet);


        #endregion

        #region Analysis routines

        /// <summary>
        /// Run analyzis starting at EntryMethodGraph
        /// </summary>
        private void analyse()
        {
            checkAlreadyAnalysed();

            _callStack = new AnalysisCallStack(_services);

            var input = new FlowInputSet();
            var entryDispatch = new CallDispatch(EntryMethodGraph.start, input);
            var entryLevel = new CallDispatchLevel(entryDispatch,_services);

            ProgramPointGraph = entryLevel.CurrentContext.ProgramPointGraph;

            _callStack.Push(entryLevel);

            while (!_callStack.IsEmpty)
            {
                var currentContext = _callStack.CurrentContext;
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
                
                flowThroughNextPartial(currentContext);
                currentContext.MoveNextPartial();
            }
        }

        private void flowThroughNextPartial(AnalysisCallContext currentContext)
        {
            //TODO handle dispatching
            var controller = new FlowControler(currentContext.CurrentInputSet, currentContext.CurrentOutputSet);
            _walker.EvalNext(controller, currentContext.CurrentPartial);
            throw new NotImplementedException();
        }

        private void handleCompletedCallDispatch(AnalysisCallContext[] callResult)
        {
            throw new NotImplementedException();
        }

        private FlowOutputSet createEmptySet()
        {
            throw new NotImplementedException();
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
