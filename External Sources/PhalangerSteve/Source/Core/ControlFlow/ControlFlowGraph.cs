using System.Linq;
using PHP.Core.AST;
using PHP.Core.ControlFlow;
using System.Collections.Generic;
using System.Text;
using PHP.Core.Reflection;
using System;
using System.Collections.ObjectModel;
using PHP.Core;

namespace PHP.ControlFlow
{
    public class ControlFlowGraph
    {
        private SourceUnit sourceUnit;

        private IList<BasicBlock> blocks = new List<BasicBlock>();

        private IList<BasicBlock> dfsOrder = new List<BasicBlock>();

        private BasicBlock start;

        /// <summary>
        /// By convention the first node is the same as <see cref="Start"/>.
        /// </summary>
        public IReadOnlyArray<BasicBlock> Blocks { get { return ReadOnlyArray.Create(this.blocks); } }

        public IReadOnlyArray<BasicBlock> BlocksInDFSOrder { get { return ReadOnlyArray.Create(this.dfsOrder); } }

        /// <summary>
        /// By convention this should be the first node in <see cref="Blocks"/> collection.
        /// </summary>
        public BasicBlock Start { get { return this.start; } }

        public static ControlFlowGraph Construct(LangElement element)
        {
            var result = new ControlFlowGraph();
            element.VisitMe(new CFGVisitor(result));
            result.CompactAndUpdateDfsOrder();
            return result;
        }

        public static ControlFlowGraph Construct(GlobalCode code)
        {
            var result = new ControlFlowGraph();
            result.sourceUnit = code.SourceUnit;
            code.VisitMe(new CFGVisitor(result));
            result.CompactAndUpdateDfsOrder();
            return result;
        }

