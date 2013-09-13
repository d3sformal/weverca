using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Reflection;

using Weverca.ControlFlowGraph;
using Weverca.Analysis.Expressions;

using Weverca.Analysis.Memory;

using Weverca.Analysis.ProgramPoints;

namespace Weverca.Analysis
{

    delegate ProgramPointBase ProgramPointCreator();

    /// <summary>
    /// TODO it needs to be refactored, because of extended program point changes
    /// </summary>
    public class ProgramPointGraph
    {
        #region Private fields


        private readonly HashSet<ProgramPointBase> _points = new HashSet<ProgramPointBase>();

        /// <summary>
        /// End points of statement resolved from LangElement
        /// </summary>
        private readonly Dictionary<LangElement, ProgramPointBase> _statementEnds = new Dictionary<LangElement, ProgramPointBase>();

        private readonly Dictionary<object, ProgramPointBase> _chainStarts = new Dictionary<object, ProgramPointBase>();

        private readonly Dictionary<AssumptionCondition, ProgramPointBase> _conditionStarts = new Dictionary<AssumptionCondition, ProgramPointBase>();

        private readonly Dictionary<AssumptionCondition, ProgramPointBase> _conditionEnds = new Dictionary<AssumptionCondition, ProgramPointBase>();

        /// <summary>
        /// Set of processed blocks - is used for avoiding cycling at graph building
        /// </summary>
        private readonly HashSet<BasicBlock> _processedBlocks = new HashSet<BasicBlock>();

        #endregion


        #region Public fields

        /// <summary>
        /// Object that is source for program point graph (Function declaration, GlobalCode,...)
        /// </summary>
        public readonly LangElement SourceObject;

        /// <summary>
        /// All program points defined in program point graph
        /// </summary>
        public IEnumerable<ProgramPointBase> Points { get { return _points; } }

        /// <summary>
        /// Input program point into program point graph
        /// </summary>
        public readonly ProgramPointBase Start;
        /// <summary>
        /// Output program point from program point graph
        /// </summary>
        public readonly ProgramPointBase End;

        #endregion

        #region Program point graph creating

        /// <summary>
        /// Create program point graph from source begining by entryPoint
        /// </summary>
        /// <param name="entryPoint">Entry point into source (all feasible basic blocks will be included in program point graph)</param>
        /// <param name="sourceObject">Object that is source for program point graph (Function declaration, GlobalCode,...)</param>
        public ProgramPointGraph(BasicBlock entryPoint, LangElement sourceObject)
        {
            SourceObject = sourceObject;
            Start = getEmpty(entryPoint);
            addChild(Start, entryPoint);

            var endPoints = new List<ProgramPointBase>();
            foreach (var point in _points)
            {
                if (point.FlowChildren.Any())
                    continue;

                endPoints.Add(point);
            }

            if (endPoints.Count == 0)
            {
                //End point is not reachable
                End = null;
                return;
            }

            End = getEmpty(Start);
            foreach (var endPoint in endPoints)
            {
                endPoint.AddFlowChild(End);
            }
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
            var cfg = new ControlFlowGraph.ControlFlowGraph(declaration);

            return new ProgramPointGraph(cfg.start, declaration);
        }

        /// <summary>
        /// Creates program point graph for given method declaration
        /// </summary>
        /// <param name="declaration">Method which program point graph will be created</param>
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromSource(MethodDecl declaration)
        {
            var cfg = new ControlFlowGraph.ControlFlowGraph(declaration);

            return new ProgramPointGraph(cfg.start, declaration);
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
            return builder.Output;
        }

        #endregion

        
        #region Program point graph building

