using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

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
                    string label = "" ;
                    if (!edge.Condition.Position.IsValid)
                    {
                        if (edge.Condition.GetType() == typeof(BoolLiteral))
                        {
                            label = edge.Condition.Value.ToString();
                        }
                        if (edge.Condition.GetType() == typeof(UnaryEx))
                        {
                            UnaryEx expression = (UnaryEx)edge.Condition;
                            label = globalCode.SourceUnit.GetSourceCode(expression.Expr.Position);
                            //dirty trick how to acces internal field
                            var a = expression.GetType().GetField("operation",BindingFlags.NonPublic | BindingFlags.Instance);
                            if ((Operations)a.GetValue(expression) == Operations.LogicNegation)
                            {
                                label = "not " + label; 
                            }
                        }
                        if (edge.Condition.GetType() == typeof(BinaryEx))
                        {
                            BinaryEx bin=(BinaryEx) edge.Condition;
                             //dirty trick how to acces internal field
                            var a=bin.GetType().GetField("operation",BindingFlags.NonPublic | BindingFlags.Instance);
                            if((Operations)a.GetValue(bin)==Operations.Equal){
                                Expression l = (Expression)bin.GetType().GetField("leftExpr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bin);
                                Expression r = (Expression)bin.GetType().GetField("rightExpr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bin);
                                if (l.Position.IsValid==false)
                                {
                                    label += "default";
                                }
                                else
                                {
                                    label += globalCode.SourceUnit.GetSourceCode(l.Position);
                                    label += "=";
                                    label += globalCode.SourceUnit.GetSourceCode(r.Position);

                                }
                            }
                            else
                            {
                                label += "default";
                                //label = globalCode.SourceUnit.GetSourceCode(edge.Condition.Position);
                            }
                            
                            
                        }

                    }
                    else
                    {
                        label = globalCode.SourceUnit.GetSourceCode(edge.Condition.Position);
                    }
                    result += "node" + i + " -> node" + index + "[headport=n, tailport=s,label=\"" + label + "\"]" + Environment.NewLine;
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
        }
        public static BasicBlockEdge MakeNewAndConnect(BasicBlock From, BasicBlock To, Expression Condition)
        {
            var edge=new BasicBlockEdge(From,To,Condition);
            From.AddOutgoingEdge(edge);
            To.AddIncommingEdge(edge);
            return edge;
        }
    }

}
