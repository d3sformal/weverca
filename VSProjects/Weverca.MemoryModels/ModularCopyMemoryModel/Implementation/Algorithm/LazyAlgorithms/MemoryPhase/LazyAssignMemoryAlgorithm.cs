using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryPhase
{
    class LazyAssignMemoryAlgorithmFactory : IAlgorithmFactory<IAssignAlgorithm>
    {
        private IAssignAlgorithm singletonIstance = new LazyAssignMemoryAlgorithm();

        public IAssignAlgorithm CreateInstance()
        {
            return singletonIstance;
        }
    }

    class LazyAssignMemoryAlgorithm : IAssignAlgorithm
    {
        /// <inheritdoc />
        public void Assign(Snapshot snapshot, MemoryPath path, MemoryEntry value, bool forceStrongWrite)
        {
            if (snapshot.AssignInfo == null)
            {
                snapshot.AssignInfo = new AssignInfo();
            }
            MemoryIndexModificationList pathModifications = snapshot.AssignInfo.GetOrCreatePathModification(path);

            MemoryEntryCollector entryCollector = new MemoryEntryCollector(snapshot);
            entryCollector.ProcessRootMemoryEntry(value);

            TreeIndexCollector treeCollector = new TreeIndexCollector(snapshot);
            treeCollector.PostProcessAliases = true;
            treeCollector.ProcessPath(path);

            AssignWorker worker = new AssignWorker(snapshot, entryCollector, treeCollector, pathModifications);
            worker.ForceStrongWrite = forceStrongWrite;
            worker.Assign();
        }

        /// <inheritdoc />
        public void AssignAlias(Snapshot snapshot, MemoryPath targetPath, MemoryPath sourcePath)
        {
            if (snapshot.AssignInfo == null)
            {
                snapshot.AssignInfo = new AssignInfo();
            }

            // Collects memory location of alias sources
            TreeIndexCollector aliasSourcesCollector = new TreeIndexCollector(snapshot);
            aliasSourcesCollector.ProcessPath(sourcePath);

            // Creates missing source locations and collect source data
            AliasWorker aliasWorker = new AliasWorker(snapshot, aliasSourcesCollector, snapshot.AssignInfo.AliasAssignModifications);
            aliasWorker.CollectAliases();

            // Collects target locations
            TreeIndexCollector aliasTargetCollector = new TreeIndexCollector(snapshot);
            aliasTargetCollector.ProcessPath(targetPath);

            // Creates missing target locations, create aliases and assign source data
            AssignWorker assignWorker = new AssignWorker(snapshot, aliasWorker.EntryCollector, aliasTargetCollector, snapshot.AssignInfo.AliasAssignModifications);
            assignWorker.AssignAliasesIntoCollectedIndexes = true;
            assignWorker.Assign();
        }

        /// <inheritdoc />
        public void WriteWithoutCopy(Snapshot snapshot, MemoryPath path, MemoryEntry value)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers.AssignWithoutCopyWorker worker
                = new Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers.AssignWithoutCopyWorker(snapshot);
            worker.Assign(collector, value);
        }
    }
}
