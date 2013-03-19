using PHP.ControlFlow;
using PHP.Core.AST;
using PHP.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHP.Core.ControlFlow
{
    /// <summary>
    /// Basic block is a stream of statements that do not contain any control 
    /// flow statements, and final (conditional) goto statement plus list of 
    /// blocks where this goto can jump. Basic blocks form nodes of <see cref="ControlFlowGraph"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     For convenience basic blocks also provide set of backward edges.
    /// </para>
    /// <para>
    ///     The statements in basic block can be enumerated as they appear in the AST 
    ///     from which they were taken, or they can be enumerated in a 3 address code 
    ///     fashion. Meaning that for an expression that consists of several 
    ///     nested instances of <see cref="Expression"/>, all the binary expressions are 
    ///     enumerated in the order in which they need to be evaluated. One can 
    ///     think of it as if they were each time assigned to a temporary variable.
    /// </para>
    /// </remarks>
    public class BasicBlock
    {
        private IList<LangElement> threeAddressStatements;

        private List<BasicBlock> prev = new List<BasicBlock>();

        private List<ControlFlowEdge> next = new List<ControlFlowEdge>();

        private BasicBlock()
        {
            this.Statements = new List<LangElement>();
        }

        public IList<LangElement> Statements { get; private set; }

        /// <summary>
        /// By convention if <see cref="Previous"/> contains itself (self-loop), 
        /// then it will be the first item in this collection.
        /// </summary>
        public IReadOnlyArray<BasicBlock> Previous { get { return ReadOnlyArray.Create(this.prev); } }

        public IEnumerable<BasicBlock> Next { get { return this.next.Select(x => x.Target); } }

        public IReadOnlyArray<ControlFlowEdge> OutgoingEdges { get { return ReadOnlyArray.Create(this.next); } }

        /// <summary>
        /// This property can be used by a graph traversing algorithm.
        /// </summary>
        public bool Visited { get; set; }

        /// <summary>
        /// This property can be used by a graph algorithm that 
        /// needs to store some node specific data. 
        /// </summary>
        public object Tag { get; set; }

        public static BasicBlock Create(ControlFlowGraph graph)
        {
            var result = new BasicBlock();
            graph.AddBasicBlock(result);
            return result;
        }

        public IEnumerable<LangElement> GetThreeAddressStatements()
        {
            if (threeAddressStatements == null)
            {
                this.threeAddressStatements = new List<LangElement>();
                var visitor = new ThreeAddressVisitor(threeAddressStatements);
                foreach (var stmt in this.Statements)
                    stmt.VisitMe(visitor);
            }

            return this.threeAddressStatements;
        }

        public string ToString(SourceUnit unit)
        {
            return string.Join("\\n", this.Statements.Select(x => unit.GetSourceCode(x.Position)).ToArray());
        }

        internal void AddNext(ControlFlowEdge edge)
        {
            this.next.Add(edge);
            edge.Target.AddPrev(this);
        }

        internal void AddNext(BasicBlock next)
        {
            this.next.Add(new ControlFlowEdge(next, null));
            next.AddPrev(this);
        }

        internal void DisconnectFromGraph()
        {
            Debug.Assert(this.Next.Count() == 1,
                "Disconnection is supported only for node with one ancestor." +
                "Nodes with no ancestor are exits and should not be leaved out.");

            // remove ourselves from the prev of our only ancessor
            this.next.First().Target.prev.Remove(this);
                
            // for each precedessor we will find the edge that goes into us, 
            // we will reconnect it to our next and update 
            // the precedessor's prev collection
            foreach (var b in this.prev)
            {
                this.next.First().Target.AddPrev(b);
                foreach (var edge in b.next)
                {
                    if (edge.Target == this)
                    {
                        edge.Target = this.next.First().Target;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This method makes sure that if we are adding this, 
        /// then it will be first in the prev list.
        /// </summary>
        private void AddPrev(BasicBlock prev)
        {
            if (prev == this)
                this.prev.Insert(0, this);
            else
                this.prev.Add(prev);
        }

        private class ThreeAddressVisitor : TreeVisitor
        {
            private IList<LangElement> statements;

            public ThreeAddressVisitor(IList<LangElement> statements)
            {
                this.statements = statements;
            }

            public override void VisitElement(LangElement element)
            {
                this.statements.Add(element);
            }

            public override void VisitDirectVarUse(DirectVarUse x)
            {
                // Filtered out: does not have any side effects, 
                // is used in expressions: cannot be on its own
            }

            public override void VisitEchoStmt(EchoStmt x)
            {
                this.VisitExpressionsList(x.Parameters);
                this.statements.Add(x);
            }

            public override void VisitIncDecEx(IncDecEx x)
            {
                this.statements.Add(x);
            }

            public override void VisitValueAssignEx(ValueAssignEx x)
            {
                x.RValue.VisitMe(this);
                this.statements.Add(x);
            }

            public override void VisitBinaryEx(BinaryEx x)
            {
                x.LeftExpr.VisitMe(this);
                x.RightExpr.VisitMe(this);
                this.statements.Add(x);
            }

            public override void VisitExpressionStmt(ExpressionStmt x)
            {
                x.Expression.VisitMe(this);
            }

            public override void VisitDirectFcnCall(DirectFcnCall x)
            {
                foreach (var p in x.CallSignature.Parameters)
                    p.VisitMe(this);

                this.statements.Add(x);
            }

            private void VisitExpressionsList(IEnumerable<Expression> list)
            {
                if (list != null)
                    foreach (var e in list)
                        e.VisitMe(this);
            }
        }
    }

}
