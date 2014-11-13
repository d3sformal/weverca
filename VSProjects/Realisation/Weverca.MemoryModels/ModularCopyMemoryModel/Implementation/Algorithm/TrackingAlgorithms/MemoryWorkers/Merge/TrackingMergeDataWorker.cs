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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    class TrackingMergeDataWorker : AbstractTrackingMergeWorker
    {
        public ISnapshotDataProxy Data { get; set; }
        private IWriteableSnapshotData writeableTargetData;
        private IReadonlyChangeTracker<MemoryIndex, IReadOnlySnapshotData> commonAncestor;

        public TrackingMergeDataWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots, bool isCallMerge = false)
            : base(targetSnapshot, sourceSnapshots, isCallMerge)
        {

        }

        public void MergeData(ISnapshotStructureProxy targetStructure)
        {
            Structure = targetStructure;
            this.targetStructure = targetStructure.Readonly;

            isStructureWriteable = false;

            createSnapshotContexts();
            collectDataChanges();
            selectParentSnapshot();
            createNewData();

            mergeObjectDefinitions();
            mergeMemoryStacksRoots();

            processMergeOperations();
        }

        private void collectDataChanges()
        {
            IReadonlyChangeTracker<MemoryIndex, IReadOnlySnapshotData> ancestor = snapshotContexts[0].SourceData.IndexChangeTracker;

            List<MemoryIndexTree> changes = new List<MemoryIndexTree>();
            changes.Add(snapshotContexts[0].ChangedIndexesTree);
            for (int x = 1; x < snapshotContexts.Count; x++)
            {
                SnapshotContext context = snapshotContexts[x];
                MemoryIndexTree currentChanges = context.ChangedIndexesTree;
                changes.Add(currentChanges);

                ancestor = getFirstCommonAncestor(context.SourceData.IndexChangeTracker, ancestor, currentChanges, changes);
            }
            commonAncestor = ancestor;
        }

        private void createNewData()
        {
            Data = Snapshot.SnapshotDataFactory.CopyInstance(targetSnapshot, parentSnapshotContext.SourceSnapshot.Data);
            writeableTargetData = Data.Writeable;

            writeableTargetData.ReinitializeIndexTracker(commonAncestor.Container);
        }

        protected override TrackingMergeWorkerOperationAccessor createNewOperationAccessor(MergeOperation operation)
        {
            return new OperationAccessor(operation, targetSnapshot, writeableTargetData, targetStructure);
        }

        class OperationAccessor : TrackingMergeWorkerOperationAccessor
        {
            private MergeOperation operation;

            private HashSet<Value> values = new HashSet<Value>();
            private Snapshot targetSnapshot;
            private IWriteableSnapshotData writeableTargetData;
            private IReadOnlySnapshotStructure targetStructure;

            public OperationAccessor(MergeOperation operation, Snapshot targetSnapshot, IWriteableSnapshotData writeableTargetData, IReadOnlySnapshotStructure targetStructure)
            {
                this.operation = operation;
                this.targetSnapshot = targetSnapshot;
                this.writeableTargetData = writeableTargetData;
                this.targetStructure = targetStructure;
            }

            public override void addSource(MergeOperationContext operationContext, IIndexDefinition sourceDefinition)
            {
                MemoryEntry entry;
                if (operationContext.SnapshotContext.SourceData.TryGetMemoryEntry(operationContext.Index, out entry))
                {
                    foreach (Value value in entry.PossibleValues)
                    {
                        if (! (value is AssociativeArray))
                        {
                            values.Add(value);
                        }
                    }
                }
                else
                {
                    values.Add(targetSnapshot.UndefinedValue);
                }
            }

            public override void provideCustomOperation(MemoryIndex targetIndex)
            {
                if (operation.IsUndefined)
                {
                    values.Add(targetSnapshot.UndefinedValue);
                }

                IIndexDefinition definition;
                if (targetStructure.TryGetIndexDefinition(operation.TargetIndex, out definition))
                {
                    if (definition.Array != null)
                    {
                        values.Add(definition.Array);
                    }
                }

                writeableTargetData.SetMemoryEntry(operation.TargetIndex, targetSnapshot.CreateMemoryEntry(values));
            }
        }
    }
}
