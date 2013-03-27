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
                    
            /*
            Prechod grafu do hlbky a poznameniae vsetkych hran do nodesô 
            */
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
                    if (node.DefaultBranch != null && !nodes.Contains(node.DefaultBranch.To))
                    {
                        queue.Enqueue(node.DefaultBranch.To);
                    }
                }
            }

            /*
            Generovanie textu pre vsetky uzly
             */ 
            int i=0;
            foreach (var node in nodes) {
                string label = "";
                foreach (var statement in node.Statements) {
                    label +=globalCode.SourceUnit.GetSourceCode(statement.Position) + Environment.NewLine;
                }
                label=label.Replace("\"", "\\\"");
                result += "node" + i + "[label=\"" + label + "\"]" + Environment.NewLine;
                i++;
            }
            /*
            
            */
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
                        /*if (edge.Condition.GetType() == typeof(UnaryEx))
                        {
                            UnaryEx expression = (UnaryEx)edge.Condition;
                            label = globalCode.SourceUnit.GetSourceCode(expression.Expr.Position);
                            //dirty trick how to acces internal field
                            var a = expression.GetType().GetField("operation",BindingFlags.NonPublic | BindingFlags.Instance);
                            if ((Operations)a.GetValue(expression) == Operations.LogicNegation)
                            {
                                label = "not " + label; 
                            }
                        }*/
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
                                    label += "else";
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
                                label += "else";
                                //label = globalCode.SourceUnit.GetSourceCode(edge.Condition.Position);
                            }
                        }
                    }
                    else
                    {
                        label = globalCode.SourceUnit.GetSourceCode(edge.Condition.Position);
                    }
                    label = label.Replace("\"", "\\\"");
                    result += "node" + i + " -> node" + index + "[headport=n, tailport=s,label=\"" + label + "\"]" + Environment.NewLine;
                }

                if (node.DefaultBranch != null)
                {
                    int index = nodes.IndexOf(node.DefaultBranch.To);
                    result += "node" + i + " -> node" + index + "[headport=n, tailport=s,label=\" else  \"]" + Environment.NewLine;
                }    
                
                i++;
                    
            }
            
            result += "\n}" + Environment.NewLine;
          
            return result;
        }

    }

    public class BasicBlock {
        public List<LangElement> Statements;

        public List<ConditionalEdge> OutgoingEdges;
        public List<IBasicBlockEdge> IncommingEdges;

        public DirectEdge DefaultBranch;

        public BasicBlock() {
            Statements = new List<LangElement>();
            OutgoingEdges = new List<ConditionalEdge>();
            IncommingEdges = new List<IBasicBlockEdge>();
            DefaultBranch = null;
        }


        public void AddElement(LangElement element)
        {
            Statements.Add(element);
        }

        public void AddIncommingEdge(IBasicBlockEdge edge) {
            IncommingEdges.Add(edge);
        }

        public void AddOutgoingEdge(ConditionalEdge edge)
        {
            OutgoingEdges.Add(edge);
        }

        public void SetDefaultBranch(DirectEdge edge)
        {
            DefaultBranch = edge;
        }
    }


    public interface IBasicBlockEdge
    {
        BasicBlock From { set; get; }
        BasicBlock To { set; get; }
    }

    public class ConditionalEdge : IBasicBlockEdge
    {
        public BasicBlock From { set; get; }
        public BasicBlock To { set; get; }
        public Expression Condition { set; get; }

        public ConditionalEdge(BasicBlock From, BasicBlock To, Expression Condition)
        {
            this.From = From;
            this.To = To;
            this.Condition = Condition;
        }
        public static ConditionalEdge MakeNewAndConnect(BasicBlock From, BasicBlock To, Expression Condition)
        {
            var edge = new ConditionalEdge(From, To, Condition);
            From.AddOutgoingEdge(edge);
            To.AddIncommingEdge(edge);
            return edge;
        }
    }

    public class DirectEdge : IBasicBlockEdge
    {
        public BasicBlock From { set; get; }
        public BasicBlock To { set; get; }

        public DirectEdge(BasicBlock From, BasicBlock To)
        {
            this.From = From;
            this.To = To;
        }
        public static DirectEdge MakeNewAndConnect(BasicBlock From, BasicBlock To)
        {
            var edge = new DirectEdge(From, To);
            From.SetDefaultBranch(edge);
            To.AddIncommingEdge(edge);
            return edge;
        }
    }


}
