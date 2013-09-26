using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.ControlFlowGraph;

using Weverca.Analysis.ProgramPoints;

namespace Weverca.Analysis
{
    /// <summary>
    /// Wrapps program points to behave like solid block. This abstraction also allows doing graph contractions for PPG vertices reduction.
    /// </summary>
    class PointsBlock
    {
        #region Private members

        /// <summary>
        /// Program points that are contained in points block (ordered by flow order)
        /// </summary>
        private readonly List<ProgramPointBase> _containedPoints = new List<ProgramPointBase>();

        /// <summary>
        /// Determine that this block needs contraction
        /// <remarks>Contractions are processed to reduce graph about unneeded empty points</remarks>
        /// </summary>
        private bool _needsContraction;

        #endregion

        #region Internal members

        /// <summary>
        /// Outgoing basic blocks (their points will be connected as current block flow children)
        /// </summary>
        internal readonly IEnumerable<BasicBlock> OutgoingBlocks;

        /// <summary>
        /// Outgoing conditional edges (their points will be connected as childs via assume blocks)
        /// </summary>
        internal readonly IEnumerable<ConditionalEdge> ConditionalEdges;

        /// <summary>
        /// Outgoing block used as default branch for conditional edges
        /// </summary>
        internal readonly BasicBlock Default;

        /// <summary>
        /// Last program point in contained program points sequence
        /// </summary>
        internal ProgramPointBase LastPoint { get { return _containedPoints[_containedPoints.Count - 1]; } }

        #endregion

        /// <summary>
        /// Creates points block from given edge blocks
        /// </summary>
        /// <param name="outgoingBlocks">Outgoing basic blocks (their points will be connected as current block flow children)</param>
        /// <param name="conditionalEdges">Outgoing conditional edges (their points will be connected as childs via assume blocks)</param>
        /// <param name="defaultBlock">Outgoing block used as default branch for conditional edges</param>
        private PointsBlock(IEnumerable<BasicBlock> outgoingBlocks, IEnumerable<ConditionalEdge> conditionalEdges, BasicBlock defaultBlock)
        {
            OutgoingBlocks = outgoingBlocks;
            ConditionalEdges = conditionalEdges;
            Default = defaultBlock;
        }

        #region Points block creating

        /// <summary>
        /// Creates non-contractable points block for single program point
        /// </summary>
        /// <param name="createdPoint">Program points which block is created</param>
        /// <param name="outgoingBlocks">Outgoing edges from current block</param>
        /// <returns>Created block</returns>
        internal static PointsBlock ForPoint(ProgramPointBase createdPoint, IEnumerable<BasicBlock> outgoingBlocks)
        {
            var pointsBlock = new PointsBlock(outgoingBlocks, new ConditionalEdge[0], null);
            pointsBlock._containedPoints.Add(createdPoint);
            pointsBlock._needsContraction = false;
            return pointsBlock;
        }

        /// <summary>
        /// Creates points block from statement points belonging to given block
        /// </summary>
        /// <param name="statementPoints">Statement points belonging to given block</param>
        /// <param name="block">Block which points block is created</param>
        /// <param name="needsContraction">Determine that given block will be contractable</param>
        /// <returns>Created block</returns>
        internal static PointsBlock ForBlock(IEnumerable<ProgramPointBase> statementPoints, BasicBlock block, bool needsContraction)
        {
            var defaultBlock = block.DefaultBranch == null ? null : block.DefaultBranch.To;
            var pointsBlock = new PointsBlock(new BasicBlock[0], block.OutgoingEdges, defaultBlock);

            pointsBlock._containedPoints.AddRange(statementPoints);
            pointsBlock._needsContraction = needsContraction;

            return pointsBlock;
        }

        /// <summary>
        /// Creates uncontractable points block from expression points
        /// </summary>
        /// <param name="expressionPoints">Points representing expression</param>
        /// <returns>Created block</returns>
        internal static PointsBlock ForExpression(IEnumerable<ProgramPointBase> expressionPoints)
        {
            //for expression there are no outgoing edges 
            var pointsBlock = new PointsBlock(new BasicBlock[0], new ConditionalEdge[0], null);
            pointsBlock._containedPoints.AddRange(expressionPoints);

            return pointsBlock;
        }
        #endregion

        /// <summary>
        /// Add child points block for current points block
        /// <remarks>Program points contained in point blocks are connected with flow edge</remarks>
        /// </summary>
        /// <param name="childBlock">Added child points block</param>
        internal void AddChild(PointsBlock childBlock)
        {
            LastPoint.AddFlowChild(childBlock._containedPoints[0]);
        }
    }
}
