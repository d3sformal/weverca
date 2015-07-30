using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryPhase
{
    class TrackingMergeMemoryAlgorithmFactory : IAlgorithmFactory<IMergeAlgorithm>
    {
        public IMergeAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new TrackingMergeMemoryAlgorithm(factories);
        }
    }

    /// <summary>
    /// Tracking implementation of the merge algorithm. Use a change tracker to determine 
    /// which memory location differs and should be merged. Other locations remains unchanged.
    /// 
    /// Works only with tracking structure - needs change tracker and parallel call stack.
    /// </summary>
    class TrackingMergeMemoryAlgorithm : AlgorithmBase, IMergeAlgorithm
    {
        public TrackingMergeMemoryAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        /// <inheritdoc />
        public void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot)
        {
            ISnapshotStructureProxy structure = Factories.SnapshotStructureFactory.CopyInstance(sourceSnapshot.Structure);
            ISnapshotDataProxy data = Factories.SnapshotDataFactory.CopyInstance(sourceSnapshot.Data);
            int localLevel = sourceSnapshot.CallLevel;

            extendedSnapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        /// <inheritdoc />
        public void ExtendAsCall(Snapshot extendedSnapshot, Snapshot sourceSnapshot, ProgramPointGraph calleeProgramPoint, MemoryEntry thisObject)
        {
            int localLevel = calleeProgramPoint.ProgramPointGraphID;
            ISnapshotStructureProxy structure = Factories.SnapshotStructureFactory.CopyInstance(sourceSnapshot.Structure);

            if (!structure.Writeable.ContainsStackWithLevel(localLevel))
            {
                structure.Writeable.AddStackLevel(localLevel);
            }
            structure.Writeable.SetLocalStackLevelNumber(localLevel);
            structure.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
            structure.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_EXTEND);

            ISnapshotDataProxy data = Factories.SnapshotDataFactory.CopyInstance(sourceSnapshot.Data);
            data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
            data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_EXTEND);

            extendedSnapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        /// <inheritdoc />
        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            TrackingMergeStructureWorker structureWorker = new TrackingMergeStructureWorker(Factories, snapshot, snapshots);
            structureWorker.MergeStructure();
            ISnapshotStructureProxy structure = structureWorker.Structure;

            TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(Factories, snapshot, structure.Readonly, snapshots);
            dataWorker.MergeData();
            ISnapshotDataProxy data = dataWorker.Data;

            int localLevel = structure.Readonly.CallLevel;
            structure.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
            structure.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.MERGE);

            data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
            data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.MERGE);

            snapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        /// <inheritdoc />
        public void MergeAtSubprogram(Snapshot snapshot, List<Snapshot> snapshots, ProgramPointBase[] extendedPoints)
        {
            TrackingMergeStructureWorker structureWorker = new TrackingMergeStructureWorker(Factories, snapshot, snapshots);
            structureWorker.MergeStructure();
            ISnapshotStructureProxy structure = structureWorker.Structure;

            TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(Factories, snapshot, structure.Readonly, snapshots);
            dataWorker.MergeData();
            ISnapshotDataProxy data = dataWorker.Data;

            int localLevel = structure.Readonly.CallLevel;
            var structureTracker = structure.Writeable.WriteableChangeTracker;
            structureTracker.SetCallLevel(localLevel);
            structureTracker.SetConnectionType(TrackerConnectionType.SUBPROGRAM_MERGE);

            var dataTracker = data.Writeable.WriteableChangeTracker;
            dataTracker.SetCallLevel(localLevel);
            dataTracker.SetConnectionType(TrackerConnectionType.SUBPROGRAM_MERGE);

            for (int x = 0; x < snapshots.Count; x++)
            {
                Snapshot callSnapshot = (Snapshot)extendedPoints[x].OutSet.Snapshot;
                Snapshot mergeAncestor = snapshots[x];

                structureTracker.AddCallTracker(callSnapshot, mergeAncestor.Structure.Readonly.ReadonlyChangeTracker);
                dataTracker.AddCallTracker(callSnapshot, mergeAncestor.Data.Readonly.ReadonlyChangeTracker);
            }

            snapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        /// <inheritdoc />
        public void MergeWithCall(Snapshot snapshot, Snapshot callSnapshot, List<Snapshot> snapshots)
        {
            TrackingMergeStructureWorker structureWorker = new TrackingMergeStructureWorker(Factories, snapshot, snapshots);
            structureWorker.CallMergeStructure(callSnapshot);
            ISnapshotStructureProxy structure = structureWorker.Structure;

            TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(Factories, snapshot, structure.Readonly, snapshots);
            dataWorker.MergeCallData(callSnapshot);
            ISnapshotDataProxy data = dataWorker.Data;

            int localLevel = structure.Readonly.CallLevel;
            structure.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
            structure.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_MERGE);

            data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
            data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_MERGE);

            snapshot.SetMemoryMergeResult(localLevel, structure, data);
        }

        /// <inheritdoc />
        public void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry)
        {
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, dataEntry);
        }
    }
}