        /// <summary>
        /// Add child created from given block. This child already contains all its children within basic block.
        /// </summary>
        /// <param name="parent">Parent which child will be added</param>
        /// <param name="block">Block which first statement generates child</param>
        private void addChild(ProgramPointBase parent, BasicBlock block)
        {
            //Check that given basic block is not already processed
            if (_processedBlocks.Contains(block))
            {
                fillWithBlockEntry(parent, block);
                return;
            }

            _processedBlocks.Add(block);


            var current = parent;
            foreach (var stmt in block.Statements)
            {
                //create chain of program points
                var child = getStatementStart(stmt, block);
                current.AddFlowChild(child);
                current = getStatementEnd(stmt, block);
            }

            foreach (var edge in block.OutgoingEdges)
            {
                var condition = getConditionEnd(edge.Condition, block);
                addChild(condition, edge.To);
                var conditionStart = getConditionStart(edge.Condition, block);
                current.AddFlowChild(conditionStart);
            }

            if (block.DefaultBranch != null)
            {
                var conditionExpressions = from edge in block.OutgoingEdges select edge.Condition;

                if (conditionExpressions.Any())
                {
                    var defaultPoint = getDefaultBranchEnd(conditionExpressions, block);
                    addChild(defaultPoint, block.DefaultBranch.To);
                    var conditionStart = getDefaultBranchStart(conditionExpressions, block);
                    current.AddFlowChild(conditionStart);
                }
                else
                {
                    //there is no condition on edge
                    addChild(current, block.DefaultBranch.To);
                }
            }
        }

        private void fillWithBlockEntry(ProgramPointBase point, BasicBlock block, HashSet<BasicBlock> fillingBlocks = null)
        {
            if (fillingBlocks == null)
            {
                fillingBlocks = new HashSet<BasicBlock>();
            }

            if (!fillingBlocks.Add(block))
            {
                //empty cycle
                point.AddFlowChild(point);
                return;
            }

            //parent has to point to first statement of block
            if (block.Statements.Count > 0)
            {
                var entryPoint = getStatementStart(block.Statements[0], block);
                point.AddFlowChild(entryPoint);
                return;
            }
            else
            {
                var conditionExpressions = new List<Expression>();
                foreach (var edge in block.OutgoingEdges)
                {
                    //every outgoing edge is child of point, because of empty block body
                    point.AddFlowChild(getConditionStart(edge.Condition, block));
                    conditionExpressions.Add(edge.Condition);
                }

                if (conditionExpressions.Count == 0)
                {
                    //there is no program point on default branch     

                    if (block.DefaultBranch != null)
                    {
                        fillWithBlockEntry(point, block.DefaultBranch.To, fillingBlocks);
                    }
                    else
                    {
                        //TODO Are there cases, where point wont get to EndPoint ?
                    }

                }
                else
                {
                    //there is program point on default branch
                    var defaultPoint = getDefaultBranchStart(conditionExpressions, block);
                    point.AddFlowChild(defaultPoint);
                }
            }

        }

        /// <summary>
        /// Get program point indexed by default branch of given expressions. If there is no such a point, new is created.
        /// </summary>
        /// <param name="expressions">Expressions for created ConditionForm.SomeNot condition</param>
        /// <param name="outerBlock">Block where expressions are located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPointBase getDefaultBranchStart(IEnumerable<Expression> expressions, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.SomeNot, expressions.ToArray());
            return getConditionStart(assumption, outerBlock);
        }

