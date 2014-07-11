#define TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis;
using Weverca.Output;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.CodeMetrics;
using Weverca.Parsers;
using Weverca.Taint;
using Weverca.AnalysisFramework.ProgramPoints;
using System.IO;
using PHP.Core;

namespace Weverca
{

    /// <summary>
    /// The program class including <see cref="Main"/> entry point and parsing of command-line arguments
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Relative path to the trunk folder in SVN
        /// MUST BE CHANGED IN CASE OF CHANDES IN SVN DIRECTORY STRUCTURE
        /// </summary>
        public static readonly string TRUNK_PATH = @"..\..\..\..\..\";

        /// <summary>
        /// Directory with PHP sources
        /// </summary>
        public static readonly string PHP_SOURCES_DIR = TRUNK_PATH + @"PHP_sources\";

        /// <summary>
        /// Startup method for Weverca
        /// </summary>
        /// <param name="args">arguments: -sa [-mm CopyMM|VrMM] FILENAME [FILENAME] or -metrics FILENAME [FILENAME] ...</param>
        private static void Main(string[] args)
        {
            
            if (args.Length < 2)
            {
                Console.WriteLine("Missing argument");
                Console.WriteLine(@"Example of usage: weverca.exe -options ..\..\..\..\..\PHP_sources\test_programs\testfile.php");
                Console.WriteLine(@"-sa [-mm CopyMM|VrMM] FILENAME [FILENAME] ...");
                Console.WriteLine(@"-metrics FILENAME [FILENAME] ...");
                
                /*Console.WriteLine(@"  Static analysis");
                Console.WriteLine(@"-cmide [-options_cmide]");
                Console.WriteLine(@"  Code metrics for IDE integration");
                Console.WriteLine(@"  -cmide -constructs list_of_constructs_separated_by_space");
                Console.WriteLine(@"    Constructs search");
                Console.WriteLine(@"  -cmide -quantity");
                Console.WriteLine(@"    Quantity and rating code metrics computation");*/
                Console.ReadKey();
                return;
            }

            switch (args[0])
            {
                case "-sa":
                    var filesIndex = 1;
                    var memoryModel = MemoryModels.MemoryModels.VirtualReferenceMM;
                   
                    if (args.Length > filesIndex+1 && args[filesIndex] == "-mm")
                    {
                        if (args[filesIndex + 1].ToLower() == "copymm") memoryModel = MemoryModels.MemoryModels.CopyMM;
                        else if (args[filesIndex + 1].ToLower() == "modularcopymm") memoryModel = MemoryModels.MemoryModels.ModularCopyMM;
						if (args[filesIndex+1].ToLower() == "modularmm") memoryModel = MemoryModels.MemoryModels.ModularCopyMM;
                        filesIndex += 2;
                    }

                    bool benchmark = false;
                    string benchmarkFile = "";
                    if (args.Length > filesIndex + 1 && args[filesIndex] == "-b")
                    {
                        benchmark = true;
                        benchmarkFile = args[filesIndex + 1];
                        filesIndex += 2;
                    }

                    if (args.Length <= filesIndex)
                    {
                        Console.WriteLine("file name missing");
                        break;
                    }
                    var analysisFiles = new string[args.Length - filesIndex];
                    Array.Copy(args, filesIndex, analysisFiles, 0, args.Length - filesIndex);
                    RunStaticAnalysis(analysisFiles, memoryModel);

                    if (benchmark)
                    {
                        showBenchmarkResult(memoryModel, benchmarkFile);
                    }

                    break;
                case "-cmide":
                    var metricsArgs = new string[args.Length - 3];
                    Array.Copy(args, 2, metricsArgs, 0, args.Length - 3);
                    MetricsForIDEIntegration.Run(args[1], args[args.Length - 1], metricsArgs);
                    break;
                case "-metrics":
                    var metricFileNames=new string[args.Length-1];
                    Array.Copy(args, 1, metricFileNames, 0, args.Length - 1);
                    RunMetrics(metricFileNames);
                    break;
                default:
                    Console.WriteLine("Unknown option: \"{0}\"", args[0]);
                    break;
            }
        }

        private static void showBenchmarkResult(MemoryModels.MemoryModels memoryModel, string benchmarkFile)
        {
            if (memoryModel == MemoryModels.MemoryModels.ModularCopyMM)
            {
                MemoryModels.ModularCopyMemoryModel.Snapshot.Benchmark.WriteResultsToFile(benchmarkFile);
            }
        }

        /// <summary>
        /// Execute the static analysis and print results
        /// </summary>
        /// <param name="filenames">List of file name patterns from command line</param>
        /// <param name="memoryModel">The memory model used for analysis</param>
        private static void RunStaticAnalysis(string[] filenames, MemoryModels.MemoryModels memoryModel)
        {
            var console = new ConsoleOutput();
            console.CommentLine("Using " + memoryModel.ToString());
            console.CommentLine("");
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
                    continue;
                }

