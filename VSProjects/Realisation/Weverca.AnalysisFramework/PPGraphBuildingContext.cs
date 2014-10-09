/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


﻿using System;
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
        private readonly Dictionary<BasicBlock, PointsBlock> _createdBasicBlocks = new Dictionary<BasicBlock, PointsBlock>();

        /// <summary>
        /// All point blocks that has been created
        /// </summary>
        private readonly List<PointsBlock> _createdBlocks = new List<PointsBlock>();

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

        internal void ConnectBlocks()
        {
            foreach (var block in _createdBlocks)
            {
                block.Connect();
            }
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

            _createdBlocks.Add(createdBlock);
            _createdBasicBlocks.Add(block, createdBlock);
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
            PointsBlock createdBlock;
            if (_creadtedExpressionBlocks.TryGetValue(expression, out createdBlock))
            {
                return createdBlock;
            }

            var points = ElementExpander.ExpandStatement(expression, reportCreation);
            if (points.Any())
            {
                createdBlock = PointsBlock.ForExpression(points);                
                _creadtedExpressionBlocks.Add(expression, createdBlock);

                _createdBlocks.Add(createdBlock);
                return createdBlock;
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

            var createdBlock= PointsBlock.ForPoint(point, new[] { outgoingBlock });
            _createdBlocks.Add(createdBlock);

            return createdBlock;
        }

        /// <summary>
        /// Creates points block containing just empty program point and returns this point.
        /// <remarks>Created points block is not contractable</remarks>
        /// </summary>
        /// <param name="createdPoint">Created program point</param>
        /// <param name="outgoingBlocks">Blocks used as outcomming edges</param>
        /// <returns>Created points block</returns>
        internal PointsBlock CreateEmptyBlock(out ProgramPointBase createdPoint, params BasicBlock[] outgoingBlocks)
        {
            createdPoint = new EmptyProgramPoint();
            return CreateBlockFromProgramPoint(createdPoint, outgoingBlocks);
        }

        /// <summary>
        /// Creates points block containing just subprogram entry program point and returns this point.
        /// <remarks>Created points block is not contractable</remarks>
        /// </summary>
        /// <param name="createdPoint">Created program point</param>
        /// <param name="outgoingBlocks">Blocks used as outcomming edges</param>
        /// <returns>Created points block</returns>
        internal PointsBlock CreateSubprogramEntryBlock(out ProgramPointBase createdPoint, params BasicBlock[] outgoingBlocks)
        {
            createdPoint = new SubprogramEntryPoint();
            return CreateBlockFromProgramPoint(createdPoint, outgoingBlocks);
        }

        /// <summary>
        /// Creates points block containing just given program point.
        /// <remarks>Created points block is not contractable</remarks>
        /// </summary>
        /// <param name="createdPoint">Program point that will be contained in created block.</param>
        /// <param name="outgoingBlocks">Blocks used as outcomming edges</param>
        /// <returns>Created points block</returns>
        private PointsBlock CreateBlockFromProgramPoint(ProgramPointBase newPoint, params BasicBlock[] outgoingBlocks)
        {
            reportCreation(newPoint);

            var createdBlock = PointsBlock.ForPoint(newPoint, outgoingBlocks);
            _createdBlocks.Add(createdBlock);

            return createdBlock;
        }

        /// <summary>
        /// Create try scope start point from given catch blocks
        /// </summary>
        /// <param name="catchBlocks">Catch blocks that are valid within try scope start</param>
        /// <returns>Created try scope start point</returns>
        internal TryScopeStartsPoint CreateCatchScopeStart(IEnumerable<CatchBlockDescription> catchBlocks)
        {
            var scopeStart = new TryScopeStartsPoint(catchBlocks);

            reportCreation(scopeStart);

            return scopeStart;
        }

        /// <summary>
        /// Create try scope end point from given catch blocks
        /// </summary>
        /// <param name="catchBlocks">Catch blocks which are ending scope</param>
        /// <returns>Created try scope end point</returns>
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
            return _createdBasicBlocks[block];
        }

        /// <summary>
        /// Determine that points block for given block is already created
        /// </summary>
        /// <param name="child">Requested block</param>
        /// <returns>True if points block has been already created, false otherwise</returns>
        internal bool IsCreated(BasicBlock child)
        {
            return _createdBasicBlocks.ContainsKey(child);
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