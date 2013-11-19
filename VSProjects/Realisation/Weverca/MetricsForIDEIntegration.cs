using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Parsers;
using Weverca.CodeMetrics;
using PHP.Core;
using PHP.Core.AST;

namespace Weverca
{
    /// <summary>
    /// Computation of code metrics for IDE integration
    /// </summary>
    class MetricsForIDEIntegration
    {
        public static readonly string PHP_FILE_EXTENSION = ".php";

        /// <summary>
        /// Runs computation of code metrics for IDE integration (option -cmide of the analyzer)
        /// Do not modify the format of the output. For another format of output use another option of the analyzer.
        /// </summary>
        internal static void Run(string metricsType, string analyzedFile, string[] metricsArgs)
        {
            //The first argument determines an action
            if (metricsType == "-quantity")
            {
                if (Directory.Exists(analyzedFile))
                {
                    ProcessDirectory(analyzedFile);
                }
                else if (File.Exists(analyzedFile))
                {
                    string fileExtension = Path.GetExtension(analyzedFile);
                    if (fileExtension == PHP_FILE_EXTENSION)
                    {
                        ProcessFile(analyzedFile);
                    }
                }
            }
            else if (metricsType == "-constructs" && metricsArgs.Length > 0)
            {
                if (Directory.Exists(analyzedFile))
                {
                    ProcessDirectory(analyzedFile, metricsArgs);
                }
                else if (File.Exists(analyzedFile))
                {
                    string fileExtension = Path.GetExtension(analyzedFile);
                    if (fileExtension == PHP_FILE_EXTENSION)
                    {
                        ProcessFile(analyzedFile, metricsArgs);
                    }
                }
            }
        }

        /// <summary>
        /// For each PHP sources in the given directory calls ProcessFile method.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        private static void ProcessDirectory(string directoryName)
        {
            Console.WriteLine("Processing directory: {0}", directoryName);
            System.Diagnostics.Debug.Assert(Directory.Exists(directoryName));

            foreach (string fileName in Directory.EnumerateFiles(directoryName, "*.php", SearchOption.TopDirectoryOnly))
            {
                string fileExtension = Path.GetExtension(fileName);
                if (fileExtension == PHP_FILE_EXTENSION)
                {
                    ProcessFile(fileName);
                }
            }
        }
        private static void ProcessDirectory(string directoryName, string[] constructs)
        {
            Console.WriteLine("Processing directory: {0}", directoryName);
            System.Diagnostics.Debug.Assert(Directory.Exists(directoryName));

            foreach (string fileName in Directory.EnumerateFiles(directoryName, "*.php", SearchOption.AllDirectories))
            {

                string fileExtension = Path.GetExtension(fileName);
                if (fileExtension == PHP_FILE_EXTENSION)
                {
                    ProcessFile(fileName, constructs);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private static void ProcessFile(string fileName)
        {
            Console.WriteLine("Process file: {0}", fileName);

            System.Diagnostics.Debug.Assert(File.Exists(fileName));

            Console.WriteLine("Processing file: {0}", fileName);
            SyntaxParser parser = GenerateParser(fileName);
            parser.Parse();
            if (parser.Errors.AnyError) return;
            MetricInfo info = MetricInfo.FromParsers(true, parser);
            Console.WriteLine(fileName + "," + info.GetQuantity(Quantity.NumberOfLines) + "," + info.GetQuantity(Quantity.NumberOfSources) + "," +
            info.GetQuantity(Quantity.MaxInheritanceDepth) + "," + info.GetQuantity(Quantity.MaxMethodOverridingDepth) + "," +
                //info.GetRating(Rating.Cyclomacity + "," +  NOT IMPLEMENTED
            info.GetRating(Rating.ClassCoupling) + "," + info.GetRating(Rating.PhpFunctionsCoupling));
        }

        private static void ProcessFile(string fileName, string[] constructs)
        {
            Console.WriteLine("Process file: {0}", fileName);

            System.Diagnostics.Debug.Assert(File.Exists(fileName));

            Console.WriteLine("Processing file: {0}", fileName);
            SyntaxParser parser = GenerateParser(fileName);
            parser.Parse();
            if (parser.Errors.AnyError)
            {
                Console.WriteLine("error");
                return;
            }
            MetricInfo info = MetricInfo.FromParsers(true, parser);

            foreach (string construct in constructs)
            {
                if (info.HasIndicator(StringToIndicator(construct)))
                    foreach (AstNode node in info.GetOccurances(StringToIndicator(construct)))
                    {
                        Console.WriteLine("File *" + fileName + "* contains indicator *" + construct + "*" + ((LangElement)node).Position.FirstLine.ToString() + "," + ((LangElement)node).Position.FirstOffset.ToString() +
                                          "," + ((LangElement)node).Position.LastLine.ToString() + "," + ((LangElement)node).Position.LastOffset.ToString());
                    }
            }




        }

        private static ConstructIndicator StringToIndicator(string construct)
        {
            // TODO pouzit Dictionary - nie je kompletny

            if (construct == "Aliasing")
            { return ConstructIndicator.Alias; }
            if (construct == "Autoload")
            { return ConstructIndicator.Autoload; }
            if (construct == "Class presence")
            { return ConstructIndicator.Class; }
            if (construct == "Dynamic call")
            { return ConstructIndicator.DynamicCall; }
            if (construct == "Dynamic dereference")
            { return ConstructIndicator.DynamicDereference; }
            if (construct == "Dynamic include")
            { return ConstructIndicator.DynamicInclude; }
            if (construct == "Eval")
            { return ConstructIndicator.Eval; }
            if (construct == "Inside function declaration")
            { return ConstructIndicator.InsideFunctionDeclaration; }
            if (construct == "Magic methods")
            { return ConstructIndicator.MagicMethod; }
            if (construct == "SQL")
            { return ConstructIndicator.MySQL; }
            if (construct == "Passing by reference at call side")
            { return ConstructIndicator.PassingByReferenceAtCallSide; }
            if (construct == "Sessions")
            { return ConstructIndicator.Session; }
            else //(construct == "Use of super global variable")
            { return ConstructIndicator.SuperGlobalVariable; }
            // return null; - non-nullable         
        }



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
    }
}
