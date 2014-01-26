using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis;
using Weverca.Output;

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
                Console.WriteLine(@"-sa FILENAME [FILENAME]...");
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
                    var analysisFiles = new string[args.Length - 1];
                    Array.Copy(args, 1, analysisFiles, 0, args.Length - 1);
                    RunStaticAnalysis(analysisFiles);
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
        private static void RunStaticAnalysis(string[] filenames)
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
                    Analyzer.Run(fileInfo);

                    // Process analysis
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var ppGraph = Analyzer.Run(fileInfo);
                    watch.Stop();

                    // Build output
                    var console = new ConsoleOutput();
                    console.CommentLine(string.Format("File path: {0}\n", fileInfo.FullName));
                    var graphWalker = new GraphWalking.CallGraphPrinter(ppGraph);
                    console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));

                    graphWalker.Run(console);

                    console.Warnings(AnalysisWarningHandler.GetWarningsToOutput());

                    console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));
                    Console.ReadKey();
                    Console.WriteLine();
                }
            }
        }
    }
}
