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
using Weverca.App.Settings;
using Weverca.ControlFlowGraph;
using Weverca.MemoryModels.ModularCopyMemoryModel;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;
using Weverca.Output.Output;
using Weverca.Taint;

namespace Weverca.App
{
    enum AnalysisState
    {
        Initialising, ForwardAnalysis, NextPhaseAnalysis
    }

    enum AnalysisEndState
    {
        NotFinished, Success, Crash, Abort, AbortMemory
    }

    class Analyser
    {
        public MemoryModels.MemoryModelFactory MemoryModel { get; private set; }

        public bool IsFinished { get; private set; }

        public AnalysisEndState EndState { get; private set; }

        public bool IsFirstPhaseStarted { get; private set; }

        public bool IsSecondPhaseStarted { get; private set; }

        public bool IsFirstPhaseFinished { get; private set; }

        public bool IsSecondPhaseFinished { get; private set; }

        public int RepetitionCounter { get; private set; }

        public AnalysisState State { get; private set; }

        public Exception AnalysisException { get; private set; }


        public string FileName { get; private set; }

        public SecondPhaseType SecondPhaseType { get; private set; }

        public MemoryModelType MemoryModelType { get; private set; }

        private ControlFlowGraph.ControlFlowGraph controlFlowGraph;
        private ProgramPointGraph programPointGraph;

        private ForwardAnalysisBase firstPhaseAnalysis;
        private NextPhaseAnalysis secondPhaseAnalysis;

        public Stopwatch Watch { get; set; }
        public Stopwatch WatchFirstPhase { get; set; }
        public Stopwatch WatchSecondPhase { get; set; }

        private IReadOnlyCollection<AnalysisWarning> analysisWarnings;
        private IReadOnlyCollection<AnalysisSecurityWarning> securityWarnings;
        private IReadOnlyCollection<AnalysisWarning> secondPhaseWarnings;
        private IBenchmark memoryModelBenchmark;

        private List<BenchmarkResult> benchmarkResults = new List<BenchmarkResult>();

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

            GC.Collect();
        }

        public void StartAnalysis()
        {
            this.memoryModelBenchmark = null;

            var builder = selectMemoryModel().Builder();
            builder.Benchmark = new EmptyMemoryModelBenchmark();
            ModularMemoryModelFactories factories = builder.Build();

            MemoryModel = factories.MemoryModelSnapshotFactory;

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
            /*catch (Exception e)
            {
                AnalysisException = e;
                EndState = AnalysisEndState.Crash;
            }*/
            finally
            {
                Watch.Stop();
                IsFinished = true;
            }
        }

        public void StartBenchmark(int numberOfRepetitions)
        {
            this.memoryModelBenchmark = new MemoryModelBenchmark();

            var builder = selectMemoryModel().Builder();
            builder.Benchmark = this.memoryModelBenchmark;
            ModularMemoryModelFactories factories = builder.Build();

            MemoryModel = factories.MemoryModelSnapshotFactory;

            start(numberOfRepetitions);
        }