                foreach (var fileInfo in filesInfo)
                {
                    // This is for time consumption analyzing only
                    // Analyze twice - because of omitting .NET initialization we get better analysis time
                    //Analyzer.Run(fileInfo, memoryModel);
                   
#if TEST
                    // Process analysis
					// First phase
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var ppGraph = Analyzer.Run(fileInfo, memoryModel);
                    watch.Stop();

					// Second phase
					var watch2 = System.Diagnostics.Stopwatch.StartNew();
					var nextPhase = new TaintForwardAnalysis(ppGraph);
					nextPhase.Analyse();
					watch2.Stop();

                    // Build output

                    console.CommentLine(string.Format("File path: {0}\n", fileInfo.FullName));
                    
					var graphWalker = new GraphWalking.CallGraphPrinter(ppGraph);
                    console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));

					//graphWalker.Run(console);

					console.Warnings(AnalysisWarningHandler.GetWarnings(), AnalysisWarningHandler.GetSecurityWarnings());

                    console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));
                    console.CommentLine(string.Format("The number of nodes in the application is: {0}\n", numProgramPoints(new HashSet<ProgramPointGraph>(), ppGraph)));


					//printIncludes(console, ppGraph);
                    

					console.CommentLine(string.Format("Analysis in the second phase completed in: {0}ms\n", watch2.ElapsedMilliseconds));
					//console.WarningsTaint(nextPhase.analysisTaintWarnings);

					if (ppGraph.End.OutSet != null)
                    {
                        console.CommentLine(string.Format("The number of memory locations is: {0}\n", ppGraph.End.OutSnapshot.NumMemoryLocations()));
                        int[] statistics = ppGraph.End.OutSnapshot.GetStatistics().GetStatisticsValues();
                        console.CommentLine(string.Format("The number of memory entry assigns is: {0}\n", statistics[(int)Statistic.MemoryEntryAssigns]));
                        console.CommentLine(string.Format("The number of value reads is: {0}\n", statistics[(int)Statistic.ValueReads]));
                        console.CommentLine(string.Format("The number of memory entry merges is: {0}\n", statistics[(int)Statistic.MemoryEntryMerges]));
                        console.CommentLine(string.Format("The number of index reads is: {0}\n", statistics[(int)Statistic.IndexReads]));
                        console.CommentLine(string.Format("The number of value reads is: {0}\n", statistics[(int)Statistic.ValueReads]));

                    }
                    else 
                    {
                        console.CommentLine(string.Format("Snapshot statistics are not available, because end point was not reached"));
                    }
#else

                    console.CommentLine(string.Format("File path: {0}\n", fileInfo.FullName));

                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var ppGraph = Analyzer.Run(fileInfo, memoryModel);
                    watch.Stop();

                    if (ppGraph.End.OutSet != null)
                    {
                        console.ProgramPointInfo("End point", ppGraph.End);
                    }
                    else
                    {
                        console.Error("End point was not reached");
                    }
                    console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));

					console.Warnings(AnalysisWarningHandler.GetWarnings(), AnalysisWarningHandler.GetSecurityWarnings());
                

