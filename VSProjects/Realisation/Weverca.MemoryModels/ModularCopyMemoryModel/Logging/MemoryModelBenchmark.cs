using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    class MemoryModelBenchmark : IBenchmark
    {

        Dictionary<Snapshot, Stopwatch> transactions = new Dictionary<Snapshot, Stopwatch>();
        Dictionary<AlgorithmKey, Stopwatch> runningAlgorithms = new Dictionary<AlgorithmKey, Stopwatch>();
        Dictionary<AlgorithmType, AlgorithmEntry> algorithms = new Dictionary<AlgorithmType, AlgorithmEntry>();

        private int transactionStarts;
        private int transactionStops;
        private long transactionTime;
        private int algorithmStarts;
        private int algorithmStops;
        private long algorithmTime;
        private int initializations;

        public void InitializeSnapshot(Snapshot snapshot)
        {
            initializations++;
        }

        public void StartTransaction(Snapshot snapshot)
        {
            Stopwatch transaction  = new Stopwatch();
            transactions.Add(snapshot, transaction);
            transaction.Start();
            transactionStarts++;
        }

        public void FinishTransaction(Snapshot snapshot)
        {
            Stopwatch transaction;
            if (transactions.TryGetValue(snapshot, out transaction))
            {
                transaction.Stop();
                transactionStops++;
                transactionTime += transaction.ElapsedMilliseconds;

                transactions.Remove(snapshot);
            }
        }

        public void StartAlgorithm(Snapshot snapshot, IAlgorithm algorithmInstance, AlgorithmType algorithmType)
        {
            AlgorithmKey key = new AlgorithmKey(algorithmType, algorithmInstance);
            Stopwatch algorithm = new Stopwatch();
            runningAlgorithms.Add(key, algorithm);

            AlgorithmEntry entry;
            if (!algorithms.TryGetValue(algorithmType, out entry))
            {
                entry = new AlgorithmEntry(algorithmType);
                algorithms.Add(algorithmType, entry);
            }

            entry.Starts = entry.Starts + 1;
            algorithmStarts++;

            algorithm.Start();
        }

        public void FinishAlgorithm(Snapshot snapshot, IAlgorithm algorithmInstance, AlgorithmType algorithmType)
        {
            AlgorithmKey key = new AlgorithmKey(algorithmType, algorithmInstance);
            Stopwatch algorithm;
            if (runningAlgorithms.TryGetValue(key, out algorithm))
            {
                algorithm.Stop();
                runningAlgorithms.Remove(key);

                AlgorithmEntry entry;
                if (!algorithms.TryGetValue(algorithmType, out entry))
                {
                    entry = new AlgorithmEntry(algorithmType);
                    algorithms.Add(algorithmType, entry);
                }

                algorithmStops++;
                algorithmTime += algorithm.ElapsedMilliseconds;

                entry.Stops = entry.Stops + 1;
                entry.Time = entry.Time + algorithm.ElapsedMilliseconds;
            }
        }


        public void WriteResultsToFile(string benchmarkFile)
        {
            System.IO.File.Delete(benchmarkFile);
            using (System.IO.StreamWriter w = System.IO.File.AppendText(benchmarkFile))
            {
                string caption = "algorithm;runs;time;avg;percentil";
                w.WriteLine(caption);

                foreach (var algorithm in algorithms)
                {
                    AlgorithmType type = algorithm.Key;
                    int runs = algorithm.Value.Stops;
                    long total = algorithm.Value.Time;
                    double avg = (double)(total) / runs;
                    double percentil = (double) total / algorithmTime;

                    string line = string.Format("{0};{1};{2};{3};{4}",
                        type, runs, total, avg, percentil);

                    w.WriteLine(line);
                }
            }
        }
    }
}
