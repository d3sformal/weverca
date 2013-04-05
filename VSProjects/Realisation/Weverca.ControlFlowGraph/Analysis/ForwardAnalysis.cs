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
        #endregion

        /// <summary>
        /// Determine that analysis has been already runned.
        /// </summary>
        public bool IsAnalysed{get;private set;}
        /// <summary>
        /// Control flow graph of method which is entry point of analysis.
        /// </summary>
        public ControlFlowGraph EntryMethodGraph{get; private set;}

        /// <summary>
        /// Create forward analysis object for given entry method graph.
        /// </summary>
        /// <param name="entryMethodGraph">Control flow graph of method which is entry point of analysis</param>
        public ForwardAnalysis(ControlFlowGraph entryMethodGraph){
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
        protected abstract void BlockMerge(IEnumerable<FlowInputSet<FlowInfo>> inSets,FlowOutputSet<FlowInfo> outSet);
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
            var entryContext = new AnalysisCallContext<FlowInfo>(EntryMethodGraph);

            _callStack = new AnalysisCallStack<FlowInfo>();            
            _callStack.Push(entryContext);

            while (!_callStack.IsEmpty)
            {
                var currentContext = _callStack.Peek;
                if (currentContext.IsEmpty)
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
        }

        /// <summary>
        /// Flow through next statement in given context.
        /// NOTE: Can add dispatching into callStack
        /// </summary>
        /// <param name="context"></param>
        private void flowThroughNextStmt(AnalysisCallContext<FlowInfo> context)
        {
            var inSet = context.CurrentInputSet;
            var statement = context.DequeueNextStatement();
            var outSet = NewEmptySet();

            FlowThrough(inSet, statement, outSet);
                        
            _callStack.AddDispathes(outSet.CallDispatches);        
            //TODO block merge, dispatch
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
