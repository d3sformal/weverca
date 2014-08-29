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


/*
Copyright (c) 2012-2014 David Hauzar and Mirek Vodolan.

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

using Weverca.ControlFlowGraph;

using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
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

        /// <summary>
        /// Determine that points block has already been connected
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Children blocks of current block
        /// </summary>
        private readonly List<PointsBlock> _childrenBlocks = new List<PointsBlock>();

        /// <summary>
        /// Parent blocks of current block
        /// </summary>
        private readonly List<PointsBlock> _parentBlocks = new List<PointsBlock>();

        #endregion

        #region Internal members

        /// <summary>
        /// Outgoing basic blocks (their points will be connected as current block flow children)
        /// </summary>
        internal readonly IEnumerable<BasicBlock> OutgoingBlocks;

        /// <summary>
        /// Outgoing conditional edges (their points will be connected as childs via assume blocks)
        /// </summary>
        internal readonly IEnumerable<IBasicBlockEdge> ConditionalEdges;

        /// <summary>
        /// Outgoing block used as default branch for conditional edges
        /// </summary>
        internal readonly BasicBlock Default;

        /// <summary>
        /// Last program point in contained program points sequence
        /// </summary>
        internal ProgramPointBase LastPoint { get { return _containedPoints[_containedPoints.Count - 1]; } }

        /// <summary>
        /// First program point in contained program points sequence
        /// </summary>
        internal ProgramPointBase FirstPoint { get { return _containedPoints[0]; } }

        #endregion

        /// <summary>
        /// Creates points block from given edge blocks
        /// </summary>
        /// <param name="outgoingBlocks">Outgoing basic blocks (their points will be connected as current block flow children)</param>
        /// <param name="conditionalEdges">Outgoing conditional edges (their points will be connected as childs via assume blocks)</param>
        /// <param name="defaultBlock">Outgoing block used as default branch for conditional edges</param>
        private PointsBlock(IEnumerable<BasicBlock> outgoingBlocks, IEnumerable<IBasicBlockEdge> conditionalEdges, BasicBlock defaultBlock)
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
        /// <remarks>Program points contained in point blocks are connected with flow edge after connect is called</remarks>
        /// </summary>
        /// <param name="childBlock">Added child points block</param>
        internal void AddChild(PointsBlock childBlock)
        {
            childBlock._parentBlocks.Add(this);
            _childrenBlocks.Add(childBlock);
        }

        /// <summary>
        /// Disallow making contractions for current block
        /// </summary>
        internal void DisallowContraction()
        {
            _needsContraction = false;
        }

        /// <summary>
        /// Prepend program point as first of contained points.
        /// Is connected with flow edge to FirstPoint
        /// </summary>
        /// <param name="programPoint">Prepended point</param>
        internal void PreprendFlowWith(ProgramPointBase programPoint)
        {
            var first = FirstPoint;

            programPoint.AddFlowChild(first);
            _containedPoints.Insert(0, programPoint);
        }

        /// <summary>
        /// Append program point as last of contained points
        /// Is connected with flow edge to LastPoint
        /// </summary>
        /// <param name="programPoint">Appended point</param>
        internal void AppendFlow(ProgramPointBase programPoint)
        {
            LastPoint.AddFlowChild(programPoint);

            _containedPoints.Add(programPoint);
        }

        /// <summary>
        /// Connect with flow edges to children points
        /// </summary>
        internal void Connect()
        {
            if (_isConnected)
            {
                throw new NotSupportedException("Cannot connect block twice");
            }

            _isConnected = true;

            foreach (var child in _childrenBlocks)
            {
                LastPoint.AddFlowChild(child.FirstPoint);
            }
        }
    }
}