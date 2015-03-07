using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Weverca.Analysis;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.App.Settings;
using Weverca.Output.Output;
using Weverca.Taint;

namespace Weverca.App
{
    enum AnalysisState
    {
        Initialising, ForwardAnalysis, NextPhaseAnalysis
    }

    class Analyser
    {
        public MemoryModels.MemoryModels MemoryModel { get; set; }

        public bool IsFinished { get; private set; }

        public bool IsCrashed { get; private set; }

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
            IsCrashed = false;

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
            }
            catch (Exception e)
            {
                AnalysisException = e;
                IsCrashed = true;
            }

            Watch.Stop();
            IsFinished = true;
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

            firstPhaseAnalysis = new Weverca.Analysis.ForwardAnalysis(controlFlowGraph, MemoryModel);
            firstPhaseAnalysis.Analyse();

            WatchFirstPhase.Stop();

            programPointGraph = firstPhaseAnalysis.ProgramPointGraph;
            IsFirstPhaseFinished = true;

            analysisWarnings = AnalysisWarningHandler.GetWarnings();
            securityWarnings = AnalysisWarningHandler.GetSecurityWarnings();
        }

        private void runSecondPhaseAnalysis()
        {
            secondPhaseAnalysis = createNextPhaseAnalysis();
            if (secondPhaseAnalysis != null)
            {
                secondPhaseWarnings = getSecondPhaseWarnings();

                State = AnalysisState.NextPhaseAnalysis;
                IsSecondPhaseStarted = true;

                WatchSecondPhase = Stopwatch.StartNew();
                secondPhaseAnalysis.Analyse();
                WatchSecondPhase.Stop();

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


            output.EmptyLine();
            output.Headline2("Time consumption");
            output.CommentLine("Weverca analyzer time consumption: " + Watch.Elapsed.ToString());
            output.CommentLine("First phase time consumption: " + WatchFirstPhase.Elapsed.ToString());
            if (IsSecondPhaseFinished)
            {
                output.CommentLine("Second phase time consumption: " + WatchSecondPhase.Elapsed.ToString());
            }


            output.EmptyLine();
            output.Headline2("Code statistics");

            var programLines = new Dictionary<string, HashSet<int>>();
            output.CommentLine("The number of nodes in the application is: " + numProgramPoints(new HashSet<ProgramPointGraph>(), programLines, programPointGraph));

            var programLinesNum = 0;
            foreach (var script in programLines.Values)
            {
                programLinesNum += script.Count;
            }
            output.CommentLine("The number of processed lines of code is: " + programLinesNum);

            if (programPointGraph != null && programPointGraph.End.OutSnapshot != null)
            {
                output.CommentLine("The number of memory locations in final snapshot is: " + programPointGraph.End.OutSnapshot.NumMemoryLocations());
            }
            else
            {
                output.CommentLine("End program point was not reached");
            }


            output.EmptyLine();
            output.Headline2("Warnings");
            output.CommentLine("Total number of warnings: " + GetNumberOfWarnings());
            output.CommentLine("Number of warnings in the first phase: " + (analysisWarnings.Count + securityWarnings.Count));
            if (IsSecondPhaseFinished)
            {
                output.CommentLine("Number of warnings in the second phase: " + secondPhaseWarnings.Count);
            }
        }

        public void GenerateWarnings(OutputBase output)
        {
            output.Headline("Warnings");
            output.EmptyLine();
            output.CommentLine("Total number of warnings: " + GetNumberOfWarnings());


            if (IsFirstPhaseFinished)
            {
                output.CommentLine("Number of analysis warnings in the first phase: " + (analysisWarnings.Count));
                output.CommentLine("Number of security warnings in the first phase: " + (securityWarnings.Count));
            }
            if (IsSecondPhaseFinished)
            {
                output.CommentLine("Number of warnings in the second phase: " + secondPhaseWarnings.Count);
            }

            if (IsFirstPhaseFinished)
            {
                output.Warnings(analysisWarnings, "First phase analysis warnings");
                output.Warnings(securityWarnings, "First phase security warnings");
            }
            if (IsSecondPhaseFinished)
            {
                output.Warnings(secondPhaseWarnings, "Second phase analysis warnings");
            }
        }

        public void GenerateFinalSnapshotText(OutputBase output)
        {
            if (programPointGraph != null && programPointGraph.End != null)
            {
                output.ProgramPointInfo("", programPointGraph.End);
            }
            else
            {
                output.Error("End point was not reached");
            }
        }

        public void GenerateMemoryModelStatisticsOutput(OutputBase output)
        {

        }

        #endregion

        #region Watch methods

        public int GetNumberOfWarnings()
        {
            int warnings = 0;
            if (IsFirstPhaseFinished)
            {
                warnings += AnalysisWarningHandler.NumberOfWarnings;
            }

            if (IsFirstPhaseFinished)
            {
                warnings += secondPhaseWarnings.Count;
            }

            return warnings;
        }

        #endregion







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
    }
}
