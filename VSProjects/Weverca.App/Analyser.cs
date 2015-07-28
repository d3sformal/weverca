using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Weverca.Analysis;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.App.Benchmarking;
using Weverca.App.Settings;
using Weverca.ControlFlowGraph;
using Weverca.MemoryModels.ModularCopyMemoryModel;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;
using Weverca.Output.Output;
using Weverca.Taint;

namespace Weverca.App
{
    /// <summary>
    /// The state of analysis performed by an analyzer
    /// </summary>
    public enum AnalysisState
    {
        Initialising, ForwardAnalysis, NextPhaseAnalysis
    }

    /// <summary>
    /// Defines the end state of finished analysis
    /// </summary>
    public enum AnalysisEndState
    {
        NotFinished, Success, Crash, Abort, AbortMemory
    }

    /// <summary>
    /// Analyzer class which performes an analysis with defined settings.
    /// </summary>
    public class Analyser
    {
        /// <summary>
        /// Gets the name of the file to perform the analysis.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the type of the second phase of the analysis.
        /// </summary>
        /// <value>
        /// The type of the second phase.
        /// </value>
        public SecondPhaseType SecondPhaseType { get; private set; }

        /// <summary>
        /// Gets the type of the memory model.
        /// </summary>
        /// <value>
        /// The type of the memory model.
        /// </value>
        public MemoryModelType MemoryModelType { get; private set; }

        /// <summary>
        /// Gets the memory model.
        /// </summary>
        /// <value>
        /// The memory model.
        /// </value>
        public MemoryModels.MemoryModelFactory MemoryModel { get; private set; }

        /// <summary>
        /// Gets a value indicating whether an analysis is finished.
        /// </summary>
        /// <value>
        /// <c>true</c> if an analysis is finished; otherwise, <c>false</c>.
        /// </value>
        public bool IsFinished { get; private set; }
        
        /// <summary>
        /// Gets the state of the analysis.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public AnalysisState State { get; private set; }

