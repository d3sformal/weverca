using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign
{
    abstract class AssignOperation
    {
        public MemoryIndex TargetIndex { get; private set; }
        public MemoryEntryCollectorNode Node { get; private set; }
        public HashSet<Value> Values { get; private set; }
        public bool ProcessAliases { get; private set; }

        public AssignWorker Worker { get; private set; }

        public AssignOperation(AssignWorker worker, MemoryIndex targetIndex, MemoryEntryCollectorNode memoryEntryNode, bool processAliases)
        {
            TargetIndex = targetIndex;
            Node = memoryEntryNode;
            Worker = worker;
            ProcessAliases = processAliases;

            Values = new HashSet<Value>();
        }

        protected void setValues()
        {
            Worker.Data.SetMemoryEntry(TargetIndex, new MemoryEntry(Values));
        }

        public abstract void ProcessOperation();

        protected abstract AssignOperation prepareOperationToUnknownOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode);
        protected abstract AssignOperation prepareOperationToIndexOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode);

        protected AssociativeArray createAssignedArray()
        {
            // Creates new empty array
            AssociativeArray arrayValue = Worker.Snapshot.CreateArray(TargetIndex);
            IArrayDescriptorBuilder builder = Worker.Structure.GetDescriptor(arrayValue).Builder(Worker.Structure);

            // Adds nodes children into array
            foreach (var item in Node.NamedChildren)
            {
                string name = item.Key;
                MemoryEntryCollectorNode node = item.Value;

                MemoryIndex childIndex = TargetIndex.CreateIndex(name);
                builder.AddIndex(name, childIndex);
                Worker.AddOperation(prepareOperationToIndexOfCreatedArray(childIndex, node));
            }
            Worker.AddOperation(prepareOperationToUnknownOfCreatedArray(builder.UnknownIndex, Node.AnyChild));

            // Sets the updated descriptor
            Worker.Structure.SetDescriptor(arrayValue, builder.Build(Worker.Structure));
            return arrayValue;
        }

        protected void processObjects()
        {
            if (Node.Objects != null && Node.Objects.Count > 0)
            {
                IObjectValueContainer objects = Worker.Factories.StructuralContainersFactories.ObjectValueContainerFactory.CreateObjectValueContainer(Worker.Structure, Node.Objects);
                Worker.Structure.SetObjects(TargetIndex, objects);

                CollectionMemoryUtils.AddAll(Values, Node.Objects);
            }
        }

        protected void processObjects(IObjectValueContainer objects)
        {
            if (objects != null && objects.Count > 0)
            {
                IObjectValueContainerBuilder builder = Worker.Factories.StructuralContainersFactories.ObjectValueContainerFactory.CreateObjectValueContainer(Worker.Structure, objects).Builder(Worker.Structure);

                if (Node.Objects != null)
                {
                    builder.AddAll(Node.Objects);
                    CollectionMemoryUtils.AddAll(Values, Node.Objects);
                }

                Worker.Structure.SetObjects(TargetIndex, builder.Build(Worker.Structure));

                CollectionMemoryUtils.AddAll(Values, objects.Values);
            }
            else
            {
                processObjects();
            }
        }

        protected void processIndexModifications()
        {
            if (Node.SourceIndexes != null)
            {
                var indexModification = Worker.PathModifications.GetOrCreateModification(TargetIndex);
                foreach (var sourceIndex in Node.SourceIndexes)
                {
                    indexModification.AddDatasource(sourceIndex.Index, sourceIndex.Snapshot);
                }
            }
        }

        protected void processIndexModifications(MemoryIndex index)
        {
            var indexModification = Worker.PathModifications.GetOrCreateModification(TargetIndex);
            indexModification.AddDatasource(index, Worker.Snapshot);

            if (Node.SourceIndexes != null)
            {
                foreach (var sourceIndex in Node.SourceIndexes)
                {
                    indexModification.AddDatasource(sourceIndex.Index, sourceIndex.Snapshot);
                }
            }
        }
    }

    class MemoryIndexMustAssignOperation : AssignOperation
    {
        public MemoryIndexMustAssignOperation(AssignWorker worker, MemoryIndex targetIndex, MemoryEntryCollectorNode memoryEntryNode, bool processAliases = true)
            : base(worker, targetIndex, memoryEntryNode, processAliases)
        {
        }

        public override void ProcessOperation()
        {
            IIndexDefinition definition = Worker.Structure.GetIndexDefinition(TargetIndex);
            if (Node.ScalarValues != null)
            {
                CollectionMemoryUtils.AddAll(Values, Node.ScalarValues);
            }

            processArrays(definition.Array);
            processAliases(definition.Aliases);
            processObjects();
            processIndexModifications();

            setValues();
        }

        #region Array Processing

        private void processArrays(AssociativeArray arrayValue)
        {
            if (arrayValue != null)
            {
                if (Node.Arrays != null && Node.Arrays.Count > 0)
                {
                    mergeWithAssignedArray(arrayValue);
                    Values.Add(arrayValue);
                }
                else
                {
                    deleteArray(arrayValue);
                }
            }
            else
            {
                if (Node.Arrays != null && Node.Arrays.Count > 0)
                {
                    AssociativeArray array = createAssignedArray();
                    Values.Add(array);
                }
            }
        }

        private void deleteArray(AssociativeArray arrayValue)
        {
            IArrayDescriptor oldDescriptor = Worker.Structure.GetDescriptor(arrayValue);
            foreach (var item in oldDescriptor.Indexes)
            {
                string name = item.Key;
                MemoryIndex index = item.Value;
                Worker.AddOperation(new MemoryIndexDeleteAssignOperation(Worker, index));
            }
            Worker.AddOperation(new MemoryIndexDeleteAssignOperation(Worker, oldDescriptor.UnknownIndex));

            Worker.Structure.RemoveArray(TargetIndex, arrayValue);
        }

        private void mergeWithAssignedArray(AssociativeArray arrayValue)
        {
            // Get descriptors
            IArrayDescriptor descriptor = Worker.Structure.GetDescriptor(arrayValue);
            IArrayDescriptorBuilder builder = Worker.Structure.GetDescriptor(arrayValue).Builder(Worker.Structure);

            // Iterate source indexes which are not present in node
            List<string> namesToRemove = new List<string>();
            foreach (var item in descriptor.Indexes)
            {
                string name = item.Key;
                MemoryIndex index = item.Value;

                if (!Node.NamedChildren.ContainsKey(name))
                {
                    // Index is present in source but not in node
                    Worker.AddOperation(new MemoryIndexDeleteAssignOperation(Worker, index));
                    namesToRemove.Add(name);
                }
            }

            // Safely removes nodes which are no longer used (prevents changes in lazy builder)
            foreach (string name in namesToRemove)
            {
                builder.RemoveIndex(name);
            }

            // Iterate all child nodes
            foreach (var item in Node.NamedChildren)
            {
                string name = item.Key;
                MemoryEntryCollectorNode node = item.Value;

                if (descriptor.ContainsIndex(name))
                {
                    // Index is present in source and node
                    Worker.AddOperation(new MemoryIndexMustAssignOperation(Worker, descriptor.GetIndex(name), node));
                }
                else
                {
                    // Index is present in node but not in source
                    MemoryIndex createdIndex = TargetIndex.CreateIndex(name);
                    builder.AddIndex(name, createdIndex);

                    Worker.AddOperation(new UndefinedMustAssignOperation(Worker, createdIndex, node));
                }
            }

            // Merge unknown index with unknown node (unknown index was created in array initialization - scip new array)
            Worker.AddOperation(new MemoryIndexMustAssignOperation(Worker, descriptor.UnknownIndex, Node.AnyChild));

            // Build and set modified target descriptor
            Worker.Structure.SetDescriptor(arrayValue, builder.Build(Worker.Structure));
        }

        protected override AssignOperation prepareOperationToUnknownOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new MemoryIndexMustAssignOperation(Worker, targetIndex, sourceNode);
        }

        protected override AssignOperation prepareOperationToIndexOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new UndefinedMayAssignOperation(Worker, targetIndex, sourceNode);
        }

        #endregion

        private void processAliases(IMemoryAlias memoryAlias)
        {
            if (ProcessAliases)
            {
                // Assign new aliases if any
                if (Node.HasAliases)
                {
                    Worker.Snapshot.DestroyAliases(TargetIndex);

                    IEnumerable<MemoryIndex> mustAliases = Node.References.GetMustReferences();
                    List<MemoryIndex> filteredMustAliases = new List<MemoryIndex>();
                    foreach (MemoryIndex alias in mustAliases)
                    {
                        if (!alias.Equals(TargetIndex))
                        {
                            Worker.Snapshot.AddAlias(alias, TargetIndex, null);
                            filteredMustAliases.Add(alias);
                        }
                    }

                    IEnumerable<MemoryIndex> mayAliases = Node.References.GetMayReferences();
                    List<MemoryIndex> filteredMayAliases = new List<MemoryIndex>();
                    foreach (MemoryIndex alias in mayAliases)
                    {
                        if (!alias.Equals(TargetIndex))
                        {
                            Worker.Snapshot.AddAlias(alias, null, TargetIndex);
                            filteredMayAliases.Add(alias);
                        }
                    }

                    Worker.Snapshot.MustSetAliasesWithoutDelete(TargetIndex, filteredMustAliases, filteredMayAliases);
                }
                // Destroys old aliases if any
                else if (memoryAlias != null && memoryAlias.HasAliases)
                {
                    Worker.Snapshot.DestroyAliases(TargetIndex);
                }
            }
        }
    }

    class MemoryIndexMayAssignOperation : AssignOperation
    {
        public MemoryIndexMayAssignOperation(AssignWorker worker, MemoryIndex targetIndex, MemoryEntryCollectorNode memoryEntryNode, bool processAliases = true)
            : base(worker, targetIndex, memoryEntryNode, processAliases)
        {
        }

        public override void ProcessOperation()
        {
            IIndexDefinition definition = Worker.Structure.GetIndexDefinition(TargetIndex);
            if (Node.ScalarValues != null)
            {
                CollectionMemoryUtils.AddAll(Values, Node.ScalarValues);
            }

            MemoryEntry oldEntry = SnapshotDataUtils.GetMemoryEntry(Worker.Snapshot, Worker.Data, TargetIndex);
            CollectionMemoryUtils.AddAll(Values, oldEntry.PossibleValues);

            processArrays(definition.Array);
            processAliases(definition.Aliases);
            processObjects(definition.Objects);
            processIndexModifications(TargetIndex);

            setValues();
        }

        #region Array Processing

        private void processArrays(AssociativeArray arrayValue)
        {
            if (arrayValue != null)
            {
                Values.Add(arrayValue);

                if (Node.Arrays != null && Node.Arrays.Count > 0)
                {
                    mergeWithAssignedArray(arrayValue);
                }
                else
                {
                    mergeWithEmptyAssignedArray(arrayValue);
                }
            }
            else
            {
                if (Node.Arrays != null && Node.Arrays.Count > 0)
                {
                    AssociativeArray array = createAssignedArray();
                    Values.Add(array);
                }
            }
        }

        private void mergeWithEmptyAssignedArray(AssociativeArray arrayValue)
        {
            IArrayDescriptor descriptor = Worker.Structure.GetDescriptor(arrayValue);
            foreach (var item in descriptor.Indexes)
            {
                string name = item.Key;
                MemoryIndex index = item.Value;

                Worker.AddOperation(new MemoryIndexMayAssignOperation(Worker, index, MemoryEntryCollectorNode.GetEmptyNode(Worker.Snapshot)));
            }
            Worker.AddOperation(new MemoryIndexMayAssignOperation(Worker, descriptor.UnknownIndex, MemoryEntryCollectorNode.GetEmptyNode(Worker.Snapshot)));
        }

        private void mergeWithAssignedArray(AssociativeArray arrayValue)
        {
            // Get descriptors
            IArrayDescriptor descriptor = Worker.Structure.GetDescriptor(arrayValue);
            IArrayDescriptorBuilder builder = Worker.Structure.GetDescriptor(arrayValue).Builder(Worker.Structure);

            // Iterate source indexes which are not present in node
            foreach (var item in descriptor.Indexes)
            {
                string name = item.Key;
                MemoryIndex index = item.Value;

                if (!Node.NamedChildren.ContainsKey(name))
                {
                    // Index is present in source but not in node
                    Worker.AddOperation(new MemoryIndexMayAssignOperation(Worker, index, MemoryEntryCollectorNode.GetEmptyNode(Worker.Snapshot)));
                }
            }

            // Iterate all child nodes
            foreach (var item in Node.NamedChildren)
            {
                string name = item.Key;
                MemoryEntryCollectorNode node = item.Value;

                if (descriptor.ContainsIndex(name))
                {
                    // Index is present in source and node
                    Worker.AddOperation(new MemoryIndexMayAssignOperation(Worker, descriptor.GetIndex(name), node));
                }
                else
                {
                    // Index is present in node but not in source
                    MemoryIndex createdIndex = TargetIndex.CreateIndex(name);
                    builder.AddIndex(name, createdIndex);

                    Worker.AddOperation(new UnknownIndexMayAssign(Worker, builder.UnknownIndex, createdIndex, node));
                }
            }

            // Merge unknown index with unknown node (unknown index was created in array initialization - scip new array)
            Worker.AddOperation(new MemoryIndexMayAssignOperation(Worker, descriptor.UnknownIndex, Node.AnyChild));

            // Build and set modified target descriptor
            Worker.Structure.SetDescriptor(arrayValue, builder.Build(Worker.Structure));
        }

        protected override AssignOperation prepareOperationToUnknownOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new MemoryIndexMayAssignOperation(Worker, targetIndex, sourceNode);
        }

        protected override AssignOperation prepareOperationToIndexOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new UndefinedMayAssignOperation(Worker, targetIndex, sourceNode);
        }

        #endregion

        private void processAliases(IMemoryAlias memoryAlias)
        {
            if (ProcessAliases)
            {
                // Assign new aliases if any
                if (Node.HasAliases)
                {
                    IEnumerable<MemoryIndex> aliases = Node.References.GetAllReferences();
                    foreach (MemoryIndex alias in aliases)
                    {
                        Worker.Snapshot.AddAlias(alias, null, TargetIndex);
                    }

                    Worker.Snapshot.MaySetAliases(TargetIndex, aliases);
                }
                // Must aliases has to be invalidated
                else if (memoryAlias != null && memoryAlias.HasMustAliases)
                {
                    IMemoryAliasBuilder builder = memoryAlias.Builder(Worker.Structure);
                    Worker.Snapshot.ConvertAliasesToMay(TargetIndex, builder);
                }
            }
        }
    }

    class MemoryIndexDeleteAssignOperation : AssignOperation
    {
        public MemoryIndexDeleteAssignOperation(AssignWorker worker, MemoryIndex targetIndex)
            : base(worker, targetIndex, null, true)
        {
        }

        public override void ProcessOperation()
        {
            IIndexDefinition definition = Worker.Structure.GetIndexDefinition(TargetIndex);

            processArrays(definition.Array);
            processAliases(definition.Aliases);

            Worker.Structure.RemoveIndex(TargetIndex);
            Worker.Data.RemoveMemoryEntry(TargetIndex);
        }

        #region Array Processing

        private void processArrays(AssociativeArray arrayValue)
        {
            if (arrayValue != null)
            {
                IArrayDescriptor descriptor = Worker.Structure.GetDescriptor(arrayValue);
                foreach (var item in descriptor.Indexes)
                {
                    string name = item.Key;
                    MemoryIndex index = item.Value;

                    Worker.AddOperation(new MemoryIndexDeleteAssignOperation(Worker, index));
                }
                Worker.AddOperation(new MemoryIndexDeleteAssignOperation(Worker, descriptor.UnknownIndex));

                Worker.Structure.RemoveArray(TargetIndex, arrayValue);
            }
        }

        protected override AssignOperation prepareOperationToUnknownOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            throw new InvalidOperationException();
        }

        protected override AssignOperation prepareOperationToIndexOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            throw new InvalidOperationException();
        }

        #endregion

        private void processAliases(IMemoryAlias memoryAlias)
        {
            if (ProcessAliases)
            {
                // Destroys old aliases if any
                if (memoryAlias != null && memoryAlias.HasAliases)
                {
                    Worker.Snapshot.DestroyAliases(TargetIndex);
                }
            }
        }
    }

    class UnknownIndexMayAssign : AssignOperation
    {

        public MemoryIndex SourceIndex { get; private set; }
        public bool CreateNewIndex { get; set; }

        public UnknownIndexMayAssign(AssignWorker worker, MemoryIndex sourceIndex, MemoryIndex targetIndex, MemoryEntryCollectorNode memoryEntryNode, bool processAliases = true)
            : base(worker, targetIndex, memoryEntryNode, processAliases)
        {
            SourceIndex = sourceIndex;

            CreateNewIndex = true;
        }

        public override void ProcessOperation()
        {
            if (CreateNewIndex)
            {
                Worker.Structure.NewIndex(TargetIndex);
            }

            IIndexDefinition definition = Worker.Structure.GetIndexDefinition(SourceIndex);
            if (Node.ScalarValues != null)
            {
                CollectionMemoryUtils.AddAll(Values, Node.ScalarValues);
            }

            processSourceValues();

            processArrays(definition.Array);
            processAliases(definition.Aliases);
            processObjects(definition.Objects);
            processIndexModifications(SourceIndex);

            setValues();
        }

        private void processSourceValues()
        {
            MemoryEntry oldEntry = SnapshotDataUtils.GetMemoryEntry(Worker.Snapshot, Worker.Data, SourceIndex);

            if (oldEntry.ContainsAssociativeArray)
            {
                foreach (Value value in oldEntry.PossibleValues)
                {
                    if (!(value is AssociativeArray))
                    {
                        Values.Add(value);
                    }
                }
            }
            else
            {
                CollectionMemoryUtils.AddAll(Values, oldEntry.PossibleValues);
            }
        }

        #region Array Processing

        private void processArrays(AssociativeArray sourceArray)
        {
            if (sourceArray != null)
            {
                AssociativeArray createdArray = Worker.Snapshot.CreateArray(TargetIndex);
                Values.Add(createdArray);

                if (Node.Arrays != null && Node.Arrays.Count > 0)
                {
                    mergeWithAssignedArray(sourceArray, createdArray);
                }
                else
                {
                    mergeWithEmptyAssignedArray(sourceArray, createdArray);
                }
            }
            else
            {
                if (Node.Arrays != null && Node.Arrays.Count > 0)
                {
                    AssociativeArray array = createAssignedArray();
                    Values.Add(array);
                }
            }
        }

        private void mergeWithEmptyAssignedArray(AssociativeArray sourceArray, AssociativeArray targetArray)
        {
            // Get descriptors
            IArrayDescriptor sourceDescriptor = Worker.Structure.GetDescriptor(sourceArray);
            IArrayDescriptorBuilder targetDescriptorBuilder = Worker.Structure.GetDescriptor(targetArray).Builder(Worker.Structure);

            // Create child index and merge with empty node
            foreach (var item in sourceDescriptor.Indexes)
            {
                string name = item.Key;
                MemoryIndex index = item.Value;

                MemoryIndex createdIndex = TargetIndex.CreateIndex(name);
                targetDescriptorBuilder.AddIndex(name, createdIndex);

                Worker.AddOperation(new UnknownIndexMayAssign(Worker, index, createdIndex, MemoryEntryCollectorNode.GetEmptyNode(Worker.Snapshot)));
            }

            // Merge unknown index with empty node (unknown index was created in array initialization - scip new array)
            UnknownIndexMayAssign toUnknownAssignOperation = new UnknownIndexMayAssign(Worker, sourceDescriptor.UnknownIndex,
                targetDescriptorBuilder.UnknownIndex, MemoryEntryCollectorNode.GetEmptyNode(Worker.Snapshot));
            toUnknownAssignOperation.CreateNewIndex = false;
            Worker.AddOperation(toUnknownAssignOperation);

            // Build and set modified target descriptor
            Worker.Structure.SetDescriptor(targetArray, targetDescriptorBuilder.Build(Worker.Structure));
        }

        private void mergeWithAssignedArray(AssociativeArray sourceArray, AssociativeArray targetArray)
        {
            // Get descriptors
            IArrayDescriptor sourceDescriptor = Worker.Structure.GetDescriptor(sourceArray);
            IArrayDescriptorBuilder targetDescriptorBuilder = Worker.Structure.GetDescriptor(targetArray).Builder(Worker.Structure);

            // Iterate source indexes which are not present in node
            foreach (var item in sourceDescriptor.Indexes)
            {
                string name = item.Key;
                MemoryIndex index = item.Value;

                if (!Node.NamedChildren.ContainsKey(name))
                {
                    // Index is present in source but not in node
                    MemoryIndex createdIndex = TargetIndex.CreateIndex(name);
                    targetDescriptorBuilder.AddIndex(name, createdIndex);

                    Worker.AddOperation(new UnknownIndexMayAssign(Worker, index, createdIndex, MemoryEntryCollectorNode.GetEmptyNode(Worker.Snapshot)));
                }
            }

            // Iterate all child nodes
            foreach (var item in Node.NamedChildren)
            {
                string name = item.Key;
                MemoryEntryCollectorNode node = item.Value;

                if (sourceDescriptor.ContainsIndex(name))
                {
                    // Index is present in source and node
                    MemoryIndex createdIndex = TargetIndex.CreateIndex(name);
                    targetDescriptorBuilder.AddIndex(name, createdIndex);

                    Worker.AddOperation(new UnknownIndexMayAssign(Worker, sourceDescriptor.GetIndex(name), createdIndex, node));
                }
                else
                {
                    // Index is present in node but not in source
                    MemoryIndex createdIndex = TargetIndex.CreateIndex(name);
                    targetDescriptorBuilder.AddIndex(name, createdIndex);

                    Worker.AddOperation(new UnknownIndexMayAssign(Worker, sourceDescriptor.UnknownIndex, createdIndex, node));
                }
            }

            // Merge unknown index with unknown node (unknown index was created in array initialization - scip new array)
            UnknownIndexMayAssign toUnknownAssignOperation = new UnknownIndexMayAssign(Worker, sourceDescriptor.UnknownIndex,
                targetDescriptorBuilder.UnknownIndex, Node.AnyChild);
            toUnknownAssignOperation.CreateNewIndex = false;
            Worker.AddOperation(toUnknownAssignOperation);

            // Build and set modified target descriptor
            Worker.Structure.SetDescriptor(targetArray, targetDescriptorBuilder.Build(Worker.Structure));
        }

        protected override AssignOperation prepareOperationToUnknownOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new MemoryIndexMayAssignOperation(Worker, targetIndex, sourceNode);
        }

        protected override AssignOperation prepareOperationToIndexOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new UndefinedMayAssignOperation(Worker, targetIndex, sourceNode);
        }

        #endregion

        private void processAliases(IMemoryAlias memoryAlias)
        {
            if (ProcessAliases)
            {
                if (Node.HasAliases || memoryAlias != null && memoryAlias.HasAliases)
                {
                    HashSet<MemoryIndex> aliases = new HashSet<MemoryIndex>();
                    CollectionMemoryUtils.AddAll(aliases, Node.References.GetAllReferences());

                    if (memoryAlias != null && memoryAlias.HasAliases)
                    {
                        CollectionMemoryUtils.AddAll(aliases, memoryAlias.MustAliases);
                        CollectionMemoryUtils.AddAll(aliases, memoryAlias.MayAliases);
                    }

                    foreach (MemoryIndex alias in aliases)
                    {
                        Worker.Snapshot.AddAlias(alias, null, TargetIndex);
                    }

                    Worker.Snapshot.MaySetAliases(TargetIndex, aliases);
                }
            }
        }
    }

    class UndefinedMustAssignOperation : AssignOperation
    {
        public UndefinedMustAssignOperation(AssignWorker worker, MemoryIndex targetIndex, MemoryEntryCollectorNode memoryEntryNode, bool processAliases = true)
            : base(worker, targetIndex, memoryEntryNode, processAliases)
        {
        }

        public override void ProcessOperation()
        {
            Worker.Structure.NewIndex(TargetIndex);

            if (Node.ScalarValues != null)
            {
                CollectionMemoryUtils.AddAll(Values, Node.ScalarValues);
            }

            processArrays();
            processAliases();
            processObjects();
            processIndexModifications();

            setValues();
        }

        #region Array Processing

        private void processArrays()
        {
            if (Node.Arrays != null && Node.Arrays.Count > 0)
            {
                AssociativeArray array = createAssignedArray();
                Values.Add(array);
            }
        }

        protected override AssignOperation prepareOperationToUnknownOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new MemoryIndexMustAssignOperation(Worker, targetIndex, sourceNode);
        }

        protected override AssignOperation prepareOperationToIndexOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new UndefinedMustAssignOperation(Worker, targetIndex, sourceNode);
        }

        #endregion        
        
        private void processAliases()
        {
            if (ProcessAliases)
            {
                if (Node.HasAliases)
                {
                    IEnumerable<MemoryIndex> mustAliases = Node.References.GetMustReferences();
                    foreach (MemoryIndex alias in mustAliases)
                    {
                        Worker.Snapshot.AddAlias(alias, TargetIndex, null);
                    }

                    IEnumerable<MemoryIndex> mayAliases = Node.References.GetMayReferences();
                    foreach (MemoryIndex alias in mayAliases)
                    {
                        Worker.Snapshot.AddAlias(alias, null, TargetIndex);
                    }

                    Worker.Snapshot.MustSetAliasesWithoutDelete(TargetIndex, mustAliases, mayAliases);
                }
            }
        }
    }

    class UndefinedMayAssignOperation : AssignOperation
    {
        public UndefinedMayAssignOperation(AssignWorker worker, MemoryIndex targetIndex, MemoryEntryCollectorNode memoryEntryNode, bool processAliases = true)
            : base(worker, targetIndex, memoryEntryNode, processAliases)
        {
        }

        public override void ProcessOperation()
        {
            Worker.Structure.NewIndex(TargetIndex);

            if (Node.ScalarValues != null)
            {
                CollectionMemoryUtils.AddAll(Values, Node.ScalarValues);
            }
            Values.Add(Worker.Snapshot.UndefinedValue);

            processArrays();
            processAliases();
            processObjects();
            processIndexModifications();

            setValues();
        }

        #region Array Processing

        private void processArrays()
        {
            if (Node.Arrays != null && Node.Arrays.Count > 0)
            {
                AssociativeArray array = createAssignedArray();
                Values.Add(array);
            }
        }

        protected override AssignOperation prepareOperationToUnknownOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new MemoryIndexMayAssignOperation(Worker, targetIndex, sourceNode);
        }

        protected override AssignOperation prepareOperationToIndexOfCreatedArray(MemoryIndex targetIndex, MemoryEntryCollectorNode sourceNode)
        {
            return new UndefinedMayAssignOperation(Worker, targetIndex, sourceNode);
        }

        #endregion

        private void processAliases()
        {
            if (ProcessAliases)
            {
                if (Node.HasAliases)
                {
                    IEnumerable<MemoryIndex> aliases = Node.References.GetAllReferences();
                    foreach (MemoryIndex alias in aliases)
                    {
                        Worker.Snapshot.AddAlias(alias, null, TargetIndex);
                    }

                    Worker.Snapshot.MaySetAliases(TargetIndex, aliases);
                }
            }
        }
    }

}
