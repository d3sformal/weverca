using PHP.ControlFlow;
using PHP.Core.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHP.Core.ControlFlow
{
    /// <summary>
    /// Defines interface for an object that is capable of reporting 
    /// whether its state has been changed since the last 
    /// invocation of <see cref="ResetHasChanged"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For data flow analysis we need to know whether the transfer function 
    /// (in out terminology <see cref="IFlowAnalysis.FlowThrough"/>) has changed 
    /// its argument so that we know when we reached the fixed point.
    /// </para>
    /// <para>
    /// Soot makes a copy of the data flow before invoking the transfer function 
    /// and then uses equals method to find out whether the output is the same 
    /// as the copy. From the performance reasons, we abolished this approach as 
    /// it makes unnecessary copies of possibly large objects.
    /// </para>
    /// </remarks>
    public interface IChangeable
    {
        bool HasChanged { get; }

        void ResetHasChanged();
    }

    public enum FlowAnalysisDirection
    {
        Forward,
        Backward
    }

    /// <summary>
    /// Represents a flow analysis that can be either forward or backward.
    /// </summary>
    public interface IFlowAnalysis
    {
        FlowAnalysisDirection Direction { get; }

        IChangeable GetInitialFlow();

        /// <summary>
        /// Merges its two arguments into the first one.
        /// </summary>
        /// <param name="resultDataFlow">Result and the first argument for the merge operation.</param>
        /// <param name="dataFlow">The second argument for the merge operation.</param>
        void Merge(IChangeable resultDataFlow, IChangeable dataFlow);

        void Copy(IChangeable source, IChangeable dest);

        void FlowThrough(IChangeable dataFlow, LangElement statement);
    }

    /// <summary>
    /// Generic implementation of <see cref="IFlowAnalysis"/>.
    /// </summary>
    /// <remarks>
    /// Implements the common pattern of easy to use non-generic interface and 
    /// easy to implement generic abstract class.
    /// </remarks>
    /// <typeparam name="T">The type of the data flow information (IN and OUT sets). 
    /// It must not be a struct as its internal state is expected to be changed in some methods and 
    /// and this change must be visible to outside.</typeparam>
    public abstract class FlowAnalysis<T> : IFlowAnalysis where T : class, IChangeable
    {
        public FlowAnalysis(FlowAnalysisDirection direction)
        {
            this.Direction = direction;
        }

        public FlowAnalysisDirection Direction { get; private set;  }

        protected abstract T GetInitialFlow();

        protected abstract void Merge(T resultDataFlow, T dataFlow);

        protected abstract void FlowThrough(T set, LangElement statement);

        protected abstract void Copy(T source, T dest);

        IChangeable IFlowAnalysis.GetInitialFlow()
        {
            return this.GetInitialFlow();
        }

        void IFlowAnalysis.Merge(IChangeable resultDataFlow, IChangeable dataFlow)
        {
            this.Merge((T)resultDataFlow, (T)dataFlow);
        }

        void IFlowAnalysis.FlowThrough(IChangeable dataFlow, LangElement statement)
        {
            this.FlowThrough((T)dataFlow, statement);
        }

        void IFlowAnalysis.Copy(IChangeable source, IChangeable dest)
        {
            this.Copy((T)source, (T)dest);
        }
    }

    public interface IFlowAnalysisRunner
    {
        void Run(IFlowAnalysis analysis, ControlFlowGraph graph);
    }

    public class WorklistAlgorithm : IFlowAnalysisRunner
    {
        public void Run(IFlowAnalysis analysis, ControlFlowGraph graph)
        {
            // We will use BasicBlock.Tag property to store each BasicBlock's data flow
            var blocks = graph.Blocks;
            for (int i = 0; i < blocks.Length; i++)
            {
                 blocks[i].Tag = analysis.GetInitialFlow();
            }

            FlowThrough(analysis, graph.Start, (IChangeable)graph.Start.Tag);

            // We will use Visited to indicate that the item is already in the queue so that 
            // we don't add items again if they are already in the queue
            var toQueue = graph.BlocksInDFSOrder.Where(x => x.Previous.Any());
            foreach (var block in toQueue)
                block.Visited = true;

            var worklist = new Queue<BasicBlock>(toQueue);
            while (worklist.Any())
            {
                var current = worklist.Dequeue();
                current.Visited = false;

                // compute the IN from all predecessors' OUTs
                var dataFlow = (IChangeable)current.Tag;

                // If we are out own predecessor, save the copying of the data
                if (current != current.Previous.First())
                    analysis.Copy(dataFlow, (IChangeable)current.Previous.First().Tag);

                if (current.Previous.Count() > 1)
                {
                    foreach (var prev in current.Previous.Skip(1))
                        analysis.Merge(dataFlow, (IChangeable)prev.Tag);
                }

                // computer the transfer function, and if it changes its argument add all the 
                // successors to the worklist (but only those that aren't already there)
                if (FlowThrough(analysis, current, dataFlow))
                {
                    foreach (var next in current.Next.Where(x => !x.Visited))
                    {
                        next.Visited = true;
                        worklist.Enqueue(next);
                    }
                }
            }
        }

        private static bool FlowThrough(IFlowAnalysis analysis, BasicBlock block, IChangeable dataFlow)
        {
            dataFlow.ResetHasChanged();
            foreach (var stmt in block.GetThreeAddressStatements())
                analysis.FlowThrough(dataFlow, stmt);

            return dataFlow.HasChanged;
        }
    }
}
