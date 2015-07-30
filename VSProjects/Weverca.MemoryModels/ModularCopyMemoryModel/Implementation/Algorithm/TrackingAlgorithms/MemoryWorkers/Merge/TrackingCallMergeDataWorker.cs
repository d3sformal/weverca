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
    /// Implementation of tracking merge operation to merge data back to the call point.
    /// 
    /// Implementation collects changes within the function.
    class TrackingCallMergeDataWorker : AbstractTrackingMergeWorker
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public ISnapshotDataProxy Data { get; set; }

        private IWriteableSnapshotData writeableTargetData;
        private Snapshot callSnapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingCallMergeDataWorker"/> class.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <param name="targetSnapshot">The target snapshot.</param>
        /// <param name="callSnapshot">The call snapshot.</param>
        /// <param name="sourceSnapshots">The source snapshots.</param>
        public TrackingCallMergeDataWorker(ModularMemoryModelFactories factories, Snapshot targetSnapshot, Snapshot callSnapshot, List<Snapshot> sourceSnapshots)
            : base(factories, targetSnapshot, sourceSnapshots, true)
        {
            this.callSnapshot = callSnapshot;
        }

        /// <summary>
        /// Merges the data.
        /// </summary>
        /// <param name="targetStructure">The target structure.</param>
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

        /// <summary>
        /// Collects the data changes.
        /// </summary>
        private void collectDataChanges()
        {
            List<MemoryIndexTree> changes = new List<MemoryIndexTree>();
            var ancestor = callSnapshot.Data.Readonly.ReadonlyChangeTracker;
            for (int x = 0; x < snapshotContexts.Count; x++)
            {
                SnapshotContext context = snapshotContexts[x];

                MemoryIndexTree currentChanges = context.ChangedIndexesTree;
                changes.Add(currentChanges);

                collectSingleFunctionChanges(callSnapshot, context.SourceData.ReadonlyChangeTracker, currentChanges, changes);
                //ancestor = getFirstCommonAncestor(context.SourceData.ReadonlyChangeTracker, ancestor, currentChanges, changes);
            }

            if (targetSnapshot.DataCallChanges != null)
            {
                CollectionMemoryUtils.AddAll(this.changeTree, targetSnapshot.DataCallChanges);
            }

            targetSnapshot.DataCallChanges = changeTree.StoredIndexes;
        }

        /// <summary>
        /// Creates the new data.
        /// </summary>
        private void createNewData()
        {
            Data = Factories.SnapshotDataFactory.CopyInstance(callSnapshot.Data);
            writeableTargetData = Data.Writeable;
        }

        /// <summary>
        /// Creates the new operation accessor.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns>Operation accesor</returns>
        protected override TrackingMergeWorkerOperationAccessor createNewOperationAccessor(MergeOperation operation)
        {
            return new OperationAccessor(operation, targetSnapshot, writeableTargetData, targetStructure);
        }

        /// <summary>
        /// Provides custom operation
        /// </summary>
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
