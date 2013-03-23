using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;


namespace Weverca.ControlFlowGraph
{
    public class ControlFlowGraph
    {


    }

    public class BasicBlock {
        public List<Statement> Statements;
        public List<BasicBlockEdge> OutgoingEdges;
        public List<BasicBlockEdge> IncommingEdges;

        public BasicBlock() {
            Statements = new List<Statement>();
            OutgoingEdges = new List<BasicBlockEdge>();
            IncommingEdges = new List<BasicBlockEdge>();
        }

    }

    public class BasicBlockEdge {
        public BasicBlock From { set; get; }
        public BasicBlock To { set; get; }
        public ExpressionStmt Condition { set; get; }
        BasicBlockEdge(BasicBlock From, BasicBlock To, ExpressionStmt Condition)
        {
            this.From = From;
            this.To = To;
            this.Condition = Condition;
        } 
    }

}
