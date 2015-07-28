using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.App.Benchmarking
{
    /// <summary>
    /// Contains aggregated result for the single algorithm
    /// </summary>
    public class AlgorithmAggregationEntry
    {
        /// <summary>
        /// Gets the type of the algorithm.
        /// </summary>
        /// <value>
        /// The type of the algorithm.
        /// </value>
        public AlgorithmType AlgorithmType { get; private set; }

        /// <summary>
        /// Gets the number of runs.
        /// </summary>
        /// <value>
        /// The number of runs.
        /// </value>
        public int NumberOfRuns { get; private set; }

        /// <summary>
        /// Gets the total time.
        /// </summary>
        /// <value>
        /// The total time.
        /// </value>
        public double TotalTime { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmAggregationEntry"/> class.
        /// </summary>
        /// <param name="algorithmType">Type of the algorithm.</param>
        public AlgorithmAggregationEntry(AlgorithmType algorithmType)
        {
            AlgorithmType = algorithmType;

            NumberOfRuns = 0;
            TotalTime = 0;
        }

        /// <summary>
        /// Adds an algorithm entry to the statistics
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void AlgorithmStopped(AlgorithmEntry entry) 
        {
            NumberOfRuns++;
            TotalTime += entry.AlgorithmTime;
        }
    }
}
