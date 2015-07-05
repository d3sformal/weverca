using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers
{
    abstract class TrackingMergeWorkerOperationAccessor
    {
        public abstract void addSource(MergeOperationContext operationContext, IIndexDefinition sourceDefinition);
        public abstract void provideCustomOperation(MemoryIndex targetIndex);

        public abstract void provideCustomDeleteOperation(MemoryIndex targetIndex, IIndexDefinition targetDefinition);
    }

    abstract class AbstractTrackingMergeWorker
    {
        protected LinkedList<MergeOperation> operationQueue = new LinkedList<MergeOperation>();

        protected Snapshot targetSnapshot;
        protected List<Snapshot> sourceSnapshots;
        protected bool isCallMerge;

        protected List<SnapshotContext> snapshotContexts = new List<SnapshotContext>();
        protected MemoryIndexTree changeTree = new MemoryIndexTree();

        protected HashSet<QualifiedName> functionChages = new HashSet<QualifiedName>();
        protected HashSet<QualifiedName> classChanges = new HashSet<QualifiedName>();

        protected IReadOnlySnapshotStructure targetStructure;
        protected IWriteableSnapshotStructure writeableTargetStructure;

        protected bool isStructureWriteable;

        public ISnapshotStructureProxy Structure { get; set; }


        /*List<IReadOnlySnapshotStructure> sourceStructures;
        List<Snapshot> filteredSourceSnapshots;
        MemoryIndexTree structureChanges;

        public ISnapshotDataProxy Data { get; set; }
        public ISnapshotStructureProxy Structure { get; set; }

        public ISnapshotStructureProxy parentStructure;

        */

        public AbstractTrackingMergeWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots, bool isCallMerge = false)
        {
            this.targetSnapshot = targetSnapshot;
            this.sourceSnapshots = sourceSnapshots;
            this.isCallMerge = isCallMerge;
        }

        /*public void SetParentSnapshot(Snapshot parentSnapshot)
        {
            SnapshotContext context = new SnapshotContext(parentSnapshot);
            context.SourceStructure = parentSnapshot.Structure.Readonly;
            context.SourceData = parentSnapshot.CurrentData.Readonly;
            context.CallLevel = parentSnapshot.CallLevel;

            parentSnapshotContext = context;
        }*/

        protected abstract TrackingMergeWorkerOperationAccessor createNewOperationAccessor(MergeOperation operation);

        protected abstract MemoryIndex createNewTargetIndex(ITargetContainerContext targetContainerContext, string childName);

        protected abstract void deleteChild(ITargetContainerContext targetContainerContext, string childName);

        protected abstract bool MissingStacklevel(int stackLevel);

        protected void createSnapshotContexts()
        {
            foreach (Snapshot sourceSnapshot in sourceSnapshots)
            {
                SnapshotContext context = new SnapshotContext(sourceSnapshot);
                context.SourceStructure = sourceSnapshot.Structure.Readonly;
                context.SourceData = sourceSnapshot.CurrentData.Readonly;
                context.CallLevel = sourceSnapshot.CallLevel;
                context.ChangedIndexesTree = new MemoryIndexTree();

                snapshotContexts.Add(context);
            }
        }

        protected void mergeObjectDefinitions()
        {
            foreach (var objectDefinition in changeTree.ObjectTreeRoots)
            {
                ObjectValue objectValue = objectDefinition.Key;
                MemoryIndexTreeNode treeNode = objectDefinition.Value;

                IObjectDescriptor targetDescriptor;
                bool containsTargetDescriptor = targetStructure.TryGetDescriptor(objectValue, out targetDescriptor);

                bool alwaysDefined = true;
                List<ContainerContext> sourceContainers = new List<ContainerContext>();

                // Iterate sources to collect object descriptors
                foreach (SnapshotContext context in snapshotContexts)
                {
                    IObjectDescriptor descriptor;
                    if (context.SourceStructure.TryGetDescriptor(objectValue, out descriptor))
                    {
                        // Source contains object - add descriptor to sources
                        sourceContainers.Add(new ContainerContext(context, descriptor));
                        if (targetDescriptor == null)
                        {
                            // Descriptor is not in the target structure - use current as target
                            targetDescriptor = descriptor;
                        }
                    }
                    else
                    {
                        // Object is at least once undefined - weak merge
                        alwaysDefined = false;
                    }
                }

                // Merge object descriptor and prepare operations of modified fields
                if (targetDescriptor != null)
                {
                    ObjectTargetContainerContext objectContext = new ObjectTargetContainerContext(writeableTargetStructure, targetDescriptor);
                    createAndEnqueueOperations(objectContext, treeNode, sourceContainers, alwaysDefined);

                    // Save updated descriptor if instance changed or is not stored in target
                    if (isStructureWriteable)
                    {
                        IObjectDescriptor currentDescriptor = objectContext.getCurrentDescriptor();
                        if (!containsTargetDescriptor || currentDescriptor != targetDescriptor)
                        {
                            writeableTargetStructure.SetDescriptor(objectValue, currentDescriptor);
                        }
                    }
                }
            }
        }

        protected void mergeMemoryStacksRoots()
        {
            foreach (var memoryStack in changeTree.MemoryStack)
            {
                MemoryIndexTreeStackContext treeStackContext = memoryStack.Value;
                int stackLevel = memoryStack.Key;

                bool processStackLevel = true;
                if (!targetStructure.ContainsStackWithLevel(stackLevel))
                {
                    processStackLevel = MissingStacklevel(stackLevel);
                }

                if (processStackLevel)
                {
                    List<ContainerContext> variablesContainers = new List<ContainerContext>();
                    List<ContainerContext> controllsContainers = new List<ContainerContext>();
                    List<IReadonlySet<MemoryIndex>> temporaryContainers = new List<IReadonlySet<MemoryIndex>>();

                    bool alwaysDefined = true;

                    foreach (SnapshotContext context in snapshotContexts)
                    {
                        if (context.SourceStructure.ContainsStackWithLevel(stackLevel))
                        {
                            IReadonlyStackContext stackContext = context.SourceStructure.GetReadonlyStackContext(stackLevel);

                            variablesContainers.Add(new ContainerContext(context, stackContext.ReadonlyVariables));
                            controllsContainers.Add(new ContainerContext(context, stackContext.ReadonlyControllVariables));
                            //temporaryContainers.Add(stackContext.ReadonlyTemporaryVariables);

                            // TODO temporary indexes
                        }
                        else
                        {
                            alwaysDefined = false;
                        }
                    }

                    VariableTargetContainerContext variableContext = new VariableTargetContainerContext(targetStructure, writeableTargetStructure, stackLevel);
                    createAndEnqueueOperations(variableContext, treeStackContext.VariablesTreeRoot, variablesContainers, alwaysDefined);

                    ControllVariableTargetContainerContext controllContext = new ControllVariableTargetContainerContext(targetStructure, writeableTargetStructure, stackLevel);
                    createAndEnqueueOperations(controllContext, treeStackContext.ControlsTreeRoot, controllsContainers, alwaysDefined);
                }
            }
        }

        protected void processMergeOperations()
        {
            // Process operations while queue is not empty
            while (operationQueue.Count > 0)
            {
                // Dequeue next operation
                MergeOperation operation = operationQueue.First.Value;
                operationQueue.RemoveFirst();

                TrackingMergeWorkerOperationAccessor operationAccessor = createNewOperationAccessor(operation);
                if (operation.IsDeleteOperation)
                {
                    processDeleteOperation(operation, operationAccessor);
                }
                else
                {
                    processMergeOperation(operation, operationAccessor);
                }
            }
        }

        private void processMergeOperation(MergeOperation operation, TrackingMergeWorkerOperationAccessor operationAccessor)
        {
            MemoryIndex targetIndex = operation.TargetIndex;
            AssociativeArray targetArray = null;
            List<ContainerContext> sourceArrays = new List<ContainerContext>();
            bool arrayAlwaysDefined = !operation.IsUndefined;
            bool cotainsArray = false;

            // Iterate sources
            foreach (MergeOperationContext operationContext in operation.Indexes)
            {
                // Retreive source context and definition
                MemoryIndex sourceIndex = operationContext.Index;
                SnapshotContext context = operationContext.SnapshotContext;
                IIndexDefinition sourceDefinition = context.SourceStructure.GetIndexDefinition(sourceIndex);

                // Provide custom operation for merge algorithm
                operationAccessor.addSource(operationContext, sourceDefinition);

                // Source array
                if (sourceDefinition.Array != null)
                {
                    // Becomes target array when not set
                    if (targetArray == null && sourceIndex.Equals(targetIndex))
                    {
                        targetArray = sourceDefinition.Array;
                    }
                    cotainsArray = true;

                    // Save source array to merge descriptors
                    IArrayDescriptor descriptor = context.SourceStructure.GetDescriptor(sourceDefinition.Array);
                    sourceArrays.Add(new ContainerContext(context, descriptor, operationContext.OperationType));

                    // Equeue all array indexes when whole subtree should be merged
                    if (operationContext.OperationType == MergeOperationType.WholeSubtree)
                    {
                        foreach (var index in descriptor.Indexes)
                        {
                            operation.TreeNode.GetOrCreateChild(index.Key);
                        }
                        operation.TreeNode.GetOrCreateAny();
                    }
                }
                else
                {
                    // Source do not contain array - at least one source is empty
                    arrayAlwaysDefined = false;
                }
            }

            IIndexDefinition targetDefinition;
            IArrayDescriptor targetArrayDescriptor = null;
            if (targetStructure.TryGetIndexDefinition(targetIndex, out targetDefinition))
            {
                // Index is set in target snapshot
                if (targetDefinition.Array != null)
                {
                    // Target contains array - continue merging
                    targetArray = targetDefinition.Array;
                    targetArrayDescriptor = targetStructure.GetDescriptor(targetArray);
                }
            }
            else
            {
                // Index is not set in target snapshot - create it
                if (isStructureWriteable)
                {
                    writeableTargetStructure.NewIndex(targetIndex);
                }
                else
                {
                    throw new Exception("Error merging structure in readonly mode - undefined index " + targetIndex);
                }
            }

            // Provide custom operation for merge algorithm
            operationAccessor.provideCustomOperation(targetIndex);

            // Process next array
            if (cotainsArray)
            {
                if (targetArray == null)
                {
                    targetArray = targetSnapshot.CreateArray();
                }

                if (targetArrayDescriptor == null)
                {
                    // Target does not contain array - create and add new in target snapshot
                    if (isStructureWriteable)
                    {
                        targetArrayDescriptor = targetSnapshot.MemoryModelFactory.StructuralContainersFactories.ArrayDescriptorFactory.CreateArrayDescriptor(writeableTargetStructure, targetArray, targetIndex);
                        writeableTargetStructure.SetDescriptor(targetArray, targetArrayDescriptor);
                        writeableTargetStructure.NewIndex(targetArrayDescriptor.UnknownIndex);
                        writeableTargetStructure.SetArray(targetIndex, targetArray);
                    }
                    else
                    {
                        throw new Exception("Error merging structure in readonly mode - target descriptor for " + targetIndex);
                    }
                }

                // Create context and merge descriptors
                var arrayContext = new ArrayTargetContainerContext(writeableTargetStructure, targetArrayDescriptor);
                createAndEnqueueOperations(arrayContext, operation.TreeNode, sourceArrays, arrayAlwaysDefined);

                if (isStructureWriteable)
                {
                    // Ubdate current descriptor when changed
                    IArrayDescriptor currentDescriptor = arrayContext.getCurrentDescriptor();
                    if (currentDescriptor != targetArrayDescriptor)
                    {
                        writeableTargetStructure.SetDescriptor(targetArray, currentDescriptor);
                    }
                }
            }
        }

        private void processDeleteOperation(MergeOperation operation, TrackingMergeWorkerOperationAccessor operationAccessor)
        {
            MemoryIndex targetIndex = operation.TargetIndex;
            IIndexDefinition targetDefinition;
            if (targetStructure.TryGetIndexDefinition(targetIndex, out targetDefinition))
            {

                // Index is set in target snapshot
                if (targetDefinition.Array != null)
                {
                    // Target contains array - continue deleting
                    AssociativeArray targetArray = targetDefinition.Array;
                    IArrayDescriptor targetArrayDescriptor = targetStructure.GetDescriptor(targetArray);

                    foreach (var index in targetArrayDescriptor.Indexes)
                    {
                        // Enqueue delete operation for every child index
                        MemoryIndex childIndex = index.Value;
                        MergeOperation childOperation = new MergeOperation();
                        childOperation.SetTargetIndex(childIndex);
                        childOperation.SetDeleteOperation();
                        operationQueue.AddLast(childOperation);
                    }

                    // Enqueue delete operation for unknown index
                    MergeOperation unknownOperation = new MergeOperation();
                    unknownOperation.SetTargetIndex(targetArrayDescriptor.UnknownIndex);
                    unknownOperation.SetUndefined();
                    unknownOperation.SetDeleteOperation();
                    operationQueue.AddLast(unknownOperation);
                }

                operationAccessor.provideCustomDeleteOperation(targetIndex, targetDefinition);
            }
        }

        protected IReadonlyChangeTracker<T> getFirstCommonAncestor<T>(
            IReadonlyChangeTracker<T> trackerA,
            IReadonlyChangeTracker<T> trackerB,
            MemoryIndexTree currentChanges, List<MemoryIndexTree> changes
            ) where T : class
        {
            bool swapped = false;
            while (trackerA != trackerB)
            {
                if (trackerA == null || trackerB != null && trackerA.TrackerId < trackerB.TrackerId)
                {
                    var swap = trackerA;
                    trackerA = trackerB;
                    trackerB = swap;

                    swapped = true;
                }

                CollectionMemoryUtils.AddAll(currentChanges, trackerA.IndexChanges);
                CollectionMemoryUtils.AddAll(this.changeTree, trackerA.IndexChanges);

                CollectionMemoryUtils.AddAllIfNotNull(functionChages, trackerA.FunctionChanges);
                CollectionMemoryUtils.AddAllIfNotNull(classChanges, trackerA.ClassChanges);

                if (swapped)
                {
                    foreach (MemoryIndexTree tree in changes)
                    {
                        CollectionMemoryUtils.AddAll(tree, trackerA.IndexChanges);
                    }
                }

                trackerA = trackerA.PreviousTracker;
            }

            return trackerA;
        }

        protected void collectSingleFunctionChanges<T>(
            Snapshot callSnapshot, IReadonlyChangeTracker<T> tracker,
            MemoryIndexTree currentChanges, List<MemoryIndexTree> changes)

            where T : class
        {
            int functionCallLevel = tracker.CallLevel;

            bool done = false;
            while (!done && tracker != null && tracker.CallLevel == functionCallLevel)
            {
                if (tracker.ConnectionType != TrackerConnectionType.SUBPROGRAM_MERGE)
                {
                    done = tracker.ConnectionType == TrackerConnectionType.CALL_EXTEND;

                    CollectionMemoryUtils.AddAll(currentChanges, tracker.IndexChanges);
                    CollectionMemoryUtils.AddAll(this.changeTree, tracker.IndexChanges);

                    CollectionMemoryUtils.AddAllIfNotNull(functionChages, tracker.FunctionChanges);
                    CollectionMemoryUtils.AddAllIfNotNull(classChanges, tracker.ClassChanges);

                    tracker = tracker.PreviousTracker;
                }
                else
                {
                    IReadonlyChangeTracker<T> callTracker;
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

        private void createAndEnqueueOperations(
            ITargetContainerContext targetContainerContext, 
            MemoryIndexTreeNode treeNode,
            List<ContainerContext> sourceContainers, 
            bool alwaysDefined)
        {

            IReadonlyIndexContainer targetContainer = targetContainerContext.getSourceContainer();

            // Creates and enques merge operations for all child nodes of given node
            foreach (var childNode in treeNode.ChildNodes)
            {
                string childName = childNode.Key;
                MemoryIndexTreeNode childTreeNode = childNode.Value;

                MergeOperation operation = new MergeOperation();
                bool isChildDefined = collectIndexes(childName, sourceContainers, operation);

                if (isChildDefined)
                {
                    // Child is defined at least in one collection - enqueue merge operation

                    MemoryIndex targetIndex;
                    // Use index from target collection or crete and add it to the target collection
                    if (!targetContainer.TryGetIndex(childName, out targetIndex))
                    {
                        targetIndex = createNewTargetIndex(targetContainerContext, childName);

                        if (targetIndex == null)
                        {
                            continue;
                        }
                    }

                    // Set parameters and add it to collection
                    operation.TreeNode = childTreeNode;
                    operation.SetTargetIndex(targetIndex);

                    if (!alwaysDefined)
                    {
                        operation.SetUndefined();
                    }

                    operationQueue.AddLast(operation);
                }
                else
                {
                    // Child is not defined - enqueue delete operation

                    MemoryIndex targetIndex;
                    if (targetContainer.TryGetIndex(childName, out targetIndex))
                    {
                        // Enque delete operation only if target index exists in paret snapshot
                        operation.TreeNode = childTreeNode;
                        operation.SetTargetIndex(targetIndex);
                        operation.SetDeleteOperation();
                        operationQueue.AddLast(operation);

                        deleteChild(targetContainerContext, childName);
                    }
                }

            }

            // Enqueue merge operation for unknown index if is defined
            if (treeNode.AnyChild != null)
            {
                MergeOperation unknownOperation = new MergeOperation();

                foreach (ContainerContext containerContext in sourceContainers)
                {
                    unknownOperation.Add(new MergeOperationContext(
                        containerContext.IndexContainer.UnknownIndex, containerContext.SnapshotContext));
                }

                unknownOperation.TreeNode = treeNode.AnyChild;
                unknownOperation.SetTargetIndex(targetContainer.UnknownIndex);
                unknownOperation.SetUndefined();

                operationQueue.AddLast(unknownOperation);
            }
        }

        private bool collectIndexes(string childName, List<ContainerContext> sourceContainers, MergeOperation operation)
        {
            bool childDefined = false;

            // Collect source indexes from source collection
            foreach (ContainerContext containerContext in sourceContainers)
            {
                MemoryIndex sourceIndex;
                if (containerContext.IndexContainer.TryGetIndex(childName, out sourceIndex))
                {
                    // Collection contains field - use it
                    operation.Add(new MergeOperationContext(sourceIndex, containerContext.SnapshotContext));
                    childDefined = true;
                }
                else
                {
                    // Collection do not contain - use unknown index as source
                    // When unknown index is the source - all subtree has to be merged into
                    operation.Add(
                        new MergeOperationContext(
                            containerContext.IndexContainer.UnknownIndex,
                            containerContext.SnapshotContext,
                            MergeOperationType.WholeSubtree)
                        );
                    operation.SetUndefined();
                }
            }

            return childDefined;
        }
    }
}
