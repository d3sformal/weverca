using System;
using System.IO;
using System.Text;
using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;
using Weverca.Parsers;

namespace Weverca.Parser.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "./file.php";          
            PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)), new FullPath(fileName));
            String code = @"<?php
            l1:
         
            include 'a.php';
           
            class A{}
            function a(){
                return $p;
            }
            function a($v){
                return $p;
            }
            $a=4;
            if($a==4)
                echo $a;
            echo $b;
            f($a);
            if(f($a))   
                goto l1;
                
            echo true;
            $p=new array();
            
            ?>";
            var parser = new SyntaxParser(source_file, code);
            parser.Parse();
            foreach (Statement statement in parser.Ast.Statements)
            {
                Console.WriteLine(statement);
            }
           
        }
    }
}
