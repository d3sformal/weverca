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
                    assignCreatedAliases(extendedSnapshot);
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

                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshots);
                        dataWorker.MergeData(structure);
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
                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshots);
                        dataWorker.MergeData(snapshot.Structure);

                        data = dataWorker.Data;
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

                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshots);
                        dataWorker.MergeData(structure);
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
                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshots);
                        dataWorker.MergeData(snapshot.Structure);

                        data = dataWorker.Data;
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
                        TrackingCallMergeStructureWorker structureWorker = new TrackingCallMergeStructureWorker(snapshot, callSnapshot, snapshots);
                        structureWorker.MergeStructure();
                        structure = structureWorker.Structure;

                        TrackingCallMergeDataWorker dataWorker = new TrackingCallMergeDataWorker(snapshot, callSnapshot, snapshots);
                        dataWorker.MergeData(structure);
                        data = dataWorker.Data;

                        localLevel = structure.Readonly.CallLevel;
                        structure.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                        structure.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_MERGE);

                        data.Writeable.WriteableChangeTracker.SetCallLevel(localLevel);
                        data.Writeable.WriteableChangeTracker.SetConnectionType(TrackerConnectionType.CALL_MERGE);

                        /*TrackingMergeStructureWorker structureWorker = new TrackingMergeStructureWorker(snapshot, snapshots);
                        structureWorker.SetParentSnapshot(callSnapshot);
                        structureWorker.SetOnlyparentStackContexts();
                        structureWorker.MergeStructure();
                        structureWorker.StoreLocalArays();
                        structure = structureWorker.Structure;

                        TrackingMergeDataWorker dataWorker = new TrackingMergeDataWorker(snapshot, snapshots);
                        dataWorker.SetParentSnapshot(callSnapshot);
                        dataWorker.MergeData(structure);
                        data = dataWorker.Data;*/

                        /*MergeWorker worker = new MergeWorker(snapshot, snapshots, true);
                        worker.Merge();

                        structure = worker.Structure;
                        data = worker.Data;*/
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        MergeInfoWorker worker = new MergeInfoWorker(snapshot, snapshots, true);
                        worker.Merge();

                        structure = worker.Structure;
                        data = worker.Infos;
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

        private void assignCreatedAliases(Snapshot snapshot)
        {
            /*
            if (snapshot.AssignInfo != null)
            {
                List<Tuple<MemoryIndex, HashSet<Value>>> valuesToAssign = new List<Tuple<MemoryIndex, HashSet<Value>>>();

                foreach (var item in snapshot.AssignInfo.AliasAssignModifications.Modifications)
                {
                    MemoryIndex index = item.Key;
                    MemoryIndexModification indexModification = item.Value;

                    HashSet<Value> values = new HashSet<Value>();
                    valuesToAssign.Add(new Tuple<MemoryIndex, HashSet<Value>>(index, values));

                    foreach (var datasource in indexModification.Datasources)
                    {
                        MemoryEntry entry;
                        
                        ISnapshotDataProxy infos;
                        if (snapshot == datasource.SourceSnapshot)
                        {
                            infos = data;
                        }
                        else
                        {
                            infos = datasource.SourceSnapshot.Infos;
                        }

                        if (infos.Readonly.TryGetMemoryEntry(datasource.SourceIndex, out entry))
                        {
                            CollectionTools.AddAll(values, entry.PossibleValues);
                        }
                    }
                }

                foreach (var item in valuesToAssign)
                {
                    MemoryIndex index = item.Item1;
                    HashSet<Value> values = item.Item2;

                    MemoryEntry entry = new MemoryEntry(values);
                    data.Writeable.SetMemoryEntry(index, entry);
                }
            }*/

            foreach (IMemoryAlias aliasData in snapshot.CreatedAliases)
            {
                MemoryEntry entry = data.Readonly.GetMemoryEntry(aliasData.SourceIndex);
                foreach (MemoryIndex mustAlias in aliasData.MustAliases)
                {
                    if (mustAlias != null)
                    {
                        data.Writeable.SetMemoryEntry(mustAlias, entry);
                    }
                }

                foreach (MemoryIndex mayAlias in aliasData.MayAliases)
                {
                    if (mayAlias != null)
                    {
                        MemoryEntry aliasEntry = data.Readonly.GetMemoryEntry(mayAlias);
                        HashSet<Value> values = new HashSet<Value>(aliasEntry.PossibleValues);
                        CollectionTools.AddAll(values, entry.PossibleValues);
                        data.Writeable.SetMemoryEntry(mayAlias, snapshot.CreateMemoryEntry(values));
                    }
                }
            }
        }
    }
}
