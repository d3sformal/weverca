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

namespace Weverca.Analysis
{

    delegate ProgramPoint ProgramPointCreator();

    public class ProgramPointGraph
    {
        #region Private fields

        /// <summary>
        /// Program points according to their defining objects (conditions, statements,..)
        /// </summary>
        private readonly Dictionary<object, ProgramPoint> _points = new Dictionary<object, ProgramPoint>();

        /// <summary>
        /// Partial call extensions where program point graph is included
        /// </summary>
        private readonly HashSet<PartialExtension<LangElement>> _containingCallExtensions = new HashSet<PartialExtension<LangElement>>();

        /// <summary>
        /// Partial include extensions where program point graph is included
        /// </summary>
        private readonly HashSet<PartialExtension<string>> _containingIncludeExtensions = new HashSet<PartialExtension<string>>();

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
        public IEnumerable<ProgramPoint> Points { get { return _points.Values; } }

        /// <summary>
        /// Partial call extensions where program point graph is included
        /// </summary>
        public IEnumerable<PartialExtension<LangElement>> ContainingCallExtensions { get { return _containingCallExtensions; } }

        /// <summary>
        /// Partial include extensions where program point graph is included
        /// </summary>
        public IEnumerable<PartialExtension<string>> ContainingIncludeExtensions { get { return _containingIncludeExtensions; } }

        /// <summary>
        /// Input program point into program point graph
        /// </summary>
        public readonly ProgramPoint Start;
        /// <summary>
        /// Output program point from program point graph
        /// </summary>
        public readonly ProgramPoint End;

        #endregion

        #region Program point graph creating

        /// <summary>
        /// Create program point graph from source begining by entryPoint
        /// </summary>
        /// <param name="entryPoint">Entry point into source (all feasible basic blocks will be included in program point graph)</param>
        /// <param name="sourceObject">Object that is source for program point graph (Function declaration, GlobalCode,...)</param>
        public ProgramPointGraph(BasicBlock entryPoint,LangElement sourceObject)
        {
            SourceObject = sourceObject;
            Start = getEmpty(entryPoint);
            addChild(Start, entryPoint);

            var endPoints = new List<ProgramPoint>();
            foreach (var point in _points.Values)
            {
                if (point.Children.Any())
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
                endPoint.AddChild(End);
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
            return new ProgramPointGraph(basicBlock,analyzer);
        }

        /// <summary>
        /// Creates program point graph for given function declaration
        /// </summary>
        /// <param name="declaration">Function which program point graph will be created</param>
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromSource(FunctionDecl declaration)
        {
            var cfg = new ControlFlowGraph.ControlFlowGraph(declaration);

            return new ProgramPointGraph(cfg.start,declaration);
        }

        /// <summary>
        /// Creates program point graph for given method declaration
        /// </summary>
        /// <param name="declaration">Method which program point graph will be created</param>
        /// <returns>Created program point graph</returns>
        internal static ProgramPointGraph FromSource(MethodDecl declaration)
        {
            var cfg = new ControlFlowGraph.ControlFlowGraph(declaration);

            return new ProgramPointGraph(cfg.start,declaration);
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

        #region Containing extensions handling

        /// <summary>
        /// Add call extension which contains this program point graph
        /// </summary>
        /// <param name="extension">Extension which contains this program point graph</param>
        internal void AddContainingCallExtension(PartialExtension<LangElement> extension)
        {
            _containingCallExtensions.Add(extension);
        }

        /// <summary>
        /// Remove call extension which doesn't contains this program point graph yet
        /// </summary>
        /// <param name="extension">Removed extension</param>
        internal void RemoveContainingCallExtension(PartialExtension<LangElement> extension)
        {
            _containingCallExtensions.Remove(extension);
        }

        /// <summary>
        /// Add include extension which contains this program point graph
        /// </summary>
        /// <param name="extension">Extension which contains this program point graph</param>
        internal void AddContainingIncludeExtension(PartialExtension<string> extension)
        {
            _containingIncludeExtensions.Add(extension);
        }

        /// <summary>
        /// Remove include extension which doesn't contains this program point graph yet
        /// </summary>
        /// <param name="extension">Removed extension</param>
        internal void RemoveContainingIncludeExtension(PartialExtension<string> extension)
        {
            _containingIncludeExtensions.Remove(extension);
        }

        #endregion


        #region Program point graph building

        /// <summary>
        /// Add child created from given block. This child already contains all its children within basic block.
        /// </summary>
        /// <param name="parent">Parent which child will be added</param>
        /// <param name="block">Block which first statement generates child</param>
        private void addChild(ProgramPoint parent, BasicBlock block)
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
                var child = getStatement(stmt, block);
                current.AddChild(child);
                current = child;
            }

            foreach (var edge in block.OutgoingEdges)
            {
                var condition = getCondition(edge.Condition, block);
                addChild(condition, edge.To);
                current.AddChild(condition);
            }

            if (block.DefaultBranch != null)
            {
                var conditionExpressions = from edge in block.OutgoingEdges select edge.Condition;

                if (conditionExpressions.Any())
                {
                    var defaultPoint = getDefaultBranch(conditionExpressions, block);
                    addChild(defaultPoint, block.DefaultBranch.To);
                    current.AddChild(defaultPoint);
                }
                else
                {
                    //there is no condition on edge
                    addChild(current, block.DefaultBranch.To);
                }
            }
        }

