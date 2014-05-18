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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms
{
    class CopyAssignAlgorithm : IAssignAlgorithm, IAlgorithmFactory<IAssignAlgorithm>
    {
        /// <inheritdoc />
        public IAssignAlgorithm CreateInstance()
        {
            return new CopyAssignAlgorithm();
        }

        /// <inheritdoc />
        public void Assign(Snapshot snapshot, MemoryPath path, MemoryEntry value, bool forceStrongWrite)
        {
            if (snapshot.CurrentMode == SnapshotMode.MemoryLevel)
            {
                TemporaryIndex temporaryIndex = snapshot.CreateTemporary();
                MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
                mergeWorker.MergeMemoryEntry(temporaryIndex, value);

                AssignCollector collector = new AssignCollector(snapshot);
                collector.ProcessPath(path);

                if (forceStrongWrite)
                {
                    collector.SetAllToMust();
                }

                AssignWorker worker = new AssignWorker(snapshot);
                worker.Assign(collector, temporaryIndex);

                snapshot.ReleaseTemporary(temporaryIndex);
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
