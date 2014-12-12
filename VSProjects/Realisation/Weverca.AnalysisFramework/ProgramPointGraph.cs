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
using System.IO;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.ControlFlowGraph;
using Weverca.AnalysisFramework.Expressions;

using Weverca.AnalysisFramework.Memory;

using Weverca.AnalysisFramework.ProgramPoints;
using Weverca.AnalysisFramework.GraphVisualizer;

namespace Weverca.AnalysisFramework
{

    delegate ProgramPointBase ProgramPointCreator();

    /// <summary>
    /// TODO it needs to be refactored, because of extended program point changes
    /// </summary>
    public class ProgramPointGraph
    {
        #region Private fields

        /// <summary>
        /// Global identifier counter for ProgramPointGraph instances
        /// </summary>
        private static int PPG_ID = 1;

        /// <summary>
        /// Context used for building program point graph
        /// </summary>
        private readonly PPGraphBuildingContext _context;

        #endregion

        #region Public fields

        /// <summary>
        /// The program point graph unique identifier
        /// </summary>
        public readonly int ProgramPointGraphID;

        /// <summary>
        /// Context of this program point graph - consists of program points that have this graph
        /// as an extension (that include or call this graph).
        /// </summary>
        public readonly PPGraphContext Context;

        /// <summary>
        /// Object that is source for program point graph (Function declaration, GlobalCode,...)
        /// </summary>
        public readonly LangElement SourceObject;

        /// <summary>
        /// All program points defined in program point graph
        /// </summary>
        public IEnumerable<ProgramPointBase> Points { get { return _context.CreatedPoints; } }

        /// <summary>
        /// Input program point into program point graph
        /// </summary>
        public readonly ProgramPointBase Start;
        /// <summary>
        /// Output program point from program point graph
        /// </summary>
        public readonly ProgramPointBase End;

        /// <summary>
        /// The script in which program points in this program point graph are defined
        /// </summary>
        public readonly FileInfo OwningScript;

        /// <summary>
        /// The name of the function or method that is represented by this program point graph.
        /// Null in the case that this program point graph represetns script
        /// </summary>
        public string FunctionName { get; private set; }

        /// <summary>
        /// Evaluation log, that contains values of all points created within context of this program point graph
        /// </summary>
        public readonly EvaluationLog EvaluationLog = new EvaluationLog(new ProgramPointBase[0]);

        #endregion

        /// <summary>
        /// Gets statistics about usage of output snapshots of all program points.
        /// </summary>
        /// <returns>Snapshot statistis</returns>
        public SnapshotStatistics GetStatistics()
        {
            SnapshotStatistics statistics = new SnapshotStatistics();
            foreach (var point in Points)
            {
                if (point.InSet != null)
                {
                    SnapshotStatistics tempStat = point.InSnapshot.GetStatistics();
                    tempStat.MergeWith(point.OutSnapshot.GetStatistics());
                    statistics.MergeWith(tempStat);
                }
            }
            return statistics;
        }

        #region Program point graph creating
        
        /// <summary>
        /// Create program point graph from source begining by entryPoint
        /// </summary>
        /// <param name="cfg">Entry cfg into source (all feasible basic blocks will be included in program point graph)</param>
        /// <param name="sourceObject">Object that is source for program point graph (Function declaration, GlobalCode,...)</param>
        private ProgramPointGraph(ControlFlowGraph.ControlFlowGraph cfg, LangElement sourceObject)
            : this(cfg.start, sourceObject)
        {
            OwningScript = cfg.File;
        }

        /// <summary>
        /// Create program point graph from source begining by entryPoint
        /// </summary>
        /// <param name="entryBlock">Entry point into source (all feasible basic blocks will be included in program point graph)</param>
        /// <param name="sourceObject">Object that is source for program point graph (Function declaration, GlobalCode,...)</param>
        private ProgramPointGraph(BasicBlock entryBlock, LangElement sourceObject)
        {
            ProgramPointGraphID = PPG_ID++;

            Context = new PPGraphContext(this);
            SourceObject = sourceObject;

            _context = new PPGraphBuildingContext(this);

            var startBlock = _context.CreateSubprogramEntryBlock(out Start, entryBlock);
            var endBlock = _context.CreateEmptyBlock(out End);

            buildGraph(startBlock);
            //connecting end points has to be done before contracting (because of loosing child less points)
            connectChildLessPoints();

            contractBlocks();

            foreach (var point in Points)
            {
                point.SetOwningGraph(this);
            }

            //Associate everything in subgraph
            EvaluationLog.AssociatePointHierarchy(Start);
        }

