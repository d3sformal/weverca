using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryPhase
{
    class CopyMergeMemoryAlgorithmFactory : IAlgorithmFactory<IMergeAlgorithm>
    {
        public IMergeAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new CopyMergeMemoryAlgorithm(factories);
        }
    }

    /// <summary>
    /// Copy implementation of the merge algorithm.
    /// 
    /// Always merges whle content of all given memory trees. Works only with an incremental call stack.
    /// </summary>
    class CopyMergeMemoryAlgorithm : AlgorithmBase, IMergeAlgorithm
    {
        public CopyMergeMemoryAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        /// <inheritdoc />
        public void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot)
        {
            ISnapshotStructureProxy structure = Factories.SnapshotStructureFactory.CopyInstance(sourceSnapshot.Structure);
            ISnapshotDataProxy data = Factories.SnapshotDataFactory.CopyInstance(sourceSnapshot.Data);

            extendedSnapshot.SetMemoryMergeResult(sourceSnapshot.CallLevel, structure, data);
        }

        /// <inheritdoc />
        public void ExtendAsCall(Snapshot extendedSnapshot, Snapshot sourceSnapshot, ProgramPointGraph calleeProgramPoint, MemoryEntry thisObject)
        {
            ISnapshotStructureProxy structure = Factories.SnapshotStructureFactory.CopyInstance(sourceSnapshot.Structure);
            ISnapshotDataProxy data = Factories.SnapshotDataFactory.CopyInstance(sourceSnapshot.Data);
            int localLevel = sourceSnapshot.CallLevel + 1;

            structure.Writeable.AddLocalLevel();

            extendedSnapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        /// <inheritdoc />
        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            int localLevel = findMaxCallLevel(snapshots);

            MergeWorker worker = new MergeWorker(Factories, snapshot, snapshots, localLevel);
            worker.Merge();

            ISnapshotStructureProxy structure = worker.Structure;
            ISnapshotDataProxy data = worker.Data;

            snapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        /// <inheritdoc />
        public void MergeAtSubprogram(Snapshot snapshot, List<Snapshot> snapshots, ProgramPointBase[] extendedPoints)
        {
            Merge(snapshot, snapshots);
        }

        /// <inheritdoc />
        public void MergeWithCall(Snapshot snapshot, Snapshot callSnapshot, List<Snapshot> snapshots)
        {
            int localLevel = callSnapshot.CallLevel;
            MergeWorker worker = new MergeWorker(Factories, snapshot, snapshots, localLevel, true);
            worker.Merge();

            ISnapshotStructureProxy structure = worker.Structure;
            ISnapshotDataProxy data = worker.Data;

            snapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        /// <inheritdoc />
        public void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry)
        {
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, dataEntry);
        }

        private int findMaxCallLevel(List<Snapshot> snapshots)
        {
            int callLevel = -1;
            foreach (var snapshot in snapshots)
            {
                if (callLevel > 0)
                {
                    if (callLevel < snapshot.CallLevel)
                    {
                        callLevel = snapshot.CallLevel;
                    }
                }
                else
                {
                    callLevel = snapshot.CallLevel;
                }
            }

            if (callLevel > 500)
            {
                throw new Exception("CallLevel level is too big - recursion is not supported");
            }

            return callLevel;
        }
    }
}