#endif
                   
                }
				//Console.ReadKey();
                Console.WriteLine();
            }
        }

		private static void printIncludes(ConsoleOutput console, ProgramPointGraph ppGraph)
		{
			Console.WriteLine();
			var includes = new HashSet<String>();
			getIncludes(includes, new HashSet<ProgramPointGraph>(), ppGraph);

			console.CommentLine(string.Format("Included files: {0}\n", includes.Count));
			foreach (var incl in includes)
			{
				console.CommentLine(incl);
			}
			Console.WriteLine();
		}

        /// <summary>
        /// Number of program points in program point graph, including program points that are its extensions.
        /// </summary>
        public static int numProgramPoints(HashSet<ProgramPointGraph> processedGraphs, ProgramPointGraph ppg)
        {
            int num = ppg.Points.Cast<object>().Count();
            processedGraphs.Add(ppg);
            foreach (var point in ppg.Points)
            {
                foreach (var branch in point.Extension.Branches) 
                {
                    if (!processedGraphs.Contains (branch.Graph)) {
                        num += numProgramPoints(processedGraphs, branch.Graph);
                    }
                }
            }

            return num;
        }

		/// <summary>
		/// Gets all includes in the program point graph and program point graphs that are its transitive extensions.
		/// TODO: numProgramPoints, includes, NextPhaseAnalysis.resetPoints - code duplication of 
		/// traversing program point graph - implement this using visitors.
		/// </summary>
		public static void getIncludes(HashSet<String> includes, HashSet<ProgramPointGraph> processedGraphs, ProgramPointGraph ppg)
		{
			processedGraphs.Add(ppg);
			foreach (var point in ppg.Points)
			{
				includes.UnionWith (point.Extension.KeysIncludes.Select (i => (String)i));
				foreach (var branch in point.Extension.Branches) 
				{
					if (!processedGraphs.Contains (branch.Graph)) {
						getIncludes(includes, processedGraphs, branch.Graph);
					}
				}
			}
		}

        private static void RunMetrics(string[] files)
        {
            Dictionary<ConstructIndicator, string> contructMetrics = new Dictionary<ConstructIndicator, string>();
            contructMetrics.Add(ConstructIndicator.Autoload, "__autoload redeclaration presence or spl_autoload_register call");
            contructMetrics.Add(ConstructIndicator.MagicMethods, "Magic function presence");
            contructMetrics.Add(ConstructIndicator.ClassOrInterface, "Class construct presence");
            contructMetrics.Add(ConstructIndicator.DynamicInclude, "Include based on dynamic variable presence");
            contructMetrics.Add(ConstructIndicator.References, "Alias presence");
            
            contructMetrics.Add(ConstructIndicator.Session, "Session usage");
            contructMetrics.Add(ConstructIndicator.InsideFunctionDeclaration, "Declaration of class/function inside another function presence");
            contructMetrics.Add(ConstructIndicator.SuperGlobalVariable, "Super global variable usage");
            contructMetrics.Add(ConstructIndicator.Eval, "Eval function usage");
            contructMetrics.Add(ConstructIndicator.DynamicCall, "Dynamic call presence");
            
            contructMetrics.Add(ConstructIndicator.DynamicDereference, "Dynamic dereference presence");
            contructMetrics.Add(ConstructIndicator.DuckTyping, "Duck Typing presence");
            contructMetrics.Add(ConstructIndicator.PassingByReferenceAtCallSide, "Passing variable by reference at call site");
            contructMetrics.Add(ConstructIndicator.MySql, "My SQL functions presence");
            contructMetrics.Add(ConstructIndicator.ClassAlias, "Class alias construction presence");

            Dictionary<Rating, string> ratingMetrics = new Dictionary<Rating, string>();
            ratingMetrics.Add(Rating.ClassCoupling,"Class coupling");
            ratingMetrics.Add(Rating.PhpFunctionsCoupling, "PHP standard functions coupling");

            Dictionary<Quantity, string> quantityMetrics = new Dictionary<Quantity, string>();
            quantityMetrics.Add(Quantity.MaxInheritanceDepth,"Maximum inheritance depth");
            quantityMetrics.Add(Quantity.NumberOfLines,"Number of lines");
            quantityMetrics.Add(Quantity.NumberOfSources,"Number of sources");
            quantityMetrics.Add(Quantity.MaxMethodOverridingDepth,"Maximum method overriding depth");


            var console = new ConsoleOutput();
            foreach (string file in files)
            {
                console.CommentLine(string.Format("File path: {0}\n", file));
                try
                {
                    string code;
                    using (StreamReader reader = new StreamReader(file))
                    {
                        code = reader.ReadToEnd();
                    }

                    PhpSourceFile source_file = new PhpSourceFile(new FullPath(Path.GetDirectoryName(file)), new FullPath(file));
                    var metrics = MetricInfo.FromParsers(true, new SyntaxParser(source_file, code));
                    Console.WriteLine();
                    console.CommentLine("Indicator Metrics");
                    Console.WriteLine();
                    foreach (ConstructIndicator value in ConstructIndicator.GetValues(typeof(ConstructIndicator)))
                    {
                        string result = contructMetrics[value] + " : ";
                        if (metrics.HasIndicator(value))
                        {
                            result += "YES";
                        }
                        else 
                        {
                            result += "NO";
                        }
                        console.Metric(result);
                    }
                    Console.WriteLine();
                    console.CommentLine("Rating Metrics");
                    Console.WriteLine();
                    foreach (Rating value in Rating.GetValues(typeof(Rating)))
                    {
                        if (value != Rating.Cyclomacity)
                        {
                            string result = ratingMetrics[value] + " : ";
                            result+=metrics.GetRating(value);
                            console.Metric(result);
                        }
                    }
                    Console.WriteLine();
                    console.CommentLine("Quantity Metrics");
                    Console.WriteLine();
                    foreach (Quantity value in Quantity.GetValues(typeof(Quantity)))
                    {
                        string result = quantityMetrics[value]+ " : ";
                        result += metrics.GetQuantity(value);
                        console.Metric(result);
                        
                    }

                }
                catch (Exception e)
                {
                    console.Error(e.Message);
                }
            }
            Console.ReadKey();
            Console.WriteLine();
        }
        
    }
}
