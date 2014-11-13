using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers
{
    abstract class TrackingMergeWorkerOperationAccessor
    {
        public abstract void addSource(MergeOperationContext operationContext, IIndexDefinition sourceDefinition);
        public abstract void provideCustomOperation(MemoryIndex targetIndex);
    }

    abstract class AbstractTrackingMergeWorker
    {
        protected LinkedList<MergeOperation> operationQueue = new LinkedList<MergeOperation>();

        protected Snapshot targetSnapshot;
        protected List<Snapshot> sourceSnapshots;
        protected bool isCallMerge;

        protected List<SnapshotContext> snapshotContexts = new List<SnapshotContext>();
        protected SnapshotContext parentSnapshotContext;
        protected MemoryIndexTree changeTree = new MemoryIndexTree();

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

        protected abstract TrackingMergeWorkerOperationAccessor createNewOperationAccessor(MergeOperation operation);

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

        protected void selectParentSnapshot()
        {
            foreach (SnapshotContext context in snapshotContexts)
            {
                if (context.CallLevel == targetSnapshot.CallLevel)
                {
                    if (parentSnapshotContext == null
                    || context.ChangedIndexesTree.Count > parentSnapshotContext.ChangedIndexesTree.Count)
                    {
                        parentSnapshotContext = context;
                    }
                }
                else
                {
                    throw new NotImplementedException("Current implementation of merge can only handle same stack level of source snapshots.");
                }
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
                    ObjectTargetContainerContext objectContext = new ObjectTargetContainerContext(targetDescriptor);
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

                if (targetSnapshot.CallLevel >= stackLevel)
                {
                    List<ContainerContext> variablesContainers = new List<ContainerContext>();
                    List<ContainerContext> controllsContainers = new List<ContainerContext>();
                    List<IReadonlySet<MemoryIndex>> temporaryContainers = new List<IReadonlySet<MemoryIndex>>();

                    bool alwaysDefined = true;

                    foreach (SnapshotContext context in snapshotContexts)
                    {
                        if (context.SourceStructure.CallLevel >= stackLevel)
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

                // Index information
                MemoryIndex targetIndex = operation.TargetIndex;

                // Target array information
                AssociativeArray targetArray = null;
                List<ContainerContext> sourceArrays = new List<ContainerContext>();
                bool arrayAlwaysDefined = ! operation.IsUndefined;

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
                        if (targetArray == null) {
                            targetArray = sourceDefinition.Array;
                        }

                        // Save source array to merge descriptors
                        IArrayDescriptor descriptor = context.SourceStructure.GetDescriptor(sourceDefinition.Array);
                        sourceArrays.Add(new ContainerContext(context, descriptor, operationContext.OperationType));

                        // Equeue all array indexes when whole subtree should be merged
                        if (operationContext.OperationType == MergeOperationType.WholeSubtree) {
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
                if (targetArray != null)
                {
                    if (targetArrayDescriptor == null)
                    {
                        // Target does not contain array - create and add new in target snapshot
                        if (isStructureWriteable)
                        {
                            targetArrayDescriptor = Structure.CreateArrayDescriptor(targetArray, targetIndex);
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
                    var arrayContext = new ArrayTargetContainerContext(targetArrayDescriptor);
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
        }

        protected IReadonlyChangeTracker<MemoryIndex, T> getFirstCommonAncestor<T>(
            IReadonlyChangeTracker<MemoryIndex, T> trackerA,
            IReadonlyChangeTracker<MemoryIndex, T> trackerB,
            MemoryIndexTree currentChanges, List<MemoryIndexTree> changes)
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

                CollectionTools.AddAll(currentChanges, trackerA.ChangedValues);
                CollectionTools.AddAll(this.changeTree, trackerA.ChangedValues);

                if (swapped)
                {
                    foreach (MemoryIndexTree tree in changes)
                    {
                        CollectionTools.AddAll(tree, trackerA.ChangedValues);
                    }
                }

                trackerA = trackerA.PreviousTracker;
            }

            return trackerA;
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

                // Child is defined at least in one collection - enqueue operation
                if (childDefined)
                {
                    MemoryIndex targetIndex;

                    // Use index from target collection or crete and add it to the target collection
                    if (!targetContainer.TryGetIndex(childName, out targetIndex))
                    {
                        if (isStructureWriteable)
                        {
                            targetIndex = targetContainerContext.createMemoryIndex(childName);
                            targetContainerContext.getWriteableSourceContainer().AddIndex(childName, targetIndex);
                        }
                        else
                        {
                            throw new Exception("Error merging structure in readonly mode - adding new index into collection: " + targetIndex + ", index: " + childName);
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

        /*public void MergeStructure()
        {
            sourceStructures = new List<IReadOnlySnapshotStructure>();
            structureChanges = new MemoryIndexTree();

            var changeTrackers = new List<IReadonlyChangeTracker<MemoryIndex, IReadOnlySnapshotStructure>>();
            foreach (Snapshot sourceSnapshot in sourceSnapshots)
            {
                var sourceStructure = sourceSnapshot.Structure.Readonly;
                changeTrackers.Add(sourceStructure.IndexChangeTracker);

                if (sourceSnapshot.CallLevel == targetSnapshot.CallLevel && parentStructure == null)
                {
                    parentStructure = sourceSnapshot.Structure;
                }
                else
                {
                    sourceStructures.Add(sourceStructure);
                    filteredSourceSnapshots.Add(sourceSnapshot);
                }
            }

            var ancestor = getFirstCommonAncestor(changeTrackers, structureChanges);

            Structure = Snapshot.SnapshotStructureFactory.CopyInstance(targetSnapshot, parentStructure);

            mergeStructureRoot();
        }

        private void mergeStructureRoot()
        {
            foreach (var stack in structureChanges.MemoryStack)
            {
                int level = stack.Key;
                MemoryIndexTreeStackContext stackContext = stack.Value;
                
                List<IReadonlyIndexContainer> variables = new List<IReadonlyIndexContainer>();
                List<IReadonlyIndexContainer> controlls = new List<IReadonlyIndexContainer>();
                List<IReadonlySet<MemoryIndex>> temporary = new List<IReadonlySet<MemoryIndex>>();
                List<Snapshot> snapshots = new List<Snapshot>();

                int structurePos = 0;
                foreach (var sourceStructure in sourceStructures)
                {
                    if (sourceStructure.CallLevel < level)
                    {
                        var sourceContext = sourceStructure.GetReadonlyStackContext(level);
                        variables.Add(sourceContext.ReadonlyVariables);
                        controlls.Add(sourceContext.ReadonlyControllVariables);
                        temporary.Add(sourceContext.ReadonlyTemporaryVariables);

                        snapshots.Add(filteredSourceSnapshots[structurePos]);
                    }
                    structurePos++;
                }

                var targetContext = Structure.Writeable.GetWriteableStackContext(level);

                mergeVariables(targetContext.WriteableVariables, variables, snapshots, stackContext.VariablesTreeRoot);
                mergeVariables(targetContext.WriteableControllVariables, controlls, snapshots, stackContext.ControlsTreeRoot);
                mergeTemporary(targetContext.WriteableTemporaryVariables, temporary, snapshots, stackContext.TemporaryTreeRoot);
            }

            foreach (var objectRoot in structureChanges.ObjectTreeRoots)
            {

            }
        }

        private void mergeVariables(
            IWriteableIndexContainer targetVariables, 
            IEnumerable<IReadonlyIndexContainer> sourceVariables, 
            List<Snapshot> snapshots,
            MemoryIndexTreeNode treeNode)
        {
            foreach (var variable in treeNode.ChildNodes)
            {
                string name = variable.Key;
                MemoryIndexTreeNode variableNode = variable.Value;

                MergeOperation operation = new MergeOperation();
                operation.CurrentNode = variableNode;
                operation.IsRoot = true;

                int containerIndex = 0;
                foreach (IReadonlyIndexContainer sourceContainer in sourceVariables)
                {
                    MemoryIndex sourceIndex;
                    if (sourceContainer.TryGetIndex(name, out sourceIndex))
                    {
                        operation.Add(sourceIndex, snapshots[containerIndex]);
                        operation.IsDefined = true;
                        operation.TargetIndex = sourceIndex;
                    }
                    else
                    {
                        operation.Add(sourceContainer.UnknownIndex, snapshots[containerIndex]);
                        operation.IsUndefined = true;
                    }

                    containerIndex++;
                }

                MemoryIndex targetIndex;
                if (targetVariables.TryGetIndex(name, out targetIndex))
                {
                    operation.TargetIndex = targetIndex;
                    operation.IsDefined = true;

                    operationStack.AddLast(operation);
                }
                else if (operation.IsDefined)
                {
                    operation.IsUndefined = true;

                    targetVariables.AddIndex(name, operation.TargetIndex);

                    operationStack.AddLast(operation);
                }
            }

            if (treeNode.AnyChild == null)
            {
                MergeOperation anyOperation = new MergeOperation();
                anyOperation.CurrentNode = treeNode.AnyChild;
                anyOperation.IsRoot = true;
                anyOperation.TargetIndex = targetVariables.UnknownIndex;
                anyOperation.IsDefined = true;
                anyOperation.IsUndefined = true;

                int containerIndex = 0;
                foreach (IReadonlyIndexContainer sourceContainer in sourceVariables)
                {
                    anyOperation.Add(sourceContainer.UnknownIndex, snapshots[containerIndex]);
                    containerIndex++;
                }

                operationStack.AddLast(anyOperation);
            }
        }

        private void mergeTemporary(
            IWriteableSet<MemoryIndex> writeableSet,
            List<IReadonlySet<MemoryIndex>> temporary,
            List<Snapshot> snapshots,
            MemoryIndexTreeNode memoryIndexTreeNode)
        {
            throw new NotImplementedException();
        }









        private IReadonlyChangeTracker<T, C> getFirstCommonAncestor<T, C>(
            IReadonlyChangeTracker<T, C> trackerA,
            IReadonlyChangeTracker<T, C> trackerB,
            ICollection<T> changes)
        {
            while (trackerA != trackerB)
            {
                if (trackerA == null || trackerB != null && trackerA.TrackerId < trackerB.TrackerId)
                {
                    var swap = trackerA;
                    trackerA = trackerB;
                    trackerB = swap;
                }

                CollectionTools.AddAll(changes, trackerA.AddedOrModifiedValues);
                trackerA = trackerA.PreviousTracker;
            }

            return trackerA;
        }

        private IReadonlyChangeTracker<T, C> getFirstCommonAncestor<T, C>(
            List<IReadonlyChangeTracker<T, C>> trackers,
            ICollection<T> changes)
        {
            IReadonlyChangeTracker<T, C> ancestor = trackers[0];
            for (int x = 1; x < trackers.Count; x++)
            {
                ancestor = getFirstCommonAncestor(trackers[x], ancestor, changes);
            }

            return ancestor;
        }*/


    }
}
