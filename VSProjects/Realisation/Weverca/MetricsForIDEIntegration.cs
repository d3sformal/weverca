using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.CodeMetrics;
using Weverca.Parsers;
using Weverca.Output;
using Weverca.Analysis;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.ProgramPoints;
using Weverca.Taint;

namespace Weverca
{
    /// <summary>
    /// Computation of code metrics for IDE integration.
    /// </summary>
    internal class MetricsForIDEIntegration
    {
        public static readonly string PHP_FILE_EXTENSION = ".php";

        /// <summary>
        /// Runs computation of code metrics for IDE integration (option -cmide of the analyzer)
        /// Do not modify the format of the output. For another format of output use another option of the analyzer.
        /// </summary>
        /// <param name="metricsType"></param>
        /// <param name="analyzedFile"></param>
        /// <param name="otherArgs"></param>
        internal static void Run(string metricsType, string analyzedFile, string[] otherArgs)
        {
            // The first argument determines an action
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
            else if (metricsType == "-constructs" && otherArgs.Length > 0)
            {
                if (Directory.Exists(analyzedFile))
                {
                    ProcessDirectory(analyzedFile, otherArgs);
                }
                else if (File.Exists(analyzedFile))
                {
                    string fileExtension = Path.GetExtension(analyzedFile);
                    if (fileExtension == PHP_FILE_EXTENSION)
                    {
                        ProcessFile(analyzedFile, otherArgs);
                    }
                }
            }
            else if (metricsType == "-constructsFromFileOfFiles" && otherArgs.Length > 0)
            {
                var fileToRead = new StreamReader(analyzedFile);
                string line;
                while ((line = fileToRead.ReadLine()) != null)
                {
                    if (Directory.Exists(line))
                    {
                        ProcessDirectory(line, otherArgs);
                    }
                    else if (File.Exists(line))
                    {
                        var fileExtension = Path.GetExtension(line);
                        if (fileExtension == PHP_FILE_EXTENSION)
                        {
                            ProcessFile(line, otherArgs);
                        }
                    }
                }

                fileToRead.Close();
            }
            else if (metricsType == "-staticanalysis")
            {
                List<string> filesToAnalyze = new List<string>();

                string fileExtension = Path.GetExtension(analyzedFile);
                if (fileExtension == PHP_FILE_EXTENSION)
                {
                    filesToAnalyze.Add(analyzedFile);
                }

                for (int i = 0; i < otherArgs.Length; i++)
                {
                    fileExtension = Path.GetExtension(otherArgs[i]);
                    if (fileExtension == PHP_FILE_EXTENSION)
                    {
                        filesToAnalyze.Add(otherArgs[i]);
                    }
                }

                var memoryModel = MemoryModels.MemoryModels.CopyMM;

                RunStaticAnalysis(filesToAnalyze.ToArray(), memoryModel);
            }

        }

        /// <summary>
        /// For each PHP sources in the given directory calls ProcessFile method.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        private static void ProcessDirectory(string directoryName)
        {
            Console.WriteLine("Processing directory: {0}", directoryName);
            Debug.Assert(Directory.Exists(directoryName));

            foreach (var fileName in Directory.EnumerateFiles(directoryName, "*.php",
                SearchOption.AllDirectories))
            {
                var fileExtension = Path.GetExtension(fileName);
                if (fileExtension == PHP_FILE_EXTENSION)
                {
                    ProcessFile(fileName);
                }
            }
        }

