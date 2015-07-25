using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.App
{
    class BenchmarkResult
    {
        public static readonly long GARBAGE_THRESHOLD = 10000000;

        public IEnumerable<TransactionEntry> TransactionResults { get; private set; }
        public IReadOnlyDictionary<AlgorithmType, AlgorithmAggregationEntry> AlgorithmResults { get; private set; }

        public double TotalOperationTime { get; private set; }
        public double TotalAlgorithmTime { get; private set; }

        public int NumberOfOperations { get; private set; }
        public int NumberOfAlgorithms { get; private set; }
        public int NumberOfTransactions { get; private set; }

        public double TotalAnalysisTime { get; private set; }

        public long InitialMemory { get; private set; }
        public long FinalMemory { get; private set; }

        private Stopwatch stopwatch;
        
        public BenchmarkResult()
        {
            stopwatch = new Stopwatch();
        }

        public void StartBenchmarking()
        {
            InitialMemory = GC.GetTotalMemory(true);
            stopwatch.Start();
        }

        public void StopBenchmarking(IBenchmark benchmark)
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