        private void start(int numberOfRepetitions)
        {
            WatchFirstPhase = new Stopwatch();
            WatchSecondPhase = new Stopwatch();
            RepetitionCounter = 1;

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

                case Settings.MemoryModelType.LazyAndDiffContainers:
                    return MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.LazyAndDiffContainers;

                case Settings.MemoryModelType.Tracking:
                    return MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.Tracking;

                case Settings.MemoryModelType.TrackingDiff:
                    return MemoryModels.ModularCopyMemoryModel.ModularMemoryModelVariants.TrackingDiffContainers;

                default:
                    throw new Exception("Unrecognized type of memory model " + MemoryModelType);
            }
        }

        private void createControlFlowGraph()
        {
            FileInfo sourceFile = new FileInfo(FileName);
            controlFlowGraph = ControlFlowGraph.ControlFlowGraph.FromFile(sourceFile);
        }

        private void runFirstPhaseAnalysis()
        {
            State = AnalysisState.ForwardAnalysis;

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
                }

                IsSecondPhaseFinished = true;
            }
        }

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

        #region Output methods

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
                int numberOfProgramPoints = numProgramPoints(new HashSet<ProgramPointGraph>(), programLines, programPointGraph);
                int numberOfProgramLines = numProgramLines(programLines);

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

        public void GenerateMemoryModelStatisticsOutput(OutputBase output)
        {
            output.Headline("Memory model statistics");
            output.EmptyLine();

            if (programPointGraph != null && programPointGraph.End != null && programPointGraph.End.OutSet != null)
            {
                if (this.memoryModelBenchmark != null)
                {
                    output.CommentLine("Total number of memory model transactions: " + memoryModelBenchmark.NumberOfTransactions);
                    output.CommentLine("Total number of memory model operations: " + memoryModelBenchmark.NumberOfOperations);
                    output.CommentLine("Total number of memory model algorithm runs: " + memoryModelBenchmark.NumberOfAlgorithms);

                    output.EmptyLine();

                    output.CommentLine("Total time consumed by memory model: " + new TimeSpan(0, 0, 0, 0, (int)memoryModelBenchmark.TotalOperationTime).ToString());

                    output.CommentLine("Total time consumed by memory model algorithms: " + new TimeSpan(0, 0, 0, 0, (int)memoryModelBenchmark.TotalAlgorithmTime).ToString());

                    generateAlgorithmFamilyResult(output, AlgorithmFamilies.Write, "Write");
                    generateAlgorithmFamilyResult(output, AlgorithmFamilies.Commit, "Commit");
                    generateAlgorithmFamilyResult(output, AlgorithmFamilies.Merge, "Merge");
                    generateAlgorithmFamilyResult(output, AlgorithmFamilies.Extend, "Extend");
                    generateAlgorithmFamilyResult(output, AlgorithmFamilies.Read, "Read");
                    generateAlgorithmFamilyResult(output, AlgorithmFamilies.Memory, "Memory");
                }
                else
                {
                    output.Error("Memory model statistics report is allowed for ModularMemoryModel only");
                }
            }
            else
            {
                output.Error("End point was not reached");
            }
        }

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

        private void generateAlgorithmFamilyResult(OutputBase output, AlgorithmType[] algorithmType, string header)
        {
            output.EmptyLine();
            output.Headline2(header + " algorithms statistics");

            foreach (AlgorithmType type in algorithmType)
            {
                AlgorithmAggregationEntry data;
                if (memoryModelBenchmark.AlgorithmResults.TryGetValue(type, out data))
                {
                    string name = getAlgorithmName(type);
                    int runs = data.NumberOfRuns;
                    double time = data.TotalTime;
                    double timePercentil = time / memoryModelBenchmark.TotalAlgorithmTime;

                    output.line();
                    output.variable(name);
                    output.Indent();

                    output.line();
                    output.hint("Runs: ");
                    output.info(runs.ToString());

                    output.line();
                    output.hint("Time: ");
                    output.info(new TimeSpan(0, 0, 0, 0, (int)time).ToString());

                    output.line();
                    output.hint("Total percentil: ");
                    output.info(String.Format("{0:0.0}%", timePercentil * 100));

                    output.Dedent();
                    output.line();
                }
            }

            output.line();
        }

        private string getAlgorithmName(AlgorithmType type)
        {
            StringBuilder builder = new StringBuilder(type.ToString().ToLower());
            if (builder.Length > 0)
            {
                for (int x = 0; x < builder.Length; x++)
                {
                    if (builder[x] == '_')
                    {
                        builder[x] = ' ';
                    }
                }

                builder[0] = Char.ToUpper(builder[0]);
            }

            return builder.ToString();
        }

        #endregion

        #region Watch methods

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





        #region Private helpers

        private Snapshot getSnapshot(ProgramPointBase programPointBase)
        {
            SnapshotBase snapshotBase = programPointBase.OutSnapshot;
            return snapshotBase as Snapshot;
        }

        public static int numProgramPoints(HashSet<ProgramPointGraph> processedGraphs, Dictionary<string, HashSet<int>> processedLines, ProgramPointGraph ppg)
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
                        num += numProgramPoints(processedGraphs, processedLines, branch.Graph);
                    }
                }
            }

            return num;
        }

        private int numProgramLines(Dictionary<string, HashSet<int>> programLines)
        {
            var programLinesNum = 0;
            foreach (var script in programLines.Values)
            {
                programLinesNum += script.Count;
            }
            return programLinesNum;
        }

        #endregion


        internal void WriteOutBenchmarkStats(StreamWriter writer)
        {
            writer.WriteLine("Iteration;Ananlysis time;Operation time;Algorithm time;Transactions;Operations;Algorithms;Memory difference");

            int benchmarkRun = 1;
            foreach (var result in benchmarkResults)
            {
                writer.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7}", benchmarkRun, result.TotalAnalysisTime, result.TotalOperationTime, result.TotalAlgorithmTime,
                    result.NumberOfTransactions, result.NumberOfOperations, result.NumberOfAlgorithms, result.FinalMemory - result.InitialMemory
                ));
                benchmarkRun++;
            }
        }

        internal void WriteOutTransactionBenchmark(StreamWriter writer)
        {
            writer.WriteLine("Iteration;Transaction number;Memory mode;Locations;Time;Start memory;End memory;Memory difference");

            int benchmarkRun = 1;
            foreach (var result in benchmarkResults)
            {
                long initialMemory = result.InitialMemory;

                foreach (TransactionEntry transaction in result.TransactionResults)
                {
                    long memDiff = transaction.EndMemory - transaction.StartMemory;
                    writer.WriteLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7}", benchmarkRun, transaction.TransactionID, transaction.Mode,
                        transaction.NumberfLocations, transaction.TransactionTime, transaction.StartMemory - initialMemory,
                        transaction.EndMemory - initialMemory, memDiff
                        ));
                }
                benchmarkRun++;
            }
        }

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
                    median = (memoryList[half] + memoryList[half + 1]) / 2;
                }
                else
                {
                    median = memoryList[half];
                }

                memoryMediansWriter.WriteLine(string.Format("{0};{1}", transactionRow, median));
                transactionRow++;
            }
        }

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
    }
}