        /// <summary>
        /// Create program point graph from given declaration
        /// </summary>
        /// <param name="function">Declaration which program point graph is created</param>
        /// <returns>Created program point graph</returns>
        public static ProgramPointGraph From(FunctionValue function)
        {
            var builder = new FunctionProgramPointBuilder();
            function.Accept(builder);

            builder.Output.FunctionName = function.Name.Value;
            return builder.Output;
        }

        /// <summary>
        /// Creates program point graph for native analyzer
        /// </summary>
        /// <param name="analyzer">Native analyzer</param>
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromNative(NativeAnalyzer analyzer)
        {
            var basicBlock = new BasicBlock();
            basicBlock.AddElement(analyzer);
            return new ProgramPointGraph(basicBlock, analyzer);
        }

        /// <summary>
        /// Creates program point graph for given function declaration
        /// </summary>
        /// <param name="declaration">Function which program point graph will be created</param>
        /// <param name="file">File info describing where declaration comes from</param>
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromSource(FunctionDecl declaration, FileInfo file)
        {
            var cfg = ControlFlowGraph.ControlFlowGraph.FromFunction(declaration, file);

            return new ProgramPointGraph(cfg, declaration);
        }

        /// <summary>
        /// Creates program point graph for given method declaration
        /// </summary>
        /// <param name="declaration">Method which program point graph will be created</param>
        /// <param name="file">File info describing where declaration comes from</param>
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromSource(MethodDecl declaration, FileInfo file)
        {
            var cfg = ControlFlowGraph.ControlFlowGraph.FromMethod(declaration, file);

            return new ProgramPointGraph(cfg, declaration);
        }

        /// <summary>
        /// Creates program point graph for given control flow graph
        /// </summary>
        /// <param name="cfg">Input control flow graph</param>
        /// <returns>Created program point graph</returns>
        public static ProgramPointGraph FromSource(ControlFlowGraph.ControlFlowGraph cfg)
        {
            var ppgraph = new ProgramPointGraph(cfg, null);
            return ppgraph;
        }

        #endregion

        #region Graph building

        /// <summary>
        /// Build graph from given starting block
        /// <remarks>Uses _context graph build</remarks>
        /// </summary>
        /// <param name="startBlock">Block containing graph Start point</param>
        private void buildGraph(PointsBlock startBlock)
        {
            //blocks that needs to handle children
            var pendingBlocks = new Queue<PointsBlock>();
            pendingBlocks.Enqueue(startBlock);

            while (pendingBlocks.Count > 0)
            {
                var parentBlock = pendingBlocks.Dequeue();

                //connect all its outgoing blocks (not condition edges)
                connectConditionLessEdges(parentBlock, pendingBlocks);

                //connect conditional edges (also with default branch)
                connectConditionEdges(parentBlock, pendingBlocks);
            }

            _context.ConnectBlocks();
        }

        /// <summary>
        /// Contract blocks that are not needed to be present in PPG
        /// <remarks>Contracted blocks usually belongs to empty basic block from CFG</remarks>
        /// </summary>
        private void contractBlocks()
        {
            //TODO implemente graph contraction because of empty points reduction
        }

        /// <summary>
        /// Connect points that doesnt have any childs to End point
        /// </summary>
        private void connectChildLessPoints()
        {
            //collect points without flow children
            var childLessPoints = new List<ProgramPointBase>();
            foreach (var point in Points)
            {
                if (point.FlowChildren.Any())
                    continue;

                childLessPoints.Add(point);
            }

            //Prevent self edge on End point
            childLessPoints.Remove(End);

            //connect points to End
            foreach (var point in childLessPoints)
            {
                point.AddFlowChild(End);
            }
        }

