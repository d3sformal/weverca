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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms
{
    class LazyAssignAlgorithm : IAssignAlgorithm, IAlgorithmFactory<IAssignAlgorithm>
    {
        /// <inheritdoc />
        public IAssignAlgorithm CreateInstance()
        {
            return new LazyAssignAlgorithm();
        }

        /// <inheritdoc />
        public void Assign(Snapshot snapshot, MemoryPath path, MemoryEntry value, bool forceStrongWrite)
        {
            if (snapshot.CurrentMode == SnapshotMode.MemoryLevel)
            {
                /*if (!value.ContainsAssociativeArray)
                {
                    Snapshot.Benchmark.StartAlgorithm(snapshot, this, Logging.AlgorithmType.NEW_ASSIGN_COLLECTOR);
                    TreeIndexCollector treeCollector = new TreeIndexCollector(snapshot);
                    treeCollector.ProcessPath(path);
                    Snapshot.Benchmark.FinishAlgorithm(snapshot, this, Logging.AlgorithmType.NEW_ASSIGN_COLLECTOR);

                    Snapshot.Benchmark.StartAlgorithm(snapshot, this, Logging.AlgorithmType.NEW_ASSIGN);
                    LazyAssignWorker worker = new LazyAssignWorker(snapshot, treeCollector);
                    worker.Assign(value, forceStrongWrite);
                    Snapshot.Benchmark.FinishAlgorithm(snapshot, this, Logging.AlgorithmType.NEW_ASSIGN);
                }*/






                //if (value.ContainsAssociativeArray)
                {
                    Snapshot.Benchmark.StartAlgorithm(snapshot, this, Logging.AlgorithmType.WRITE_CREATE_TEMPORARY);
                    TemporaryIndex temporaryIndex = snapshot.CreateTemporary();
                    MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
                    mergeWorker.MergeMemoryEntry(temporaryIndex, value);
                    Snapshot.Benchmark.FinishAlgorithm(snapshot, this, Logging.AlgorithmType.WRITE_CREATE_TEMPORARY);

                    Snapshot.Benchmark.StartAlgorithm(snapshot, this, Logging.AlgorithmType.WRITE_COLLECTING);
                    AssignCollector collector = new AssignCollector(snapshot);
                    collector.ProcessPath(path);

                    if (forceStrongWrite)
                    {
                        collector.SetAllToMust();
                    }
                    Snapshot.Benchmark.FinishAlgorithm(snapshot, this, Logging.AlgorithmType.WRITE_COLLECTING);

                    Snapshot.Benchmark.StartAlgorithm(snapshot, this, Logging.AlgorithmType.WRITE_ASSIGN);
                    AssignWorker worker = new AssignWorker(snapshot);
                    worker.Assign(collector, temporaryIndex);
                    Snapshot.Benchmark.FinishAlgorithm(snapshot, this, Logging.AlgorithmType.WRITE_ASSIGN);

                    Snapshot.Benchmark.StartAlgorithm(snapshot, this, Logging.AlgorithmType.WRITE_RELEASE_TEMPORARY);
                    snapshot.ReleaseTemporary(temporaryIndex);
                    Snapshot.Benchmark.FinishAlgorithm(snapshot, this, Logging.AlgorithmType.WRITE_RELEASE_TEMPORARY);
                }
            }
            else
            {
                AssignCollector collector = new AssignCollector(snapshot);
                collector.ProcessPath(path);

                if (forceStrongWrite)
                {
                    collector.SetAllToMust();
                }

                AssignWithoutCopyWorker worker = new AssignWithoutCopyWorker(snapshot);
                worker.Assign(collector, value);
            }
        }

        /// <inheritdoc />
        public void AssignAlias(Snapshot snapshot, MemoryPath targetPath, MemoryPath sourcePath)
        {
            if (snapshot.CurrentMode == SnapshotMode.InfoLevel)
            {
                return;
            }

            //Collect alias indexes
            AssignCollector sourceCollector = new AssignCollector(snapshot);
            sourceCollector.ProcessPath(sourcePath);

            //Memory locations where to get data from
            ReadCollector valueCollector = new ReadCollector(snapshot);
            valueCollector.ProcessPath(sourcePath);

            //Get data from locations
            ReadWorker worker = new ReadWorker(snapshot);
            MemoryEntry value = worker.ReadValue(valueCollector);

            //Makes deep copy of data to prevent changes after assign alias
            TemporaryIndex temporaryIndex = snapshot.CreateTemporary();
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, value);
            
            //Memory locations to store data into
            AssignCollector targetCollector = new AssignCollector(snapshot);
            targetCollector.AliasesProcessing = AliasesProcessing.BeforeCollecting;
            targetCollector.ProcessPath(targetPath);

            AssignAliasWorker assignWorker = new AssignAliasWorker(snapshot);
            assignWorker.AssignAlias(sourceCollector, targetCollector, temporaryIndex);

            snapshot.ReleaseTemporary(temporaryIndex);
        }

        /// <inheritdoc />
        public void WriteWithoutCopy(Snapshot snapshot, MemoryPath path, MemoryEntry value)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            AssignWithoutCopyWorker worker = new AssignWithoutCopyWorker(snapshot);
            worker.Assign(collector, value);
        }
    }
}