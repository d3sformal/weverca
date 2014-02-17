//#define TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis;
using Weverca.Output;
using Weverca.AnalysisFramework.Memory;

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
        /// <param name="args">TODO: Specification of arguments</param>
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Missing argument");
                Console.WriteLine(@"Example of usage: weverca.exe -options ..\..\..\..\..\PHP_sources\test_programs\testfile.php");
                Console.WriteLine(@"-sa [-a SimpleAnalysis|WevercaAnalysis][-mm CopyMM|VrMM FILENAME] [FILENAME]...");
                Console.WriteLine(@"  Static analysis");
                Console.WriteLine(@"-cmide [-options_cmide]");
                Console.WriteLine(@"  Code metrics for IDE integration");
                Console.WriteLine(@"  -cmide -constructs list_of_constructs_separated_by_space");
                Console.WriteLine(@"    Constructs search");
                Console.WriteLine(@"  -cmide -quantity");
                Console.WriteLine(@"    Quantity and rating code metrics computation");
                Console.ReadKey();
                return;
            }

            switch (args[0])
            {
                case "-sa":
                    int filesIndex = 1;
                    MemoryModels.MemoryModels memoryModel = MemoryModels.MemoryModels.VirtualReferenceMM;
                    Weverca.AnalysisFramework.UnitTest.Analyses analysis = Weverca.AnalysisFramework.UnitTest.Analyses.WevercaAnalysis;
                    if (args[filesIndex] == "-a")
                    {
                        if (args[filesIndex+1] == "SimpleAnalysis") analysis = Weverca.AnalysisFramework.UnitTest.Analyses.SimpleAnalysis;
                        filesIndex += 2;
                    }
                    if (args.Length >= filesIndex && args[filesIndex] == "-mm")
                    {
                        if (args[filesIndex+1] == "CopyMM") memoryModel = MemoryModels.MemoryModels.CopyMM;
                        filesIndex += 2;
                    }

                    var analysisFiles = new string[args.Length - filesIndex];
                    Array.Copy(args, filesIndex, analysisFiles, 0, args.Length - filesIndex);
                    RunStaticAnalysis(analysisFiles, analysis, memoryModel);
                    break;
                case "-cmide":
                    var metricsArgs = new string[args.Length - 3];
                    Array.Copy(args, 2, metricsArgs, 0, args.Length - 3);
                    MetricsForIDEIntegration.Run(args[1], args[args.Length - 1], metricsArgs);
                    break;
                default:
                    Console.WriteLine("Unknown option: \"{0}\"", args[0]);
                    break;
            }
        }

        /// <summary>
        /// Execute the static analysis and print results
        /// </summary>
        /// <param name="filenames">List of file name patterns from command line</param>
        /// <param name="analysis">type of analysis</param>
        /// <param name="memoryModel">The memory model used for analysis</param>
        private static void RunStaticAnalysis(string[] filenames, Weverca.AnalysisFramework.UnitTest.Analyses analysis, MemoryModels.MemoryModels memoryModel)
        {
            foreach (var argument in filenames)
            {
                var filesInfo = Analyzer.GetFileNames(argument);
                if (filesInfo == null)
                {
                    Console.WriteLine("Path \"{0}\" cannot be recognized", argument);
                    Console.ReadKey();
                    Console.WriteLine();
                    continue;
                }
                else if (filesInfo.Length <= 0)
                {
                    Console.WriteLine("Path pattern \"{0}\" does not match any file", argument);
                    Console.ReadKey();
                    Console.WriteLine();
                    continue;
                }

                foreach (var fileInfo in filesInfo)
                {
                    // TODO: This is for time consumption analyzing only
                    // Analyze twice - because of omitting .NET initialization we get better analysis time
                    //Analyzer.Run(fileInfo, memoryModel);
                   
#if TEST
                    // Process analysis
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var ppGraph = Analyzer.Run(fileInfo, analysis, memoryModel);
                    watch.Stop();
                    // Build output
                    var console = new ConsoleOutput();
                    console.CommentLine(string.Format("File path: {0}\n", fileInfo.FullName));
                    
                    var graphWalker = new GraphWalking.CallGraphPrinter(ppGraph);
                    console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));

                    graphWalker.Run(console);

                    console.Warnings(AnalysisWarningHandler.GetWarnings());

                    console.SecurityWarnings(AnalysisWarningHandler.GetSecurityWarnings());
                     

                    console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));
                    console.CommentLine(string.Format("The number of nodes in the pp graph is: {0}\n", ppGraph.Points.Cast<object>().Count()));
                    if (ppGraph.End.OutSet != null)
                    {
                        console.CommentLine(string.Format("The number of memory locations is: {0}\n", ppGraph.End.OutSnapshot.NumMemoryLocations()));
                        int[] statistics = ppGraph.End.OutSnapshot.GetStatistics().GetStatisticsValues();
                        console.CommentLine(string.Format("The number of memory entry assigns is: {0}\n", statistics[(int)Statistic.MemoryEntryAssigns]));
                        console.CommentLine(string.Format("The number of value reads is: {0}\n", statistics[(int)Statistic.ValueReads]));
                        console.CommentLine(string.Format("The number of memory entry merges is: {0}\n", statistics[(int)Statistic.MemoryEntryMerges]));
                        console.CommentLine(string.Format("The number of index assings is: {0}\n", statistics[(int)Statistic.IndexAssigns]));
                        console.CommentLine(string.Format("The number of index alias assings is: {0}\n", statistics[(int)Statistic.IndexAliasAssigns]));
                        console.CommentLine(string.Format("The number of index reads is: {0}\n", statistics[(int)Statistic.IndexReads]));
                        console.CommentLine(string.Format("The number of index reads attmpts is: {0}\n", statistics[(int)Statistic.IndexReadAttempts]));
                        console.CommentLine(string.Format("The number of value reads is: {0}\n", statistics[(int)Statistic.ValueReads]));
                        console.CommentLine(string.Format("The number of value read attempts is: {0}\n", statistics[(int)Statistic.ValueReadAttempts]));
                    }
                    else 
                    {
                        console.CommentLine(string.Format("Snapshot statistics are not available, because end point was not reached"));
                    }
#else
                    var console = new ConsoleOutput();
                    console.CommentLine(string.Format("File path: {0}\n", fileInfo.FullName));

                    try
                    {

                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        var ppGraph = Analyzer.Run(fileInfo, analysis, memoryModel);
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

                        console.Warnings(AnalysisWarningHandler.GetWarnings());

                        console.SecurityWarnings(AnalysisWarningHandler.GetSecurityWarnings());
                    }
                    catch (Exception e)
                    {
                        console.Error(e.Message);
                    }

#endif
                    Console.ReadKey();
                    Console.WriteLine();
                }
            }
        }
    }
}
