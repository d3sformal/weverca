using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryPhase
{    
    class CopyAssignMemoryAlgorithmFactory : IAlgorithmFactory<IAssignAlgorithm>
    {
        public IAssignAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new CopyAssignMemoryAlgorithm(factories);
        }
    }

    class CopyAssignMemoryAlgorithm : AlgorithmBase, IAssignAlgorithm
    {
        public CopyAssignMemoryAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        public void Assign(Snapshot snapshot, Memory.MemoryPath path, AnalysisFramework.Memory.MemoryEntry value, bool forceStrongWrite)
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

        public void AssignAlias(Snapshot snapshot, Memory.MemoryPath targetPath, Memory.MemoryPath sourcePath)
        {
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

        public void WriteWithoutCopy(Snapshot snapshot, Memory.MemoryPath path, AnalysisFramework.Memory.MemoryEntry value)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            AssignWithoutCopyWorker worker = new AssignWithoutCopyWorker(Factories, snapshot);
            worker.Assign(collector, value);
        }
    }


}
