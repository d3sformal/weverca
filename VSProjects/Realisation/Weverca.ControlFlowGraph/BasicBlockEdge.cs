using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;

namespace Weverca.ControlFlowGraph
{
    /// <summary>
    /// Describes common functionality for edge connecting two basic blocks
    /// </summary>
    public interface IBasicBlockEdge
    {
        /// <summary>
        /// Gets or sets source basic block.
        /// </summary>
        BasicBlock From { set; get; }

        /// <summary>
        /// Gets or sets target basic block.
        /// </summary>
        BasicBlock To { set; get; }

        /// <summary>
        /// Sets new target block and adds edge into its incoming list
        /// </summary>
        /// <param name="newDestination">New target block</param>
        void ChangeDestination(BasicBlock newDestination);
    }

    /// <summary>
    /// Represents conditional basic block connection 
    /// </summary>
    public class ConditionalEdge : IBasicBlockEdge
    {
        /// <summary>
        /// Gets or sets source basic block.
        /// </summary>
        public BasicBlock From { set; get; }

        /// <summary>
        /// Gets or sets target basic block.
        /// </summary>
        public BasicBlock To { set; get; }

        /// <summary>
        /// Gets or sets condition for this edge
        /// </summary>
        public Expression Condition { set; get; }

        /// <summary>
        /// Creates new edge between specified basic blocks and adds it into the basic block incoming and outgoing lists.
        /// </summary>
        /// <param name="From">Source basic block.</param>
        /// <param name="To">Target basic block.</param>
        /// <param name="Condition">Condition for this edge.</param>
        /// <returns>New edge</returns>
        public static ConditionalEdge MakeNewAndConnect(BasicBlock From, BasicBlock To, Expression Condition)
        {
            var edge = new ConditionalEdge(From, To, Condition);
            From.AddOutgoingEdge(edge);
            To.AddIncommingEdge(edge);
            return edge;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalEdge"/> class.
        /// </summary>
        /// <param name="From">Source basic block.</param>
        /// <param name="To">Target basic block.</param>
        /// <param name="Condition">Condition for this edge.</param>
        public ConditionalEdge(BasicBlock From, BasicBlock To, Expression Condition)
        {
            this.From = From;
            this.To = To;
            this.Condition = Condition;
        }

        /// <summary>
        /// Sets new target block and adds edge into its incoming list
        /// </summary>
        /// <param name="newDestination">New target block</param>
        public void ChangeDestination(BasicBlock newDestination)
        {
            To = newDestination;
            newDestination.IncommingEdges.Add(this);
        }
    }

    public class ForEachSpecialEdge : IBasicBlockEdge
    {
        /// <summary>
        /// Gets or sets source basic block.
        /// </summary>
        public BasicBlock From { set; get; }

        /// <summary>
        /// Gets or sets target basic block.
        /// </summary>
        public BasicBlock To { set; get; }

        /// <summary>
        /// Creates new edge between specified basic blocks and adds it into the basic block incoming and outgoing lists.
        /// </summary>
        /// <param name="From">Source basic block.</param>
        /// <param name="To">Target basic block.</param>
        /// <param name="Condition">Condition for this edge.</param>
        /// <returns>New edge</returns>
        public static ForEachSpecialEdge MakeNewAndConnect(BasicBlock From, BasicBlock To)
        {
            var edge = new ForEachSpecialEdge(From, To);
            From.AddOutgoingEdge(edge);
            To.AddIncommingEdge(edge);
            return edge;
        }

        public ForEachSpecialEdge(BasicBlock From, BasicBlock To)
        {
            this.From = From;
            this.To = To;
        }

        /// <summary>
        /// Sets new target block and adds edge into its incoming list
        /// </summary>
        /// <param name="newDestination">New target block</param>
        public void ChangeDestination(BasicBlock newDestination)
        {
            To = newDestination;
            newDestination.IncommingEdges.Add(this);
        }
    }

    /// <summary>
    /// Represents direct basic block connection or the else branch connection
    /// </summary>
    public class DirectEdge : IBasicBlockEdge
    {
        /// <summary>
        /// Gets or sets source basic block.
        /// </summary>
        public BasicBlock From { set; get; }

        /// <summary>
        /// Gets or sets target basic block.
        /// </summary>
        public BasicBlock To { set; get; }


        /// <summary>
        /// Creates new edge between specified basic blocks and adds it into the basic block incoming list and sets direct edge reference.
        /// </summary>
        /// <param name="From">Source basic block.</param>
        /// <param name="To">Target basic block.</param>
        /// <returns>New edge</returns>
        public static DirectEdge MakeNewAndConnect(BasicBlock From, BasicBlock To)
        {
            var edge = new DirectEdge(From, To);
            From.SetDefaultBranch(edge);
            To.AddIncommingEdge(edge);
            return edge;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectEdge"/> class.
        /// </summary>
        /// <param name="From">Source basic block.</param>
        /// <param name="To">Target basic block.</param>
        public DirectEdge(BasicBlock From, BasicBlock To)
        {
            this.From = From;
            this.To = To;
        }

        /// <summary>
        /// Sets new target block and adds edge into its incoming list
        /// </summary>
        /// <param name="newDestination">New target block</param>
        public void ChangeDestination(BasicBlock newDestination)
        {
            To = newDestination;
            newDestination.IncommingEdges.Add(this);
        }
    }

}
