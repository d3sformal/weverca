/*
Copyright (c) 2012-2014 Marcel Kikta and David Hauzar.

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
    /// Types of basic block edges.
    /// </summary>
    public enum BasicBlockEdgeTypes
    {
        /// <summary>
        /// Represents direct basic block connection or the else branch connection
        /// </summary>
        DIRECT,
        /// <summary>
        /// Represents conditional basic block connection 
        /// </summary>
        CONDITIONAL,
        /// <summary>
        /// Special Edge, which represents that condition in foreach.
        /// </summary>
        FOREACH
    }

    /// <summary>
    /// Describes common functionality for edge connecting two basic blocks
    /// </summary>
    public abstract class BasicBlockEdge
    {
        /// <summary>
        /// Gets or sets source basic block.
        /// </summary>
        public abstract BasicBlock From { set; get; }

        /// <summary>
        /// Gets or sets target basic block.
        /// </summary>
        public abstract BasicBlock To { set; get; }

        /// <summary>
        /// Gets the type of the edge.
        /// </summary>
        public abstract BasicBlockEdgeTypes EdgeType { get; }

        /// <summary>
        /// Gets condition for this edge. Valid only if the edge is of TypeConditional.
        /// </summary>
        public virtual Expression Condition { get { throw new NotSupportedException("This attribute is not valid for the edge of this type."); } }


        #region Interface for connecting edges to basic blocks
        /// <summary>
        /// Sets new target block and adds edge into its incoming list
        /// </summary>
        /// <param name="newDestination">New target block</param>
        internal abstract void ChangeDestination(BasicBlock newDestination);

        /// <summary>
        /// CreatesForEachSpecialEdge between specified basic blocks and adds it into the basic block incoming and outgoing lists.
        /// </summary>
        /// <param name="From">Source basic block.</param>
        /// <param name="To">Target basic block.</param>
        /// <returns>New edge</returns>
        internal static void ConnectForeachEdge(BasicBlock From, BasicBlock To)
        {
            var edge = new ForEachSpecialEdge(From, To);
            From.AddOutgoingEdge(edge);
            To.AddIncommingEdge(edge);
        }

        /// <summary>
        /// Creates new edge between specified basic blocks and adds it into the basic block incoming list and sets direct edge reference.
        /// </summary>
        /// <param name="From">Source basic block.</param>
        /// <param name="To">Target basic block.</param>
        /// <returns>New edge</returns>
        internal static void ConnectDirectEdge(BasicBlock From, BasicBlock To)
        {
            var edge = new DirectEdge(From, To);
            From.SetDefaultBranch(edge);
            To.AddIncommingEdge(edge);
        }

        /// <summary>
        /// Connects TrueBranch and FalseBranch to From. TrueBranch is followed from From if the condition holds, 
        /// FalseBranch is followed from From if the condition does not hold.
        /// 
        /// If decompose is true, it decomposes the condition expression using logical operations with respect to 
        /// shortcircuit evaluation.
        /// Note that analyzer now expects that the condition expressions are decomposed and it no longer supports
        /// condition expressions that are not decomposed.
        /// </summary>
        /// <param name="condition">the condition of the branching.</param>
        /// <param name="From">the basic block where from which the branching starts.</param>
        /// <param name="TrueBranch">the branch which is taken if the condition holds.</param>
        /// <param name="FalseBranch">the branch which is taken if the condition does not hold.</param>
        /// <param name="decompose"></param>
        internal static void ConnectConditionalBranching(Expression condition, BasicBlock From, BasicBlock TrueBranch, BasicBlock FalseBranch, bool decompose = true)
        {
            var binaryCondition = condition as BinaryEx;
            if (!decompose || binaryCondition == null || (binaryCondition.PublicOperation != Operations.And && binaryCondition.PublicOperation != Operations.Or && binaryCondition.PublicOperation != Operations.Xor))
            {
                ConditionalEdge.AddConditionalEdge(From, TrueBranch, condition);
                DirectEdge.ConnectDirectEdge(From, FalseBranch);
                return;
            }

            BasicBlock intermediateBasicBlock = null;
            switch (binaryCondition.PublicOperation)
            {
                case Operations.And:
                    intermediateBasicBlock = new BasicBlock();
                    ConnectConditionalBranching(binaryCondition.LeftExpr, From, intermediateBasicBlock, FalseBranch);
                    From = intermediateBasicBlock;
                    ConnectConditionalBranching(binaryCondition.RightExpr, From, TrueBranch, FalseBranch);
                    break;
                case Operations.Or:
                    intermediateBasicBlock = new BasicBlock();
                    ConnectConditionalBranching(binaryCondition.LeftExpr, From, TrueBranch, intermediateBasicBlock);
                    From = intermediateBasicBlock;
                    ConnectConditionalBranching(binaryCondition.RightExpr, From, TrueBranch, FalseBranch);
                    break;
                case Operations.Xor:
                    // Expands A xor B to (A and !B) || (!A and B)

                    // Expansion expands A to A and !A and B to B and !B
                    // For A and !A we the AST elements cannot be shared (must be unique) - the same for B and !B
                    // We thus make copies of ast elements of left and right expression and use the copies to represent !A and !B
                    var leftNegation = new UnaryEx(Operations.LogicNegation, CFGVisitor.DeepCopyAstExpressionCopyVisitor(binaryCondition.LeftExpr));
                    var rightNegation = new UnaryEx(Operations.LogicNegation, CFGVisitor.DeepCopyAstExpressionCopyVisitor(binaryCondition.RightExpr));

                    var leftExpression = new BinaryEx(Operations.And, binaryCondition.LeftExpr, rightNegation);
                    var rightExpression = new BinaryEx(Operations.And, leftNegation, binaryCondition.RightExpr);
                    var xorExpression = new BinaryEx(Operations.Or, leftExpression, rightExpression);
                    ConnectConditionalBranching(xorExpression, From, TrueBranch, FalseBranch);

                    /*
                    // Translation of xor in the level of control flow graph. More efficient than expansion of AST (translation in the level of program code).
                    // Does not work because AST of sharing AST elements
                    var intermediateBasicBlock1 = new BasicBlock();
                    var intermediateBasicBlock2 = new BasicBlock();
                    VisitIfStmtRec(binaryCondition.LeftExpr, intermediateBasicBlock1, intermediateBasicBlock2);
                    FromBlock = intermediateBasicBlock1;
                    VisitIfStmtRec(binaryCondition.RightExpr, FalseSink, TrueSink);
                    FromBlock = intermediateBasicBlock2;
                    VisitIfStmtRec(binaryCondition.RightExpr, TrueSink, FalseSink);*/
                    break;
            }
        }

        #endregion

        #region Private helpers
        
        /// <summary>
        /// Creates new edge between specified basic blocks and adds it into the basic block incoming and outgoing lists.
        /// </summary>
        /// <param name="From">Source basic block.</param>
        /// <param name="To">Target basic block.</param>
        /// <param name="Condition">Condition for this edge.</param>
        /// <returns>New edge</returns>
        private static void AddConditionalEdge(BasicBlock From, BasicBlock To, Expression Condition)
        {
            var edge = new ConditionalEdge(From, To, Condition);
            From.AddOutgoingEdge(edge);
            To.AddIncommingEdge(edge);
        }

        #endregion

        #region Implementation of edges of particular types.

        /// <summary>
        /// Represents conditional basic block connection 
        /// </summary>
        private class ConditionalEdge : BasicBlockEdge
        {
            /// <summary>
            /// Gets or sets source basic block.
            /// </summary>
            public override BasicBlock From { set; get; }

            /// <summary>
            /// Gets or sets target basic block.
            /// </summary>
            public override BasicBlock To { set; get; }

            /// <inheritdoc />
            public override BasicBlockEdgeTypes EdgeType { get {return BasicBlockEdgeTypes.CONDITIONAL;} }

            /// <summary>
            /// Gets or sets condition for this edge
            /// </summary>
            public override Expression Condition {get {return _condition;}}

            private readonly Expression _condition;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConditionalEdge"/> class.
            /// </summary>
            /// <param name="From">Source basic block.</param>
            /// <param name="To">Target basic block.</param>
            /// <param name="Condition">Condition for this edge.</param>
            internal ConditionalEdge(BasicBlock From, BasicBlock To, Expression Condition)
            {
                this.From = From;
                this.To = To;
                this._condition = Condition;
            }

            /// <summary>
            /// Sets new target block and adds edge into its incoming list
            /// </summary>
            /// <param name="newDestination">New target block</param>
            internal override void ChangeDestination(BasicBlock newDestination)
            {
                To = newDestination;
                newDestination.IncommingEdges.Add(this);
            }
        }

        /// <summary>
        /// Special Edge, which represents that condition in foreach.
        /// Analysis goes with this edge while iterating foreach cycle. 
        /// </summary>
        private class ForEachSpecialEdge : BasicBlockEdge
        {
            /// <summary>
            /// Gets or sets source basic block.
            /// </summary>
            public override BasicBlock From { set; get; }

            /// <summary>
            /// Gets or sets target basic block.
            /// </summary>
            public override BasicBlock To { set; get; }

            /// <inheritdoc />
            public override BasicBlockEdgeTypes EdgeType { get {return BasicBlockEdgeTypes.FOREACH;} }

            /// <summary>
            /// Creates new instance of ForEachSpecialEdge.
            /// </summary>
            /// <param name="From">Source basic block.</param>
            /// <param name="To">Target basic block.</param>
            internal ForEachSpecialEdge(BasicBlock From, BasicBlock To)
            {
                this.From = From;
                this.To = To;
            }

            /// <summary>
            /// Sets new target block and adds edge into its incoming list
            /// </summary>
            /// <param name="newDestination">New target block</param>
            internal override void ChangeDestination(BasicBlock newDestination)
            {
                To = newDestination;
                newDestination.IncommingEdges.Add(this);
            }
        }

        /// <summary>
        /// Represents direct basic block connection or the else branch connection
        /// </summary>
        private class DirectEdge : BasicBlockEdge
        {
            /// <summary>
            /// Gets or sets source basic block.
            /// </summary>
            public override BasicBlock From { set; get; }

            /// <summary>
            /// Gets or sets target basic block.
            /// </summary>
            public override BasicBlock To { set; get; }

            /// <inheritdoc />
            public override BasicBlockEdgeTypes EdgeType { get {return BasicBlockEdgeTypes.DIRECT;} }

            /// <summary>
            /// Initializes a new instance of the <see cref="DirectEdge"/> class.
            /// </summary>
            /// <param name="From">Source basic block.</param>
            /// <param name="To">Target basic block.</param>
            internal DirectEdge(BasicBlock From, BasicBlock To)
            {
                this.From = From;
                this.To = To;
            }

            /// <summary>
            /// Sets new target block and adds edge into its incoming list
            /// </summary>
            /// <param name="newDestination">New target block</param>
            internal override void ChangeDestination(BasicBlock newDestination)
            {
                To = newDestination;
                newDestination.IncommingEdges.Add(this);
            }
        }

        #endregion
    }

    

}