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
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.InfoPhase
{
    class CopyMergeInfoAlgorithmFactory : IAlgorithmFactory<IMergeAlgorithm>
    {
        public IMergeAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new CopyMergeInfoAlgorithm(factories);
        }
    }

    /// <summary>
    /// Copy implementation of the merge algorithm for info phase. Always merges whole info container. 
    /// Doesn't change structural data.
    /// </summary>
    class CopyMergeInfoAlgorithm : AlgorithmBase, IMergeAlgorithm
    {
        public CopyMergeInfoAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        /// <inheritdoc />
        public void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot)
        {
            ISnapshotDataProxy data = Factories.SnapshotDataFactory.CopyInstance(sourceSnapshot.Infos);
            extendedSnapshot.AssignCreatedAliases(extendedSnapshot, data);

            extendedSnapshot.SetInfoMergeResult(data);
        }

        /// <inheritdoc />
        public void ExtendAsCall(Snapshot extendedSnapshot, Snapshot sourceSnapshot, ProgramPointGraph calleeProgramPoint, MemoryEntry thisObject)
        {
            ISnapshotDataProxy data = Factories.SnapshotDataFactory.CopyInstance(sourceSnapshot.Infos);

            extendedSnapshot.SetInfoMergeResult(data);
        }

        /// <inheritdoc />
        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            int localLevel = findMaxCallLevel(snapshots);
            MergeInfoWorker worker = new MergeInfoWorker(Factories, snapshot, snapshots, localLevel);
            worker.Merge();

            ISnapshotDataProxy data = worker.Infos;

            snapshot.SetInfoMergeResult(data);
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
            MergeInfoWorker worker = new MergeInfoWorker(Factories, snapshot, snapshots, localLevel, true);
            worker.Merge();

            ISnapshotDataProxy data = worker.Infos;

            snapshot.SetInfoMergeResult(data);
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
                    if (callLevel != snapshot.CallLevel)
                    {
                        throw new NotImplementedException("Cannot merge snapshots with different call levels");
                    }
                }
                else
                {
                    callLevel = snapshot.CallLevel;
                }
            }

            return callLevel;
        }
    }
}
