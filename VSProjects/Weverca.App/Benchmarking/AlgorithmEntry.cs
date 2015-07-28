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
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;

namespace Weverca.App.Benchmarking
{
    /// <summary>
    /// Represents statistics for the single run of the algorithm
    /// </summary>
    public class AlgorithmEntry
    {
        /// <summary>
        /// Gets the type of the algorithm.
        /// </summary>
        /// <value>
        /// The type of the algorithm.
        /// </value>
        public AlgorithmType AlgorithmType { get; private set; }

        /// <summary>
        /// Gets the transaction.
        /// </summary>
        /// <value>
        /// The transaction.
        /// </value>
        public TransactionEntry Transaction { get; private set; }

        /// <summary>
        /// Gets the algorithm time.
        /// </summary>
        /// <value>
        /// The algorithm time.
        /// </value>
        public double AlgorithmTime { get; private set; }
        
        private Stopwatch stopwatch;

        /// <summary>
        /// Creates the the algorithm entry object and starts measuring the statistics.
        /// </summary>
        /// <param name="algorithmType">Type of the algorithm.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns>New created algorithm object.</returns>
        public static AlgorithmEntry CreateAndStartAlgorithm(AlgorithmType algorithmType, TransactionEntry transaction)
        {
            AlgorithmEntry entry = new AlgorithmEntry(algorithmType, transaction);
            entry.stopwatch.Start();

            return entry;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmEntry"/> class.
        /// </summary>
        /// <param name="algorithmType">Type of the algorithm.</param>
        /// <param name="transaction">The transaction.</param>
        private AlgorithmEntry(AlgorithmType algorithmType, TransactionEntry transaction)
        {
            this.AlgorithmType = algorithmType;
            this.Transaction = transaction;
            stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Stops the algorithm measuring.
        /// </summary>
        public void StopAlgorithm()
        {
            if (stopwatch.IsRunning)
            {
                AlgorithmTime = stopwatch.Elapsed.TotalMilliseconds;
            }
        }
    }
}