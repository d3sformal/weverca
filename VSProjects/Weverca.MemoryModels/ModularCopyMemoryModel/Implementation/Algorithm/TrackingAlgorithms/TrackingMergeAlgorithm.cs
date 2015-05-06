using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms
{
    class TrackingMergeAlgorithm : IMergeAlgorithm, IAlgorithmFactory<IMergeAlgorithm>
    {
        private ISnapshotStructureProxy structure;
        private ISnapshotDataProxy data;
        private int localLevel;

        /// <inheritdoc />
        public IMergeAlgorithm CreateInstance()
        {
            return new TrackingMergeAlgorithm();
        }

        /// <inheritdoc />
        public void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot)
        {
            switch (extendedSnapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    structure = Snapshot.SnapshotStructureFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Structure);
                    data = Snapshot.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Data);
                    localLevel = sourceSnapshot.CallLevel;
                    break;

                case SnapshotMode.InfoLevel:
                    data = Snapshot.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Infos);
                    extendedSnapshot.AssignCreatedAliases(extendedSnapshot, data);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + extendedSnapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void ExtendAsCall(Snapshot extendedSnapshot, Snapshot sourceSnapshot, ProgramPointGraph calleeProgramPoint, MemoryEntry thisObject)
        {
            switch (extendedSnapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    localLevel = calleeProgramPoint.ProgramPointGraphID;
                    structure = Snapshot.SnapshotStructureFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Structure);

                    if (!structure.Writeable.ContainsStackWithLevel(localLevel))
                    {
                        structure.Writeable.AddStackLevel(localLevel);
                    }
                    structure.Writeable.SetLocalStackLevelNumber(localLevel);
                    structure.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                    structure.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_EXTEND);

                    data = Snapshot.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Data);
                    data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                    data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_EXTEND);

                    break;

                case SnapshotMode.InfoLevel:
                    data = Snapshot.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Infos);
                    data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                    data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_EXTEND);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + extendedSnapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    {
                        TrackingMergeStructureWorker structureWorker = new TrackingMergeStructureWorker(snapshot, snapshots);
                        structureWorker.MergeStructure();
                        structure = structureWorker.Structure;

                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, structure.Readonly, snapshots);
                        dataWorker.MergeData();
                        data = dataWorker.Data;

                        localLevel = structure.Readonly.CallLevel;
                        structure.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                        structure.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.MERGE);

                        data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                        data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.MERGE);
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshot.Structure.Readonly, snapshots);
                        dataWorker.MergeInfo();
                        data = dataWorker.Data;

                        localLevel = snapshot.Structure.Readonly.CallLevel;
                        data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                        data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.MERGE);
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void MergeAtSubprogram(Snapshot snapshot, List<Snapshot> snapshots, ProgramPointBase[] extendedPoints)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    {
                        TrackingMergeStructureWorker structureWorker = new TrackingMergeStructureWorker(snapshot, snapshots);
                        structureWorker.MergeStructure();
                        structure = structureWorker.Structure;

                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, structure.Readonly, snapshots);
                        dataWorker.MergeData();
                        data = dataWorker.Data;

                        localLevel = structure.Readonly.CallLevel;
                        var structureTracker = structure.Writeable.WriteableChangeTracker;
                        structureTracker.SetCallLevel(localLevel);
                        structureTracker.SetConnectionType(TrackerConnectionType.SUBPROGRAM_MERGE);

                        var dataTracker = data.Writeable.WriteableChangeTracker;
                        dataTracker.SetCallLevel(localLevel);
                        dataTracker.SetConnectionType(TrackerConnectionType.SUBPROGRAM_MERGE);

                        for (int x = 0; x < snapshots.Count; x++)
                        {
                            Snapshot callSnapshot = (Snapshot) extendedPoints[x].OutSet.Snapshot;
                            Snapshot mergeAncestor = snapshots[x];

                            structureTracker.AddCallTracker(callSnapshot, mergeAncestor.Structure.Readonly.ReadonlyChangeTracker);
                            dataTracker.AddCallTracker(callSnapshot, mergeAncestor.Data.Readonly.ReadonlyChangeTracker);
                        }
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshot.Structure.Readonly, snapshots);
                        dataWorker.MergeInfo();
                        data = dataWorker.Data;

                        localLevel = snapshot.Structure.Readonly.CallLevel;
                        var dataTracker = data.Writeable.WriteableChangeTracker;
                        dataTracker.SetCallLevel(localLevel);
                        dataTracker.SetConnectionType(TrackerConnectionType.SUBPROGRAM_MERGE);
                        
                        for (int x = 0; x < snapshots.Count; x++)
                        {
                            Snapshot callSnapshot = (Snapshot)extendedPoints[x].OutSet.Snapshot;
                            Snapshot mergeAncestor = snapshots[x];

                            dataTracker.AddCallTracker(callSnapshot, mergeAncestor.Data.Readonly.ReadonlyChangeTracker);
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void MergeWithCall(Snapshot snapshot, Snapshot callSnapshot, List<Snapshot> snapshots)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    {
                        TrackingMergeStructureWorker structureWorker = new TrackingMergeStructureWorker(snapshot, snapshots);
                        structureWorker.CallMergeStructure(callSnapshot);
                        structure = structureWorker.Structure;

                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, structure.Readonly, snapshots);
                        dataWorker.MergeCallData(callSnapshot);
                        data = dataWorker.Data;

                        localLevel = structure.Readonly.CallLevel;
                        structure.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                        structure.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_MERGE);

                        data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                        data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_MERGE);
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshot.Structure.Readonly, snapshots);
                        dataWorker.MergeCallData(callSnapshot);
                        data = dataWorker.Data;

                        localLevel = snapshot.Structure.Readonly.CallLevel;
                        data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                        data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_MERGE);
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry)
        {
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, dataEntry);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy GetMergedStructure()
        {
            return structure;
        }

        /// <inheritdoc />
        public ISnapshotDataProxy GetMergedData()
        {
            return data;
        }

        /// <inheritdoc />
        public int GetMergedLocalLevelNumber()
        {
            return localLevel;
        }
    }
}
