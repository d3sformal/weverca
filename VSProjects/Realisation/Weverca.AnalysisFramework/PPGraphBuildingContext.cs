using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using PHP.Core;
using PHP.Core.AST;

using Weverca.ControlFlowGraph;

using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Context providing services for building program point graphs
    /// </summary>
    class PPGraphBuildingContext
    {
        /// <summary>
        /// Graph owning this context
        /// </summary>
        private readonly ProgramPointGraph _owningPPG;

        /// <summary>
        /// Set of all created program points
        /// </summary>
        private readonly HashSet<ProgramPointBase> _createdProgramPoints = new HashSet<ProgramPointBase>();

        /// <summary>
        /// All point blocks that has been created directly from BasicBlocks
        /// <remarks>It is used for finding already created blocks</remarks>
        /// </summary>
        private readonly Dictionary<BasicBlock, PointsBlock> _createdBlocks = new Dictionary<BasicBlock, PointsBlock>();

        /// <summary>
        /// Expression blocks remembered for possibility of sharing
        /// </summary>
        private readonly Dictionary<Expression, PointsBlock> _creadtedExpressionBlocks = new Dictionary<Expression, PointsBlock>();

        /// <summary>
        /// Program points that has been created during computation, without nodes that has been contracted out from graph        
        /// </summary>
        internal IEnumerable<ProgramPointBase> CreatedPoints { get { return _createdProgramPoints; } }

        /// <summary>
        /// Creates context that will be used by owningPPG to build itself
        /// </summary>
        /// <param name="owningPPG">Program point graph using context to build itself</param>
        internal PPGraphBuildingContext(ProgramPointGraph owningPPG)
        {
            _owningPPG = owningPPG;
        }

        #region Points block creation API

        /// <summary>
        /// Creates points block directly from basic block. It is checked that
        /// block is not empty, otherwise contractable block will be created
        /// </summary>
        /// <param name="block">Block witch statements fill created block</param>
        /// <returns>Created points block</returns>
        internal PointsBlock CreateFromBlock(BasicBlock block)
        {
            PointsBlock createdBlock;
            var points = expandStatements(block.Statements);

            if (points.Count > 0)
            {
                createdBlock = PointsBlock.ForBlock(points, block, false);
            }
            else
            {
                var empty = new EmptyProgramPoint();
                reportCreation(empty);
                createdBlock = PointsBlock.ForBlock(new[] { empty }, block, true);
            }

            _createdBlocks.Add(block, createdBlock);
            return createdBlock;
        }

        /// <summary>
        /// Creates points block from given expression
        /// <remarks>It's supposed to be used as parent for assume block</remarks>
        /// </summary>
        /// <param name="expression">Expression which points block will be created</param>
        /// <returns>Created points block</returns>
        internal PointsBlock CreateFromExpression(Expression expression)
        {
            PointsBlock result;
            if (_creadtedExpressionBlocks.TryGetValue(expression, out result))
            {
                return result;
            }

            var points = ElementExpander.ExpandStatement(expression, reportCreation);
            if (points.Any())
            {
                result = PointsBlock.ForExpression(points);
                _creadtedExpressionBlocks.Add(expression, result);
                return result;
            }
            else
            {
                throw new NotSupportedException("Empty expression is not supported");
            }
        }

        /// <summary>
        /// Creates points block from given condition
        /// Points block will have single outgoing block as outcomming edge
        /// </summary>
        /// <param name="condition">Condition which points block is created</param>
        /// <param name="outgoingBlock">Block used as outcomming edge</param>
        /// <param name="expressionValues">Expression parts of assumed condition</param>
        /// <returns>Created points block</returns>
        internal PointsBlock CreateAssumeBlock(AssumptionCondition condition, BasicBlock outgoingBlock, params ValuePoint[] expressionValues)
        {
            var point = new AssumePoint(condition, expressionValues);
            reportCreation(point);

            return PointsBlock.ForPoint(point, new[] { outgoingBlock });
        }

        /// <summary>
        /// Creates points block containing empty program point
        /// <remarks>Created points block is not contractable</remarks>
        /// </summary>
        /// <param name="createdPoint">Created program point</param>
        /// <returns>Created points block</returns>
        internal PointsBlock CreateEmptyPoint(out EmptyProgramPoint createdPoint, params BasicBlock[] outgoingBlocks)
        {
            createdPoint = new EmptyProgramPoint();
            reportCreation(createdPoint);
            return PointsBlock.ForPoint(createdPoint, outgoingBlocks);
        }

        internal TryScopeStartsPoint CreateCatchScopeStart(IEnumerable<CatchBlockDescription> catchBlocks)
        {
            var scopeStart = new TryScopeStartsPoint(catchBlocks);

            reportCreation(scopeStart);

            return scopeStart;
        }

        internal TryScopeEndsPoint CreateCatchScopeEnd(IEnumerable<CatchBlockDescription> catchBlocks)
        {
            var scopeEnd = new TryScopeEndsPoint(catchBlocks);

            reportCreation(scopeEnd);

            return scopeEnd;
        }

        #endregion

        #region Block existence determining API

        /// <summary>
        /// Returns points block that has already been created for block
        /// <remarks>Throws exception when block isn't created yet</remarks>
        /// </summary>
        /// <param name="block">Block which points block is requested</param>
        /// <returns>Existing points block</returns>
        internal PointsBlock GetBlock(BasicBlock block)
        {
            return _createdBlocks[block];
        }

        /// <summary>
        /// Determine that points block for given block is already created
        /// </summary>
        /// <param name="child">Requested block</param>
        /// <returns>True if points block has been already created, false otherwise</returns>
        internal bool IsCreated(BasicBlock child)
        {
            return _createdBlocks.ContainsKey(child);
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Expand given statements int program point base chain
        /// </summary>
        /// <param name="statements">Statements to expand</param>
        /// <returns>Program points created from statements expanding</returns>
        private List<ProgramPointBase> expandStatements(IEnumerable<LangElement> statements)
        {
            var points = new List<ProgramPointBase>();
            ProgramPointBase lastPoint = null;
            foreach (var statement in statements)
            {
                var expanded = ElementExpander.ExpandStatement(statement, reportCreation);

                if (lastPoint != null)
                {
                    //connect before expanded points
                    lastPoint.AddFlowChild(expanded[0]);
                }

                lastPoint = expanded[expanded.Length - 1];

                points.AddRange(expanded);
            }

            return points;
        }

        /// <summary>
        /// All created program points are reported here
        /// </summary>
        /// <param name="createdPoint">Created program point</param>
        private void reportCreation(ProgramPointBase createdPoint)
        {
            _createdProgramPoints.Add(createdPoint);
        }

        #endregion
    }
}