        /// <summary>
        /// Gets the end state of analysis.
        /// </summary>
        /// <value>
        /// The end state.
        /// </value>
        public AnalysisEndState EndState { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the first phase was started.
        /// </summary>
        /// <value>
        /// <c>true</c> if this the first phase was started; otherwise, <c>false</c>.
        /// </value>
        public bool IsFirstPhaseStarted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the second was phase started.
        /// </summary>
        /// <value>
        /// <c>true</c> if the second phase was started; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecondPhaseStarted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the first phase was finished.
        /// </summary>
        /// <value>
        /// <c>true</c> if the first phase was finished; otherwise, <c>false</c>.
        /// </value>
        public bool IsFirstPhaseFinished { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the second phase was finished.
        /// </summary>
        /// <value>
        /// <c>true</c> if the second phase was finished; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecondPhaseFinished { get; private set; }

        /// <summary>
        /// Gets the number repetition of the benchmarked analysis.
        /// </summary>
        /// <value>
        /// The repetition counter.
        /// </value>
        public int RepetitionCounter { get; private set; }

        /// <summary>
        /// Gets the first phase initial memory.
        /// </summary>
        /// <value>
        /// The first phase initial memory.
        /// </value>
        public long FirstPhaseInitialMemory { get; private set; }

        /// <summary>
        /// Gets the first phase end memory.
        /// </summary>
        /// <value>
        /// The first phase end memory.
        /// </value>
        public long FirstPhaseEndMemory { get; private set; }

        /// <summary>
        /// Gets the second phase end memory.
        /// </summary>
        /// <value>
        /// The second phase end memory.
        /// </value>
        public long SecondPhaseEndMemory { get; private set; }

        /// <summary>
        /// Gets the analysis exception. Contains the reason why an analysis crashed. 
        /// Valid only if an analysis is finished and the end state is crashed.
        /// </summary>
        /// <value>
        /// The analysis exception.
        /// </value>
        public Exception AnalysisException { get; private set; }

        // CFG and PPG generated by weverca
        private ControlFlowGraph.ControlFlowGraph controlFlowGraph;
        private ProgramPointGraph programPointGraph;

        // Analysis objects
        private ForwardAnalysisBase firstPhaseAnalysis;
        private NextPhaseAnalysis secondPhaseAnalysis;

        // Measuring
        public Stopwatch Watch { get; set; }
        public Stopwatch WatchFirstPhase { get; set; }
        public Stopwatch WatchSecondPhase { get; set; }

        // Warning containers
        private IReadOnlyCollection<AnalysisWarning> analysisWarnings;
        private IReadOnlyCollection<AnalysisSecurityWarning> securityWarnings;
        private IReadOnlyCollection<AnalysisWarning> secondPhaseWarnings;

        // Benchmarking object and benchmark results
        private MemoryModelBenchmark memoryModelBenchmark;
        private List<BenchmarkResult> benchmarkResults = new List<BenchmarkResult>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Analyser"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="secondPhaseType">Type of the second phase.</param>
        /// <param name="memoryModelType">Type of the memory model.</param>
        public Analyser(string fileName, SecondPhaseType secondPhaseType, MemoryModelType memoryModelType)
        {
            this.FileName = fileName;
            this.SecondPhaseType = secondPhaseType;
            this.MemoryModelType = memoryModelType;
            
            IsFinished = false;
            EndState = AnalysisEndState.NotFinished;

            State = AnalysisState.Initialising;
            clearComputation();
        }

        /// <summary>
        /// Clears the inner fields before new computation is performed.
        /// </summary>
        private void clearComputation()
        {
            AnalysisWarningHandler.ResetWarnings();

            IsFirstPhaseStarted = false;
            IsSecondPhaseStarted = false;

            IsFirstPhaseFinished = false;
            IsSecondPhaseFinished = false;

            analysisWarnings = null;
            securityWarnings = null;
            secondPhaseWarnings = null;

            programPointGraph = null;
            firstPhaseAnalysis = null;
            secondPhaseAnalysis = null;

            // Force garbage collecting
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Starts the analysis.
        /// </summary>
        public void StartAnalysis()
        {
            this.memoryModelBenchmark = null;

            MemoryModel = selectMemoryModel().MemoryModelSnapshotFactory;

            WatchFirstPhase = new Stopwatch();
            WatchSecondPhase = new Stopwatch();

            Watch = Stopwatch.StartNew();
            try
            {
                createControlFlowGraph();
                clearComputation();
                
                runFirstPhaseAnalysis();
                runSecondPhaseAnalysis();

                EndState = AnalysisEndState.Success;
            }
            catch (ThreadAbortException e)
            {
                EndState = (AnalysisEndState)e.ExceptionState;
            }
            catch (Exception e)
            {
                AnalysisException = e;
                EndState = AnalysisEndState.Crash;
            }
            finally
            {
                Watch.Stop();
                IsFinished = true;
            }
        }

        /// <summary>
        /// Starts the benchmark.
        /// </summary>
        /// <param name="numberOfRepetitions">The number of repetitions.</param>
        public void StartBenchmark(int numberOfRepetitions)
        {
            this.memoryModelBenchmark = new MemoryModelBenchmark();

            var builder = selectMemoryModel().Builder();
            builder.Benchmark = this.memoryModelBenchmark;
            ModularMemoryModelFactories factories = builder.Build();

            MemoryModel = factories.MemoryModelSnapshotFactory;

            WatchFirstPhase = new Stopwatch();
            WatchSecondPhase = new Stopwatch();
            RepetitionCounter = 1;

            initialiseAnalysisSingletons();
            Watch = Stopwatch.StartNew();
            try
            {
                createControlFlowGraph();

                while (RepetitionCounter <= numberOfRepetitions)
                {
                    clearComputation();

                    BenchmarkResult result = new BenchmarkResult();
                    benchmarkResults.Add(result);
                    result.StartBenchmarking();

                    runFirstPhaseAnalysis();
                    runSecondPhaseAnalysis();

                    result.StopBenchmarking(this.memoryModelBenchmark);
                    this.memoryModelBenchmark.ClearResults();

                    RepetitionCounter++;
                }

                EndState = AnalysisEndState.Success;
            }
            catch (ThreadAbortException e)
            {
                EndState = (AnalysisEndState)e.ExceptionState;
            }
            catch (Exception e)
            {
                AnalysisException = e;
                EndState = AnalysisEndState.Crash;
            }
            finally
            {
                Watch.Stop();
                IsFinished = true;
            }
        }

        #region Analysis methods

        /// <summary>
        /// Selects the memory model.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unrecognized type of memory model  + MemoryModelType</exception>
        private ModularMemoryModelFactories selectMemoryModel()
        {
            switch (MemoryModelType)
            {
                case Settings.MemoryModelType.Copy:
                    return MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.Copy;

                case Settings.MemoryModelType.LazyExtendCommit:
                    return MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.LazyExtendCommit;

                case Settings.MemoryModelType.LazyContainers:
                    return MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.LazyContainers;
                    
                case Settings.MemoryModelType.Tracking:
                    return MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.Tracking;

                case Settings.MemoryModelType.TrackingDiff:
                    return MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.TrackingDiffContainers;

                default:
                    throw new Exception("Unrecognized type of memory model " + MemoryModelType);
            }
        }

        /// <summary>
        /// Initialises the analysis singletons.
        /// </summary>
        private void initialiseAnalysisSingletons()
        {
            ControlFlowGraph.ControlFlowGraph cfg = ControlFlowGraph.ControlFlowGraph.FromSource("<? $a = 1; echo $a; ?>", FileName);
            var analysis = new Weverca.Analysis.ForwardAnalysis(cfg, MemoryModel);
            analysis.Analyse();
        }

        /// <summary>
        /// Creates the control flow graph.
        /// </summary>
        private void createControlFlowGraph()
        {
            FileInfo sourceFile = new FileInfo(FileName);
            controlFlowGraph = ControlFlowGraph.ControlFlowGraph.FromFile(sourceFile);
        }

        /// <summary>
        /// Runs the first phase analysis.
        /// </summary>
        private void runFirstPhaseAnalysis()
        {
            State = AnalysisState.ForwardAnalysis;
            FirstPhaseInitialMemory = GC.GetTotalMemory(true);

            WatchFirstPhase.Start();

            try
            {
                firstPhaseAnalysis = new Weverca.Analysis.ForwardAnalysis(controlFlowGraph, MemoryModel);

                IsFirstPhaseStarted = true;
                firstPhaseAnalysis.Analyse();
            }
            finally
            {
                WatchFirstPhase.Stop();

                FirstPhaseEndMemory = GC.GetTotalMemory(true);

                if (IsFirstPhaseStarted)
                {
                    programPointGraph = firstPhaseAnalysis.ProgramPointGraph;

                    analysisWarnings = AnalysisWarningHandler.GetWarnings();
                    securityWarnings = AnalysisWarningHandler.GetSecurityWarnings();
                }
            }

            if (IsFirstPhaseStarted)
            {
                IsFirstPhaseFinished = true;
            }
        }

        /// <summary>
        /// Runs the second phase analysis.
        /// </summary>
        private void runSecondPhaseAnalysis()
        {
            secondPhaseAnalysis = createNextPhaseAnalysis();
            if (secondPhaseAnalysis != null)
            {
                secondPhaseWarnings = getSecondPhaseWarnings();

                State = AnalysisState.NextPhaseAnalysis;
                IsSecondPhaseStarted = true;

                try
                {
                    WatchSecondPhase.Start();
                    secondPhaseAnalysis.Analyse();
                }
                finally
                {
                    WatchSecondPhase.Stop();
                    SecondPhaseEndMemory = GC.GetTotalMemory(true);
                }

                IsSecondPhaseFinished = true;
            }
        }

        /// <summary>
        /// Gets the second phase warnings.
        /// </summary>
        /// <returns>Returns the second phase warnings.</returns>
        private IReadOnlyCollection<AnalysisWarning> getSecondPhaseWarnings()
        {
            switch (SecondPhaseType)
            {
                case SecondPhaseType.TaintAnalysis:
                    TaintAnalyzer analyzer = secondPhaseAnalysis.getNextPhaseAnalyzer() as TaintAnalyzer;
                    if (analyzer != null)
                    {
                        return analyzer.analysisTaintWarnings;
                    }
                    break;
            }

            List<AnalysisWarning> warnings = new List<AnalysisWarning>();
            return warnings;
        }

        /// <summary>
        /// Creates the next phase analysis.
        /// </summary>
        /// <returns>Returns the next phase analysis.</returns>
        private NextPhaseAnalysis createNextPhaseAnalysis()
        {
            switch (SecondPhaseType)
            {
                case SecondPhaseType.TaintAnalysis:
                    return new TaintForwardAnalysis(programPointGraph);
            }

            return null;
        }

        #endregion

        #region Watch methods

        /// <summary>
        /// Gets the number of program points in program point graph
        /// </summary>
        /// <param name="processedGraphs">The processed graphs.</param>
        /// <param name="processedLines">The processed lines.</param>
        /// <param name="ppg">The PPG.</param>
        /// <returns></returns>
        public int NumProgramPoints(HashSet<ProgramPointGraph> processedGraphs, Dictionary<string, HashSet<int>> processedLines, ProgramPointGraph ppg)
        {
            int num = ppg.Points.Cast<object>().Count();
            processedGraphs.Add(ppg);

            HashSet<int> currentScriptLines = null;
            if (ppg.OwningScript != null && !processedLines.TryGetValue(ppg.OwningScript.FullName, out currentScriptLines))
            {
                currentScriptLines = new HashSet<int>();
                processedLines.Add(ppg.OwningScript.FullName, currentScriptLines);
            }

            foreach (var point in ppg.Points)
            {
                if (currentScriptLines != null && point.Partial != null)
                    currentScriptLines.Add(point.Partial.Position.FirstLine);

                foreach (var branch in point.Extension.Branches)
                {
                    if (!processedGraphs.Contains(branch.Graph))
                    {
                        num += NumProgramPoints(processedGraphs, processedLines, branch.Graph);
                    }
                }
            }

            return num;
        }

        /// <summary>
        /// Gets the number of the program lines.
        /// </summary>
        /// <param name="programLines">The program lines.</param>
        /// <returns>Return the number of the program lines</returns>
        public int NumProgramLines(Dictionary<string, HashSet<int>> programLines)
        {
            var programLinesNum = 0;
            foreach (var script in programLines.Values)
            {
                programLinesNum += script.Count;
            }
            return programLinesNum;
        }
        
        /// <summary>
        /// Gets the number of warnings found by both phases of the analysis.
        /// </summary>
        /// <returns>Returns the number of warnings</returns>
        public int GetNumberOfWarnings()
        {
            int warnings = 0;
            if (IsFirstPhaseStarted)
            {
                warnings += AnalysisWarningHandler.NumberOfWarnings;
            }

            if (IsSecondPhaseStarted)
            {
                warnings += secondPhaseWarnings.Count;
            }

            return warnings;
        }

        #endregion
        
        #region Output methods

        /// <summary>
        /// Generates the output of the analysis.
        /// </summary>
        /// <param name="output">The output.</param>
        public void GenerateOutput(OutputBase output)
        {
            output.EmptyLine();
            output.Headline("Analysis summary");

            if (Watch != null)
            {
                var ts = Watch.Elapsed;

                output.EmptyLine();
                output.Headline2("Time consumption");
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                output.CommentLine("Weverca analyzer time consumption: " + elapsedTime);

                if (IsFirstPhaseStarted)
                {
                    var ts1 = WatchFirstPhase.Elapsed;
                    elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts1.Hours, ts1.Minutes, ts1.Seconds, ts1.Milliseconds);
                    output.CommentLine("First phase time consumption: " + elapsedTime);
                }
                if (IsSecondPhaseStarted)
                {
                    var ts2 = WatchSecondPhase.Elapsed;
                    elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", ts2.Hours, ts2.Minutes, ts2.Seconds, ts2.Milliseconds);
                    output.CommentLine("Second phase time consumption: " + elapsedTime);
                }
            }

            if (FirstPhaseInitialMemory != 0)
            {
                output.EmptyLine();
                output.Headline2("Memory consumption");
                output.CommentLine(string.Format("Initial memory before the first phase: {0}",
                    OutputUtils.GetMemoryText(FirstPhaseInitialMemory)
                    ));

                if (IsFirstPhaseStarted)
                {
                    output.CommentLine(string.Format("Memory at the end of the first phase: {0} (diff: {1})",
                        OutputUtils.GetMemoryText(FirstPhaseEndMemory),
                        OutputUtils.GetMemoryText(FirstPhaseEndMemory - FirstPhaseInitialMemory)
                        ));
                }
                if (IsSecondPhaseStarted)
                {
                    output.CommentLine(string.Format("Memory at the end of the second phase: {0} (diff: {1})",
                    OutputUtils.GetMemoryText(SecondPhaseEndMemory),
                    OutputUtils.GetMemoryText(SecondPhaseEndMemory - FirstPhaseEndMemory)
                    ));
                }
            }

            output.EmptyLine();
            output.Headline2("Code statistics");

            if (controlFlowGraph != null)
            {
                List<BasicBlock> basicBlocks = controlFlowGraph.CollectAllBasicBlocks();
                output.CommentLine("The number of basic blocks of code is: " + basicBlocks.Count);
            }

            if (programPointGraph != null)
            {
                var programLines = new Dictionary<string, HashSet<int>>();
                int numberOfProgramPoints = NumProgramPoints(new HashSet<ProgramPointGraph>(), programLines, programPointGraph);
                int numberOfProgramLines = NumProgramLines(programLines);

                output.CommentLine("The number of processed lines of code is: " + numberOfProgramLines);
                output.CommentLine("The number of program points in the application is: " + numberOfProgramPoints);

                if (programPointGraph.End.OutSet != null)
                {
                    output.CommentLine("The number of memory locations in final snapshot is: " + programPointGraph.End.OutSnapshot.NumMemoryLocations());
                }
                else
                {
                    output.CommentLine("End program point was not reached");
                }
            }
            else
            {
                output.CommentLine("Program point graph was not built");
            }

            output.EmptyLine();
            output.Headline2("Warnings");
            output.CommentLine("Total number of warnings: " + GetNumberOfWarnings());
            if (IsFirstPhaseStarted)
            {
                output.CommentLine("Number of warnings in the first phase: " + (analysisWarnings.Count + securityWarnings.Count));
            }
            if (IsSecondPhaseStarted)
            {
                output.CommentLine("Number of warnings in the second phase: " + secondPhaseWarnings.Count);
            }
        }

        /// <summary>
        /// Generates the warnings report.
        /// </summary>
        /// <param name="output">The output.</param>
        public void GenerateWarnings(OutputBase output)
        {
            output.Headline("Warnings");
            output.EmptyLine();
            output.CommentLine("Total number of warnings: " + GetNumberOfWarnings());


            if (IsFirstPhaseStarted)
            {
                output.CommentLine("Number of analysis warnings in the first phase: " + (analysisWarnings.Count));
                output.CommentLine("Number of security warnings in the first phase: " + (securityWarnings.Count));
            }
            if (IsSecondPhaseStarted)
            {
                output.CommentLine("Number of warnings in the second phase: " + secondPhaseWarnings.Count);
            }

            if (IsFirstPhaseStarted)
            {
                GenerateWarningsOutput(output, analysisWarnings, "First phase analysis warnings");
                GenerateWarningsOutput(output, securityWarnings, "First phase security warnings");
            }
            if (IsSecondPhaseStarted)
            {
                GenerateWarningsOutput(output, secondPhaseWarnings, "Second phase analysis warnings", true);
            }
        }

        /// <summary>
        /// Generates the final snapshot text representation.
        /// </summary>
        /// <param name="output">The output.</param>
        public void GenerateFinalSnapshotText(OutputBase output)
        {
            output.Headline("Final snapshot content");
            output.EmptyLine();

            if (programPointGraph != null && programPointGraph.End != null && programPointGraph.End.OutSet != null)
            {
                output.CommentLine("Number of memory locations: " + programPointGraph.End.OutSnapshot.NumMemoryLocations());

                Snapshot snapshot = getSnapshot(programPointGraph.End);
                if (snapshot != null)
                {
                    output.CommentLine("Number of variables: " + snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyVariables.Count);
                    output.CommentLine("Number of control variables: " + snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyControllVariables.Count);
                    output.CommentLine("Number of temporary variables: " + snapshot.Structure.Readonly.ReadonlyGlobalContext.ReadonlyTemporaryVariables.Count);
                    output.CommentLine("Number of arrays: " + snapshot.Structure.Readonly.ArrayDescriptors.Count());
                    output.CommentLine("Number of objects: " + snapshot.Structure.Readonly.ObjectDescriptors.Count());

                    SnapshotTextGenerator generator = new SnapshotTextGenerator(output);
                    generator.GenerateSnapshotText(snapshot);
                }
                else
                {
                    output.EmptyLine();
                    output.ProgramPointInfo("", programPointGraph.End);
                }
            }
            else
            {
                output.Error("End point was not reached");
            }
        }
        
        /// <summary>
        /// Generates the warnings output.
        /// </summary>
        /// <typeparam name="T">Type of the warning</typeparam>
        /// <param name="output">The output.</param>
        /// <param name="warnings">The warnings.</param>
        /// <param name="headLine">The head line.</param>
        /// <param name="displayTaintFlows">if set to <c>true</c> the display taint flows.</param>
        public void GenerateWarningsOutput<T>(OutputBase output, IReadOnlyCollection<T> warnings, string headLine, bool displayTaintFlows = false) where T : AnalysisWarning
        {
            output.line();
            output.Headline(headLine);
            if (warnings.Count == 0)
            {
                output.line();
                output.comment("No warnings");
                output.line();
            }
            string fileName = "/";
            bool dedent = false;
            foreach (var s in warnings)
            {
                if (fileName != s.FullFileName)
                {
                    if (dedent)
                    {
                        output.Dedent();
                    }
                    output.line();
                    fileName = s.FullFileName;
                    output.head2("File: " + fileName);

                    output.Indent();
                    dedent = true;

                    output.line();
                    output.line();
                }
                output.variableInfoLine(s.ToString());
                output.Indent();
                output.line();
                output.hint("Called from: ");
                output.comment(s.ProgramPoint.OwningPPGraph.Context.ToString());
                if (s is AnalysisTaintWarning && displayTaintFlows)
                {
                    output.line();
                    output.hint("Taint propagation: ");
                    output.Indent();
                    output.line();
                    output.comment(((AnalysisTaintWarning)(object)s).TaintFlow);
                    output.Dedent();
                }
                output.Dedent();

                output.line();
            }

            if (dedent)
            {
                output.Dedent();
            }
            output.line();
        }
        
        #endregion
        
        #region Benchmark stats

        /// <summary>
        /// Writes the out benchmark stats.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal void WriteOutBenchmarkStats(StreamWriter writer)
        {
            writer.WriteLine("Iteration;Ananlysis time;Operation time;Algorithm time;Transactions;Operations;Algorithms;Memory difference;Start memory; End memory");

            int benchmarkRun = 1;
            foreach (var result in benchmarkResults)
            {
                writer.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}", benchmarkRun, result.TotalAnalysisTime, result.TotalOperationTime, result.TotalAlgorithmTime,
                    result.NumberOfTransactions, result.NumberOfOperations, result.NumberOfAlgorithms, result.FinalMemory - result.InitialMemory, result.InitialMemory, result.FinalMemory
                ));
                benchmarkRun++;
            }
        }

