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

    public class ControlFlowGraph
    {
        public BasicBlock start;
        private GlobalCode globalCode;
        private CFGVisitor visitor;

    //    private readonly Dictionary<FunctionDecl, BasicBlock> declaredFunctions = new Dictionary<FunctionDecl, BasicBlock>();
    //    private readonly List<ClassDeclaration> declaredClasses = new List<ClassDeclaration>();

        public ControlFlowGraph(GlobalCode globalCode)
        {
            this.globalCode = globalCode;
            this.visitor = new CFGVisitor(this);
            globalCode.VisitMe(visitor);

            Simplify();
        }

        public ControlFlowGraph(GlobalCode globalCode,MethodDecl function)
        {
            this.globalCode = globalCode;
            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function,function.Body);
            Simplify();
        }

        public ControlFlowGraph(GlobalCode globalCode, FunctionDecl function)
        {
            this.globalCode = globalCode;
            
            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function, function.Body);
            Simplify();
        }

        public ControlFlowGraph(MethodDecl function)
        {

            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function, function.Body);
            Simplify();
        }

        public ControlFlowGraph(FunctionDecl function)
        {
            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function, function.Body);
            Simplify();
        }

        public void Simplify()
        {
            start.SimplifyGraph();

          /*  foreach (var function in declaredFunctions)
            {
                function.Value.SimplifyGraph();
            }

            foreach (var declaredClass in declaredClasses)
            {
                declaredClass.SimplifyMethods();
            }*/
        }
        public string getTextRepresentation()
        {
            string result = "digraph g {node [shape=box]" + Environment.NewLine + " graph[rankdir=\"TB\", concentrate=true];" + Environment.NewLine;

            result += generateText(0);
            result += "\n}" + Environment.NewLine;
            return result;
        }


        private string generateText(int counter)
        {
            string result = "";
            List<BasicBlock> nodes = new List<BasicBlock>();
            Queue<BasicBlock> queue = new Queue<BasicBlock>();

            /* Nejprve pridam vsechny deklarovane funkce */
            /*foreach (var func in declaredFunctions)
            {
                queue.Enqueue(func.Value);
            }*/
            /* a tridy */
            /*foreach (var cl in declaredClasses)
            {
                foreach (var method in cl.DeclaredMethods)
                {
                    queue.Enqueue(method.Value);
                }
            }*/

            /*
            Prechod grafu do hlbky a poznameniae vsetkych hran do nodes 
            */
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                BasicBlock node = queue.Dequeue();
                if (!nodes.Contains(node))
                {
                    nodes.Add(node);
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
                    if (statement.GetType() == typeof(FunctionDecl))
                    {
                        FunctionDecl function = (FunctionDecl)statement;
                        label += "function " + function.Function.Name+ Environment.NewLine;
                        ControlFlowGraph cfg = new ControlFlowGraph(globalCode, function);
                        functionsResult+=cfg.generateText((counter / 10000 + 1) * 10000);
                        counter+=10000;
                    }
                    else if (statement.GetType() == typeof(TypeDecl))
                    {
                        TypeDecl clas = (TypeDecl)statement;
                        label += "class " + clas.Name.ToString() + Environment.NewLine;
                    }
                    else
                    {
                        label += globalCode.SourceUnit.GetSourceCode(statement.Position) + Environment.NewLine;
                    }
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
                    //v pripdae ze som tam umelo pridal podmienku v cfg a tato podmienka nebola v kode
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


        /*public ClassDeclaration AddClassDeclaration(TypeDecl x)
        {
            ClassDeclaration declaration = new ClassDeclaration(x);
            declaredClasses.Add(declaration);
            return declaration;
        }

        public void AddFunctionDeclaration(FunctionDecl x, BasicBlock functionBasicBlock)
        {
            declaredFunctions.Add(x, functionBasicBlock);
        }

        public BasicBlock GetBasicBlock(FunctionDecl function)
        {
            return declaredFunctions[function];
        }*/
    }

}
