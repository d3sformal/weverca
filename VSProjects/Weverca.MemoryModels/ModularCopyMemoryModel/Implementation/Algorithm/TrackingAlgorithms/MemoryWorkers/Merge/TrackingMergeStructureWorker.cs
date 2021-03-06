﻿using PHP.Core;
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
    /// <summary>
    /// Tracking version of merge algorithm which merges the structural data.
    /// </summary>
    class TrackingMergeStructureWorker : IReferenceHolder
    {

        /// <summary>
        /// Gets or sets the structure.
        /// </summary>
        /// <value>
        /// The structure.
        /// </value>
        public ISnapshotStructureProxy Structure { get; set; }

        /// <summary>
        /// Gets or sets the factories.
        /// </summary>
        /// <value>
        /// The factories.
        /// </value>
        public ModularMemoryModelFactories Factories { get; set; }

        /// <summary>
        /// Gets the memory aliases.
        /// </summary>
        /// <value>
        /// The memory aliases.
        /// </value>
        public Dictionary<MemoryIndex, MemoryAliasInfo> MemoryAliases { get; private set; }

        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <value>
        /// The snapshot.
        /// </value>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingMergeStructureWorker"/> class.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <param name="targetSnapshot">The target snapshot.</param>
        /// <param name="sourceSnapshots">The source snapshots.</param>
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

        /// <summary>
        /// Merges the structure.
        /// </summary>
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

        /// <summary>
        /// Calls the merge structure.
        /// </summary>
        /// <param name="callSnapshot">The call snapshot.</param>
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

        /// <summary>
        /// Creates the snapshot contexts.
        /// </summary>
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

        /// <summary>
        /// Collects the structure changes and find ancestor.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Creates the new structure from common ancestor.
        /// </summary>
        /// <param name="commonAncestor">The common ancestor.</param>
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

        /// <summary>
        /// Creates the stack levels.
        /// </summary>
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

        /// <summary>
        /// Collects the call structure changes.
        /// </summary>
        /// <param name="callSnapshot">The call snapshot.</param>
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

        /// <summary>
        /// Creates the new structure from call snapshot.
        /// </summary>
        /// <param name="callSnapshot">The call snapshot.</param>
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

        /// <summary>
        /// Stores the local arays.
        /// </summary>
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

        /// <summary>
        /// Merges the declarations.
        /// </summary>
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

        /// <summary>
        /// Merges the object definitions.
        /// </summary>
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

        /// <summary>
        /// Merges the memory stacks roots.
        /// </summary>
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

        /// <summary>
        /// Processes the merge operations.
        /// </summary>
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

        /// <summary>
        /// Updates the aliases.
        /// </summary>
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

        /// <summary>
        /// Ensures the tracking changes.
        /// </summary>
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

        /// <summary>
        /// Gets the first common ancestor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trackerA">The tracker a.</param>
        /// <param name="trackerB">The tracker b.</param>
        /// <param name="currentChanges">The current changes.</param>
        /// <param name="changes">The changes.</param>
        /// <returns>Located ancestor</returns>
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

        /// <summary>
        /// Collects the single function changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callSnapshot">The call snapshot.</param>
        /// <param name="tracker">The tracker.</param>
        /// <param name="currentChanges">The current changes.</param>
        /// <param name="changes">The changes.</param>
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

        /// <summary>
        /// Creates the and enqueue operations.
        /// </summary>
        /// <param name="targetContainerContext">The target container context.</param>
        /// <param name="treeNode">The tree node.</param>
        /// <param name="sourceContainers">The source containers.</param>
        /// <param name="alwaysDefined">if set to <c>true</c> [always defined].</param>
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

        /// <summary>
        /// Collects the indexes.
        /// </summary>
        /// <param name="childName">Name of the child.</param>
        /// <param name="sourceContainers">The source containers.</param>
        /// <param name="operation">The operation.</param>
        /// <returns>True if there is some defined child; otherwise false</returns>
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

        /// <summary>
        /// Enqueues the merge operation.
        /// </summary>
        /// <param name="childName">Name of the child.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="targetContainerContext">The target container context.</param>
        /// <param name="childTreeNode">The child tree node.</param>
        /// <param name="alwaysDefined">if set to <c>true</c> [always defined].</param>
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

        /// <summary>
        /// Enqueues the delete operation.
        /// </summary>
        /// <param name="childName">Name of the child.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="targetContainerContext">The target container context.</param>
        /// <param name="childTreeNode">The child tree node.</param>
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

        /// <summary>
        /// Enqueues the merge unknown operation.
        /// </summary>
        /// <param name="targetContainerContext">The target container context.</param>
        /// <param name="anyNode">Any node.</param>
        /// <param name="sourceContainers">The source containers.</param>
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

        /// <summary>
        /// Creates the new index of the target.
        /// </summary>
        /// <param name="targetContainerContext">The target container context.</param>
        /// <param name="childName">Name of the child.</param>
        /// <returns></returns>
        private MemoryIndex createNewTargetIndex(ITargetContainerContext targetContainerContext, string childName)
        {
            MemoryIndex targetIndex = targetContainerContext.createMemoryIndex(childName);
            targetContainerContext.getWriteableSourceContainer().AddIndex(childName, targetIndex);

            return targetIndex;
        }

        #endregion

        #region Processing operations

        #region Merge operation

        /// <summary>
        /// Processes the merge operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
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

        /// <summary>
        /// Processes the delete operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
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

        /// <summary>
        /// Deletes the array.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="targetArray">The target array.</param>
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

        /// <summary>
        /// Deletes the aliases.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="targetAliases">The target aliases.</param>
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

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAliases">The must aliases.</param>
        /// <param name="mayAliases">The may aliases.</param>
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

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAlias">The must alias.</param>
        /// <param name="mayAlias">The may alias.</param>
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

        /// <summary>
        /// Gets the alias information.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Alias info for the processed index</returns>
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