        /// <summary>
        /// Get program point indexed by default branch of given expressions. If there is no such a point, new is created.
        /// </summary>
        /// <param name="expressions">Expressions for created ConditionForm.SomeNot condition</param>
        /// <param name="outerBlock">Block where expressions are located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPointBase getDefaultBranchEnd(IEnumerable<Expression> expressions, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.SomeNot, expressions.ToArray());
            return getConditionEnd(assumption, outerBlock);
        }

        /// <summary>
        /// Get program point indexed by statement. If there is no such a point, new is created.
        /// </summary>
        /// <param name="statement">Index of program point</param>
        /// <param name="outerBlock">Block where statement is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPointBase getStatementEnd(LangElement statement, BasicBlock outerBlock)
        {
            prepareStatement(statement, outerBlock);
            return _statementEnds[statement];
        }

        /// <summary>
        /// Get program point indexed by statement. If there is no such a point, new is created.
        /// </summary>
        /// <param name="statement">Index of program point</param>
        /// <param name="outerBlock">Block where statement is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPointBase getStatementStart(LangElement statement, BasicBlock outerBlock)
        {
            return prepareStatement(statement, outerBlock);
        }


        private ProgramPointBase prepareStatement(LangElement statement, BasicBlock outerBlock)
        {
            return getPoint(statement, () =>
            {
                var statementStart = ElementExpander.ExpandStatement(statement, onPointCreated);
                var statementEnd = findChainEnd(statementStart);
                _statementEnds.Add(statement, statementEnd);

                return statementStart;
            });
        }

        private ProgramPointBase findChainEnd(ProgramPointBase point)
        {
            while (point.FlowChildren.Any())
            {
                point = point.FlowChildren.First();
            }

            return point;
        }

        /// <summary>
        /// Get program point indexed by ConditionForm.All condition, created from given expression. If there is no such point, new is created.
        /// </summary>
        /// <param name="expression">Expression for created condition</param>
        /// <param name="outerBlock">Block where expression is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPointBase getConditionStart(Expression expression, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.All, expression);
            return getConditionStart(assumption, outerBlock);
        }


        /// <summary>
        /// Get program point indexed by ConditionForm.All condition, created from given expression. If there is no such point, new is created.
        /// </summary>
        /// <param name="expression">Expression for created condition</param>
        /// <param name="outerBlock">Block where expression is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPointBase getConditionEnd(Expression expression, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.All, expression);
            return getConditionEnd(assumption, outerBlock);
        }



        /// <summary>
        /// Get program point indexed by condition. If there is no such point, new is created.
        /// </summary>
        /// <param name="condition">Index of program point</param>
        /// <param name="outerBlock">Block where condition is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPointBase getConditionStart(AssumptionCondition condition, BasicBlock outerBlock)
        {
            prepareCondition(condition, outerBlock);
            return _conditionStarts[condition];
        }


        /// <summary>
        /// Get program point indexed by condition. If there is no such point, new is created.
        /// </summary>
        /// <param name="condition">Index of program point</param>
        /// <param name="outerBlock">Block where condition is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPointBase getConditionEnd(AssumptionCondition condition, BasicBlock outerBlock)
        {
            prepareCondition(condition, outerBlock);
            return _conditionEnds[condition];
        }

        private void prepareCondition(AssumptionCondition condition, BasicBlock outerBlock)
        {
            getPoint(condition, () =>
            {
                var conditionStart = ElementExpander.ExpandCondition(condition, onPointCreated);
                var conditionEnd = findChainEnd(conditionStart);
                _conditionEnds.Add(condition, conditionEnd);
                _conditionStarts.Add(condition, conditionStart);

                return conditionStart;
            });
        }

        /// <summary>
        /// Get program point indexed by key. If there is no such point, create new empty point.
        /// </summary>
        /// <param name="key">Key of created program point</param>
        /// <returns>Program point indexed by key</returns>
        private ProgramPointBase getEmpty(object key)
        {
            return getPoint(key, () => {
                var result = new EmptyProgramPoint();
                onPointCreated(key, result);
                return result;
            });
        }

        /// <summary>
        /// Get point indexed by given key. If there is no such point, create new with creator
        /// </summary>
        /// <param name="key">Index of program point</param>
        /// <param name="creator">Creator of program point</param>
        /// <returns>Program point indexed by key</returns>
        private ProgramPointBase getPoint(object key, ProgramPointCreator creator)
        {
            ProgramPointBase result;

            if (!_chainStarts.TryGetValue(key, out result))
            {
                result = creator();

                _chainStarts[key] = result; 
            }
            return result;
        }
        #endregion


        private void onPointCreated(object key, ProgramPointBase point)
        {
            _points.Add(point);
        }
    }
}