        public string GetDotGraph(SourceUnit sourceUnit = null)
        {
            if (sourceUnit == null)
                sourceUnit = this.sourceUnit;

            if (sourceUnit == null)
                throw new InvalidOperationException(
                    "GetDotGraph was invoked with null sourceUnit and the ControlFlowGraph itself does not " +
                    "contain a sourceUnit. This happens when the graph was not constructured from AST element that " +
                    "provides its sourceUnit. Construct the graph from e.g. GlobalCode AST element or provide sourceUnit" +
                    "as an argument to this function");

            var sb = new StringBuilder();
            sb.AppendLine("digraph flow {");
            sb.AppendLine("\t node[shape=box]");
            sb.AppendLine("\t graph[rankdir=\"TB\", concentrate=true];");
            for (int i = 0; i < this.blocks.Count; i++)
            {
                sb.AppendFormat("\t{0} [label=\"{1}\"]\n", i, string.Join("\\n",
                    this.blocks[i].Statements.Select(x => sourceUnit.GetSourceCode(x.Position)).ToArray()));
            }

            for (int i = 0; i < this.blocks.Count; i++)
            {
                foreach (var edge in this.blocks[i].OutgoingEdges)
                {
                    string label = "";
                    if (edge.HasBranchExpression)
                        label = string.Format(",label=\"{0}{1}\"", edge.Negation ? "NOT " : "",
                             sourceUnit.GetSourceCode(edge.BranchExpression.Position));

                    sb.AppendFormat("\t{0} -> {1} [headport=n, tailport=s{2}]\n", i, this.blocks.IndexOf(edge.Target), label);
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        internal void AddBasicBlock(BasicBlock block)
        {
            this.blocks.Add(block);
        }

        /// <summary>
        /// Comparts the flow graph: removes empty nodes and nodes 
        /// that are obviously not reachable by using depth first search algorithm.
        /// </summary>
        private void CompactAndUpdateDfsOrder()
        {
            foreach (var b in this.blocks)
                b.Visited = false;

            var stack = new Stack<BasicBlock>(this.blocks.Count);
            this.dfsOrder.Clear();
            stack.Push(this.start);
            while (stack.Any())
            {
                var current = stack.Pop();
                current.Visited = true;

                // If it is a sink node, we just continue, 
                // sink nodes are not leaved out even if they are empty
                if (!current.Next.Any())
                {
                    this.dfsOrder.Add(current);
                    continue;
                }

                // Nodes that are empty are leaved out from the graph
                if (current.Statements.Count == 0)
                {
                    current.DisconnectFromGraph();
                    this.blocks.Remove(current);
                }
                else
                {
                    this.dfsOrder.Add(current);
                }

                foreach (var next in current.Next.Where(x => !x.Visited))
                {
                    stack.Push(next);
                }
            }

            // Nodes that weren't visited are not reachable
            for (int i = 0; i < this.blocks.Count; i++)
            {
                if (!this.blocks[i].Visited)
                {
                    this.blocks[i].DisconnectFromGraph();
                    this.blocks.RemoveAt(i);
                }
            }
        }

        /* TODO:
         *      - switch-case statement
         *      - try-catch (optinally with possibility of not encoding it into CFG at all)
         */
        private class CFGVisitor : TreeVisitor
        {
            private ControlFlowGraph graph;

            private BasicBlock current;

            private IDictionary<string, BasicBlock> labels = new Dictionary<string, BasicBlock>();

            private Stack<LoopAndSink> currentLoop = new Stack<LoopAndSink>();

            public CFGVisitor(ControlFlowGraph graph)
            {
                this.graph = graph;
            }

            public override void VisitElement(LangElement element)
            {
                this.current.Statements.Add(element);
            }

            public override void VisitGlobalCode(GlobalCode x)
            {
                this.current = this.graph.start = BasicBlock.Create(this.graph);
                this.VisitStatementList(x.Statements);
            }

            public override void VisitLabelStmt(LabelStmt x)
            {
                var next = BasicBlock.Create(this.graph);
                this.current.AddNext(next);
                this.current = next;
                this.labels.Add(x.Name.Value, this.current);
            }

            public override void VisitGotoStmt(GotoStmt x)
            {
                BasicBlock next;
                if (!this.labels.TryGetValue(x.LabelName.Value, out next))
                    throw new Exception("Goto to a non existing label " + x.LabelName.Value);

                this.current.AddNext(next);

                /* We are creating a block that will have no incoming edges.
                 * These nodes are created so that the algorithm can continue seamlessly.
                 * Nodes with no incoming edges are easily cleared out at the end. */
                this.current = BasicBlock.Create(this.graph);
            }

            public override void VisitIfStmt(IfStmt x)
            {
                this.VisitConditionalStatements(x.Conditions);
            }

            public override void VisitSwitchStmt(SwitchStmt x)
            {
                throw new NotImplementedException("TODO");
                /* problems: 
                 *  - can a switch expression have side effects?
                 *  - is the expression guaranteed to be evaluated excatly once or once for each case?
                 *  - how is switch emitted? How to emit it here so that we will not loose possibility 
                 *  to implement better switch with hashtables?
                 */
            }

            public override void VisitJumpStmt(JumpStmt x)
            {
                if (x.Type == JumpStmt.Types.Return)
                {
                    // possibly connection to a single sink node, but for now we 
                    // admit more sink nodes so we just leave the current node alone
                    this.current = BasicBlock.Create(this.graph);
                }
                else
                {
                    this.VisitBreakOrContinue(x);
                }
            }

            private void VisitBreakOrContinue(JumpStmt x)
            {
                /* Construct list of loops where the jump might be jumping.
                 * break and continue statements can have an expression as an argument 
                 * in which case we will connect such jump to all possible target loops.
                 * However, break and continue with integer literal are so common that 
                 * we check if this case specifically to reduce the number of edges. */
                var loops = this.currentLoop.AsEnumerable();
                if (x.Expression is IntLiteral || x.Expression == null)
                {
                    int value = 1;
                    if (x.Expression != null)
                    {
                        var objValue = ((IntLiteral)x.Expression).Value;
                        Debug.Assert(objValue is int);
                        value = (int)objValue;
                    }

                    // break 0; is interpreted the same as break 1; source: php.net
                    if (value == 0)
                        value = 1;

                    Debug.Assert(loops.Count() >= value);
                    loops = new[] { loops.Reverse().Skip(value - 1).First() };
                }

                if (x.Type == JumpStmt.Types.Break)
                {
                    foreach (var loop in loops)
                        this.current.AddNext(loop.Sink);
                }
                else
                {
                    foreach (var loop in loops)
                        this.current.AddNext(loop.Loop);
                }

                this.current = BasicBlock.Create(this.graph);
            }

            public override void VisitForStmt(ForStmt x)
            {
                this.VisitExpressionList(x.InitExList);
                this.VisitExpressionList(x.CondExList);

                // TOOD: can there really be more than one expression in CondExList???

                var forBody = BasicBlock.Create(this.graph);
                var afterFor = BasicBlock.Create(this.graph);
                this.current.AddNext(new ControlFlowEdge(forBody, x.CondExList.First()));
                this.current.AddNext(ControlFlowEdge.WithNegatedExpression(afterFor, x.CondExList.First()));
                this.current = forBody;

                this.currentLoop.Push(new LoopAndSink(forBody, afterFor));
                x.Body.VisitMe(this);
                this.VisitExpressionList(x.ActionExList);
                this.VisitExpressionList(x.CondExList);
                var peek = this.currentLoop.Pop();
                Debug.Assert(peek.Loop == forBody);

                this.current.AddNext(new ControlFlowEdge(forBody, x.CondExList.First()));
                this.current.AddNext(ControlFlowEdge.WithNegatedExpression(afterFor, x.CondExList.First()));

                this.current = afterFor;
            }

            public override void VisitWhileStmt(WhileStmt x)
            {
                var body = BasicBlock.Create(this.graph);
                var sink = BasicBlock.Create(this.graph);

                if (x.LoopType == WhileStmt.Type.While)
                {
                    x.CondExpr.VisitMe(this);
                    this.current.AddNext(new ControlFlowEdge(body, x.CondExpr));
                    this.current.AddNext(ControlFlowEdge.WithNegatedExpression(sink, x.CondExpr));
                }
                else
                {
                    this.current.AddNext(body);
                }

                this.currentLoop.Push(new LoopAndSink(body, sink));
                this.current = body;
                x.Body.VisitMe(this);
                x.CondExpr.VisitMe(this);
                var peek = this.currentLoop.Pop();
                Debug.Assert(peek.Loop == body);

                this.current.AddNext(new ControlFlowEdge(body, x.CondExpr));
                this.current.AddNext(ControlFlowEdge.WithNegatedExpression(sink, x.CondExpr));

                this.current = sink;
            }

            #region Forwarding to VisitStatementList or VisitExpressionList

            public override void VisitSwitchItem(SwitchItem x)
            {
                VisitStatementList(x.Statements);
            }

            public override void VisitBlockStmt(BlockStmt x)
            {
                VisitStatementList(x.Statements);
            }

            #endregion

            #region Forwarding to default VisitElement

            public override void VisitStringLiteral(StringLiteral x)
            {
                this.VisitElement(x);
            }

            public override void VisitDirectVarUse(DirectVarUse x)
            {
                this.VisitElement(x);
            }

            public override void VisitConstantUse(ConstantUse x)
            {
                this.VisitElement(x);
            }

            public override void VisitEchoStmt(EchoStmt x)
            {
                this.VisitElement(x);
            }

            public override void VisitBinaryEx(BinaryEx x)
            {
                this.VisitElement(x);
            }

            public override void VisitValueAssignEx(ValueAssignEx x)
            {
                this.VisitElement(x);
            }

            public override void VisitIncDecEx(IncDecEx x)
            {
                this.VisitElement(x);
            }

            #endregion

            private void VisitStatementList(List<Statement> list)
            {
                foreach (var stmt in list)
                    stmt.VisitMe(this);
            }

            private void VisitExpressionList(List<Expression> list)
            {
                foreach (var e in list)
                    e.VisitMe(this);
            }

            private void VisitConditionalStatements(List<ConditionalStmt> list)
            {               
                // all the branches will have to be connected to the first basic block 
                // after the conditional statement. 
                var sink = BasicBlock.Create(this.graph);

                for (int i = 0; i < list.Count; i++)
                {
                    var condStmt = list[i];
                    if (condStmt.Condition == null)
                    {
                        // else branch
                        condStmt.Statement.VisitMe(this);
                        this.current.AddNext(sink);
                    }
                    else
                    {
                        condStmt.Condition.VisitMe(this);

                        /* Next block will be used to evaluate expression 
                         * that is in the next else-if branch. This expression must be evaluated 
                         * if the current then branch is not taken, because we will be branching 
                         * again but before that we have to evaluate the expression. 
                         * However if we are the last conditional statement in the chain, 
                         * we do not need to add it and we can connect directly to the sink */
                        BasicBlock nextBlock = null;
                        if (i < list.Count - 1)
                        {
                            nextBlock = BasicBlock.Create(this.graph);
                            this.current.AddNext(
                                ControlFlowEdge.WithNegatedExpression(nextBlock, condStmt.Condition));
                        }
                        else
                        {
                            this.current.AddNext(
                                ControlFlowEdge.WithNegatedExpression(sink, condStmt.Condition));
                        }

                        // Then block
                        var then = BasicBlock.Create(this.graph);
                        this.current.AddNext(new ControlFlowEdge(then, condStmt.Condition));
                        this.current = then;                 
                        condStmt.Statement.VisitMe(this);
                        this.current.AddNext(sink);

                        if (i < list.Count - 1)
                            this.current = nextBlock;
                    }
                }

                this.current = sink;
            }

            private class LoopAndSink
            {
                public LoopAndSink(BasicBlock loop, BasicBlock sink)
                {
                    this.Loop = loop;
                    this.Sink = sink;
                }

                public BasicBlock Loop { get; private set; }

                public BasicBlock Sink { get; private set; }
            }
        }
    }
}
