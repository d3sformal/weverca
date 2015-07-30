using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.GraphVisualizer;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    class TrackingMergeStructureWorker : IReferenceHolder
    {

        public ISnapshotStructureProxy Structure { get; set; }
        public ModularMemoryModelFactories Factories { get; set; }

        public Dictionary<MemoryIndex, MemoryAliasInfo> MemoryAliases { get; private set; }
        public Snapshot Snapshot { get { return targetSnapshot; } }

        private Snapshot targetSnapshot;
        private List<Snapshot> sourceSnapshots;

        private IReadOnlySnapshotStructure targetStructure;
        private IWriteableSnapshotStructure writeableTargetStructure;

        private List<SnapshotContext> snapshotContexts = new List<SnapshotContext>();

        private MemoryIndexTree changeTree = new MemoryIndexTree();

        private HashSet<QualifiedName> functionChages = new HashSet<QualifiedName>();
        private HashSet<QualifiedName> classChanges = new HashSet<QualifiedName>();

        private LinkedList<MergeOperation> operationQueue = new LinkedList<MergeOperation>();
        private MergeObjectsStructureWorker objectWorker;
        private MergeAliasStructureWorker aliasWorker;
        private MergeArrayStructureWorker arrayWorker;

        public TrackingMergeStructureWorker(ModularMemoryModelFactories factories, Snapshot targetSnapshot, List<Snapshot> sourceSnapshots)
        {
            MemoryAliases = new Dictionary<MemoryIndex, MemoryAliasInfo>();
            Factories = factories;

            this.targetSnapshot = targetSnapshot;
            this.sourceSnapshots = sourceSnapshots;

            if (targetSnapshot.MergeInfo == null)
            {
                targetSnapshot.MergeInfo = new MergeInfo();
            }
        }

        public void MergeStructure()
        {
            createSnapshotContexts();
            
            IReadonlyChangeTracker<IReadOnlySnapshotStructure> commonAncestor 
                = collectStructureChangesAndFindAncestor();

            createNewStructureFromCommonAncestor(commonAncestor);
            createStackLevels();

            mergeDeclarations();
            mergeObjectDefinitions();
            mergeMemoryStacksRoots();

            processMergeOperations();
            updateAliases();

            ensureTrackingChanges();
        }

        public void CallMergeStructure(Snapshot callSnapshot)
        {
            createSnapshotContexts();
            collectCallStructureChanges(callSnapshot);

            createNewStructureFromCallSnapshot(callSnapshot);

            mergeDeclarations();
            mergeObjectDefinitions();
            mergeMemoryStacksRoots();

            processMergeOperations();
            updateAliases();
            storeLocalArays();

            ensureTrackingChanges();
        }

        #region Merge methods

        private void createSnapshotContexts()
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

        #region Merge

        private IReadonlyChangeTracker<IReadOnlySnapshotStructure> collectStructureChangesAndFindAncestor()
        {
            SnapshotContext ancestorContext = snapshotContexts[0];
            IReadonlyChangeTracker<IReadOnlySnapshotStructure> ancestor = ancestorContext.SourceStructure.ReadonlyChangeTracker;

            List<MemoryIndexTree> changes = new List<MemoryIndexTree>();
            changes.Add(snapshotContexts[0].ChangedIndexesTree);
            for (int x = 1; x < snapshotContexts.Count; x++)
            {
                SnapshotContext context = snapshotContexts[x];
                MemoryIndexTree currentChanges = context.ChangedIndexesTree;
                changes.Add(currentChanges);

                ancestor = getFirstCommonAncestor(context.SourceStructure.ReadonlyChangeTracker, ancestor, currentChanges, changes);
            }

            return ancestor;
        }

        private void createNewStructureFromCommonAncestor(IReadonlyChangeTracker<IReadOnlySnapshotStructure> commonAncestor)
        {
            Structure = Factories.SnapshotStructureFactory.CreateNewInstanceWithData(commonAncestor.Container);

            writeableTargetStructure = Structure.Writeable;
            targetStructure = writeableTargetStructure;

            writeableTargetStructure.ReinitializeTracker(commonAncestor.Container);

            // Initializes merge workers with newly created structure
            arrayWorker = new MergeArrayStructureWorker(writeableTargetStructure, this);
            aliasWorker = new MergeAliasStructureWorker(writeableTargetStructure, this);
            objectWorker = new MergeObjectsStructureWorker(writeableTargetStructure, this);
        }

        private void createStackLevels()
        {
            int localLevel = -1;
            bool isLocalLevelFound = false;
            foreach (var context in sourceSnapshots)
            {
                foreach (var stack in context.Structure.Readonly.ReadonlyStackContexts)
                {
                    if (!targetStructure.ContainsStackWithLevel(stack.StackLevel))
                    {
                        writeableTargetStructure.AddStackLevel(stack.StackLevel);
                    }
                }

                if (localLevel != context.Structure.Readonly.CallLevel)
                {
                    if (!isLocalLevelFound)
                    {
                        localLevel = context.Structure.Readonly.CallLevel;
                        isLocalLevelFound = true;
                    }
                    else
                    {
                        localLevel = Snapshot.GLOBAL_CALL_LEVEL;
                    }
                }
            }
            writeableTargetStructure.SetLocalStackLevelNumber(localLevel);
            writeableTargetStructure.WriteableChangeTracker.SetCallLevel(localLevel);
        }

        #endregion

        #region Call merge

        private void collectCallStructureChanges(Snapshot callSnapshot)
        {
            List<MemoryIndexTree> changes = new List<MemoryIndexTree>();
            var ancestor = callSnapshot.Structure.Readonly.ReadonlyChangeTracker;
            for (int x = 0; x < snapshotContexts.Count; x++)
            {
                SnapshotContext context = snapshotContexts[x];

                MemoryIndexTree currentChanges = context.ChangedIndexesTree;
                changes.Add(currentChanges);

                collectSingleFunctionChanges(callSnapshot, context.SourceStructure.ReadonlyChangeTracker, currentChanges, changes);
            }

            if (targetSnapshot.StructureCallChanges != null)
            {
                CollectionMemoryUtils.AddAll(this.changeTree, targetSnapshot.StructureCallChanges);
            }

            targetSnapshot.StructureCallChanges = changeTree.StoredIndexes;
        }

        private void createNewStructureFromCallSnapshot(Snapshot callSnapshot)
        {
            Structure = Factories.SnapshotStructureFactory.CopyInstance(callSnapshot.Structure);

            writeableTargetStructure = Structure.Writeable;
            targetStructure = writeableTargetStructure;

            writeableTargetStructure.ReinitializeTracker(callSnapshot.Structure.Readonly);

            // Initializes merge workers with newly created structure
            arrayWorker = new MergeArrayStructureWorker(writeableTargetStructure, this);
            aliasWorker = new MergeAliasStructureWorker(writeableTargetStructure, this);
            objectWorker = new MergeObjectsStructureWorker(writeableTargetStructure, this);
        }

        private void storeLocalArays()
        {
            foreach (var context in snapshotContexts)
            {
                foreach (AssociativeArray array in context.SourceStructure.ReadonlyLocalContext.ReadonlyArrays)
                {
                    Structure.Writeable.AddCallArray(array, context.SourceSnapshot);
                }
            }
        }

        #endregion

        private void mergeDeclarations()
        {
            foreach (QualifiedName functionName in functionChages)
            {
                HashSet<FunctionValue> declarations = new HashSet<FunctionValue>();
                foreach (var context in snapshotContexts)
                {
                    IEnumerable<FunctionValue> decl;
                    if (context.SourceStructure.TryGetFunction(functionName, out decl))
                    {
                        CollectionMemoryUtils.AddAll(declarations, decl);
                    }
                }
                writeableTargetStructure.SetFunctionDeclarations(functionName, declarations);
            }

            foreach (QualifiedName className in classChanges)
            {
                HashSet<TypeValue> declarations = new HashSet<TypeValue>();
                foreach (var context in snapshotContexts)
                {
                    IEnumerable<TypeValue> decl;
                    if (context.SourceStructure.TryGetClass(className, out decl))
                    {
                        CollectionMemoryUtils.AddAll(declarations, decl);
                    }
                }
                writeableTargetStructure.SetClassDeclarations(className, declarations);
            }
        }

        private void mergeObjectDefinitions()
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
                    CreateAndEnqueueOperations(objectContext, treeNode, sourceContainers, alwaysDefined);

                    // Save updated descriptor if instance changed or is not stored in target
                    IObjectDescriptor currentDescriptor = objectContext.getCurrentDescriptor();
                    if (!containsTargetDescriptor || currentDescriptor != targetDescriptor)
                    {
                        writeableTargetStructure.SetDescriptor(objectValue, currentDescriptor);
                    }
                }
                // Else object is never defined - skip it
            }
        }

        private void mergeMemoryStacksRoots()
        {
            foreach (var memoryStack in changeTree.MemoryStack)
            {
                MemoryIndexTreeStackContext treeStackContext = memoryStack.Value;
                int stackLevel = memoryStack.Key;

                if (targetStructure.ContainsStackWithLevel(stackLevel))
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
                    CreateAndEnqueueOperations(variableContext, treeStackContext.VariablesTreeRoot, variablesContainers, alwaysDefined);

                    ControllVariableTargetContainerContext controllContext = new ControllVariableTargetContainerContext(targetStructure, writeableTargetStructure, stackLevel);
                    CreateAndEnqueueOperations(controllContext, treeStackContext.ControlsTreeRoot, controllsContainers, alwaysDefined);
                }
            }
        }

        private void processMergeOperations()
        {
            // Process operations while queue is not empty
            while (operationQueue.Count > 0)
            {
                // Dequeue next operation
                MergeOperation operation = operationQueue.First.Value;
                operationQueue.RemoveFirst();

                if (operation.IsDeleteOperation)
                {
                    processDeleteOperation(operation);
                }
                else
                {
                    processMergeOperation(operation);
                }
            }
        }

        private void updateAliases()
        {
            foreach (var item in MemoryAliases)
            {
                MemoryIndex targetIndex = item.Key;
                MemoryAliasInfo aliasInfo = item.Value;

                if (!aliasInfo.IsTargetOfMerge)
                {
                    IMemoryAlias currentAliases;
                    if (writeableTargetStructure.TryGetAliases(targetIndex, out currentAliases))
                    {
                        aliasInfo.Aliases.MayAliases.AddAll(currentAliases.MayAliases);
                        aliasInfo.Aliases.MustAliases.AddAll(currentAliases.MustAliases);
                    }
                }
                foreach (MemoryIndex alias in aliasInfo.RemovedAliases)
                {
                    aliasInfo.Aliases.MustAliases.Remove(alias);
                    aliasInfo.Aliases.MayAliases.Remove(alias);
                }

                writeableTargetStructure.SetAlias(targetIndex, aliasInfo.Aliases.Build(writeableTargetStructure));
            }
        }

        private void ensureTrackingChanges()
        {
            foreach (MemoryIndex index in this.changeTree.StoredIndexes)
            {
                writeableTargetStructure.WriteableChangeTracker.ModifiedIndex(index);
            }

            foreach (QualifiedName name in functionChages)
            {
                writeableTargetStructure.WriteableChangeTracker.ModifiedFunction(name);
            }

            foreach (QualifiedName name in classChanges)
            {
                writeableTargetStructure.WriteableChangeTracker.ModifiedClass(name);
            }
        }

        #endregion

        #region Collection changes

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

        #endregion

        #region Creating operations

        public void CreateAndEnqueueOperations(ITargetContainerContext targetContainerContext, MemoryIndexTreeNode treeNode,
            List<ContainerContext> sourceContainers, bool alwaysDefined)
        {
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
                    enqueueMergeOperation(childName, operation, targetContainerContext, childTreeNode, alwaysDefined);
                }
                else
                {
                    // Child is not defined - enqueue delete operation
                    enqueueDeleteOperation(childName, operation, targetContainerContext, childTreeNode);
                }
            }

            // Enqueue merge operation for unknown index if is defined
            if (treeNode.AnyChild != null)
            {
                enqueueMergeUnknownOperation(targetContainerContext, treeNode.AnyChild, sourceContainers);
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

        private void enqueueMergeOperation(string childName, MergeOperation operation, ITargetContainerContext targetContainerContext, MemoryIndexTreeNode childTreeNode, bool alwaysDefined)
        {
            IReadonlyIndexContainer targetContainer = targetContainerContext.getSourceContainer();
            MemoryIndex targetIndex;
            // Use index from target collection or crete and add it to the target collection
            if (!targetContainer.TryGetIndex(childName, out targetIndex))
            {
                targetIndex = createNewTargetIndex(targetContainerContext, childName);

                if (targetIndex == null)
                {
                    return;
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

        private void enqueueDeleteOperation(string childName, MergeOperation operation, ITargetContainerContext targetContainerContext, MemoryIndexTreeNode childTreeNode)
        {
            IReadonlyIndexContainer targetContainer = targetContainerContext.getSourceContainer();
            MemoryIndex targetIndex;
            if (targetContainer.TryGetIndex(childName, out targetIndex))
            {
                // Enque delete operation only if target index exists in paret snapshot
                operation.TreeNode = childTreeNode;
                operation.SetTargetIndex(targetIndex);
                operation.SetDeleteOperation();
                operationQueue.AddLast(operation);

                // Delete child from parent container
                targetContainerContext.getWriteableSourceContainer().RemoveIndex(childName);
            }
        }

        private void enqueueMergeUnknownOperation(ITargetContainerContext targetContainerContext, MemoryIndexTreeNode anyNode, List<ContainerContext> sourceContainers)
        {
            MergeOperation unknownOperation = new MergeOperation();

            foreach (ContainerContext containerContext in sourceContainers)
            {
                unknownOperation.Add(new MergeOperationContext(
                    containerContext.IndexContainer.UnknownIndex, containerContext.SnapshotContext));
            }

            IReadonlyIndexContainer targetContainer = targetContainerContext.getSourceContainer();
            unknownOperation.TreeNode = anyNode;
            unknownOperation.SetTargetIndex(targetContainer.UnknownIndex);
            unknownOperation.SetUndefined();

            operationQueue.AddLast(unknownOperation);
        }

        private MemoryIndex createNewTargetIndex(ITargetContainerContext targetContainerContext, string childName)
        {
            MemoryIndex targetIndex = targetContainerContext.createMemoryIndex(childName);
            targetContainerContext.getWriteableSourceContainer().AddIndex(childName, targetIndex);

            return targetIndex;
        }

        #endregion

        #region Processing operations

        #region Merge operation

        private void processMergeOperation(MergeOperation operation)
        {
            MemoryIndex targetIndex = operation.TargetIndex;
            var targetIndexDatasources = targetSnapshot.MergeInfo.GetOrCreateDatasourcesContaier(targetIndex);

            // Iterate sources
            foreach (MergeOperationContext operationContext in operation.Indexes)
            {
                // Retreive source context and definition
                MemoryIndex sourceIndex = operationContext.Index;
                SnapshotContext context = operationContext.SnapshotContext;
                IIndexDefinition sourceDefinition = context.SourceStructure.GetIndexDefinition(sourceIndex);

                // Collect array and aliases data 
                arrayWorker.collectSourceArray(targetIndex, operation, operationContext, sourceDefinition.Array);
                aliasWorker.collectSourceAliases(sourceDefinition.Aliases);
                objectWorker.collectSourceObjects(sourceDefinition.Objects);

                // Store datasource for data and info merging
                targetIndexDatasources.SetDatasource(context.SourceSnapshot, sourceIndex);
            }

            IIndexDefinition targetDefinition;
            if (targetStructure.TryGetIndexDefinition(targetIndex, out targetDefinition))
            {
                // Index is set in target snapshot
                if (targetDefinition.Array != null)
                {
                    arrayWorker.SetTargetArray(targetDefinition.Array);
                }
            }
            else
            {
                // Index is not set in target snapshot - create it
                writeableTargetStructure.NewIndex(targetIndex);
            }

            aliasWorker.MergeAliasesAndClear(targetIndex, operation);
            arrayWorker.MergeArraysAndClear(targetSnapshot, targetIndex, operation);
            objectWorker.MergeObjectsAndClear(targetIndex);
        }

        #endregion

        #region Delete operation processing

        private void processDeleteOperation(MergeOperation operation)
        {
            MemoryIndex targetIndex = operation.TargetIndex;
            IIndexDefinition targetDefinition;

            // Index is set in target snapshot
            if (targetStructure.TryGetIndexDefinition(targetIndex, out targetDefinition))
            {
                // Delete array and enqueue deletein operations if exists
                if (targetDefinition.Array != null)
                {
                    DeleteArray(targetIndex, targetDefinition.Array);
                }

                // Delete aliases if any
                if (targetDefinition.Aliases != null && targetDefinition.Aliases.HasAliases)
                {
                    deleteAliases(targetIndex, targetDefinition.Aliases);
                }

                // Removes index from target structure
                writeableTargetStructure.RemoveIndex(targetIndex);
            }
        }

        public void DeleteArray(MemoryIndex targetIndex, AssociativeArray targetArray)
        {
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

            // Deletes array from structure
            writeableTargetStructure.RemoveArray(targetIndex, targetArray);
        }

        private void deleteAliases(MemoryIndex targetIndex, IMemoryAlias targetAliases)
        {
            foreach (MemoryIndex aliasIndex in targetAliases.MustAliases)
            {
                MemoryAliasInfo aliasInfo = getAliasInfo(aliasIndex);
                aliasInfo.AddRemovedAlias(targetIndex);
            }

            foreach (MemoryIndex aliasIndex in targetAliases.MayAliases)
            {
                MemoryAliasInfo aliasInfo = getAliasInfo(aliasIndex);
                aliasInfo.AddRemovedAlias(targetIndex);
            }

        }

        #endregion

        #endregion

        #region Alias processing

        public void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            MemoryAliasInfo aliasInfo = getAliasInfo(index);
            IMemoryAliasBuilder alias = aliasInfo.Aliases;

            if (mustAliases != null)
            {
                alias.MustAliases.AddAll(mustAliases);
            }
            if (mayAliases != null)
            {
                alias.MayAliases.AddAll(mayAliases);
            }

            foreach (MemoryIndex mustIndex in alias.MustAliases)
            {
                if (alias.MayAliases.Contains(mustIndex))
                {
                    alias.MayAliases.Remove(mustIndex);
                }
            }
        }

        public void AddAlias(MemoryIndex index, MemoryIndex mustAlias, MemoryIndex mayAlias)
        {
            MemoryAliasInfo aliasInfo = getAliasInfo(index);
            IMemoryAliasBuilder alias = aliasInfo.Aliases;

            if (mustAlias != null)
            {
                alias.MustAliases.Add(mustAlias);

                if (alias.MayAliases.Contains(mustAlias))
                {
                    alias.MayAliases.Remove(mustAlias);
                }
            }

            if (mayAlias != null && !alias.MustAliases.Contains(mayAlias))
            {
                alias.MayAliases.Add(mayAlias);
            }
        }

        public MemoryAliasInfo getAliasInfo(MemoryIndex index)
        {
            MemoryAliasInfo aliasInfo;
            if (!MemoryAliases.TryGetValue(index, out aliasInfo))
            {
                IMemoryAliasBuilder alias = Factories.StructuralContainersFactories.MemoryAliasFactory.CreateMemoryAlias(writeableTargetStructure, index).Builder(writeableTargetStructure);
                aliasInfo = new MemoryAliasInfo(alias, false);

                MemoryAliases[index] = aliasInfo;
            }
            return aliasInfo;
        }

        #endregion
    }
}
