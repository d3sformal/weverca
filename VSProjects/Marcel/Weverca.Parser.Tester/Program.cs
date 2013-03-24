using System;
using System.IO;
using System.Text;
using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;
using Weverca.Parsers;
using Weverca.ControlFlowGraph;

namespace Weverca.Parser.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "./file.php";          
            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            String code = @"<?php
            $a=4;
            switch($a){
                case 2:$a++;
                case 3:$p=4;do{
                $l++;
                }while(true);
                $p=4;

               // default: $b++;
                case 4: $a++;
            }
            
            ?>";
            var parser = new SyntaxParser(source_file, code);
            parser.Parse();
          
            foreach (Statement statement in parser.Ast.Statements)
            {
                Console.WriteLine(statement);
            }
            Weverca.ControlFlowGraph.ControlFlowGraph cfg = new Weverca.ControlFlowGraph.ControlFlowGraph(parser.Ast);
            Console.WriteLine(cfg.getTextRepresentation());

            /*using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\maki\Desktop\Weverca\graph.txt"))
            {
                file.WriteLine(cfg.getTextRepresentation());


            }
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = @"C:\Users\maki\Desktop\Weverca\generategraph.bat";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;

            proc.Start();
                  
            proc.WaitForExit();*/
            
        }
    }
}