        /// <summary>
        /// Connect outgoing condition less edges from parentBlock with belonging children point blocks
        /// </summary>
        /// <param name="parentBlock">Parent point block which children point blocks will be connected</param>
        /// <param name="pendingBlocks">Point blocks which children hasn't been processed yet</param>
        private void connectConditionLessEdges(PointsBlock parentBlock, Queue<PointsBlock> pendingBlocks)
        {
            foreach (var child in parentBlock.OutgoingBlocks)
            {
                connectConditionLessEdge(parentBlock, child, pendingBlocks);
            }
        }

        /// <summary>
        /// Connect outgoing condition less edge from parentBlock with given child
        /// </summary>
        /// <param name="parentBlock">Parent point block which child block will be connected</param>
        /// <param name="child">Block connected as child of parent block</param>
        /// <param name="pendingBlocks">Point blocks which children hasn't been processed yet</param>
        private void connectConditionLessEdge(PointsBlock parentBlock, BasicBlock child, Queue<PointsBlock> pendingBlocks)
        {
            var childBlock = getChildBlock(child, pendingBlocks);

            parentBlock.AddChild(childBlock);
        }

        /// <summary>
        /// Connect outgoing condition edges from parentBlock with belonging children point blocks via assume blocks
        /// </summary>
        /// <param name="parentBlock">Parent point block which children point blocks will be connected</param>
        /// <param name="pendingBlocks">Point blocks which children hasn't been processed yet</param>
        private void connectConditionEdges(PointsBlock parentBlock, Queue<PointsBlock> pendingBlocks)
        {
            //collected expression values - because of sharing with default branch
            var expressionValues = new List<ValuePoint>();

            //collected expression parts - because of default assumption condition creation
            var expressionParts = new List<Expression>();

            //collected expression blocks - because of connecting default assumption
            var expressionBlocks = new List<PointsBlock>();

            //process all outgoing conditional edges
            // For each conditional edge, create block and append it as a child of parrent block
            // TODO: in current CFG, there should be always at most one conditional edge
            foreach (var edge in parentBlock.ConditionalEdges)
            {

                Expression expression;

                if (edge.EdgeType == BasicBlockEdgeTypes.CONDITIONAL)
                {
                    expression = edge.Condition;
                }
                else if (edge.EdgeType == BasicBlockEdgeTypes.FOREACH)
                {
                    //now is foreach handled without condition processing (edge is added as non conditional)
                    connectConditionLessEdge(parentBlock, edge.To, pendingBlocks);
                    continue;
                }
                else
                {
                    throw new NotSupportedException("Not supported CFG edge of type: " + edge.GetType());
                }


                var conditionExpressionBlock = _context.CreateFromExpression(expression);
                var expressionValue = conditionExpressionBlock.LastPoint as ValuePoint;

                //collect info for default branch
                expressionValues.Add(expressionValue);
                expressionParts.Add(expression);
                expressionBlocks.Add(conditionExpressionBlock);

                var condition = new AssumptionCondition(ConditionForm.All, expression);
                parentBlock.AddChild(conditionExpressionBlock);

                //connect edge.To through assume block
                var assumeBlock = _context.CreateAssumeBlock(condition, edge.To, expressionValue);
                conditionExpressionBlock.AddChild(assumeBlock);

                //assume block needs processing of its children
                pendingBlocks.Enqueue(assumeBlock);
            }

            //if there is default branch
            if (parentBlock.Default != null)
            {
                if (expressionValues.Count == 0)
                {
                    //there is default branch without any condition - connect without assume block
                    // connect default branch to parent
                    var defaultBlock = getChildBlock(parentBlock.Default, pendingBlocks);
                    //default block needs processing of its children
                    parentBlock.AddChild(defaultBlock);
                }
                else
                {
                    //there has to be assumption condition on default branch
                    // connect default branch to conditional blocks
                    var values = expressionValues.ToArray();
					var condition = new AssumptionCondition(ConditionForm.None, expressionParts.ToArray());
                    var defaultAssumeBlock = _context.CreateAssumeBlock(condition, parentBlock.Default, values);

                    //default Assume has to be added as child of all expression blocks
                    foreach (var conditionExpression in expressionBlocks)
                    {
                        conditionExpression.AddChild(defaultAssumeBlock);
                    }

                    pendingBlocks.Enqueue(defaultAssumeBlock);
                }
            }
        }