        private void fillWithBlockEntry(ProgramPoint point, BasicBlock block, HashSet<BasicBlock> fillingBlocks=null)
        {
            if (fillingBlocks == null)
            {
                fillingBlocks = new HashSet<BasicBlock>();
            }

            if (!fillingBlocks.Add(block))
            {
                //empty cycle
                point.AddChild(point);                
                return;
            }

            //parent has to point to first statement of block
            if (block.Statements.Count > 0)
            {
                var entryPoint = getStatement(block.Statements[0], block);
                point.AddChild(entryPoint);
                return;
            }
            else
            {
                var conditionExpressions = new List<Expression>();
                foreach (var edge in block.OutgoingEdges)
                {
                    //every outgoing edge is child of point, because of empty block body
                    point.AddChild(getCondition(edge.Condition, block));
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
                    var defaultPoint = getDefaultBranch(conditionExpressions, block);
                    point.AddChild(defaultPoint);
                }
            }
            
        }

        /// <summary>
        /// Get program point indexed by default branch of given expressions. If there is no such a point, new is created.
        /// </summary>
        /// <param name="expressions">Expressions for created ConditionForm.SomeNot condition</param>
        /// <param name="outerBlock">Block where expressions are located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPoint getDefaultBranch(IEnumerable<Expression> expressions, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.SomeNot, expressions.ToArray());
            return getCondition(assumption, outerBlock);
        }

        /// <summary>
        /// Get program point indexed by statement. If there is no such a point, new is created.
        /// </summary>
        /// <param name="statement">Index of program point</param>
        /// <param name="outerBlock">Block where statement is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPoint getStatement(LangElement statement, BasicBlock outerBlock)
        {
            return getPoint(statement, () => new ProgramPoint(statement, outerBlock));
        }

        /// <summary>
        /// Get program point indexed by ConditionForm.All condition, created from given expression. If there is no such point, new is created.
        /// </summary>
        /// <param name="expression">Expression for created condition</param>
        /// <param name="outerBlock">Block where expression is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPoint getCondition(Expression expression, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.All, expression);
            return getCondition(assumption, outerBlock);
        }

        /// <summary>
        /// Get program point indexed by condition. If there is no such point, new is created.
        /// </summary>
        /// <param name="condition">Index of program point</param>
        /// <param name="outerBlock">Block where condition is located</param>
        /// <returns>Indexed program point</returns>
        private ProgramPoint getCondition(AssumptionCondition condition, BasicBlock outerBlock)
        {
            return getPoint(condition, () => new ProgramPoint(condition, outerBlock));
        }

        /// <summary>
        /// Get program point indexed by key. If there is no such point, create new empty point.
        /// </summary>
        /// <param name="key">Key of created program point</param>
        /// <returns>Program point indexed by key</returns>
        private ProgramPoint getEmpty(object key)
        {
            return getPoint(key, () => new ProgramPoint());
        }

        /// <summary>
        /// Get point indexed by given key. If there is no such point, create new with creator
        /// </summary>
        /// <param name="key">Index of program point</param>
        /// <param name="creator">Creator of program point</param>
        /// <returns>Program point indexed by key</returns>
        private ProgramPoint getPoint(object key, ProgramPointCreator creator)
        {
            ProgramPoint result;

            if (!_points.TryGetValue(key, out result))
            {
                result = creator();
                _points.Add(key, result);
            }
            return result;
        }
        #endregion
    }
}
