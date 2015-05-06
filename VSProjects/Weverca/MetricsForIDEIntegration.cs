    /*
Copyright (c) 2012-2014 Natalia Tyrpakova and David Hauzar

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
        // Indicates whether second phase is enabled
        public static readonly bool SECOND_PHASE = true;
        public static readonly string PHP_FILE_EXTENSION = ".php";

        private static StreamWriter fileOutput;

        /// <summary>
        /// Runs computation of code metrics for IDE integration (option -cmide of the analyzer)
        /// Do not modify the format of the output. For another format of output use another option of the analyzer.
        /// </summary>
        /// <param name="metricsType"></param>
        /// <param name="analyzedFile"></param>
        /// <param name="otherArgs"></param>
        internal static void Run(string metricsType, string analyzedFile, string outputFile, string[] otherArgs)
        {
            File.Delete (outputFile);
            fileOutput = new StreamWriter(outputFile);

            // The first argument determines an action
            if (metricsType == "-quantity")
            {
                List<String> nonProcessedFiles = new List<String>();
                if (Directory.Exists(analyzedFile))
                {
                    ProcessDirectory(analyzedFile, ref nonProcessedFiles);
                }
                else if (File.Exists(analyzedFile))
                {
                    string fileExtension = Path.GetExtension(analyzedFile);
                    if (fileExtension == PHP_FILE_EXTENSION)
                    {
                        ProcessFile(analyzedFile, ref nonProcessedFiles);
                    }
                }
                if (nonProcessedFiles.Count > 0)
                {
                    foreach (String file in nonProcessedFiles)
                    {
                        fileOutput.WriteLine("Not processed: " + file);
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

                var memoryModel = MemoryModels.MemoryModels.ModularCopyMM;

                RunStaticAnalysis(filesToAnalyze.ToArray(), memoryModel);
            }

            fileOutput.Close ();

        }

       
        /// <summary>
        /// For each PHP sources in the given directory calls ProcessFile method.
        /// </summary>
        /// <param name="directoryName"></param>
        /// <param name="nonProcessedFiles"></param>
        private static void ProcessDirectory(string directoryName, ref List<String> nonProcessedFiles)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            fileOutput.WriteLine("Processing directory: {0}", directoryName);
            Debug.Assert(Directory.Exists(directoryName));

            foreach (var fileName in Directory.EnumerateFiles(directoryName, "*.php",
                SearchOption.AllDirectories))
            {
                List<String> nonProcessed = new List<String>();
                var fileExtension = Path.GetExtension(fileName);
                if (fileExtension == PHP_FILE_EXTENSION)
                {
                    ProcessFile(fileName, ref nonProcessed);
                    nonProcessedFiles.AddRange(nonProcessed);
                }
            }
            watch.Stop();
            fileOutput.WriteLine("Time: {0}", watch.ElapsedMilliseconds);
        }

        private static void ProcessDirectory(string directoryName, string[] constructs)
        {
            fileOutput.WriteLine("Processing directory: {0}", directoryName);
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
        /// Processes a single file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="nonProcessedFiles"></param>
        private static void ProcessFile(string fileName, ref List<String> nonProcessedFiles)
        {
            fileOutput.WriteLine("Process file: {0}", fileName);

            Debug.Assert(File.Exists(fileName));

            fileOutput.WriteLine("Processing file: {0}", fileName);
            var parser = GenerateParser(fileName);
            try
            {
                parser.Parse();
                if (parser.Errors.AnyError)
                {
                    nonProcessedFiles.Add(fileName);
                    return;
                }

                var info = MetricInfo.FromParsers(true, parser);
                fileOutput.WriteLine(fileName + ";" + info.GetQuantity(Quantity.NumberOfLines) + ";"
                    + info.GetQuantity(Quantity.NumberOfSources) + ";"
                    + info.GetQuantity(Quantity.MaxInheritanceDepth) + ";"
                    + info.GetQuantity(Quantity.MaxMethodOverridingDepth) + ";" +
                    // info.GetRating(Rating.Cyclomacity + "," +  NOT IMPLEMENTED
                    info.GetRating(Rating.ClassCoupling) + ";" + info.GetRating(Rating.PhpFunctionsCoupling));
            }
            catch
            {
                nonProcessedFiles.Add(fileName);
                return;
            }
        
        }

        private static void ProcessFile(string fileName, string[] constructs)
        {
            fileOutput.WriteLine("Process file: {0}", fileName);

            Debug.Assert(File.Exists(fileName));

            fileOutput.WriteLine("Processing file: {0}", fileName);
            var parser = GenerateParser(fileName);
            try
            {
                parser.Parse();
                if (parser.Errors.AnyError)
                {
                    fileOutput.WriteLine("error");
                    return;
                }

                var info = MetricInfo.FromParsers(true, parser);
                foreach (var construct in constructs)
                {
                    if (info.HasIndicator(StringToIndicator(construct)))
                    {
                        foreach (var node in info.GetOccurrences(StringToIndicator(construct)))
                        {
                            fileOutput.WriteLine("File *" + fileName + "* contains indicator *" + construct
                                + "*" + ((LangElement)node).Position.FirstLine.ToString()
                                + "," + ((LangElement)node).Position.FirstOffset.ToString()
                                + "," + ((LangElement)node).Position.LastLine.ToString()
                                + "," + ((LangElement)node).Position.LastOffset.ToString());
                        }
                    }
                }
            }
            catch
            {
                return;
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
            var bigWatch = System.Diagnostics.Stopwatch.StartNew();
            var console = new ConsoleOutput();

            foreach (var argument in filenames)
            {
                var filesInfo = Analyzer.GetFileNames(argument);
                if (filesInfo == null)
                {
                    fileOutput.WriteLine("Path \"{0}\" cannot be recognized", argument);
                    fileOutput.WriteLine(); 
                    continue;
                }

                else if (filesInfo.Length <= 0)
                {
                    fileOutput.WriteLine("Path pattern \"{0}\" does not match any file", argument);
                    fileOutput.WriteLine();
                }

                foreach (var fileInfo in filesInfo)
                {
                    console.CommentLine(string.Format("File path: {0}\n", fileInfo.FullName));

                    //try
                    {
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        var ppGraph = Analyzer.Run(fileInfo, memoryModel);
                        watch.Stop();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        var watch2 = System.Diagnostics.Stopwatch.StartNew();
						var nextPhase = new TaintForwardAnalysis(ppGraph);
						nextPhase.analysisTaintWarnings = new List<AnalysisTaintWarning>();
						if (SECOND_PHASE) 
						{
							nextPhase.Analyse();
						}
                        watch2.Stop();


                        fileOutput.WriteLine("Analysis warnings:");
						var firstPhaseWarnings = AnalysisWarningHandler.GetWarnings();
						//PrintWarnings(new List<AnalysisWarning>());
						PrintWarnings(firstPhaseWarnings);
						var firstPhaseSecurityWarnings = new List<AnalysisSecurityWarning>();
						if (!SECOND_PHASE) {
							firstPhaseSecurityWarnings = AnalysisWarningHandler.GetSecurityWarnings();
							PrintSecurityWarnings(firstPhaseSecurityWarnings);
						}


						//fileOutput.WriteLine("Security warnings with taint flow:");
						if (SECOND_PHASE)
							PrintTaintWarnings(nextPhase.analysisTaintWarnings);

                        fileOutput.WriteLine("Variables:");
                        var processedPPGraphs = new HashSet<ProgramPointGraph>();
                        var processedPPoints = new HashSet<ProgramPointBase>();
                        writeAll(ppGraph, ref processedPPGraphs, ref processedPPoints);

                        bigWatch.Stop();

                        fileOutput.WriteLine("Overview:");

                        fileOutput.WriteLine("Total number of warnings: " + (firstPhaseWarnings.Count + firstPhaseSecurityWarnings.Count + nextPhase.analysisTaintWarnings.Count));
                        fileOutput.WriteLine("Number of warnings in the first phase: " + firstPhaseWarnings.Count);
                        fileOutput.WriteLine("Number of warnings in the second phase: " + nextPhase.analysisTaintWarnings.Count);
                        fileOutput.WriteLine("Weverca analyzer time consumption: " + bigWatch.ElapsedMilliseconds);
                        fileOutput.WriteLine("First phase time consumption: " + watch.ElapsedMilliseconds);
                        fileOutput.WriteLine("Second phase time consumption: " + watch2.ElapsedMilliseconds);
                        var programLines = new Dictionary<string, HashSet<int>> ();
                        fileOutput.WriteLine("The number of nodes in the application is: " + Program.numProgramPoints(new HashSet<ProgramPointGraph>(), programLines, ppGraph));
                        var programLinesNum = 0;
                        foreach (var script in programLines.Values) {
                            programLinesNum += script.Count;
                        }
                        fileOutput.WriteLine("The number of processed lines of code is: " + programLinesNum);
                    }
                    /*
                    catch (Exception e)
                    {
                        fileOutput.WriteLine("error");
                        //console.Error(e.Message);
                        fileOutput.WriteLine (e.Message);
                    }*/
                }
            }
        }

        private static void PrintWarnings(List <AnalysisWarning> warnings)
        {
            
            if (warnings.Count == 0)
            {
                fileOutput.WriteLine("No warnings");
            }
            string file = "/";
            foreach (var s in warnings)
            {
                if (file != s.FullFileName)
                {
                    file = s.FullFileName;
                    fileOutput.WriteLine("File: " + file);
                }

                fileOutput.WriteLine ("Warning at line " + s.LangElement.Position.FirstLine + 
                                    " char " + s.LangElement.Position.FirstColumn + 
                                    " firstoffset " + s.LangElement.Position.FirstOffset +
                                    " lastoffset " + s.LangElement.Position.LastOffset +
                                    " : " + s.Message.ToString());
                fileOutput.WriteLine("Called from: ");
                fileOutput.WriteLine(s.ProgramPoint.OwningPPGraph.Context.ToString());
            }
        }

        private static void PrintSecurityWarnings(List<AnalysisSecurityWarning> warnings)
        {
            if (warnings.Count == 0)
            {
                fileOutput.WriteLine("No warnings");
            }
            string file = "/";
            foreach (var s in warnings)
            {
                if (file != s.FullFileName)
                {
                    file = s.FullFileName;
                    fileOutput.WriteLine("File: " + file);
                }
                fileOutput.WriteLine("Warning at line " + s.LangElement.Position.FirstLine +
                                    " char " + s.LangElement.Position.FirstColumn +
                                    " firstoffset " + s.LangElement.Position.FirstOffset +
                                    " lastoffset " + s.LangElement.Position.LastOffset +
                                    ": " + s.Message.ToString());
                fileOutput.WriteLine("Called from: ");
                fileOutput.WriteLine(s.ProgramPoint.OwningPPGraph.Context.ToString());

            }
        }

        private static void PrintTaintWarnings(List<AnalysisTaintWarning> warnings)
        {
            if (warnings.Count == 0)
            {
                fileOutput.WriteLine("No warnings");
            }
            string file = "/";
            foreach (var s in warnings)
            {
                if (file != s.FullFileName)
                {
                    file = s.FullFileName;
                    fileOutput.WriteLine("File: " + file);
                }
                fileOutput.WriteLine("Warning at line " + s.LangElement.Position.FirstLine +
                                    " char " + s.LangElement.Position.FirstColumn +
                                    " firstoffset " + s.LangElement.Position.FirstOffset +
                                    " lastoffset " + s.LangElement.Position.LastOffset +
                                    " : " + s.Message.ToString());
                fileOutput.WriteLine("Called from: ");
                fileOutput.WriteLine(s.ProgramPoint.OwningPPGraph.Context.ToString());
                if (s.HighPriority) fileOutput.WriteLine("High priority");
                fileOutput.WriteLine("Taint Flow: ");
                fileOutput.WriteLine(s.TaintFlow);
            }
        }

        private static void writeAll(ProgramPointGraph graph,ref HashSet<ProgramPointGraph> processedPPGraphs, ref HashSet<ProgramPointBase> processedPPoints)
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
                if (p.Partial == null || !p.Partial.Position.IsValid) continue;
                //only first and last program point from one line is shown
                if ( lastPPoint != null &&
                    lastPPoint.Partial.Position.FirstLine== p.Partial.Position.FirstLine) 
                {
                    lastPPoint = p;
                }
                else
                {
                    // For efficiency reasons, information about instate of program points are now not printed
                    if (lastPPoint != null) // show the last program point
                    {
                        //writeProgramPointInformation(lastPPoint, true);
                    }
                    //writeProgramPointInformation(p, false);
                    writeProgramPointInformation(p, true);
                    
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
            if (p == null || p.Partial == null) return;
            if (p is FunctionDeclPoint) return;
            fileOutput.Write("Point position: ");
            fileOutput.WriteLine("First line: " + p.Partial.Position.FirstLine +
                                " Last line: " + p.Partial.Position.LastLine +
                                " First offset: " + p.Partial.Position.FirstOffset +
                                " Last offset: " + p.Partial.Position.LastOffset);
            if (p.OwningPPGraph.OwningScript != null) fileOutput.WriteLine("OwningScript: " + p.OwningScriptFullName);
            String callStack = p.OwningPPGraph.Context.ToString();
            if (callStack != "")
            {
                fileOutput.WriteLine("Called from: ");
                fileOutput.WriteLine(p.OwningPPGraph.Context.ToString());
            }

            fileOutput.WriteLine("Point information:");

            fileOutput.Write("Fixpoint iterations=");
            fileOutput.WriteLine(p.FixpointIterationsCount);

            if (outset)
            {
                if (p.OutSet != null) fileOutput.WriteLine(p.OutSet.Representation);
                else fileOutput.WriteLine("Dead code");
            }
            if (!outset)
            {
                if (p.InSet != null) fileOutput.WriteLine(p.InSet.Representation);
                else fileOutput.WriteLine("Dead code");
            }
        }

        private static void writeExtension(ExtensionPoint point, ref HashSet<ProgramPointGraph> processedPPGraphs, ref HashSet<ProgramPointBase> processedPPoints)
        {
            ProgramPointGraph graph = point.Graph;
            foreach (ProgramPointGraph processedGraph in processedPPGraphs)
            {
                if (graph == processedGraph) return;
            }
            fileOutput.WriteLine("Extension");
            writeAll(graph,ref processedPPGraphs, ref processedPPoints);
            fileOutput.WriteLine("End extension");
        }
 
    }

}