using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Output;

namespace Weverca
{
    class Program
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
        static void Main(string[] args)
        {
            if (args.Count() < 2)
            {
                Console.WriteLine("Missing argument");
                Console.WriteLine(@"Example of usage: weverca.exe -options ..\..\..\..\..\PHP_sources\test_programs\testfile.php ");
                Console.WriteLine(@"-sa");
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
            
            //TODO: Resolve entry file
            var analysisType = args[0];
            var analyzedFile = args[args.Length-1];

            switch (analysisType)
            {
                case "-sa":
                    RunStaticAnalysis(analyzedFile);
                    break;
                case "-cmide":
                    string[] metricsArgs = new string[args.Length-3];
                    Array.Copy(args, 2, metricsArgs, 0, args.Length - 3);
                    MetricsForIDEIntegration.Run(args[1], analyzedFile, metricsArgs);
                    break;
            }
        }

        static private void RunStaticAnalysis(string analyzedFile) 
        {
            //TODO: this is for time consumption analyzing only
            //Analyze twice - because of omitting .NET initialization we get better analysis time
            Analyzer.Run(analyzedFile);

            //Process analysis
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var ppGraph = Analyzer.Run(analyzedFile);
            watch.Stop();

            //Build output
            var console = new ConsoleOutput();
            var graphWalker = new GraphWalking.CallGraphPrinter(ppGraph);
            console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));

            graphWalker.Run(console);
            console.CommentLine(string.Format("Analysis completed in: {0}ms\n", watch.ElapsedMilliseconds));
            Console.ReadKey();
        }
    }
}
