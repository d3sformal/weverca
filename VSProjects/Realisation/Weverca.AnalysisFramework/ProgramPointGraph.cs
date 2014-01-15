using System;
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
        /// Context used for building program point graph
        /// </summary>
        private readonly PPGraphBuildingContext _context;

        #endregion

        #region Public fields

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
        public readonly EmptyProgramPoint Start;
        /// <summary>
        /// Output program point from program point graph
        /// </summary>
        public readonly EmptyProgramPoint End;

        /// <summary>
        /// The script in which program points in this program point graph are defined
        /// </summary>
        public FileInfo OwningScript
        {
            get;
            private set;
        }

        #endregion

        #region Program point graph creating


        /// <summary>
        /// Create program point graph from source begining by entryPoint
        /// </summary>
        /// <param name="entryBlock">Entry point into source (all feasible basic blocks will be included in program point graph)</param>
        /// <param name="sourceObject">Object that is source for program point graph (Function declaration, GlobalCode,...)</param>
        private ProgramPointGraph(BasicBlock entryBlock, LangElement sourceObject)
        {
            SourceObject = sourceObject;

            _context = new PPGraphBuildingContext(this);

            var startBlock = _context.CreateEmptyPoint(out Start, entryBlock);
            var endBlock = _context.CreateEmptyPoint(out End);

            buildGraph(startBlock);
            //connecting end points has to be done before contracting (because of loosing child less points)
            connectChildLessPoints();

            contractBlocks();

            foreach (var point in Points)
            {
                point.SetOwningGraph(this);
            }
        }

        /// <summary>
        /// Create program point graph from given declaration
        /// </summary>
        /// <param name="declaration">Declaration which program point graph is created</param>
        /// <returns>Created program point graph</returns>
        public static ProgramPointGraph From(FunctionValue function)
        {
            var builder = new FunctionProgramPointBuilder();
            function.Accept(builder);
            builder.Output.OwningScript = function.DeclaringScript;
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
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromSource(FunctionDecl declaration)
        {
            var cfg = ControlFlowGraph.ControlFlowGraph.FromFunction(declaration);

            return new ProgramPointGraph(cfg.start, declaration);
        }

        /// <summary>
        /// Creates program point graph for given method declaration
        /// </summary>
        /// <param name="declaration">Method which program point graph will be created</param>
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromSource(MethodDecl declaration)
        {
            var cfg = ControlFlowGraph.ControlFlowGraph.FromMethod(declaration);

            return new ProgramPointGraph(cfg.start, declaration);
        }

        /// <summary>
        /// Creates program point graph for given control flow graph
        /// </summary>
        /// <param name="cfg">Input control flow graph</param>
        /// <returns>Created program point graph</returns>
        public static ProgramPointGraph FromSource(ControlFlowGraph.ControlFlowGraph cfg, FileInfo owningScript)
        {
            var ppgraph = new ProgramPointGraph(cfg.start, null);
            ppgraph.OwningScript = owningScript;
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
                var childBlock = getChildBlock(child, pendingBlocks);

                parentBlock.AddChild(childBlock);
            }
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

            //last points block created for parentBlocks condtion expression
            //is used for conneting default branch assumption
            PointsBlock lastConditionExpressionBlock = null;

            //process all outgoing conditional edges
            foreach (var edge in parentBlock.ConditionalEdges)
            {
                var expression = (edge as ConditionalEdge).Condition;
                var conditionExpressionBlock = _context.CreateFromExpression(expression);
                var expressionValue = conditionExpressionBlock.LastPoint as ValuePoint;

                //collect info for default branch
                expressionValues.Add(expressionValue);
                expressionParts.Add(expression);

                var condition = new AssumptionCondition(ConditionForm.All, expression);
                parentBlock.AddChild(conditionExpressionBlock);

                //connect edge.To through assume block
                var assumeBlock = _context.CreateAssumeBlock(condition, edge.To, expressionValue);
                conditionExpressionBlock.AddChild(assumeBlock);
                lastConditionExpressionBlock = conditionExpressionBlock;

                //assume block needs processing of its children
                pendingBlocks.Enqueue(assumeBlock);
            }

            //if there is default branch, connect it to parent
            if (parentBlock.Default != null)
            {
                if (expressionValues.Count == 0)
                {
                    //there is default branch without any condition - connect without assume block
                    var defaultBlock = getChildBlock(parentBlock.Default, pendingBlocks);
                    //default block needs processing of its children
                    parentBlock.AddChild(defaultBlock);
                }
                else
                {
                    //there has to be assumption condition on default branch
                    var values = expressionValues.ToArray();
                    var condition = new AssumptionCondition(ConditionForm.SomeNot, expressionParts.ToArray());
                    var defaultAssumeBlock = _context.CreateAssumeBlock(condition, parentBlock.Default, values);

                    //default Assume has to be added as child of last expression block
                    //note: there is always last condition block, because of non empty expression values
                    lastConditionExpressionBlock.AddChild(defaultAssumeBlock);
                    pendingBlocks.Enqueue(defaultAssumeBlock);
                }
            }
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
                //block is try block, we has to start scope of its catch blocks
                var catchBlocks = new List<Tuple<GenericQualifiedName, ProgramPointBase>>();
                foreach (var catchBB in tryBlock.catchBlocks)
                {
                    var startingCatch = getChildBlock(catchBB, pendingBlocks);

                    startingCatch.DisallowContraction();

                    var tuple = Tuple.Create(catchBB.ClassName, startingCatch.FirstPoint);
                    catchBlocks.Add(tuple);
                }

                var scopeStart = _context.CreateCatchScopeStart(catchBlocks);
                childBlock.PreprendFlowWith(scopeStart);
            }

            //find all incomming edges from catch blocks
            var endingCatchBlocks = new List<Tuple<GenericQualifiedName, ProgramPointBase>>();
            foreach (var endingTryBlock in block.EndIngTryBlocks)
            {
                foreach (var endingCatch in endingTryBlock.catchBlocks)
                {
                    var endingCatchBlock = getChildBlock(endingCatch, pendingBlocks);
                    endingCatchBlock.DisallowContraction();

                    var tuple = Tuple.Create(endingCatch.ClassName, endingCatchBlock.FirstPoint);
                    endingCatchBlocks.Add(tuple);
                }
            }

            if (endingCatchBlocks.Count > 0)
            {
                var scopeEnd = _context.CreateCatchScopeEnd(endingCatchBlocks);
                childBlock.AppendFlow(scopeEnd);
            }

            return childBlock;
        }

        #endregion

    }
}
