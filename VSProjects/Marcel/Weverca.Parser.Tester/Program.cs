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
            ErrorSink errors = new ErrorSinkImpl();
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
            CompilationUnit compilationUnit = new CompilationUnit();
            var sourceUnit = new PHP.Core.Reflection.VirtualSourceFileUnit(compilationUnit, code, source_file, Encoding.Default);
            sourceUnit.Parse(errors, compilationUnit, Position.Initial, LanguageFeatures.Php5);

            foreach (Statement statement in sourceUnit.Ast.Statements)
            {
                Console.WriteLine(statement);
            }

        }
    }
}