        #endregion

        #region Graph Text Representation

        /// <summary>
        /// Builds the graph visualisation using given visualiser.
        /// 
        /// User of this method can specify types of program points which should be skipped - these nodes 
        /// won't appear in final graph and all edges to this nodes will be connected with the nearest 
        /// permitted parent.
        /// </summary>
        /// <param name="graphVisualizer">The graph visualizer.</param>
        /// <param name="skipProgramPoints">Types of programpoints which should be skipped from the visualisation.</param>
        public void BuildGraphVisualisation(IGraphVisualizer graphVisualizer, Type[] skipProgramPoints)
        {
            HashSet<ProgramPointGraph> processedGraphs = new HashSet<ProgramPointGraph>();
            processedGraphs.Add(this);

            BuildGraphVisualisation(graphVisualizer, skipProgramPoints, processedGraphs);
        }

        /// <summary>
        /// Builds the graph visualisation.
        /// </summary>
        /// <param name="graphVisualizer">The graph visualizer.</param>
        /// <param name="skipProgramPoints">The skip program points.</param>
        /// <param name="processedGraphs">The processed graphs.</param>
        private void BuildGraphVisualisation(IGraphVisualizer graphVisualizer, Type[] skipProgramPoints, HashSet<ProgramPointGraph> processedGraphs)
        {
            foreach (var point in Points)
            {
                bool skip = isTypeOf(point.GetType(), skipProgramPoints);

                if (!skip)
                {
                    buildNodeVisualisation(point, graphVisualizer);
                    buildEdgesVisualisation(point, graphVisualizer, skipProgramPoints);
                    enqueBuildingExtensionVisualisations(point, graphVisualizer, skipProgramPoints, processedGraphs);
                }
            }
        }

        /// <summary>
        /// Enques the building extension visualisations recursively.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="graphVisualizer">The graph visualizer.</param>
        /// <param name="skipProgramPoints">The skip program points.</param>
        /// <param name="processedGraphs">The processed graphs.</param>
        private static void enqueBuildingExtensionVisualisations(ProgramPointBase point, IGraphVisualizer graphVisualizer, Type[] skipProgramPoints, HashSet<ProgramPointGraph> processedGraphs)
        {
            bool hasBranches = false;
            foreach (var extension in point.Extension.Branches)
            {
                var ppg = extension.Graph;
                if (!processedGraphs.Contains(ppg))
                {
                    processedGraphs.Add(ppg);
                    extension.Graph.BuildGraphVisualisation(graphVisualizer, skipProgramPoints, processedGraphs);
                }
                buildNodeVisualisation(extension, graphVisualizer);
                buildEdgesVisualisation(extension, graphVisualizer, skipProgramPoints);

                hasBranches = true;
            }

            if (hasBranches)
            {
                buildNodeVisualisation(point.Extension.Sink, graphVisualizer);
                buildEdgesVisualisation(point.Extension.Sink, graphVisualizer, skipProgramPoints);
            }
        }

