using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.App.Settings;
using Weverca.Taint;

namespace Weverca.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Analyser currentAnalyser;

        private static readonly int MEMORY_COUTER_LIMIT = 4;

        int memoryCounter = 0;

        private string fileName;
        private SecondPhaseType secondPhaseType = SecondPhaseType.TaintAnalysis;
        private MemoryModelType memoryModelType = MemoryModelType.TrackingCopyAlgorithms;
        private LoggingOutputType loggingOutputType = LoggingOutputType.GuiOnly;
        private LoggingStrategyType loggingStrategyType = LoggingStrategyType.Deactivated;
        private long memoryLimit;

        Stopwatch watch;
        DispatcherTimer timer = new DispatcherTimer();
        private AnalysisState lastState;
        private bool isFirstPhaseStartNotReported;
        private bool isFirstPhaseEndNotReported;
        private bool isSecondPhaseStartNotReported;
        private bool isSecondPhaseEndNotReported;
        FlowDocumentOutput analysisOutput;

        //BackgroundWorker worker;
        private Thread currentAnalysisThred;

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_Tick;

            analysisOutput = new FlowDocumentOutput(outputFlowDocument);

            showStartAnalysisDialog();            
        }


        public void StartAnalysis()
        {
            isFirstPhaseStartNotReported = true;
            isFirstPhaseEndNotReported = true;
            isSecondPhaseStartNotReported = true;
            isSecondPhaseEndNotReported = true;

            memoryLimitText.Content = getMemoryText(memoryLimit);

            outputTab.IsSelected = true;
            warningsTab.Visibility = System.Windows.Visibility.Collapsed;
            finalSnapshotTab.Visibility = System.Windows.Visibility.Collapsed;
            statsTab.Visibility = System.Windows.Visibility.Collapsed;

            memoryText.Content = getMemoryText(GC.GetTotalMemory(true));

            abortButton.IsEnabled = true;

            currentAnalyser = new Analyser();
            currentAnalyser.FileName = fileName;
            currentAnalyser.SecondPhaseType = secondPhaseType;
            
            watch = Stopwatch.StartNew();
            timer.IsEnabled = true;

            ThreadPool.QueueUserWorkItem(startAnalyserMainMethod);

            reportAnalysisStart();
        }

        private void startAnalyserMainMethod(object sender)
        {
            currentAnalysisThred = System.Threading.Thread.CurrentThread;
            currentAnalyser.StartAnalysis();
        }

        private void analysisFinished()
        {
            watch.Stop();
            timer.IsEnabled = false;
            memoryText.Content = getMemoryText(GC.GetTotalMemory(true));

            warningsTab.Visibility = System.Windows.Visibility.Visible;
            FlowDocumentOutput warningsOutput = new FlowDocumentOutput(warningsFlowDocument);
            warningsOutput.ClearDocument();
            currentAnalyser.GenerateWarnings(warningsOutput);

            finalSnapshotTab.Visibility = System.Windows.Visibility.Visible;
            FlowDocumentOutput finalSnapshotOutput = new FlowDocumentOutput(finalSnapshotFlowDocument);
            finalSnapshotOutput.ClearDocument();
            currentAnalyser.GenerateFinalSnapshotText(finalSnapshotOutput);

            statsTab.Visibility = System.Windows.Visibility.Visible;
            FlowDocumentOutput statsOutput = new FlowDocumentOutput(statsFlowDocument);
            statsOutput.ClearDocument();
            currentAnalyser.GenerateMemoryModelStatisticsOutput(statsOutput);

            abortButton.IsEnabled = false;
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentAnalyser != null && !currentAnalyser.IsFinished)
            {
                if (currentAnalysisThred != null)
                {
                    reportEvent("Abort request - terminating the analysis");
                    currentAnalysisThred.Abort(AnalysisEndState.Abort);
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timeText.Content = watch.Elapsed.ToString(@"hh\:mm\:ss");

            actualizeState();
            actualizeMemory();
            actualizeWarnings();
            actualizeProgramPoints();
            actualizeProgress();

            if (isFirstPhaseStartNotReported && currentAnalyser.IsFirstPhaseStarted)
            {
                isFirstPhaseStartNotReported = false;
                reportFirstPhaseStart();
            }

            if (isFirstPhaseEndNotReported && currentAnalyser.IsFirstPhaseFinished)
            {
                isFirstPhaseEndNotReported = false;
                reportFirstPhaseEnd();
            }

            if (isSecondPhaseStartNotReported && currentAnalyser.IsSecondPhaseStarted)
            {
                isSecondPhaseStartNotReported = false;
                reportSecondPhaseStart();
            }

            if (isSecondPhaseEndNotReported && currentAnalyser.IsSecondPhaseFinished)
            {
                isSecondPhaseEndNotReported = false;
                reportSecondPhaseEnd();
            }
            
            if (currentAnalyser.IsFinished)
            {
                switch (currentAnalyser.EndState)
                {
                    case AnalysisEndState.Success:
                        reportAnalysisEnd();
                        break;
                    case AnalysisEndState.Crash:
                        reportAnalysisCrash();
                        break;
                    case AnalysisEndState.Abort:
                        reportAnalysisAbort();
                        break;
                    case AnalysisEndState.AbortMemory:
                        reportAnalysisAbortMemory();
                        break;
                }

                analysisFinished();
            }
        }

        private void actualizeState()
        {
            if (currentAnalyser.State != lastState)
            {
                lastState = currentAnalyser.State;
                switch (currentAnalyser.State)
                {
                    case AnalysisState.Initialising:
                        phaseText.Content = "Initialising";
                        break;

                    case AnalysisState.ForwardAnalysis:
                        phaseText.Content = "First phase";
                        break;

                    case AnalysisState.NextPhaseAnalysis:
                        phaseText.Content = "Second phase";
                        break;
                }
            }
        }

        private void actualizeMemory()
        {            
            // Report memory every 5 sec
            if (memoryCounter == 0)
            {
                memoryCounter = MEMORY_COUTER_LIMIT;
                long memoryConsumption = GC.GetTotalMemory(false);
                memoryText.Content = getMemoryText(memoryConsumption);
                if (memoryConsumption > memoryLimit)
                {
                    //reportEvent("Memory limit reached - trying to garbage collect");
                    //memoryConsumption = GC.GetTotalMemory(true);
                    if (memoryConsumption > memoryLimit && currentAnalyser != null && !currentAnalyser.IsFinished)
                    {
                        if (currentAnalysisThred != null)
                        {
                            // try to garbage collect
                            memoryConsumption = GC.GetTotalMemory(true);
                            reportEvent("Memory limit reached - terminating the analysis");
                            currentAnalysisThred.Abort(AnalysisEndState.AbortMemory);
                        }
                    }

                }

            }

            memoryCounter--;
        }

        private void actualizeWarnings()
        {
            numOfWarningsText.Content = currentAnalyser.GetNumberOfWarnings();
        }

        private void actualizeProgramPoints()
        {
        }

        private void actualizeProgress()
        {
        }

        private void reportAnalysisStart()
        {
            analysisOutput.ClearDocument();
            analysisOutput.Headline("Analysing");
            analysisOutput.EmptyLine();
            reportEvent("Analysis started");

            analysisStateText.Content = "Analysis is running";
        }

        private void reportFirstPhaseStart()
        {
            reportEvent("First phase started");
        }

        private void reportFirstPhaseEnd()
        {
            reportEvent("First phase finshed");
        }

        private void reportSecondPhaseStart()
        {
            reportEvent("Second phase started");
        }

        private void reportSecondPhaseEnd()
        {
            reportEvent("Second phase finished");
        }

        private void reportAnalysisEnd()
        {
            analysisStateText.Content = "Analysis is completed";

            reportEvent("Analysis finished");
            currentAnalyser.GenerateOutput(analysisOutput);
        }

        private void reportAnalysisCrash()
        {
            analysisStateText.Content = "Analysis crashed";

            reportEvent("Analysis crashed");

            analysisOutput.EmptyLine();
            currentAnalyser.GenerateOutput(analysisOutput);

            analysisOutput.EmptyLine();
            analysisOutput.Headline("Crash report");
            analysisOutput.Error(currentAnalyser.AnalysisException.ToString());
        }

        private void reportAnalysisAbort()
        {
            analysisStateText.Content = "Analysis aborted by user request";

            reportEvent("Analysis aborted by user request");

            analysisOutput.EmptyLine();
            currentAnalyser.GenerateOutput(analysisOutput);
        }
        private void reportAnalysisAbortMemory()
        {
            analysisStateText.Content = "Analysis reached memory limit";

            reportEvent("Analysis reached memory limit");

            analysisOutput.EmptyLine();
            currentAnalyser.GenerateOutput(analysisOutput);
        }

        private void reportEvent(string text)
        {
            analysisOutput.VariableLine(watch.Elapsed.ToString(@"\[hh\:mm\:ss\]"), " ", text);
        }

        private void showStartAnalysisDialog()
        {
            StartAnalysisWindow startAnalysisWindow = new StartAnalysisWindow()
            {
                FileName = fileName,
                MemoryModelType = memoryModelType,
                SecondPhaseType = secondPhaseType,
                LoggingOutputType = loggingOutputType,
                LoggingStrategyType = loggingStrategyType
            };

            if (startAnalysisWindow.ShowStartAnalysisDialog() == true)
            {
                fileName = startAnalysisWindow.FileName;
                memoryModelType = startAnalysisWindow.MemoryModelType;
                secondPhaseType = startAnalysisWindow.SecondPhaseType;
                loggingOutputType = startAnalysisWindow.LoggingOutputType;
                loggingStrategyType = startAnalysisWindow.LoggingStrategyType;
                memoryLimit = startAnalysisWindow.MemoryLimit;

                Title = string.Format("Weverca PHP analyzer - [{0}]", fileName);
                StartAnalysis();
            }
        }

        private string getMemoryText(long memorySize)
        {
            var units = new[] { "B", "KB", "MB", "GB", "TB" };
            var index = 0;
            double size = memorySize;
            while (size > 1024 && index < units.Length - 1)
            {
                size /= 1024;
                index++;
            }

            if (size / 100 > 1)
            {
                return string.Format("{0:0.0} {1}", size, units[index]);
            }
            else if (size / 10 > 1)
            {
                return string.Format("{0:0.00} {1}", size, units[index]);
            }
            else
            {
                return string.Format("{0:0.000} {1}", size, units[index]);
            }

        }

        private int getMemoryInMB(long memorySize)
        {
            return (int)(memorySize / (1024 * 1024));
        }

        #region Menu handlers

        private bool testRunningAnalysis(string message)
        {
            if (currentAnalyser != null && !currentAnalyser.IsFinished)
            {
                MessageBox.Show("Cannot perform selected operation - wait until finish of the analysis", message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void startMenu_Click(object sender, RoutedEventArgs e)
        {
            if (testRunningAnalysis("Cannot start new analysis"))
            {
                showStartAnalysisDialog();
            }
        }

        private void repeateMenu_Click(object sender, RoutedEventArgs e)
        {
            if (testRunningAnalysis("Cannot repeat analysis"))
            {
                StartAnalysis();
            }
        }

        private void abortMenu_Click(object sender, RoutedEventArgs e)
        {
            abortButton_Click(sender, e);
        }

        private void exitMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void exportPPGMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void exportResultsMenu_Click(object sender, RoutedEventArgs e)
        {
            var content = new TextRange(warningsFlowDocument.ContentStart, warningsFlowDocument.ContentEnd);


            if (content.CanSave(DataFormats.Rtf))
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Warnings.rtf"; // Default file name
                dlg.DefaultExt = ".rtf"; // Default file extension
                dlg.Filter = "Rich-Text-Format documents (.rtf)|*.rtf"; // Filter files by extension 

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results 
                if (result == true)
                {
                    try
                    {
                        var stream = new FileStream(dlg.FileName, FileMode.OpenOrCreate);
                        content.Save(stream, DataFormats.Rtf);
                        stream.Close();
                        MessageBox.Show("The warning report saved succesfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving the results into a file (" + ex.Message + ")", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
        }

        private void aboutMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
