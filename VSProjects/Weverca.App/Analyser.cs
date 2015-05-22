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
        public MemoryModels.MemoryModels MemoryModel { get; set; }

        public bool IsFinished { get; private set; }

        public AnalysisEndState EndState { get; private set; }

        public bool IsFirstPhaseStarted { get; private set; }

        public bool IsSecondPhaseStarted { get; private set; }

        public bool IsFirstPhaseFinished { get; private set; }

        public bool IsSecondPhaseFinished { get; private set; }

        public AnalysisState State { get; private set; }

        public Exception AnalysisException { get; private set; }


        public string FileName { get; set; }

        public SecondPhaseType SecondPhaseType { get; set; }

        public MemoryModelType MemoryModelType { get; set; }

        public LoggingOutputType LoggingOutputType { get; set; }

        public LoggingStrategyType LoggingStrategyType { get; set; }


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

        public Analyser()
        {
            IsFinished = false;
            EndState = AnalysisEndState.NotFinished;

            IsFirstPhaseStarted = false;
            IsSecondPhaseStarted = false;

            IsFirstPhaseFinished = false;
            IsSecondPhaseFinished = false;

            State = AnalysisState.Initialising;

            if (MemoryModel == null)
            {
                MemoryModel = MemoryModels.MemoryModels.ModularCopyMM;
            }
        }

        public void StartAnalysis()
        {
            Watch = Stopwatch.StartNew();
            try
            {
                prepareMemoryEntry();
                createControlFlowGraph();

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

        #region Analysis methods

        private void prepareMemoryEntry()
        {

        }

        private void createControlFlowGraph()
        {
            FileInfo sourceFile = new FileInfo(FileName);
            controlFlowGraph = ControlFlowGraph.ControlFlowGraph.FromFile(sourceFile);
        }

        private void runFirstPhaseAnalysis()
        {
            State = AnalysisState.ForwardAnalysis;
            IsFirstPhaseStarted = true;

            WatchFirstPhase = Stopwatch.StartNew();

            try
            {
                firstPhaseAnalysis = new Weverca.Analysis.ForwardAnalysis(controlFlowGraph, MemoryModel);
                firstPhaseAnalysis.Analyse();
            }
            finally
            {
                WatchFirstPhase.Stop();

                programPointGraph = firstPhaseAnalysis.ProgramPointGraph;

                analysisWarnings = AnalysisWarningHandler.GetWarnings();
                securityWarnings = AnalysisWarningHandler.GetSecurityWarnings();
            }

            IsFirstPhaseFinished = true;
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
                    WatchSecondPhase = Stopwatch.StartNew();
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
            if (SecondPhaseType != null)
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
            }

            List<AnalysisWarning> warnings = new List<AnalysisWarning>();
            return warnings;
        }

        private NextPhaseAnalysis createNextPhaseAnalysis()
        {
            if (SecondPhaseType != null)
            {
                switch (SecondPhaseType)
                {
                    case SecondPhaseType.TaintAnalysis:
                        return new TaintForwardAnalysis(programPointGraph);
                }
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
                output.EmptyLine();
                output.Headline2("Time consumption");
                output.CommentLine("Weverca analyzer time consumption: " + Watch.Elapsed.ToString());
                
                if (IsFirstPhaseStarted)
                {
                    output.CommentLine("First phase time consumption: " + WatchFirstPhase.Elapsed.ToString());
                }
                if (IsSecondPhaseStarted)
                {
                    output.CommentLine("Second phase time consumption: " + WatchSecondPhase.Elapsed.ToString());
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
                Snapshot snapshot = getSnapshot(programPointGraph.End);
                if (snapshot != null)
                {
                    IBenchmark benchmark = Snapshot.Benchmark;

                    output.CommentLine("Total number of memory model transactions: " + benchmark.TransactionStarts);
                    output.CommentLine("Total number of memory model algorithm runs: " + benchmark.AlgorithmStops);
                    output.CommentLine("Total time consumed by memory model algorithms: " + new TimeSpan(0, 0, 0, 0, (int)benchmark.AlgorithmTime).ToString());

                    generateAlgorithmFamilyResult(output, benchmark, AlgorithmFamilies.Write, "Write");
                    generateAlgorithmFamilyResult(output, benchmark, AlgorithmFamilies.Commit, "Commit");
                    generateAlgorithmFamilyResult(output, benchmark, AlgorithmFamilies.Merge, "Merge");
                    generateAlgorithmFamilyResult(output, benchmark, AlgorithmFamilies.Extend, "Extend");
                    generateAlgorithmFamilyResult(output, benchmark, AlgorithmFamilies.Read, "Read");
                    generateAlgorithmFamilyResult(output, benchmark, AlgorithmFamilies.Memory, "Memory");
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

        private void generateAlgorithmFamilyResult(OutputBase output, IBenchmark benchmark, AlgorithmType[] algorithmType, string header)
        {
            output.EmptyLine();
            output.Headline2(header + " algorithms statistics");

            foreach (AlgorithmType type in algorithmType)
            {
                AlgorithmEntry data;
                if (benchmark.AlgorithmResults.TryGetValue(type, out data))
                {
                    string name = getAlgorithmName(type);
                    int runs = data.Stops;
                    double time = data.Time;
                    double timePercentil = time / benchmark.AlgorithmTime;

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
    }
}
