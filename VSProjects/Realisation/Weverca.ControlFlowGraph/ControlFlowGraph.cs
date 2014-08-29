/*
Copyright (c) 2012-2014 Marcel Kikta and David Hauzar.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;

using Weverca.Parsers;
using System.Runtime.Serialization.Formatters.Binary;


namespace Weverca.ControlFlowGraph
{

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

        /// <summary>
        /// HashSet of elements added by controlglow grah. Usualy there are conditions created by cfg.
        /// This Hashset is used for cfg drawing purposes
        /// </summary>
        public HashSet<LangElement> cfgAddedElements = new HashSet<LangElement>();

        /// <summary>
        /// File name of source code filet
        /// </summary>
        public readonly FileInfo File;

        #endregion fields

        #region construction

        /// <summary>
        /// Creates a confrolflow graph from script with given file name.
        /// </summary>
        /// <param name="file">File</param>
        /// <returns>new instace of ControlFlowGraph</returns>
        public static ControlFlowGraph FromFile(FileInfo file)
        {
            var fileName = file.FullName;
            SyntaxParser parser = GenerateParser(fileName);
            parser.Parse();
            if (parser.Ast == null)
            {
                throw new ArgumentException("The specified file cannot be parsed.");
            }

            return new ControlFlowGraph(parser.Ast, file);
        }


        /// <summary>
        /// Creates a confrolflow graph from globalCode in parameter.
        /// </summary>
        /// <param name="globalCode">Ast tree</param>
        /// <param name="sourceFile">Information about source file</param>
        /// <returns>new instace of ControlFlowGraph</returns>
        public static ControlFlowGraph FromSource(GlobalCode globalCode, FileInfo sourceFile)
        {
            return new ControlFlowGraph(globalCode, sourceFile);
        }


        /// <summary>
        /// Creates a confrolflow graph from fileName in parameter.
        /// </summary>
        /// <param name="phpCode">source code in string</param>
        /// <param name="fileName">name of the file</param>
        /// <returns>new instace of ControlFlowGraph</returns>
        public static ControlFlowGraph FromSource(string phpCode, string fileName)
        {
            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            SyntaxParser parser = new SyntaxParser(source_file, phpCode);
            parser.Parse();
            if (parser.Ast == null)
            {
                throw new ArgumentException("The specified input cannot be parsed.");
            }

            return new ControlFlowGraph(parser.Ast, new FileInfo(fileName));
        }


        /// <summary>
        /// Constructs a confrolflow graph. This method should be used for analysis. It cannot be used for testing.
        /// </summary>
        /// <param name="function">function to construct controlflow graph</param>
        /// <param name="file">Information about source file</param>
        /// <returns>new instace of ControlFlowGraph</returns>
        public static ControlFlowGraph FromFunction(FunctionDecl function, FileInfo file)
        {
            return new ControlFlowGraph(SyntaxParser.functions[function], function, file);
        }

        /// <summary>
        /// Constructs a confrolflow graph. This method should be used for analysis. It cannot be used for testing.
        /// </summary>
        /// <param name="method">method to construct controlflow graph</param>
        /// <param name="file">Information about source file</param>
        /// <returns>new instace of ControlFlowGraph</returns>
        public static ControlFlowGraph FromMethod(MethodDecl method, FileInfo file)
        {
            return new ControlFlowGraph(SyntaxParser.functions[method], method, file);
        }

        /// <summary>
        /// Constructs a confrolflow graph 
        /// </summary>
        /// <param name="globalCode">Ast Tree</param>
        /// <param name="file">Information about source file</param>
        private ControlFlowGraph(GlobalCode globalCode, FileInfo file)
        {
            File = file;
            this.globalCode = globalCode;
            List<Statement> functionsAndClasses = new List<Statement>();
            foreach (var statement in globalCode.Statements)
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
        /// <param name="file">Information about source file</param>
        private ControlFlowGraph(GlobalCode globalCode, MethodDecl function, FileInfo file)
        {
            File = file;
            this.globalCode = globalCode;
            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function, function.Body);
            PostProcess(visitor);
        }

        /// <summary>
        /// Constructs a confrolflow graph. This method should be used only for testing with purpose of testing.
        /// </summary>
        /// <param name="globalCode">Globalcode needed for drawing</param>
        /// <param name="function">function to construct controlflow graph</param>
        /// <param name="file">Information about source file</param>
        private ControlFlowGraph(GlobalCode globalCode, FunctionDecl function, FileInfo file)
        {
            File = file;
            this.globalCode = globalCode;

            this.visitor = new CFGVisitor(this);
            start = visitor.MakeFunctionCFG(function, function.Body);
            PostProcess(visitor);
        }

        #endregion construction

        /// <summary>
        /// Function called on end of creating controlflow graph.
        /// </summary>
        /// <param name="visitor">visitor which created controlflow graph</param>
        private void PostProcess(CFGVisitor visitor)
        {
            visitor.CheckLabels();
            // TODO: simplifying control flow graph currently does not work with worklist segments (see BasicBlock.AfterWorklistSegment)
            // when CFG is simplifyed, basic blocks corresponding to ends of some segments can be removed => modify SimplifyGraph in order 
            // to not remove empty basic blocks if they end worklist segemnts.
            //start.SimplifyGraph();
        }

        /// <summary>
        /// Reads code from file name and return syntax parser.
        /// </summary>
        /// <param name="fileName">file name to read</param>
        /// <returns>Syntax parser object ready to parser given file</returns>
        private static SyntaxParser GenerateParser(string fileName)
        {
            string code;
            using (StreamReader reader = new StreamReader(fileName))
            {
                code = reader.ReadToEnd();
            }

            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            return new SyntaxParser(source_file, code);
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
            detecting all the blocks in controlflow graph
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
            generating text for all nodes
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
                     recursive generating cfg and text representation for function
                     */
                    if (statement.GetType() == typeof(FunctionDecl))
                    {
                        FunctionDecl function = (FunctionDecl)statement;
                        label += "function " + function.Function.Name + Environment.NewLine;

                        try
                        {
                            ControlFlowGraph cfg = new ControlFlowGraph(globalCode, function, File);
                            functionsResult += cfg.generateText((counter / 10000 + 1) * 10000);
                            counter += 10000;
                        }
                        catch (Weverca.ControlFlowGraph.ControlFlowException e)
                        {
                            Console.WriteLine(e.Message);
                            return "";
                        }


                    }
                    /*
                     * recursive generating cfg and text representation for objects
                    */
                    else if (statement.GetType() == typeof(TypeDecl))
                    {
                        TypeDecl clas = (TypeDecl)statement;
                        label += "class " + clas.Name.ToString() + Environment.NewLine;
                        foreach (var method in clas.Members)
                        {
                            if (method.GetType() == typeof(MethodDecl))
                            {
                                try
                                {
                                    ControlFlowGraph cfg = new ControlFlowGraph(globalCode, method as MethodDecl, File);
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
                    else if (statement is ForeachStmt)
                    {
                        var forEach = statement as ForeachStmt;
                        Position foreachHeadPosition = new Position(forEach.Position.FirstLine, forEach.Position.FirstColumn, forEach.Position.FirstOffset,
                            forEach.Body.Position.FirstLine, forEach.Body.Position.FirstColumn, forEach.Body.Position.FirstOffset-1);
                        label += globalCode.SourceUnit.GetSourceCode(foreachHeadPosition) + Environment.NewLine;
                    }
                    else
                    {
                        label += globalCode.SourceUnit.GetSourceCode(statement.Position) + Environment.NewLine;
                    }
                }
                if (node.EndIngTryBlocks.Count > 0)
                {
                    label += "//ending Try block";

                }
                label = label.Replace("\"", "\\\"");
                result += "node" + i + "[label=\"" + label + "\"]" + Environment.NewLine;
                i++;
            }
            /*
            drawing edges
            */
            i = oldCounter;
            foreach (var node in nodes)
            {
                foreach (var e in node.OutgoingEdges)
                {
                    int index = oldCounter + nodes.IndexOf(e.To);
                    if (e is ConditionalEdge)
                    {
                        ConditionalEdge edge = e as ConditionalEdge;
                        string label = "";
                        //in case a condition is not from original ast and hes been aded in constructiion of cfg, we need to write it in different way
                        if (!edge.Condition.Position.IsValid || this.cfgAddedElements.Contains(edge.Condition))
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
                    else if(e is ForEachSpecialEdge)
                    {
                        result += "node" + i + " -> node" + index + "[headport=n, tailport=s,label=\"foreach special edge\"]" + Environment.NewLine;
                    }
                    else
                    {
                        result += "node" + i + " -> node" + index + "[headport=n, tailport=s,label=\"direct edge\"]" + Environment.NewLine;
                    }
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
                if (node.EndIngTryBlocks.Count > 0)
                { 
                    foreach(var tryblock in node.EndIngTryBlocks)
                    {
                        foreach(var catchblock in tryblock.catchBlocks)
                        {
                            int index = oldCounter + nodes.IndexOf(catchblock);
                            result += "node" + i + " -> node" + index + "[headport=n, tailport=s,label=\" catched " + catchblock.ClassName.QualifiedName.Name.Value+ "  \"]" + Environment.NewLine;
                        }
                    }
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