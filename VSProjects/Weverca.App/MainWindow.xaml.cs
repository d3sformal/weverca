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
    /// Backend part for the main window of the Weverca applications
    /// Shows progress and results of an analysis
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The number of watch cycles to skip until update a memory counter is performed
        /// </summary>
        private static readonly int MEMORY_COUTER_LIMIT = 4;

        // Current settings of the program selected by the start analysis window
        private string fileName;
        private SecondPhaseType secondPhaseType = SecondPhaseType.Deactivated;
        private MemoryModelType memoryModelType = MemoryModelType.TrackingDiff;
        private bool isBenchmarkEnabled = false;
        private int numberOfRepetions = 100;
        private long memoryLimit;

        // Analyzer object and thread where the analysis is performed
        private Analyser currentAnalyser;
        private Thread currentAnalysisThred;

        // Time to update watch informations about the analysis
        private DispatcherTimer timer = new DispatcherTimer();

        // Handler of the main program output - all analysis messages goes here
        FlowDocumentOutput analysisOutput;

        // Counts the analysis time
        private Stopwatch watch;
        // Countdown of the dispather timer ticks when to write out memory data
        private int memoryCounter = 0;

        // Last reported state of the analysis
        private AnalysisState lastState;

        // Identifies which parts of analysis were reported to the output
        private bool isFirstPhaseStartNotReported;
        private bool isFirstPhaseEndNotReported;
        private bool isSecondPhaseStartNotReported;
        private bool isSecondPhaseEndNotReported;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += timer_Tick;

            analysisOutput = new FlowDocumentOutput(outputFlowDocument);

            // Shows start dialog
            if (!showStartAnalysisDialog())
            {
                //  If user closes the dialog then nothing to do - quit
                Close();
            }        
        }

        /// <summary>
        /// Shows the start analysis dialog and starts the analysis when user accepts the dialog.
        /// </summary>
        /// <returns>Returns true when analysis was started</returns>
        private bool showStartAnalysisDialog()
        {
            StartAnalysisWindow startAnalysisWindow = new StartAnalysisWindow()
            {
                FileName = fileName,
                MemoryModelType = memoryModelType,
                SecondPhaseType = secondPhaseType,
                NumberOfRepetitions = numberOfRepetions,
                IsBenchmarkEnabled = isBenchmarkEnabled
            };

            if (startAnalysisWindow.ShowStartAnalysisDialog() == true)
            {
                fileName = startAnalysisWindow.FileName;
                memoryModelType = startAnalysisWindow.MemoryModelType;
                secondPhaseType = startAnalysisWindow.SecondPhaseType;
                numberOfRepetions = startAnalysisWindow.NumberOfRepetitions;
                isBenchmarkEnabled = startAnalysisWindow.IsBenchmarkEnabled;
                memoryLimit = startAnalysisWindow.MemoryLimit;

                if (isBenchmarkEnabled)
                {
                    phaseHead.Visibility = System.Windows.Visibility.Collapsed;
                    phaseText.Visibility = System.Windows.Visibility.Collapsed;
                    repetitionHead.Visibility = System.Windows.Visibility.Visible;
                    repetitionText.Visibility = System.Windows.Visibility.Visible;
                    numOfWarningsHead.Visibility = System.Windows.Visibility.Collapsed;
                    numOfWarningsText.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    phaseHead.Visibility = System.Windows.Visibility.Visible;
                    phaseText.Visibility = System.Windows.Visibility.Visible;
                    repetitionHead.Visibility = System.Windows.Visibility.Collapsed;
                    repetitionText.Visibility = System.Windows.Visibility.Collapsed;
                    numOfWarningsHead.Visibility = System.Windows.Visibility.Visible;
                    numOfWarningsText.Visibility = System.Windows.Visibility.Visible;
                }

                Title = string.Format("Weverca PHP analyzer - [{0}]", fileName);
                startAnalysis();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Starts the analysis with specified settings. Analysis is started in separated thread. 
        /// The Dispatcher timer is started to monitor progress of the analysis.
        /// </summary>
        private void startAnalysis()
        {
            isFirstPhaseStartNotReported = true;
            isFirstPhaseEndNotReported = true;
            isSecondPhaseStartNotReported = true;
            isSecondPhaseEndNotReported = true;

            memoryLimitText.Content = OutputUtils.GetMemoryText(memoryLimit);

            outputTab.IsSelected = true;
            warningsTab.Visibility = System.Windows.Visibility.Collapsed;
            finalSnapshotTab.Visibility = System.Windows.Visibility.Collapsed;

            memoryText.Content = OutputUtils.GetMemoryText(GC.GetTotalMemory(true));

            abortButton.IsEnabled = true;

            currentAnalyser = new Analyser(fileName, secondPhaseType, memoryModelType);
            
            watch = Stopwatch.StartNew();
            timer.IsEnabled = true;

            ThreadPool.QueueUserWorkItem(startAnalyserMainMethod);

            exportBenchmarkMenu.IsEnabled = false;

            reportAnalysisStart();
        }
        
        /// <summary>
        /// Stops monitoring and reports finish state of the analysis.
        /// </summary>
        private void analysisFinished()
        {
            watch.Stop();
            timer.IsEnabled = false;
            memoryText.Content = OutputUtils.GetMemoryText(GC.GetTotalMemory(true));

            warningsTab.Visibility = System.Windows.Visibility.Visible;
            FlowDocumentOutput warningsOutput = new FlowDocumentOutput(warningsFlowDocument);
            warningsOutput.ClearDocument();
            currentAnalyser.GenerateWarnings(warningsOutput);

            finalSnapshotTab.Visibility = System.Windows.Visibility.Visible;
            FlowDocumentOutput finalSnapshotOutput = new FlowDocumentOutput(finalSnapshotFlowDocument);
            finalSnapshotOutput.ClearDocument();
            currentAnalyser.GenerateFinalSnapshotText(finalSnapshotOutput);

            if (currentAnalyser.EndState == AnalysisEndState.Success && isBenchmarkEnabled)
            {
                exportBenchmarkMenu.IsEnabled = true;
            }

            abortButton.IsEnabled = false;
        }

        /// <summary>
        /// The main method of the analysis for the counting thread.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void startAnalyserMainMethod(object sender)
        {
            currentAnalysisThred = System.Threading.Thread.CurrentThread;

            if (isBenchmarkEnabled)
            {
                currentAnalyser.StartBenchmark(numberOfRepetions);
            }
            else
            {
                currentAnalyser.StartAnalysis();
            }
        }
        
        /// <summary>
        /// Handles the Tick event of the timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void timer_Tick(object sender, EventArgs e)
        {
            timeText.Content = watch.Elapsed.ToString(@"hh\:mm\:ss");

            if (isBenchmarkEnabled)
            {
                actualizeBenchmarkMonitoring();
            }
            else
            {
                actualizeAnalysisMonitoring();
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

                actualizeMemory();
                actualizeWarnings();

                analysisFinished();
            }
        }

        #region Output methods

        private void actualizeAnalysisMonitoring()
        {
            actualizeState();
            actualizeMemory();
            actualizeWarnings();

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
        }

        private void actualizeBenchmarkMonitoring()
        {
            actualizeMemory();
            actualizeRepetitions();
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
                memoryText.Content = OutputUtils.GetMemoryText(memoryConsumption);
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

        private void actualizeRepetitions()
        {
            if (!currentAnalyser.IsFinished)
            {
                if (currentAnalyser.RepetitionCounter <= numberOfRepetions)
                {
                    repetitionText.Content = currentAnalyser.RepetitionCounter.ToString();
                }
            }
            else
            {
                repetitionText.Content = numberOfRepetions.ToString();
            }
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

        #endregion

        #region Private helpers

        private int getMemoryInMB(long memorySize)
        {
            return (int)(memorySize / (1024 * 1024));
        }

        #endregion

        #region Button and Menu Handlers
        
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
                startAnalysis();
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

        private void exportBenchmarkMenu_Click(object sender, RoutedEventArgs e)
        {
            if (isBenchmarkEnabled)
            {
                System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "Select folder to export statistics results in CSV files."
                };

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedDirectory = dlg.SelectedPath;
                    string timePrefix = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");


                    string benchmarkStatsPath = String.Format(@"{0}\{1}_{2}[{3}-{4}].csv", selectedDirectory, timePrefix, "benchmark-stats", memoryModelType, numberOfRepetions);
                    using (System.IO.StreamWriter writer = System.IO.File.AppendText(benchmarkStatsPath))
                    {
                        currentAnalyser.WriteOutBenchmarkStats(writer);
                    }

                    string memoryMediansPath = String.Format(@"{0}\{1}_{2}[{3}-{4}].csv", selectedDirectory, timePrefix, "trans-mem-med", memoryModelType, numberOfRepetions);
                    using (System.IO.StreamWriter memoryMediansWriter = System.IO.File.AppendText(memoryMediansPath))
                    {
                        currentAnalyser.WriteOutTransactionMemoryMedians(memoryMediansWriter);
                    }

                    string algorithmTotalTimesPath = String.Format(@"{0}\{1}_{2}[{3}-{4}].csv", selectedDirectory, timePrefix, "alg-tot-time", memoryModelType, numberOfRepetions);
                    using (System.IO.StreamWriter writer = System.IO.File.AppendText(algorithmTotalTimesPath))
                    {
                        currentAnalyser.WriteOutAlgorithmTotalTimes(writer);
                    }

                    string transBenchmarkPath = String.Format(@"{0}\{1}_{2}[{3}-{4}].csv", selectedDirectory, timePrefix, "trans-benchmark", memoryModelType, numberOfRepetions);
                    using (System.IO.StreamWriter writer = System.IO.File.AppendText(transBenchmarkPath))
                    {
                        currentAnalyser.WriteOutTransactionBenchmark(writer);
                    }

                    MessageBox.Show("The benchmark stats saved succesfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Benchmark results are available only if benchmark mode is enabled.", "Error exporting results", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

    }
}
