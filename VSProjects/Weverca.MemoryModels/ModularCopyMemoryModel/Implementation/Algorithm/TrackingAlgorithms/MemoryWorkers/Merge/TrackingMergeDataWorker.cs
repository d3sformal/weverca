using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    class TrackingMergeDataWorker : AbstractValueVisitor
    {
        public ISnapshotDataProxy Data { get; set; }
        public ModularMemoryModelFactories Factories { get; set; }

        private Snapshot targetSnapshot;
        private IReadOnlySnapshotStructure targetStructure;

        private List<Snapshot> sourceSnapshots;
        private IWriteableSnapshotData writeableTargetData;

        private HashSet<MemoryIndex> indexChanges = new HashSet<MemoryIndex>();
        private HashSet<Value> values = new HashSet<Value>();

        public TrackingMergeDataWorker(ModularMemoryModelFactories factories, Snapshot targetSnapshot, IReadOnlySnapshotStructure targetStructure, List<Snapshot> sourceSnapshots)
        {
            this.targetSnapshot = targetSnapshot;
            this.sourceSnapshots = sourceSnapshots;
            this.targetStructure = targetStructure;

            Factories = factories;

            if (targetSnapshot.MergeInfo == null)
            {
                targetSnapshot.MergeInfo = new MergeInfo();
            }
        }

        public void MergeData()
        {
            IReadonlyChangeTracker<IReadOnlySnapshotData> commonAncestor = collectDataChangesAndFindAncestor();
            createNewDataFromCommonAncestor(commonAncestor);

            foreach (MemoryIndex memoryIndex in indexChanges)
            {
                mergeMemoryIndexData(memoryIndex);
            }
        }

        public void MergeCallData(Snapshot callSnapshot)
        {
            collectDataChangesFromCallSnapshot(callSnapshot);
            createNewDataFromCallSnapshot(callSnapshot);

            foreach (MemoryIndex memoryIndex in indexChanges)
            {
                mergeMemoryIndexData(memoryIndex);
            }
        }

        public void MergeInfo()
        {
            IReadonlyChangeTracker<IReadOnlySnapshotData> commonAncestor = collectDataChangesAndFindAncestor();
            createNewDataFromCommonAncestor(commonAncestor);

            foreach (MemoryIndex memoryIndex in indexChanges)
            {
                mergeMemoryIndexInfo(memoryIndex);
            }
        }

        public void MergeCallInfo(Snapshot callSnapshot)
        {
            collectDataChangesFromCallSnapshot(callSnapshot);
            createNewDataFromCallSnapshot(callSnapshot);

            foreach (MemoryIndex memoryIndex in indexChanges)
            {
                mergeMemoryIndexInfo(memoryIndex);
            }
        }


        private IReadonlyChangeTracker<IReadOnlySnapshotData> collectDataChangesAndFindAncestor()
        {
            CollectionMemoryUtils.AddAll(indexChanges, targetSnapshot.MergeInfo.GetIndexes());

            IReadonlyChangeTracker<IReadOnlySnapshotData> ancestor = sourceSnapshots[0].CurrentData.Readonly.ReadonlyChangeTracker;
            for (int x = 1; x < sourceSnapshots.Count; x++)
            {
                Snapshot sourceSnapshot = sourceSnapshots[x];
                ancestor = getFirstCommonAncestor(sourceSnapshot.CurrentData.Readonly.ReadonlyChangeTracker, ancestor);
            }
            return ancestor;
        }

        private void collectDataChangesFromCallSnapshot(Snapshot callSnapshot)
        {
            CollectionMemoryUtils.AddAll(indexChanges, targetSnapshot.MergeInfo.GetIndexes());

            for (int x = 0; x < sourceSnapshots.Count; x++)
            {
                Snapshot sourceSnapshot = sourceSnapshots[x];
                collectSingleFunctionChanges(callSnapshot, sourceSnapshot.CurrentData.Readonly.ReadonlyChangeTracker);
            }
        }

        private void createNewDataFromCommonAncestor(IReadonlyChangeTracker<IReadOnlySnapshotData> commonAncestor)
        {
            Data = Factories.SnapshotDataFactory.CreateNewInstanceWithData(commonAncestor.Container);
            writeableTargetData = Data.Writeable;

            writeableTargetData.ReinitializeTracker(commonAncestor.Container);
        }

        private void createNewDataFromCallSnapshot(Snapshot callSnapshot)
        {
            Data = Factories.SnapshotDataFactory.CopyInstance(callSnapshot.CurrentData);
            writeableTargetData = Data.Writeable;

            writeableTargetData.ReinitializeTracker(callSnapshot.CurrentData.Readonly);
        }








        private void mergeMemoryIndexData(MemoryIndex targetIndex)
        {
            IIndexDefinition definition;
            if (targetStructure.TryGetIndexDefinition(targetIndex, out definition))
            {
                bool valuesAlwaysDefined = collectValues(targetIndex);

                if (definition.Array != null)
                {
                    values.Add(definition.Array);
                }
                if (!valuesAlwaysDefined)
                {
                    values.Add(targetSnapshot.UndefinedValue);
                }

                MemoryEntry entry = targetSnapshot.CreateMemoryEntry(values);
                writeableTargetData.SetMemoryEntry(targetIndex, entry);

                values.Clear();
            }
            else
            {
                // Target index is not part of the structure - remove it
                writeableTargetData.RemoveMemoryEntry(targetIndex);
            }
        }

        private void mergeMemoryIndexInfo(MemoryIndex targetIndex)
        {
            IIndexDefinition definition;
            if (targetStructure.TryGetIndexDefinition(targetIndex, out definition))
            {
                bool valuesAlwaysDefined = collectValues(targetIndex);

                MemoryEntry entry = targetSnapshot.CreateMemoryEntry(values);
                writeableTargetData.SetMemoryEntry(targetIndex, entry);

                values.Clear();
            }
            else
            {
                // Target index is not part of the structure - remove it
                writeableTargetData.RemoveMemoryEntry(targetIndex);
            }
        }

        private bool collectValues(MemoryIndex targetIndex)
        {
            bool valuesAlwaysDefined = true;
            foreach (Snapshot sourceSnapshot in sourceSnapshots)
            {
                MemoryEntry sourceEntry = getSourceEntry(targetIndex, sourceSnapshot);
                if (sourceEntry != null)
                {
                    if (sourceEntry.ContainsAssociativeArray)
                    {
                        VisitMemoryEntry(sourceEntry);
                    }
                    else
                    {
                        CollectionMemoryUtils.AddAll(values, sourceEntry.PossibleValues);
                    }
                }
                else
                {
                    valuesAlwaysDefined = false;
                }
            }

            return valuesAlwaysDefined;
        }

        private MemoryEntry getSourceEntry(MemoryIndex targetIndex, Snapshot sourceSnapshot)
        {
            IReadOnlySnapshotData data = sourceSnapshot.CurrentData.Readonly;
            MemoryEntry sourceEntry = null;

            if (!data.TryGetMemoryEntry(targetIndex, out sourceEntry))
            {
                MemoryIndex sourceIndex;
                if (targetSnapshot.MergeInfo[targetIndex].TryGetDatasource(sourceSnapshot, out sourceIndex))
                {
                    data.TryGetMemoryEntry(sourceIndex, out sourceEntry);
                }
            }

            return sourceEntry;
        }

        #region Collecting changes

        protected IReadonlyChangeTracker<IReadOnlySnapshotData> getFirstCommonAncestor(
            IReadonlyChangeTracker<IReadOnlySnapshotData> trackerA,
            IReadonlyChangeTracker<IReadOnlySnapshotData> trackerB
            )
        {
            while (trackerA != trackerB)
            {
                if (trackerA == null || trackerB != null && trackerA.TrackerId < trackerB.TrackerId)
                {
                    var swap = trackerA;
                    trackerA = trackerB;
                    trackerB = swap;
                }

                CollectionMemoryUtils.AddAll(indexChanges, trackerA.IndexChanges);
                trackerA = trackerA.PreviousTracker;
            }

            return trackerA;
        }

        protected void collectSingleFunctionChanges(
            Snapshot callSnapshot, IReadonlyChangeTracker<IReadOnlySnapshotData> tracker)
        {
            int functionCallLevel = tracker.CallLevel;

            bool done = false;
            while (!done && tracker != null && tracker.CallLevel == functionCallLevel)
            {
                if (tracker.ConnectionType != TrackerConnectionType.SUBPROGRAM_MERGE)
                {
                    done = tracker.ConnectionType == TrackerConnectionType.CALL_EXTEND;

                    CollectionMemoryUtils.AddAll(indexChanges, tracker.IndexChanges);
                    tracker = tracker.PreviousTracker;
                }
                else
                {
                    IReadonlyChangeTracker<IReadOnlySnapshotData> callTracker;
                    if (tracker.TryGetCallTracker(callSnapshot, out callTracker))
                    {
                        tracker = callTracker;
                    }
                    else
                    {
                        tracker = tracker.PreviousTracker;
                    }
                }
            }
        }

        #endregion

        #region Value Visitor

        public override void VisitValue(Value value)
        {
            values.Add(value);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            // filter out associative array
        }

        #endregion
    }
}
