using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Weverca.App.Settings;

namespace Weverca.App
{
    /// <summary>
    /// Interaction logic for StartAnalysisWindow.xaml
    /// 
    /// Represents a dialog window with options for starting the analysis.
    /// </summary>
    public partial class StartAnalysisWindow : Window
    {
        /// <summary>
        /// Gets or sets the name of the file with the PHP source code.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the type of the second phase.
        /// </summary>
        /// <value>
        /// The type of the second phase.
        /// </value>
        public SecondPhaseType SecondPhaseType { get; set; }

        /// <summary>
        /// Gets or sets the type of the memory model.
        /// </summary>
        /// <value>
        /// The type of the memory model.
        /// </value>
        public MemoryModelType MemoryModelType { get; set; }

        /// <summary>
        /// Gets or sets the memory limit. Analysis will be terminated when this liit will be exceeded.
        /// </summary>
        /// <value>
        /// The memory limit.
        /// </value>
        public long MemoryLimit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an analysis should perform a benchmark.
        /// </summary>
        /// <value>
        /// <c>true</c> if an analysis should perform a benchmark; otherwise, <c>false</c>.
        /// </value>
        public bool IsBenchmarkEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions of the analysis. Valud only if IsBenchmarkEnabled is set to true.
        /// </summary>
        /// <value>
        /// The number of repetitions.
        /// </value>
        public int NumberOfRepetitions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StartAnalysisWindow"/> class.
        /// </summary>
        public StartAnalysisWindow()
        {
            InitializeComponent();

            IsBenchmarkEnabled = false;
            NumberOfRepetitions = 10;
        }

        /// <summary>
        /// Shows the start analysis dialog.
        /// </summary>
        /// <returns>Returns true when the start analysis button was clicked.</returns>
        public bool? ShowStartAnalysisDialog()
        {
            if (FileName != null && FileName != string.Empty)
            {
                fileNameText.Text = FileName;
            }

            secondPhaseCombo.SelectedIndex = (int)SecondPhaseType;
            memoryModelCombo.SelectedIndex = (int)MemoryModelType;
            benchmarkCheck.IsChecked = IsBenchmarkEnabled;
            repetitionsText.Text = NumberOfRepetitions.ToString();
            repetitionsText.IsEnabled = IsBenchmarkEnabled;

            // sets the default value for memory limit
            // the default value is 70% of installed ram, which works nicely with 4GB...
            memoryLimitSlider.Value = Math.Log(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / (1024*1024) * 0.7);
            
            return base.ShowDialog();
        }

        private void browseFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".php",
                Filter = "PHP source file (*.php)|*.php|Text file (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Analysed file"
            };

            if (dlg.ShowDialog() == true)
            {
                fileNameText.Text = dlg.FileName;
            }
        }

        private void startAnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            if (testFileName())
            {
                bool isValid = true;
                bool isBenchmarkEnabled = benchmarkCheck.IsChecked.GetValueOrDefault(false);
                int repetitions = 10;
                if (isBenchmarkEnabled)
                {
                    if (!int.TryParse(repetitionsText.Text, out repetitions) || repetitions < 1)
                    {
                        MessageBox.Show("Number of repetitions has to be integer greater than one.\n\nPlease select valid number of repetitions.", "Analysis can not be started", MessageBoxButton.OK, MessageBoxImage.Error);
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    FileName = fileNameText.Text;
                    SecondPhaseType = (SecondPhaseType)((ComboBoxItem)secondPhaseCombo.SelectedItem).Tag;
                    MemoryModelType = (MemoryModelType)((ComboBoxItem)memoryModelCombo.SelectedItem).Tag;
                    IsBenchmarkEnabled = isBenchmarkEnabled;
                    NumberOfRepetitions = repetitions;
                    MemoryLimit = computeMemoryLimit(memoryLimitSlider.Value) * 1024 * 1024;

                    this.DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Selected file does not exist.\n\nPlease select valid PHP source file.", "Analysis can not be started", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private bool testFileName()
        {
            string fileName = fileNameText.Text;
            if (fileName != null && fileName != string.Empty)
            {
                return System.IO.File.Exists(fileName);
            }
            else
            {
                return false;
            }
        }

        private void memoryLimitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            memoryLimitLabel.Content = "Memory limit for analysis: " + computeMemoryLimit(memoryLimitSlider.Value) + " MB";
        }

        private long computeMemoryLimit(double sliderValue)
        {
            return (long)(Math.Exp(sliderValue));
        }

        private void benchmarkCheck_Click(object sender, RoutedEventArgs e)
        {
            repetitionsText.IsEnabled = benchmarkCheck.IsChecked.GetValueOrDefault(false);
        }

        
    }
}
