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
        public BasicBlock start;
        private GlobalCode globalCode;
        private CFGVisitor visitor;
        public ControlFlowGraph(GlobalCode globalCode)
        {
            this.globalCode = globalCode;
            this.visitor = new CFGVisitor(this);
            globalCode.VisitMe(visitor);

        }
        public string getTextRepresentation()
        {
            string result = "digraph g {node [shape=box]" + Environment.NewLine + " graph[rankdir=\"TB\", concentrate=true];"; 

            List<BasicBlock> nodes = new List<BasicBlock>();
            Queue<BasicBlock> queue = new Queue<BasicBlock>();
            queue.Enqueue(start);
            while(queue.Count>0) 
            {
                BasicBlock node = queue.Dequeue();
                if (!nodes.Contains(node)) {
                    nodes.Add(node);
                    foreach(var edge in node.OutgoingEdges){
                        if (!nodes.Contains(edge.To))
                        {
                            queue.Enqueue(edge.To);
                        }
                    }
                }
            }
            int i=0;
            foreach (var node in nodes) {
                string label = "";
                foreach (var statement in node.Statements) {
                    label +=globalCode.SourceUnit.GetSourceCode(statement.Position) + Environment.NewLine;
                }
                result += "node" + i + "[label=\"" + label + "\"]" + Environment.NewLine;
                i++;
            }

             i = 0;
            foreach (var node in nodes)
            {
                foreach (var edge in node.OutgoingEdges)
                {
                    int index=nodes.IndexOf(edge.To);
                    result += "node" + i + " -> node" + index + "[headport=n, tailport=s,label=\" \"]" + Environment.NewLine;
                }
                i++;
            }

            result += "\n}" + Environment.NewLine;
            return result;
        }

    }

    public class BasicBlock {
        public List<LangElement> Statements;
        public List<BasicBlockEdge> OutgoingEdges;
        public List<BasicBlockEdge> IncommingEdges;

        public BasicBlock() {
            Statements = new List<LangElement>();
            OutgoingEdges = new List<BasicBlockEdge>();
            IncommingEdges = new List<BasicBlockEdge>();
        }


        public void AddElement(LangElement element)
        {
            Statements.Add(element);
        }

        public void AddIncommingEdge(BasicBlockEdge edge) {
            IncommingEdges.Add(edge);
        }

        public void AddOutgoingEdge(BasicBlockEdge edge)
        {
            OutgoingEdges.Add(edge);
        }
    }

    public class BasicBlockEdge {
        public BasicBlock From { set; get; }
        public BasicBlock To { set; get; }
        public Expression Condition { set; get; }
        public BasicBlockEdge(BasicBlock From, BasicBlock To, Expression Condition)
        {
            this.From = From;
            this.To = To;
            this.Condition = Condition;
            From.AddOutgoingEdge(this);
            To.AddIncommingEdge(this);
        } 
    }

}
