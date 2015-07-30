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
        public IAssignAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new LazyAssignMemoryAlgorithm(factories);
        }
    }

    /// <summary>
    /// Lazy implementation of the assign algorithm.
    /// 
    /// The new implementation of an assign algorithm doesn’t need to create deep copy of assigned data 
    /// to a new temporary variable. The collecting process is designed to collect all target locations 
    /// but not to do any change within the structure. New locations and implicit objects are created after 
    /// collecting process is finished so input memory entry remains unbroken until the assign is performed.
    /// </summary>
    class LazyAssignMemoryAlgorithm : AlgorithmBase, IAssignAlgorithm
    {
        public LazyAssignMemoryAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }
        /// <inheritdoc />
        public void Assign(Snapshot snapshot, MemoryPath path, MemoryEntry value, bool forceStrongWrite)
        {
            if (snapshot.AssignInfo == null)
            {
                snapshot.AssignInfo = new AssignInfo();
            }
            MemoryIndexModificationList pathModifications = snapshot.AssignInfo.GetOrCreatePathModification(path);

            // Collecting all sources of the data
            MemoryEntryCollector entryCollector = new MemoryEntryCollector(snapshot);
            entryCollector.ProcessRootMemoryEntry(value);

            // Collecting all locations where to assign into
            TreeIndexCollector treeCollector = new TreeIndexCollector(snapshot);
            treeCollector.PostProcessAliases = true;
            treeCollector.ProcessPath(path);

            // Provides an assign operation
            AssignWorker worker = new AssignWorker(Factories, snapshot, entryCollector, treeCollector, pathModifications);
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
            AliasWorker aliasWorker = new AliasWorker(Factories, snapshot, aliasSourcesCollector, snapshot.AssignInfo.AliasAssignModifications);
            aliasWorker.CollectAliases();

            // Collects target locations
            TreeIndexCollector aliasTargetCollector = new TreeIndexCollector(snapshot);
            aliasTargetCollector.ProcessPath(targetPath);

            // Creates missing target locations, create aliases and assign source data
            AssignWorker assignWorker = new AssignWorker(Factories, snapshot, aliasWorker.EntryCollector, aliasTargetCollector, snapshot.AssignInfo.AliasAssignModifications);
            assignWorker.AssignAliasesIntoCollectedIndexes = true;
            assignWorker.Assign();
        }

        /// <inheritdoc />
        public void WriteWithoutCopy(Snapshot snapshot, MemoryPath path, MemoryEntry value)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers.AssignWithoutCopyWorker worker
                = new Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers.AssignWithoutCopyWorker(Factories, snapshot);
            worker.Assign(collector, value);
        }
    }
}
