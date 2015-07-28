using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.App.Benchmarking
{
    /// <summary>
    /// Represents analysis statistics for the single iteration of benchmarking
    /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// Gets the transaction results.
        /// </summary>
        /// <value>
        /// The transaction results.
        /// </value>
        public IEnumerable<TransactionEntry> TransactionResults { get; private set; }

        /// <summary>
        /// Gets the algorithm results.
        /// </summary>
        /// <value>
        /// The algorithm results.
        /// </value>
        public IReadOnlyDictionary<AlgorithmType, AlgorithmAggregationEntry> AlgorithmResults { get; private set; }

        /// <summary>
        /// Gets the total operation time.
        /// </summary>
        /// <value>
        /// The total operation time.
        /// </value>
        public double TotalOperationTime { get; private set; }

        /// <summary>
        /// Gets the total algorithm time.
        /// </summary>
        /// <value>
        /// The total algorithm time.
        /// </value>
        public double TotalAlgorithmTime { get; private set; }

        /// <summary>
        /// Gets the number of operations.
        /// </summary>
        /// <value>
        /// The number of operations.
        /// </value>
        public int NumberOfOperations { get; private set; }

        /// <summary>
        /// Gets the number of algorithms.
        /// </summary>
        /// <value>
        /// The number of algorithms.
        /// </value>
        public int NumberOfAlgorithms { get; private set; }

        /// <summary>
        /// Gets the number of transactions.
        /// </summary>
        /// <value>
        /// The number of transactions.
        /// </value>
        public int NumberOfTransactions { get; private set; }

        /// <summary>
        /// Gets the total analysis time.
        /// </summary>
        /// <value>
        /// The total analysis time.
        /// </value>
        public double TotalAnalysisTime { get; private set; }

        /// <summary>
        /// Gets the initial memory.
        /// </summary>
        /// <value>
        /// The initial memory.
        /// </value>
        public long InitialMemory { get; private set; }

        /// <summary>
        /// Gets the final memory.
        /// </summary>
        /// <value>
        /// The final memory.
        /// </value>
        public long FinalMemory { get; private set; }

        private Stopwatch stopwatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="BenchmarkResult"/> class.
        /// </summary>
        public BenchmarkResult()
        {
            stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Starts the benchmarking.
        /// </summary>
        public void StartBenchmarking()
        {
            InitialMemory = GC.GetTotalMemory(true);
            stopwatch.Start();
        }

        /// <summary>
        /// Stops the benchmarking.
        /// </summary>
        /// <param name="benchmark">The benchmark.</param>
        public void StopBenchmarking(MemoryModelBenchmark benchmark)
        {
            stopwatch.Stop();

            TotalAnalysisTime = stopwatch.Elapsed.TotalMilliseconds;
            FinalMemory = GC.GetTotalMemory(true);

            TransactionResults = benchmark.TransactionResults;
            AlgorithmResults = benchmark.AlgorithmResults;

            TotalOperationTime = benchmark.TotalOperationTime;
            TotalAlgorithmTime = benchmark.TotalAlgorithmTime;

            NumberOfOperations = benchmark.NumberOfOperations;
            NumberOfAlgorithms = benchmark.NumberOfAlgorithms;
            NumberOfTransactions = benchmark.NumberOfTransactions;
        }
    }
}
