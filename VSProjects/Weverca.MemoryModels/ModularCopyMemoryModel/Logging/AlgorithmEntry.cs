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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    public class AlgorithmEntry
    {
        public AlgorithmType AlgorithmType { get; private set; }

        public TransactionEntry Transaction { get; private set; }

        public double AlgorithmTime { get; private set; }


        private Stopwatch stopwatch;

        public static AlgorithmEntry CreateAndStartAlgorithm(AlgorithmType algorithmType, TransactionEntry transaction)
        {
            AlgorithmEntry entry = new AlgorithmEntry(algorithmType, transaction);
            entry.stopwatch.Start();

            return entry;
        }
        
        private AlgorithmEntry(AlgorithmType algorithmType, TransactionEntry transaction)
        {
            this.AlgorithmType = algorithmType;
            this.Transaction = transaction;
            stopwatch = new Stopwatch();
        }

        public void StopAlgorithm()
        {
            if (stopwatch.IsRunning)
            {
                AlgorithmTime = stopwatch.Elapsed.TotalMilliseconds;
            }
        }
    }
}