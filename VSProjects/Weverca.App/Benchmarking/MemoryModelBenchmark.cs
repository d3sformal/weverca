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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.App.Benchmarking
{
    /// <summary>
    /// Represents benchmarking class for the modular copy memory model.
    /// </summary>
    public class MemoryModelBenchmark : IBenchmark
    {
        /// <summary>
        /// Gets the transaction results.
        /// </summary>
        /// <value>
        /// The transaction results.
        /// </value>
        public IEnumerable<TransactionEntry> TransactionResults
        {
            get { return transactionResults.ToArray(); }
        }

        /// <summary>
        /// Gets the algorithm results.
        /// </summary>
        /// <value>
        /// The algorithm results.
        /// </value>
        public IReadOnlyDictionary<AlgorithmType, AlgorithmAggregationEntry> AlgorithmResults
        {
            get { return new Dictionary<AlgorithmType, AlgorithmAggregationEntry>(algorithmResults); }
        }

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
        /// Gets the number of algorithm calls.
        /// </summary>
        /// <value>
        /// The number of algorithm calls.
        /// </value>
        public int NumberOfAlgorithms { get; private set; }

        /// <summary>
        /// Gets the number of transactions.
        /// </summary>
        /// <value>
        /// The number of transactions.
        /// </value>
        public int NumberOfTransactions { get; private set; }

        private List<TransactionEntry> transactionResults = new List<TransactionEntry>();
        private Dictionary<AlgorithmType, AlgorithmAggregationEntry> algorithmResults = new Dictionary<AlgorithmType, AlgorithmAggregationEntry>();

        private Dictionary<Snapshot, TransactionEntry> transactions = new Dictionary<Snapshot, TransactionEntry>();
        private Dictionary<AlgorithmKey, AlgorithmEntry> algorithms = new Dictionary<AlgorithmKey, AlgorithmEntry>();

        private Stopwatch algorithmStopwatch = new Stopwatch();
        private int numberOfActiveAlgorithms;

        private Stopwatch operationStopwatch = new Stopwatch();
        private int numberOfActiveOperations;

        private int transactionCounter;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryModelBenchmark"/> class.
        /// </summary>
        public MemoryModelBenchmark()
        {
            ClearResults();
        }

        /// <summary>
        /// Clears the benchmark results.
        /// </summary>
        public void ClearResults()
        {
            transactionResults.Clear();
            algorithmResults.Clear();

            algorithmStopwatch.Reset();
            numberOfActiveAlgorithms = 0;

            operationStopwatch.Reset();
            numberOfActiveOperations = 0;

            transactionCounter = 0;

            TotalOperationTime = 0;
            TotalAlgorithmTime = 0;

            NumberOfOperations = 0;
            NumberOfTransactions = 0;
            NumberOfAlgorithms = 0;
        }

        /// <inheritdoc />
        public void InitializeSnapshot(Snapshot snapshot)
        {

        }

        /// <inheritdoc />
        public void StartTransaction(Snapshot snapshot)
        {
            if (!transactions.ContainsKey(snapshot))
            {
                TransactionEntry entry = TransactionEntry.CreateAndStartTransaction(++transactionCounter);
                transactions.Add(snapshot, entry);
            }
        }

        /// <inheritdoc />
        public void FinishTransaction(Snapshot snapshot)
        {
            TransactionEntry entry;
            if (transactions.TryGetValue(snapshot, out entry))
            {
                entry.StopTransaction(snapshot);

                transactionResults.Add(entry);
                transactions.Remove(snapshot);

                NumberOfTransactions++;
            }
        }

        /// <inheritdoc />
        public void StartAlgorithm(Snapshot snapshot, AlgorithmType algorithmType)
        {
            AlgorithmKey key = new AlgorithmKey(algorithmType, snapshot);
            if (!algorithms.ContainsKey(key))
            {
                TransactionEntry transaction;
                if (!transactions.TryGetValue(snapshot, out transaction))
                {
                    transaction = null;
                }

                AlgorithmEntry entry = AlgorithmEntry.CreateAndStartAlgorithm(algorithmType, transaction);
                algorithms.Add(key, entry);

                if (numberOfActiveAlgorithms == 0)
                {
                    algorithmStopwatch.Start();
                }
                numberOfActiveAlgorithms++;
            }
        }

        /// <inheritdoc />
        public void FinishAlgorithm(Snapshot snapshot, AlgorithmType algorithmType)
        {
            AlgorithmKey key = new AlgorithmKey(algorithmType, snapshot);
            AlgorithmEntry entry;
            if (algorithms.TryGetValue(key, out entry))
            {
                if (numberOfActiveAlgorithms > 0)
                {
                    numberOfActiveAlgorithms--;
                    if (numberOfActiveAlgorithms == 0)
                    {
                        algorithmStopwatch.Stop();
                        TotalAlgorithmTime = algorithmStopwatch.Elapsed.TotalMilliseconds;
                    }
                }

                entry.StopAlgorithm();

                AlgorithmAggregationEntry aggregation;
                if (!algorithmResults.TryGetValue(algorithmType, out aggregation))
                {
                    aggregation = new AlgorithmAggregationEntry(algorithmType);
                    algorithmResults.Add(algorithmType, aggregation);
                }

                aggregation.AlgorithmStopped(entry);
                algorithms.Remove(key);

                NumberOfAlgorithms++;
            }
        }

        /// <inheritdoc />
        public void StartOperation(Snapshot snapshot)
        {
            if (numberOfActiveOperations == 0)
            {
                operationStopwatch.Start();
            }
            numberOfActiveOperations++;
        }

        /// <inheritdoc />
        public void FinishOperation(Snapshot snapshot)
        {
            if (numberOfActiveOperations > 0)
            {
                numberOfActiveOperations--;
                NumberOfOperations++;

                if (numberOfActiveOperations == 0)
                {
                    operationStopwatch.Stop();
                    TotalOperationTime = operationStopwatch.Elapsed.TotalMilliseconds;
                }
            }
        }
    }
}
