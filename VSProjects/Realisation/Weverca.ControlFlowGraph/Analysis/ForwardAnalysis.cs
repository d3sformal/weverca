using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{
    public abstract class ForwardAnalysis<FlowInfo>
    {
        #region Private members
        AnalysisCallStack<FlowInfo> _callStack;
        AnalysisServices<FlowInfo> _services;
        #endregion

        /// <summary>
        /// Determine that analysis has been already runned.
        /// </summary>
        public bool IsAnalysed{get;private set;}

        /// <summary>
        /// Root output from analysis
        /// </summary>
        public ProgramPoint<FlowInfo> RootEndPoint { get; private set; }
        /// <summary>
        /// Control flow graph of method which is entry point of analysis.
        /// </summary>
        public ControlFlowGraph EntryMethodGraph{get; private set;}

        /// <summary>
        /// Create forward analysis object for given entry method graph.
        /// </summary>
        /// <param name="entryMethodGraph">Control flow graph of method which is entry point of analysis</param>
        public ForwardAnalysis(ControlFlowGraph entryMethodGraph){
            _services = new AnalysisServices<FlowInfo>(BlockMerge, NewEmptySet,ConfirmAssumption);
            EntryMethodGraph=entryMethodGraph;
        }

        /// <summary>
        /// Run analysis on EntryMethodGraph
        /// </summary>
        public void Analyse(){
            checkAlreadyAnalysed();
            analyse();
            IsAnalysed = true;
        }

        #region Abstract API for analysis handling

        /// <summary>
        /// Analyze given statement. 
        /// </summary>
        /// <param name="inSet">Information available before given statement.</param>
        /// <param name="statement">Statement which can add new information.</param>
        /// <param name="outSet">Output information which is known after statement analysis.</param>
        protected abstract void FlowThrough(FlowInputSet<FlowInfo> inSet, LangElement statement, FlowOutputSet<FlowInfo> outSet);

        /// <summary>
        /// Create block dispatching from nextBlocks.
        /// </summary>
        /// <param name="inSet">Available information for dispatch.</param>
        /// <param name="nextBlocks">Possible blocks to dispatch.</param>
        /// <returns>All blocks that will be analyzed and merged.</returns>
        protected abstract IEnumerable<BlockDispatch> BlockDispatch(FlowInputSet<FlowInfo> inSet, IEnumerable<ConditionalEdge> nextBlocks);

        /// <summary>
        /// All dispatched blocks are merged via this call.        
        /// </summary>
        /// <param name="inSets">Input sets from all dispatched blocks.</param>
        /// <param name="outSet">Output after merging.</param>
        protected abstract void BlockMerge(FlowInputSet<FlowInfo> inSet1,FlowInputSet<FlowInfo> inSet2,FlowOutputSet<FlowInfo> outSet);
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
        protected abstract void CallMerge(IEnumerable<FlowInputSet<FlowInfo>> inSets, FlowOutputSet<FlowInfo> outSet);

        /// <summary>
        /// Determine that given assumptionCondition is valid according to inSet. Out set will be used for assumpted flow.
        /// </summary>
        /// <param name="inSet"></param>
        /// <param name="assumptionCondition"></param>
        /// <param name="outSet"></param>
        /// <returns></returns>
        protected abstract bool ConfirmAssumption(FlowInputSet<FlowInfo> inSet,AssumptionCondition condition,FlowOutputSet<FlowInfo> outSet);
        /// <summary>
        /// Creates set without any stored information.
        /// </summary>
        /// <returns>New empty set.</returns>
        protected abstract FlowOutputSet<FlowInfo> NewEmptySet();

        #endregion

        #region Analysis routines

        /// <summary>
        /// Run analyzis starting at EntryMethodGraph
        /// </summary>
        private void analyse()
        {
            var entryContext = new AnalysisCallContext<FlowInfo>(EntryMethodGraph,_services);

            _callStack = new AnalysisCallStack<FlowInfo>();            
            _callStack.Push(entryContext);

            while (!_callStack.IsEmpty)
            {
                var currentContext = _callStack.Peek;
                if (currentContext.IsComplete)
                {
                    //pop out empty context
                    //TODO collect result of analysis 
                    //TODO collect return value
                    //TODO call merge
                    _callStack.Pop();
                    continue;
                }

                flowThroughNextStmt(currentContext);
            }
            RootEndPoint=entryContext.EndProgramPoint;
        }

        /// <summary>
        /// Flow through next statement in given context.
        /// NOTE: Can add dispatching into callStack
        /// </summary>
        /// <param name="context"></param>
        private void flowThroughNextStmt(AnalysisCallContext<FlowInfo> context)
        {
            var inSet = context.CurrentInputSet;
            var outSetOld = context.CurrentOutputSet;
            var statement = context.CurrentStatement;
            

            var outSet = outSetOld.Copy();
            FlowThrough(inSet, statement, outSet);


            context.UpdateOutputSet(outSet);
            context.SkipToNextStatement();            

            _callStack.AddDispathes(outSet.CallDispatches);                    
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
