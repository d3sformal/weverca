/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


//#define MEMORY_BENCHMARK
#define BENCHMARK

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
        Measuring currentTransaction;
        Snapshot currentSnapshot;
        //Dictionary<Snapshot, Measuring> transactions = new Dictionary<Snapshot, Measuring>();
        Dictionary<AlgorithmKey, Measuring> runningAlgorithms = new Dictionary<AlgorithmKey, Measuring>();
        Dictionary<AlgorithmType, AlgorithmEntry> algorithms = new Dictionary<AlgorithmType, AlgorithmEntry>();

        private int transactionStarts;
        private int transactionStops;
        private double transactionTime;
        private int algorithmStarts;
        private int algorithmStops;
        private double algorithmTime;
        private int initializations;
        private double transactionMemory;
        private double algorithmMemory;

        public IReadOnlyDictionary<AlgorithmType, AlgorithmEntry> AlgorithmResults { get { return algorithms; } }

        public int TransactionStarts { get { return transactionStarts; } }

        public int TransactionStops { get { return transactionStops; } }

        public double TransactionTime { get { return transactionTime; } }

        public int AlgorithmStarts { get { return algorithmStarts; } }

        public int AlgorithmStops { get { return algorithmStops; } }

        public double AlgorithmTime { get { return algorithmTime; } }

        public int Initializations { get { return initializations; } }

        public double TransactionMemory { get { return transactionMemory; } }

        public double AlgorithmMemory { get { return algorithmMemory; } }

        public void InitializeSnapshot(Snapshot snapshot)
        {
#if BENCHMARK
            initializations++;
#endif
        }

        public void StartTransaction(Snapshot snapshot)
        {
#if BENCHMARK

            if (currentSnapshot == null)
            {
                Measuring transaction = new Measuring();
                transaction.MemoryStart = getMemoryUssage();
                transaction.Stopwatch.Start();
                transactionStarts++;

                currentSnapshot = snapshot;
                currentTransaction = transaction;
            }
            else
            {
                FinishTransaction(snapshot);
                //throw new Exception("");
            }

#endif
        }

        public void FinishTransaction(Snapshot snapshot)
        {
#if BENCHMARK
            double usedMemory = getMemoryUssage();

            if (this.currentSnapshot == snapshot)
            {
                Measuring transaction = currentTransaction;
                transaction.Stopwatch.Stop();
                transactionStops++;
                transactionTime += transaction.Stopwatch.Elapsed.TotalMilliseconds;
                transactionMemory += usedMemory - transaction.MemoryStart;
            }
            else
            {
                //throw new Exception("Snapshot transactions does not match");
            }

            this.currentSnapshot = null;
            currentTransaction = null;
#endif
        }

        public void StartAlgorithm(Snapshot snapshot, IAlgorithm algorithmInstance, AlgorithmType algorithmType)
        {
#if BENCHMARK
            AlgorithmKey key = new AlgorithmKey(algorithmType, algorithmInstance);
            Measuring algorithm = new Measuring();
            runningAlgorithms.Add(key, algorithm);

            AlgorithmEntry entry;
            if (!algorithms.TryGetValue(algorithmType, out entry))
            {
                entry = new AlgorithmEntry(algorithmType);
                algorithms.Add(algorithmType, entry);
            }

            entry.Starts = entry.Starts + 1;
            algorithmStarts++;

            algorithm.MemoryStart = getMemoryUssage();
            algorithm.Stopwatch.Start();
#endif
        }

        public void FinishAlgorithm(Snapshot snapshot, IAlgorithm algorithmInstance, AlgorithmType algorithmType)
        {
#if BENCHMARK
            double usedMemory = getMemoryUssage();

            AlgorithmKey key = new AlgorithmKey(algorithmType, algorithmInstance);
            Measuring algorithm;
            if (runningAlgorithms.TryGetValue(key, out algorithm))
            {
                algorithm.Stopwatch.Stop();
                runningAlgorithms.Remove(key);

                AlgorithmEntry entry;
                if (!algorithms.TryGetValue(algorithmType, out entry))
                {
                    entry = new AlgorithmEntry(algorithmType);
                    algorithms.Add(algorithmType, entry);
                }

                algorithmStops++;
                algorithmTime += algorithm.Stopwatch.Elapsed.TotalMilliseconds;
                algorithmMemory += (usedMemory - algorithm.MemoryStart);

                entry.Stops = entry.Stops + 1;
                entry.Time = entry.Time + algorithm.Stopwatch.Elapsed.TotalMilliseconds;
                entry.Memory = entry.Memory + (usedMemory - algorithm.MemoryStart);
            }
#endif
        }


        public void WriteResultsToFile(string benchmarkFile)
        {
#if BENCHMARK
            System.IO.File.Delete(benchmarkFile);
            using (System.IO.StreamWriter w = System.IO.File.AppendText(benchmarkFile))
            {
#if MEMORY_BENCHMARK
                string caption = "algorithm ;\t runs;\t time;\t time_avg;\t time_percentil;\t memory;\t memory_avg;\t memory_percentil";
#else
                string caption = "algorithm ;\t runs;\t time;\t time_avg;\t time_percentil";
#endif
                w.WriteLine(caption);

                foreach (var algorithm in algorithms)
                {
                    AlgorithmType type = algorithm.Key;
                    int runs = algorithm.Value.Stops;
                    double time = algorithm.Value.Time;
                    double timeAvg = time / runs;
                    double timePercentil = time / algorithmTime;
                    
#if MEMORY_BENCHMARK
                    double memory = algorithm.Value.Memory;
                    double memoryAvg = memory / runs;
                    double memoryPercentil = memory / Math.Abs(algorithmMemory);                    
                    
                    string line = string.Format("{0};\t {1};\t {2};\t {3};\t {4};\t {5};\t {6};\t {7}",
                        type, runs, time, timeAvg, timePercentil, memory, memoryAvg, memoryPercentil);
#else
                    string line = string.Format("{0};\t {1};\t {2};\t {3};\t {4}",
                        type, runs, time, timeAvg, timePercentil);
#endif



                    w.WriteLine(line);
                }
            }
#endif
        }

        private double getMemoryUssage()
        {
#if MEMORY_BENCHMARK
            double memory = GC.GetTotalMemory(true);
            return memory / 1024;
#else
            return 0;
#endif
        }

    }
}
