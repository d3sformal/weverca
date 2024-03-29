﻿using System;
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
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.InfoPhase
{
    class TrackingMergeInfoAlgorithmFactory : IAlgorithmFactory<IMergeAlgorithm>
    {
        public IMergeAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new TrackingMergeInfoAlgorithm(factories);
        }
    }

    /// <summary>
    /// Tracking implementation of the merge algorithm for info phase. Use a change tracker to determine 
    /// which memory location differs and should be merged. Other locations remains unchanged.
    /// 
    /// Works only with tracking structure - needs change tracker and parallel call stack.
    /// </summary>
    class TrackingMergeInfoAlgorithm : AlgorithmBase, IMergeAlgorithm
    {
        public TrackingMergeInfoAlgorithm(ModularMemoryModelFactories factories)
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
            data.Writeable.WriteableChangeTracker.SetCallLevel(extendedSnapshot.CallLevel);
            data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_EXTEND);

            extendedSnapshot.SetInfoMergeResult(data);
        }

        /// <inheritdoc />
        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(Factories, snapshot, snapshot.Structure.Readonly, snapshots);
            dataWorker.MergeInfo();
            ISnapshotDataProxy data = dataWorker.Data;

            int localLevel = snapshot.Structure.Readonly.CallLevel;
            data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
            data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.MERGE);

            snapshot.SetInfoMergeResult(data);
        }

        /// <inheritdoc />
        public void MergeAtSubprogram(Snapshot snapshot, List<Snapshot> snapshots, ProgramPointBase[] extendedPoints)
        {
            TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(Factories, snapshot, snapshot.Structure.Readonly, snapshots);
            dataWorker.MergeInfo();
            ISnapshotDataProxy data = dataWorker.Data;

            int localLevel = snapshot.Structure.Readonly.CallLevel;
            var dataTracker = data.Writeable.WriteableChangeTracker;
            dataTracker.SetCallLevel(localLevel);
            dataTracker.SetConnectionType(TrackerConnectionType.SUBPROGRAM_MERGE);

            for (int x = 0; x < snapshots.Count; x++)
            {
                Snapshot callSnapshot = (Snapshot)extendedPoints[x].OutSet.Snapshot;
                Snapshot mergeAncestor = snapshots[x];

                dataTracker.AddCallTracker(callSnapshot, mergeAncestor.Data.Readonly.ReadonlyChangeTracker);
            }

            snapshot.SetInfoMergeResult(data);
        }

        /// <inheritdoc />
        public void MergeWithCall(Snapshot snapshot, Snapshot callSnapshot, List<Snapshot> snapshots)
        {
            TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(Factories, snapshot, snapshot.Structure.Readonly, snapshots);
            dataWorker.MergeCallData(callSnapshot);
            ISnapshotDataProxy data = dataWorker.Data;

            int localLevel = snapshot.Structure.Readonly.CallLevel;
            data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
            data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_MERGE);

            snapshot.SetInfoMergeResult(data);
        }

        /// <inheritdoc />
        public void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry)
        {
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, dataEntry);
        }
    }
}