        private static void ProcessDirectory(string directoryName, string[] constructs)
        {
            Console.WriteLine("Processing directory: {0}", directoryName);
            Debug.Assert(Directory.Exists(directoryName));

            foreach (string fileName in Directory.EnumerateFiles(directoryName, "*.php",
                SearchOption.AllDirectories))
            {
                var fileExtension = Path.GetExtension(fileName);
                if (fileExtension == PHP_FILE_EXTENSION)
                {
                    ProcessFile(fileName, constructs);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private static void ProcessFile(string fileName)
        {
            Console.WriteLine("Process file: {0}", fileName);

            Debug.Assert(File.Exists(fileName));

            Console.WriteLine("Processing file: {0}", fileName);
            var parser = GenerateParser(fileName);
            parser.Parse();
            if (parser.Errors.AnyError)
            {
                return;
            }

            var info = MetricInfo.FromParsers(true, parser);
            Console.WriteLine(fileName + "," + info.GetQuantity(Quantity.NumberOfLines) + ","
                + info.GetQuantity(Quantity.NumberOfSources) + ","
                + info.GetQuantity(Quantity.MaxInheritanceDepth) + ","
                + info.GetQuantity(Quantity.MaxMethodOverridingDepth) + "," +
                // info.GetRating(Rating.Cyclomacity + "," +  NOT IMPLEMENTED
                info.GetRating(Rating.ClassCoupling) + "," + info.GetRating(Rating.PhpFunctionsCoupling));
        }

        private static void ProcessFile(string fileName, string[] constructs)
        {
            Console.WriteLine("Process file: {0}", fileName);

            Debug.Assert(File.Exists(fileName));

            Console.WriteLine("Processing file: {0}", fileName);
            var parser = GenerateParser(fileName);
            try
            {
                parser.Parse();
            }
            catch
            {
                return;
            }

            if (parser.Errors.AnyError)
            {
                Console.WriteLine("error");
                return;
            }

            var info = MetricInfo.FromParsers(true, parser);
            foreach (var construct in constructs)
            {
                if (info.HasIndicator(StringToIndicator(construct)))
                {
                    foreach (var node in info.GetOccurrences(StringToIndicator(construct)))
                    {
                        Console.WriteLine("File *" + fileName + "* contains indicator *" + construct
                            + "*" + ((LangElement)node).Position.FirstLine.ToString()
                            + "," + ((LangElement)node).Position.FirstOffset.ToString()
                            + "," + ((LangElement)node).Position.LastLine.ToString()
                            + "," + ((LangElement)node).Position.LastOffset.ToString());
                    }
                }
            }
        }

        private static ConstructIndicator StringToIndicator(string construct)
        {
            // TODO pouzit Dictionary - nie je kompletny

            if (construct == "Aliasing")
            { return ConstructIndicator.References; }
            if (construct == "Autoload")
            { return ConstructIndicator.Autoload; }
            if (construct == "Class presence")
            { return ConstructIndicator.ClassOrInterface; }
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
            { return ConstructIndicator.MagicMethods; }
            if (construct == "SQL")
            { return ConstructIndicator.MySql; }
            if (construct == "Passing by reference at call side")
            { return ConstructIndicator.PassingByReferenceAtCallSide; }
            if (construct == "Sessions")
            { return ConstructIndicator.Session; }
            else // (construct == "Use of super global variable")
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

            var sourceFile = new PhpSourceFile(new FullPath(Path.GetDirectoryName(fileName)),
                new FullPath(fileName));
            return new SyntaxParser(sourceFile, code);
        }

        private static void RunStaticAnalysis(string[] filenames, MemoryModels.MemoryModels memoryModel)
        {
            var console = new ConsoleOutput();

            foreach (var argument in filenames)
            {
                var filesInfo = Analyzer.GetFileNames(argument);
                if (filesInfo == null)
                {
                    Console.WriteLine("Path \"{0}\" cannot be recognized", argument);
                    Console.WriteLine(); 
                    continue;
                }

                else if (filesInfo.Length <= 0)
                {
                    Console.WriteLine("Path pattern \"{0}\" does not match any file", argument);
                    Console.WriteLine();
                }

                foreach (var fileInfo in filesInfo)
                {
                    console.CommentLine(string.Format("File path: {0}\n", fileInfo.FullName));

                    try
                    {
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        var ppGraph = Analyzer.Run(fileInfo, memoryModel);
                        watch.Stop();
                        
                        var nextPhase = new TaintForwardAnalysis(ppGraph);
                        nextPhase.Analyse();
                        

                        Console.WriteLine("Analysis warnings:");
                        PrintWarnings(AnalysisWarningHandler.GetWarnings());


                        Console.WriteLine("Security warnings with taint flow:");
                        PrintTaintWarnings(nextPhase.analysisTaintWarnings);

                        Console.WriteLine("Variables:");
                        List<ProgramPointGraph> processedPPGraphs = new List<ProgramPointGraph>();
                        List<ProgramPointBase> processedPPoints = new List<ProgramPointBase>();
                        writeAll(ppGraph, ref processedPPGraphs, ref processedPPoints);

                    }
                    catch (Exception e)
                    {
                        console.Error(e.Message);
                    }
                }
            }
        }

        private static void PrintWarnings(List <AnalysisWarning> warnings)
        {
            
            if (warnings.Count == 0)
            {
                Console.WriteLine("No warnings");
            }
            string file = "/";
            foreach (var s in warnings)
            {
                if (file != s.FullFileName)
                {
                    file = s.FullFileName;
                    Console.WriteLine("File: " + file);
                }

                Console.WriteLine ("Warning at line " + s.LangElement.Position.FirstLine + 
                                    " char " + s.LangElement.Position.FirstColumn + 
                                    " firstoffset " + s.LangElement.Position.FirstOffset +
                                    " lastoffset " + s.LangElement.Position.LastOffset +
                                    " : " + s.Message.ToString());
                Console.WriteLine("Called from: ");
                Console.WriteLine(s.ProgramPoint.OwningPPGraph.Context.ToString());
            }
        }

        private static void PrintSecurityWarnings(List<AnalysisSecurityWarning> warnings)
        {
            if (warnings.Count == 0)
            {
                Console.WriteLine("No warnings");
            }
            string file = "/";
            foreach (var s in warnings)
            {
                if (file != s.FullFileName)
                {
                    file = s.FullFileName;
                    Console.WriteLine("File: " + file);
                }
                Console.WriteLine("Warning at line " + s.LangElement.Position.FirstLine +
                                    " char " + s.LangElement.Position.FirstColumn +
                                    " firstoffset " + s.LangElement.Position.FirstOffset +
                                    " lastoffset " + s.LangElement.Position.LastOffset +
                                    " : " + s.Message.ToString());
                Console.WriteLine("Called from: ");
                Console.WriteLine(s.ProgramPoint.OwningPPGraph.Context.ToString());

            }
        }

        private static void PrintTaintWarnings(List<AnalysisTaintWarning> warnings)
        {
            if (warnings.Count == 0)
            {
                Console.WriteLine("No warnings");
            }
            string file = "/";
            foreach (var s in warnings)
            {
                if (file != s.FullFileName)
                {
                    file = s.FullFileName;
                    Console.WriteLine("File: " + file);
                }
                Console.WriteLine("Warning at line " + s.LangElement.Position.FirstLine +
                                    " char " + s.LangElement.Position.FirstColumn +
                                    " firstoffset " + s.LangElement.Position.FirstOffset +
                                    " lastoffset " + s.LangElement.Position.LastOffset +
                                    " : " + s.Message.ToString());
                Console.WriteLine("Called from: ");
                Console.WriteLine(s.ProgramPoint.OwningPPGraph.Context.ToString());
                Console.WriteLine("Taint Flow: ");
                Console.WriteLine(s.TaintFlow);
            }
        }

        private static void writeAll(ProgramPointGraph graph,ref List<ProgramPointGraph> processedPPGraphs, ref List<ProgramPointBase> processedPPoints)
        {
            processedPPGraphs.Add(graph);
            ProgramPointBase lastPPoint = null;

            foreach (ProgramPointBase p in graph.Points)
            {
                foreach (ProgramPointBase processedPoint in processedPPoints)
                {
                    if (processedPoint == p) continue;
                }
                processedPPoints.Add(p);
                if (p.Partial == null) continue;
                if ( lastPPoint != null &&
                    lastPPoint.Partial.Position.FirstLine== p.Partial.Position.FirstLine && 
                    lastPPoint.Partial.Position.LastLine == p.Partial.Position.LastLine) //only first and last program point from one line is shown
                {
                    lastPPoint = p;
                }
                else
                {
                    if (lastPPoint != null) // show the last program point
                    {
                        writeProgramPointInformation(lastPPoint, true);
                    }
                    writeProgramPointInformation(p, false);
                    
                    lastPPoint = p;
                }
                // for each program poind resolve extensions
                FlowExtension ext = p.Extension;
                foreach (ExtensionPoint extPoint in ext.Branches)
                {
                    writeExtension(extPoint, ref processedPPGraphs, ref processedPPoints);
                } 
            }
            writeProgramPointInformation(lastPPoint, true);
        }

        private static void writeProgramPointInformation(ProgramPointBase p, bool outset)
        {
            if (p is FunctionDeclPoint) return;
            Console.Write("Point position: ");
            Console.WriteLine("First line: " + p.Partial.Position.FirstLine +
                                " Last line: " + p.Partial.Position.LastLine +
                                " First offset: " + p.Partial.Position.FirstOffset +
                                " Last offset: " + p.Partial.Position.LastOffset);
            if (p.OwningPPGraph.OwningScript != null) Console.WriteLine("OwningScript: " + p.OwningPPGraph.OwningScript.FullName);
            String callStack = p.OwningPPGraph.Context.ToString();
            if (callStack != "")
            {
                Console.WriteLine("Called from: ");
                Console.WriteLine(p.OwningPPGraph.Context.ToString());
            }
            Console.WriteLine("Point information:");

            if (outset)
            {
                if (p.OutSet != null) Console.WriteLine(p.OutSet.Representation);
                else Console.WriteLine("Dead code");
            }
            if (!outset)
            {
                if (p.InSet != null) Console.WriteLine(p.InSet.Representation);
                else Console.WriteLine("Dead code");
            }
        }

        private static void writeExtension(ExtensionPoint point, ref List<ProgramPointGraph> processedPPGraphs, ref List<ProgramPointBase> processedPPoints)
        {
            ProgramPointGraph graph = point.OwningPPGraph;
            foreach (ProgramPointGraph processedGraph in processedPPGraphs)
            {
                if (graph == processedGraph) return;
            }
            Console.WriteLine("Extension");
            writeAll(graph,ref processedPPGraphs, ref processedPPoints);
            Console.WriteLine("End extension");
        }
 
    }

}
