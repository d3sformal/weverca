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
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel;

namespace Weverca.App.Benchmarking
{
    public class TransactionEntry
    {
        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public int TransactionID { get; private set; }

        /// <summary>
        /// Gets the start memory.
        /// </summary>
        /// <value>
        /// The start memory.
        /// </value>
        public long StartMemory { get; private set; }

        /// <summary>
        /// Gets the end memory.
        /// </summary>
        /// <value>
        /// The end memory.
        /// </value>
        public long EndMemory { get; private set; }

        /// <summary>
        /// Gets the transaction time.
        /// </summary>
        /// <value>
        /// The transaction time.
        /// </value>
        public double TransactionTime { get; private set; }

        /// <summary>
        /// Gets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public SnapshotMode Mode { get; private set; }
        
        private Stopwatch stopwatch;
        
        /// <summary>
        /// Creates new instance of transaction enry and starts benchmarking.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <returns>New transaction entry object</returns>
        public static TransactionEntry CreateAndStartTransaction(int transactionId)
        {
            TransactionEntry transaction = new TransactionEntry(transactionId);
            transaction.stopwatch.Start();

            return transaction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionEntry"/> class.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        private TransactionEntry(int transactionId)
        {
            this.TransactionID = transactionId;

            stopwatch = new Stopwatch();
            StartMemory = GC.GetTotalMemory(false);
        }

        /// <summary>
        /// Stops the transaction and stores the results.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <exception cref="System.Exception">Trying to stop the same transaction twice</exception>
        public void StopTransaction(Snapshot snapshot)
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();

                TransactionTime = stopwatch.Elapsed.TotalMilliseconds;
                EndMemory = GC.GetTotalMemory(false);
                Mode = snapshot.CurrentMode;
            }
            else
            {
                throw new Exception("Trying to stop the same transaction twice");
            }
        }
    }
}