        /// <summary>
        /// Writes the out transaction benchmark.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal void WriteOutTransactionBenchmark(StreamWriter writer)
        {
            writer.WriteLine("Iteration;Transaction number;Memory mode;Time;Start memory;End memory;Memory difference;Raw start memory");

            int benchmarkRun = 1;
            foreach (var result in benchmarkResults)
            {
                long initialMemory = result.InitialMemory;

                foreach (TransactionEntry transaction in result.TransactionResults)
                {
                    long memDiff = transaction.EndMemory - transaction.StartMemory;
                    writer.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7}", benchmarkRun, transaction.TransactionID, transaction.Mode,
                        transaction.TransactionTime, transaction.StartMemory - initialMemory, transaction.EndMemory - initialMemory, memDiff, 
                        transaction.StartMemory
                        ));
                }
                benchmarkRun++;
            }
        }

        /// <summary>
        /// Writes the out transaction memory medians.
        /// </summary>
        /// <param name="memoryMediansWriter">The memory medians writer.</param>
        internal void WriteOutTransactionMemoryMedians(StreamWriter memoryMediansWriter)
        {

            List<List<long>> transactionsMemory = new List<List<long>>();

            int benchmarkRun = 1;
            foreach (var result in benchmarkResults)
            {
                long initialMemory = result.InitialMemory;

                int transNumber = 0;
                foreach (TransactionEntry transaction in result.TransactionResults)
                {
                    if (transactionsMemory.Count <= transNumber)
                    {
                        transactionsMemory.Add(new List<long>());
                    }
                    transactionsMemory[transNumber].Add(transaction.EndMemory - initialMemory);
                    transNumber++;
                }
                benchmarkRun++;
            }

            memoryMediansWriter.WriteLine("Transaction;End memory medians");

            int transactionRow = 1;
            foreach (var memoryList in transactionsMemory)
            {
                memoryList.Sort();
                long median = 0;

                int half = memoryList.Count / 2;
                if (memoryList.Count % 2 == 0)
                {
                    median = (memoryList[half] + memoryList[half - 1]) / 2;
                }
                else
                {
                    median = memoryList[half];
                }

                memoryMediansWriter.WriteLine(string.Format("{0};{1}", transactionRow, median));
                transactionRow++;
            }
        }

        /// <summary>
        /// Writes the out algorithm total times.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal void WriteOutAlgorithmTotalTimes(StreamWriter writer)
        {
            StringBuilder lineBuilder = new StringBuilder();
            List<AlgorithmType> algorithmTypes = new List<AlgorithmType>();
            foreach (var algorithm in benchmarkResults[0].AlgorithmResults)
            {
                algorithmTypes.Add(algorithm.Key);
                lineBuilder.Append(algorithm.Key.ToString() + ";");
            }

            lineBuilder.Length = lineBuilder.Length - 1;
            writer.WriteLine(lineBuilder);
            lineBuilder.Clear();

            foreach (var result in benchmarkResults)
            {
                foreach (var algorithmType in algorithmTypes)
                {
                    lineBuilder.Append(result.AlgorithmResults[algorithmType].TotalTime);
                    lineBuilder.Append(";");
                }

                lineBuilder.Length = lineBuilder.Length - 1;
                writer.WriteLine(lineBuilder);
                lineBuilder.Clear();
            }
        }

        #endregion
        
        #region Private helpers

        private Snapshot getSnapshot(ProgramPointBase programPointBase)
        {
            SnapshotBase snapshotBase = programPointBase.OutSnapshot;
            return snapshotBase as Snapshot;
        }

        #endregion
    }
}
