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
    //pravdepodobne sa nebude pouzivat
    public class ClassDeclaration
    {
        TypeDecl classData;
        public readonly Dictionary<MethodDecl, BasicBlock> DeclaredMethods = new Dictionary<MethodDecl, BasicBlock>();

        public ClassDeclaration(TypeDecl classData)
        {
            this.classData = classData;
        }

        public void AddFunctionDeclaration(MethodDecl x, BasicBlock functionBasicBlock)
        {
            DeclaredMethods.Add(x, functionBasicBlock);
        }

        internal void SimplifyMethods()
        {
            foreach (var method in DeclaredMethods)
            {
                method.Value.SimplifyGraph();
            }
        }
    }

    /// <summary>
    /// Represents Controlflow graph.
    /// </summary>
    public class ControlFlowGraph
    {
        #region fields

        /// <summary>
        /// Reference on the starting block inside coltroflow graph
        /// </summary>
        public BasicBlock start;

        /// <summary>
        /// Analyzed code.
        /// </summary>
        public GlobalCode globalCode { get; private set; }
        
        /// <summary>
        /// Visitor used for controflow graph construction.
        /// </summary>
        private CFGVisitor visitor;

        #endregion fields

        #region construction

        /// <summary>
        /// Constructs a confrolflow graph 
        /// </summary>
        /// <param name="globalCode"></param>
        public ControlFlowGraph(GlobalCode globalCode)
        {
            this.globalCode = globalCode;
            List<Statement> functionsAndClasses = new List<Statement>();
            foreach(var statement in globalCode.Statements)
            {
                if (statement is TypeDecl || statement is FunctionDecl)
                {
                    functionsAndClasses.Add(statement);
                }
            }

            foreach (var statement in functionsAndClasses)
            {
                globalCode.Statements.Remove(statement);
            }

            globalCode.Statements.InsertRange(0, functionsAndClasses);
            
            this.visitor = new CFGVisitor(this);
            globalCode.VisitMe(visitor);

            PostProcess(visitor);
            
        }

        /// <summary>
        /// Constructs a confrolflow graph. This method should be used only for testing with purpose of testing.
        /// </summary>
        /// <param name="globalCode">needed for drawing</param>
        /// <param name="function">function to construct controlflow graph</param>
        public ControlFlowGraph(GlobalCode globalCode,MethodDecl function)
        {
            this.globalCode = globalCode;
            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function,function.Body);
            PostProcess(visitor);
        }

        /// <summary>
        /// Constructs a confrolflow graph. This method should be used only for testing with purpose of testing.
        /// </summary>
        /// <param name="globalCode">Globalcode needed for drawing</param>
        /// <param name="function">function to construct controlflow graph</param>
        public ControlFlowGraph(GlobalCode globalCode, FunctionDecl function)
        {
            this.globalCode = globalCode;
            
            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function, function.Body);
            PostProcess(visitor);
        }

        /// <summary>
        /// Constructs a confrolflow graph. This method should be used for analysis. It cannot be used for testing.
        /// </summary>
        /// <param name="function">function to construct controlflow graph</param>
        public ControlFlowGraph(MethodDecl function)
        {

            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function, function.Body);
            PostProcess(visitor);
        }

        /// <summary>
        /// Constructs a confrolflow graph. This method should be used for analysis. It cannot be used for testing.
        /// </summary>
        /// <param name="function">function to construct controlflow graph<</param>
        public ControlFlowGraph(FunctionDecl function)
        {
            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function, function.Body);
            PostProcess(visitor);
        }

        #endregion construction

        private void PostProcess(CFGVisitor visitor)
        {
            visitor.CheckLabels();
            start.SimplifyGraph();
        }

        #region generatingTextRepresentation

        /// <summary>
        /// Generates the graph in dot language for purspose of drawing.
        /// </summary>
        /// <returns>String representation in dot language of current conftrolflow graph.</returns>
        public string getTextRepresentation()
        {
            string result = "digraph g {node [shape=box]" + Environment.NewLine + " graph[rankdir=\"TB\", concentrate=true];" + Environment.NewLine;

            result += generateText(0);
            result += "\n}" + Environment.NewLine;
            return result;
        }

        /// <summary>
        /// Recursive method used for generating the body of text representation of controlflow graph.
        /// It generates the result for globalcode and dureing the generation it calls
        /// itself resursivly on controlflow graphs of declared functions and methods.
        /// </summary>
        /// <param name="counter">Body of string representation in dot language of current conftrolflow graph.</param>
        /// <returns></returns>
        private string generateText(int counter)
        {
            string result = "";
            List<BasicBlock> nodes = new List<BasicBlock>();
            Queue<BasicBlock> queue = new Queue<BasicBlock>();

            /*
            Prechod grafu do hlbky a poznamenanie vsetkych hran do nodes 
            */
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                BasicBlock node = queue.Dequeue();
                if (!nodes.Contains(node))
                {
                    nodes.Add(node);
                    if (node is TryBasicBlock)
                    {
                        foreach (var catchNode in (node as TryBasicBlock).catchBlocks)
                        {
                            queue.Enqueue(catchNode);
                        }
                    }
                    foreach (var edge in node.OutgoingEdges)
                    {
                        if (!nodes.Contains(edge.To))
                        {
                            queue.Enqueue(edge.To);
                        }
                    }
                    if (node.DefaultBranch != null && !nodes.Contains(node.DefaultBranch.To))
                    {
                        queue.Enqueue(node.DefaultBranch.To);
                    }

                    //PAVEL - protoze GOTO muze vytvorit nedostupne vetve, tak projdeme i vstupni hrany
                    foreach (var edge in node.IncommingEdges)
                    {
                        if (!nodes.Contains(edge.From))
                        {
                            queue.Enqueue(edge.From);
                        }
                    }
                }
            }

            /*
            Generovanie textu pre vsetky uzly
             */
            string functionsResult = "";
            int i = counter;
            int oldCounter = counter;
            foreach (var node in nodes)
            {
                string label = "";
                foreach (var statement in node.Statements)
                {
                    /*
                     rekurzivne generovanie cfg a textovej reprezentacii pre funkcie
                     */
                    if (statement.GetType() == typeof(FunctionDecl))
                    {
                        FunctionDecl function = (FunctionDecl)statement;
                        label += "function " + function.Function.Name+ Environment.NewLine;
                        
                        try
                        {
                            ControlFlowGraph cfg = new ControlFlowGraph(globalCode, function);
                             functionsResult+=cfg.generateText((counter / 10000 + 1) * 10000);
                            counter+=10000;
                        }
                        catch (Weverca.ControlFlowGraph.ControlFlowException e)
                        {
                            Console.WriteLine(e.Message);
                            return "";
                        }
                      
                       
                    }
                     /*
                     rekurzivne generovanie cfg a textovej reprezentacii pre metody objektov
                     */
                    else if (statement.GetType() == typeof(TypeDecl))
                    {
                        TypeDecl clas = (TypeDecl)statement;
                        label += "class " + clas.Name.ToString() + Environment.NewLine;
                        foreach(var method in clas.Members)
                        {
                            if (method.GetType() == typeof(MethodDecl))
                            {
                                try
                                {
                                ControlFlowGraph cfg = new ControlFlowGraph(globalCode, method as MethodDecl);
                                functionsResult += cfg.generateText((counter / 10000 + 1) * 10000);
                                counter += 10000;
                                }
                                catch (Weverca.ControlFlowGraph.ControlFlowException e)
                                {
                                    Console.WriteLine(e.Message);
                                    return "";
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        label += globalCode.SourceUnit.GetSourceCode(statement.Position) + Environment.NewLine;
                    }
                }
                if (node.EndIngTryBlocks.Count > 0)
                {
                    label += "ending Try block";
                }
                label = label.Replace("\"", "\\\"");
                result += "node" + i + "[label=\"" + label + "\"]" + Environment.NewLine;
                i++;
            }
            /*
            vykreslovanie hran
            */
            i = oldCounter;
            foreach (var node in nodes)
            {
                foreach (var edge in node.OutgoingEdges)
                {
                    int index = oldCounter + nodes.IndexOf(edge.To);
                    string label = "";
                    //v pripdae ze som tam umelo pridal podmienku v cfg a tato podmienka nebola v kode, je potrebne ju inym sposobom vypisat
                    if (!edge.Condition.Position.IsValid)
                    {
                        if (edge.Condition.GetType() == typeof(BoolLiteral))
                        {
                            label = edge.Condition.Value.ToString();
                        }
                        if (edge.Condition.GetType() == typeof(BinaryEx))
                        {
                            BinaryEx bin = (BinaryEx)edge.Condition;
                            //dirty trick how to acces internal field
                            var a = bin.GetType().GetField("operation", BindingFlags.NonPublic | BindingFlags.Instance);
                            if ((Operations)a.GetValue(bin) == Operations.Equal)
                            {
                                Expression l = (Expression)bin.GetType().GetField("leftExpr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bin);
                                Expression r = (Expression)bin.GetType().GetField("rightExpr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bin);
                                if (l.Position.IsValid == false)
                                {
                                    if (l.GetType() == typeof(IntLiteral))
                                    {
                                        label += "" + ((IntLiteral)l).Value;
                                        label += "=";
                                        label += globalCode.SourceUnit.GetSourceCode(r.Position);
                                    }
                                    else
                                    {
                                        label += "else";
                                    }
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
                        if (edge.Condition.GetType() == typeof(DirectFcnCall))
                        {
                            DirectFcnCall functionCall = (DirectFcnCall)edge.Condition;

                            label += functionCall.QualifiedName + "(";
                            foreach (var parameter in functionCall.CallSignature.Parameters)
                            {
                                if (parameter.Expression.GetType() == typeof(StringLiteral))
                                {
                                    StringLiteral literal = (StringLiteral)parameter.Expression;
                                    label += "\"" + literal.Value + "\"" + ",";
                                }
                                else
                                {
                                    label += globalCode.SourceUnit.GetSourceCode(parameter.Expression.Position) + ",";
                                }
                            }
                            label += ")";
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
                    string elseString = string.Empty;
                    if (node.OutgoingEdges.Count > 0)
                    {
                        elseString = "else";
                    }

                    int index = oldCounter + nodes.IndexOf(node.DefaultBranch.To);
                    result += "node" + i + " -> node" + index + "[headport=n, tailport=s,label=\" " + elseString + "  \"]" + Environment.NewLine;
                }

                i++;

            }
            result += functionsResult;
            result += Environment.NewLine;
            return result;
        }
        #endregion generatingTextRepresentation
    }
}
