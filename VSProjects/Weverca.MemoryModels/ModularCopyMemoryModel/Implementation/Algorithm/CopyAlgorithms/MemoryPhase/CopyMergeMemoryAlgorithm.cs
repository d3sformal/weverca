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
        CopyMergeMemoryAlgorithm instance = new CopyMergeMemoryAlgorithm();

        public IMergeAlgorithm CreateInstance()
        {
            return instance;
        }
    }

    class CopyMergeMemoryAlgorithm : IMergeAlgorithm
    {
        public void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot)
        {
            ISnapshotStructureProxy structure = extendedSnapshot.MemoryModelFactory.SnapshotStructureFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Structure);
            ISnapshotDataProxy data = extendedSnapshot.MemoryModelFactory.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Data);

            extendedSnapshot.SetMemoryMergeResult(sourceSnapshot.CallLevel, structure, data);
        }

        public void ExtendAsCall(Snapshot extendedSnapshot, Snapshot sourceSnapshot, ProgramPointGraph calleeProgramPoint, MemoryEntry thisObject)
        {
            ISnapshotStructureProxy structure = extendedSnapshot.MemoryModelFactory.SnapshotStructureFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Structure);
            ISnapshotDataProxy data = extendedSnapshot.MemoryModelFactory.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Data);
            int localLevel = sourceSnapshot.CallLevel + 1;

            structure.Writeable.AddLocalLevel();

            extendedSnapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            int localLevel = findMaxCallLevel(snapshots);

            MergeWorker worker = new MergeWorker(snapshot, snapshots, localLevel);
            worker.Merge();

            ISnapshotStructureProxy structure = worker.Structure;
            ISnapshotDataProxy data = worker.Data;

            snapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        public void MergeAtSubprogram(Snapshot snapshot, List<Snapshot> snapshots, ProgramPointBase[] extendedPoints)
        {
            Merge(snapshot, snapshots);
        }

        public void MergeWithCall(Snapshot snapshot, Snapshot callSnapshot, List<Snapshot> snapshots)
        {
            int localLevel = callSnapshot.CallLevel;
            MergeWorker worker = new MergeWorker(snapshot, snapshots, localLevel, true);
            worker.Merge();

            ISnapshotStructureProxy structure = worker.Structure;
            ISnapshotDataProxy data = worker.Data;

            snapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

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
