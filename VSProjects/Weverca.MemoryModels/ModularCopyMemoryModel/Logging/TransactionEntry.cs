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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    public class TransactionEntry
    {
        public int TransactionID { get; private set; }

        public int NumberfLocations { get; private set; }

        public long StartMemory { get; private set; }

        public long EndMemory { get; private set; }

        public double TransactionTime { get; private set; }

        public SnapshotMode Mode { get; private set; }
        

        private Stopwatch stopwatch;


        public static TransactionEntry CreateAndStartTransaction(int transactionId)
        {
            TransactionEntry transaction = new TransactionEntry(transactionId);
            transaction.stopwatch.Start();

            return transaction;
        }


        private TransactionEntry(int transactionId)
        {
            this.TransactionID = transactionId;

            stopwatch = new Stopwatch();
            StartMemory = GC.GetTotalMemory(false);
        }

        public void StopTransaction(Snapshot snapshot)
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();

                NumberfLocations = snapshot.NumMemoryLocations();
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