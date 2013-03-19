using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PhpRefactoring.Utils;
using PHP.Core.AST;
using System.IO;
using System.CodeDom.Compiler;
using System.Diagnostics;
using PHP.ControlFlow;
using PHP.Core.ControlFlow;
using PHP.Tests.ControlFlow;

namespace ASTViewer
{
    public class ASTPrinter : TreeVisitor
    {
        private IndentedTextWriter textWriter;

        public ASTPrinter(TextWriter textWriter)
        {            
            this.textWriter = new IndentedTextWriter(textWriter);
        }

        public override void VisitElement(LangElement element)
        {
            this.textWriter.Indent++;
            this.textWriter.WriteLine(element.GetType().Name);
            base.VisitElement(element);
            this.textWriter.Indent--;
        }

        public override void VisitGlobalCode(GlobalCode x)
        {
            this.textWriter.Indent++;
            this.textWriter.WriteLine(x.GetType().Name);
            base.VisitGlobalCode(x);
            this.textWriter.Indent--;
        }

        public override void VisitDirectVarUse(DirectVarUse x)
        {
            this.textWriter.Indent++;
            this.textWriter.WriteLine("name={0}, value={1}", x.VarName, x.Value);
            base.VisitDirectVarUse(x);
            this.textWriter.Indent--;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new WorklistRunnerTests().TestPropagationThroughIfThen();

            ICodeComments codeComments;
            var result = PhpRefactoring.Utils.CustomCompilationUnit.ParseCode("./file.php",
                @"<?php       

                    /*switch ($x)
                    {
                        case 0: echo $x;
                            break;
                        case 1: echo 'ahoj';
                            break;
                        default: echo 'default';
                    }*/

                    while ($i < 10) {
                        $i++;
                        if ($i == 3) {
                            continue;
                        }
                        $j++;
                    }

                    $i = 4;
                    label:
                        $i++;
                        if ($i < 10) goto label;
                    echo $i;

                    if ($x == 3) echo $x;
                    else if ($x == 2) echo 2;
                    else echo 3;

                    $b = is_number($i);
                    $x = 3;
                    $y = $x * 3;

                    start:                    
                    for ($i = 0; $i < 100; ++$i) {
                        echo $x;
                        if ($x == 1) echo $x;
                        else if ($x == $i) echo 'Ahoj';
                        else goto start;
                    }

                    while ($x < $y)
                        $x++;
                ?>", 
                out codeComments);

            /* ASTViewer.exe > g.dot
             * dot.exe -Tpng g.dot > g.png
             * 
             * dot.exe is part of GraphViz */
            var graph = ControlFlowGraph.Construct(result);
            Console.WriteLine(graph.GetDotGraph());

            // Prints information about the AST
            // result.VisitMe(new ASTPrinter(Console.Out));

            // Does not work yet:
            // new ForwardFlowAnalysisRunner().Run(new UninitializedUseAnalysis(), graph);
        }
    }
}
