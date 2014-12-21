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
    class TrackingCallMergeDataWorker : AbstractTrackingMergeWorker
    {
        public ISnapshotDataProxy Data { get; set; }
        private IWriteableSnapshotData writeableTargetData;
        private Snapshot callSnapshot;

        public TrackingCallMergeDataWorker(Snapshot targetSnapshot, Snapshot callSnapshot, List<Snapshot> sourceSnapshots)
            : base(targetSnapshot, sourceSnapshots, true)
        {
            this.callSnapshot = callSnapshot;
        }

        public void MergeData(ISnapshotStructureProxy targetStructure)
        {
            Structure = targetStructure;
            this.targetStructure = targetStructure.Readonly;

            isStructureWriteable = false;

            createSnapshotContexts();
            collectDataChanges();

            createNewData();

            mergeObjectDefinitions();
            mergeMemoryStacksRoots();

            processMergeOperations();
        }

        private void collectDataChanges()
        {
            List<MemoryIndexTree> changes = new List<MemoryIndexTree>();
            var ancestor = callSnapshot.Data.Readonly.ReadonlyChangeTracker;
            for (int x = 0; x < snapshotContexts.Count; x++)
            {
                SnapshotContext context = snapshotContexts[x];

                MemoryIndexTree currentChanges = context.ChangedIndexesTree;
                changes.Add(currentChanges);

                // collectSingleFunctionChanges(context.SourceData.ReadonlyChangeTracker, currentChanges, changes);
                ancestor = getFirstCommonAncestor(context.SourceData.ReadonlyChangeTracker, ancestor, currentChanges, changes);
            }
        }

        private void createNewData()
        {
            Data = Snapshot.SnapshotDataFactory.CopyInstance(targetSnapshot, callSnapshot.Data);
            writeableTargetData = Data.Writeable;
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

            public override void provideCustomDeleteOperation(MemoryIndex targetIndex, IIndexDefinition targetDefinition)
            {
                throw new Exception("Error merging structure in readonly mode - adding new index into collection: " + targetIndex);
            }
        }

        protected override MemoryIndex createNewTargetIndex(ITargetContainerContext targetContainerContext, string childName)
        {
            //throw new Exception("Error merging structure in readonly mode - adding new index into collection: " + childName);
            return null;
        }

        protected override void deleteChild(ITargetContainerContext targetContainerContext, string childName)
        {
            throw new Exception("Error merging structure in readonly mode - deleting index: " + childName);
        }

        protected override bool MissingStacklevel(int stackLevel)
        {
            return false;
        }
    }
}