        /// <summary>
        /// Builds the edges visualisation.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="graphVisualizer">The graph visualizer.</param>
        /// <param name="skipProgramPoints">The skip program points.</param>
        private static void buildEdgesVisualisation(ProgramPointBase point, IGraphVisualizer graphVisualizer, Type[] skipProgramPoints)
        {
            HashSet<int> processed = new HashSet<int>();
            LinkedList<ProgramPointBase> edgeQueue = new LinkedList<ProgramPointBase>();
            foreach (var targetPoint in point.FlowChildren)
            {
                edgeQueue.AddLast(targetPoint);
                processed.Add(targetPoint.ProgramPointID);
            }

            string id = "pp" + point.ProgramPointID.ToString();
            while (edgeQueue.Count > 0)
            {
                ProgramPointBase targetPoint = edgeQueue.First.Value;
                edgeQueue.RemoveFirst();

                bool targetSkipped = isTypeOf(targetPoint.GetType(), skipProgramPoints);
                if (!targetSkipped)
                {
                    string outId = "pp" + targetPoint.ProgramPointID;
                    graphVisualizer.AddEdge(id, outId, "");
                }
                else
                {
                    foreach (var p in targetPoint.FlowChildren)
                    {
                        if (!processed.Contains(p.ProgramPointID))
                        {
                            edgeQueue.AddLast(p);
                            processed.Add(p.ProgramPointID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the node visualisation.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="graphVisualizer">The graph visualizer.</param>
        private static void buildNodeVisualisation(ProgramPointBase point, IGraphVisualizer graphVisualizer)
        {
            string id = "pp" + point.ProgramPointID.ToString();
            string label = string.Format("{0}\n{1}",
                point.GetType().Name.ToString(), point.ToString());

            graphVisualizer.AddNode(id, label);
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Get or creates points block from given block. Accordingly fills pendingBlocks if children processing is needed.
        /// </summary>
        /// <param name="block">Block which points block will be returned</param>
        /// <param name="pendingBlocks">Point blocks needed children processing</param>
        /// <returns>Founded or created points block</returns>
        private PointsBlock getChildBlock(BasicBlock block, Queue<PointsBlock> pendingBlocks)
        {
            PointsBlock childBlock;
            if (_context.IsCreated(block))
            {
                childBlock = _context.GetBlock(block);
                //child block was already in queue when it was created
            }
            else
            {
                childBlock = createChildBlock(block, pendingBlocks);

                //child block hasn't been in queue yet
                pendingBlocks.Enqueue(childBlock);
            }
            return childBlock;
        }

        private PointsBlock createChildBlock(BasicBlock block, Queue<PointsBlock> pendingBlocks)
        {
            PointsBlock childBlock;
            childBlock = _context.CreateFromBlock(block);

            var tryBlock = block as TryBasicBlock;
            if (tryBlock != null)
            {
                //block is try block, we have to start scope of its catch blocks
                var catchBlocks = new List<CatchBlockDescription>();
                foreach (var catchBB in tryBlock.catchBlocks)
                {
                    var startingCatch = getChildBlock(catchBB, pendingBlocks);

                    startingCatch.DisallowContraction();

                    var catchVar = new VariableIdentifier(catchBB.Variable.VarName);
                    var description = new CatchBlockDescription(startingCatch.FirstPoint, catchBB.ClassName, catchVar);
                    catchBlocks.Add(description);
                }

                var scopeStart = _context.CreateCatchScopeStart(catchBlocks);
                childBlock.PreprendFlowWith(scopeStart);
            }

            //find all incomming edges from catch blocks
            var endingCatchBlocks = new List<CatchBlockDescription>();
            foreach (var endingTryBlock in block.EndIngTryBlocks)
            {
                foreach (var endingCatch in endingTryBlock.catchBlocks)
                {
                    var endingCatchBlock = getChildBlock(endingCatch, pendingBlocks);
                    endingCatchBlock.DisallowContraction();

                    var catchVar = new VariableIdentifier(endingCatch.Variable.VarName);
                    var description = new CatchBlockDescription(endingCatchBlock.FirstPoint, endingCatch.ClassName, catchVar);
                    endingCatchBlocks.Add(description);
                }
            }

            if (endingCatchBlocks.Count > 0)
            {
                var scopeEnd = _context.CreateCatchScopeEnd(endingCatchBlocks);
                childBlock.AppendFlow(scopeEnd);
            }

            if (block.WorklistSegmentStart())
            {
                // get program point that corresponds to the end of the segment
                var afterBlock = getChildBlock(block.AfterWorklistSegment, pendingBlocks);
                childBlock.LastPoint.CreateWorklistSegment(afterBlock.FirstPoint);
            }

            return childBlock;
        }

        /// <summary>
        /// Determines whether given type is child at least of one of specified types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parentTypes">The parent types.</param>
        /// <returns></returns>
        private static bool isTypeOf(Type type, Type[] parentTypes)
        {
            foreach (var parent in parentTypes)
            {
                if (parent.IsAssignableFrom(type))
                {
                    return true;
                }
            }

            return false;
        }
             
        #endregion
    }
}