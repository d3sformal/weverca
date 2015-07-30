using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign
{
    /// <summary>
    /// Contains common functioality to provide the lazy assign operation.
    /// 
    /// This instance need the collected tree from TreeIndexCollector which 
    /// identifies the memory locations where to assign into. Second input is 
    /// </summary>
    abstract class AbstractAssignWorker : LocationCollectorNodeVisitor
    {
        /// <summary>
        /// Gets or sets the factories.
        /// </summary>
        /// <value>
        /// The factories.
        /// </value>
        public ModularMemoryModelFactories Factories { get; set; }

        internal readonly Snapshot Snapshot;
        protected TreeIndexCollector treeCollector;

        internal readonly IWriteableSnapshotStructure Structure;
        internal readonly IWriteableSnapshotData Data;
        internal readonly MemoryIndexModificationList PathModifications;

        protected LinkedList<LocationCollectorNode> collectorNodesQueue = new LinkedList<LocationCollectorNode>();
        private CopyValuesVisitor copyValuesVisitor = new CopyValuesVisitor();

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractAssignWorker"/> class.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="treeCollector">The tree collector.</param>
        /// <param name="pathModifications">The path modifications.</param>
        public AbstractAssignWorker(ModularMemoryModelFactories factories, Snapshot snapshot,
            TreeIndexCollector treeCollector, MemoryIndexModificationList pathModifications)
        {
            Factories = factories;

            this.Snapshot = snapshot;
            this.treeCollector = treeCollector;
            this.PathModifications = pathModifications;

            this.Structure = snapshot.Structure.Writeable;
            this.Data = snapshot.CurrentData.Writeable;
        }

        /// <summary>
        /// Perform custom assign operation on value node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected abstract void collectValueNode(ValueCollectorNode node);

        /// <summary>
        /// Perform custom assign operation on memory index node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected abstract void collectMemoryIndexCollectorNode(MemoryIndexCollectorNode node);

        /// <summary>
        /// Perform custom assign operation on unknown node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected abstract void collectUnknownIndexCollectorNode(UnknownIndexCollectorNode node);

        /// <summary>
        /// Perform custom assign operation on undefined node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected abstract void collectUndefinedCollectorNode(UndefinedCollectorNode node);

        /// <summary>
        /// Performs an assign operation to all nodes collected by the collector.
        /// </summary>
        public void ProcessCollector()
        {
            // Prepares all variables.
            foreach (var item in treeCollector.RootNode.VariableStackNodes)
            {
                processVariables(item.Key, item.Value);
            }

            // Prepares all control variables.
            foreach (var item in treeCollector.RootNode.ControlStackNodes)
            {
                processControls(item.Key, item.Value);
            }

            // Prepares all temporary variables.
            foreach (var item in treeCollector.RootNode.TemporaryNodes)
            {
                processTemporary(item.Value);
            }

            // Prepares all objects.
            foreach (var item in treeCollector.RootNode.ObjectNodes)
            {
                processObject(item.Key, item.Value);
            }

            // Iterates until all operations are performed.
            while (collectorNodesQueue.Count > 0)
            {
                LocationCollectorNode node = collectorNodesQueue.First.Value;
                collectorNodesQueue.RemoveFirst();
                node.Accept(this);
            }
        }

        /// <summary>
        /// Processes the controls.
        /// </summary>
        /// <param name="stackLevel">The stack level.</param>
        /// <param name="node">The node.</param>
        private void processControls(int stackLevel, CollectorNode node)
        {
            if (node.HasUndefinedChildren)
            {
                IWriteableIndexContainer writeableVariableContainer = Structure.GetWriteableStackContext(stackLevel).WriteableControllVariables;
                foreach (var newChild in node.UndefinedChildren)
                {
                    string childName = newChild.Item1;
                    MemoryCollectorNode childNode = newChild.Item2;

                    MemoryIndex index = ControlIndex.Create(childName, stackLevel);
                    childNode.TargetIndex = index;

                    writeableVariableContainer.AddIndex(childName, index);
                }
            }
            enqueueChildNodes(node);
        }

        /// <summary>
        /// Processes the variables.
        /// </summary>
        /// <param name="stackLevel">The stack level.</param>
        /// <param name="node">The node.</param>
        private void processVariables(int stackLevel, CollectorNode node)
        {
            if (node.HasUndefinedChildren)
            {
                IWriteableIndexContainer writeableVariableContainer = Structure.GetWriteableStackContext(stackLevel).WriteableVariables;
                foreach (var newChild in node.UndefinedChildren)
                {
                    string childName = newChild.Item1;
                    MemoryCollectorNode childNode = newChild.Item2;

                    MemoryIndex index = VariableIndex.Create(childName, stackLevel);
                    childNode.TargetIndex = index;

                    writeableVariableContainer.AddIndex(childName, index);
                }
            }
            enqueueChildNodes(node);
        }

        /// <summary>
        /// Processes the object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="node">The node.</param>
        private void processObject(ObjectValue objectValue, ContainerCollectorNode node)
        {
            if (node.HasUndefinedChildren)
            {
                IObjectDescriptor oldDescriptor = Structure.GetDescriptor(objectValue);
                IObjectDescriptorBuilder builder = oldDescriptor.Builder(Structure);
                foreach (var newChild in node.UndefinedChildren)
                {
                    string childName = newChild.Item1;
                    MemoryCollectorNode childNode = newChild.Item2;

                    MemoryIndex index = ObjectIndex.Create(objectValue, childName);
                    childNode.TargetIndex = index;

                    builder.AddIndex(childName, index);
                }

                IObjectDescriptor newDescriptor = builder.Build(Structure);
                Structure.SetDescriptor(objectValue, newDescriptor);
            }
            enqueueChildNodes(node);
        }

        /// <summary>
        /// Processes the temporary.
        /// </summary>
        /// <param name="memoryIndexNode">The memory index node.</param>
        private void processTemporary(MemoryIndexCollectorNode memoryIndexNode)
        {
            collectorNodesQueue.AddLast(memoryIndexNode);
        }

        /// <summary>
        /// Enqueues the child nodes.
        /// </summary>
        /// <param name="node">The node.</param>
        private void enqueueChildNodes(CollectorNode node)
        {
            foreach (var childNode in node.ChildNodes)
            {
                collectorNodesQueue.AddLast(childNode);
            }
        }

        /// <summary>
        /// Enqueues the location child nodes.
        /// </summary>
        /// <param name="node">The node.</param>
        private void enqueueLocationChildNodes(LocationCollectorNode node)
        {
            if (node.ValueNodes != null)
            {
                foreach (var valueNode in node.ValueNodes)
                {
                    valueNode.TargetIndex = node.TargetIndex;
                }
            }

            foreach (var childNode in node.ChildNodes)
            {
                collectorNodesQueue.AddLast(childNode);
            }
        }
        
        /// <summary>
        /// Continues the value node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected void continueValueNode(ValueCollectorNode node)
        {
            enqueueLocationChildNodes(node);
        }

        /// <summary>
        /// Continues the memory index collector node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected void continueMemoryIndexCollectorNode(MemoryIndexCollectorNode node)
        {
            HashSet<Value> values = new HashSet<Value>();

            testAndCreateImplicitArray(node, values);
            testAndCreateImplicitObject(node, values);
            testAndCreateUndefinedChildren(node);

            bool removeUndefined = node.IsMust && node.ContainsUndefinedValue;
            if (removeUndefined || values.Count > 0)
            {
                MemoryEntry entry = SnapshotDataUtils.GetMemoryEntry(Snapshot, Data, node.TargetIndex);
                copyEntryValues(entry, values, removeUndefined, false);
                Data.SetMemoryEntry(node.TargetIndex, Snapshot.CreateMemoryEntry(values));
            }

            enqueueLocationChildNodes(node);
        }

        /// <summary>
        /// Continues the unknown index collector node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected void continueUnknownIndexCollectorNode(UnknownIndexCollectorNode node)
        {
            Structure.NewIndex(node.TargetIndex);
            PathModifications.GetOrCreateModification(node.TargetIndex).AddDatasource(node.SourceIndex, Snapshot);

            IIndexDefinition definition = Structure.GetIndexDefinition(node.SourceIndex);
            HashSet<Value> values = new HashSet<Value>();

            processSourceAliases(node, definition.Aliases);
            processSourceArray(node, definition.Array, values);
            processSourceObjects(node, definition.Objects);
            testAndCreateImplicitObject(node, values);
            testAndCreateUndefinedChildren(node);

            MemoryEntry entry = SnapshotDataUtils.GetMemoryEntry(Snapshot, Data, node.SourceIndex);
            copyEntryValues(entry, values, node.IsMust, true);
            Data.SetMemoryEntry(node.TargetIndex, Snapshot.CreateMemoryEntry(values));

            enqueueLocationChildNodes(node);
        }

        /// <summary>
        /// Continues the undefined collector node.
        /// </summary>
        /// <param name="node">The node.</param>
        protected void continueUndefinedCollectorNode(UndefinedCollectorNode node)
        {
            Structure.NewIndex(node.TargetIndex);

            HashSet<Value> values = new HashSet<Value>();

            testAndCreateImplicitArray(node, values);
            testAndCreateImplicitObject(node, values);
            testAndCreateUndefinedChildren(node);

            if (!node.IsMust)
            {
                values.Add(Snapshot.UndefinedValue);
            }

            Data.SetMemoryEntry(node.TargetIndex, Snapshot.CreateMemoryEntry(values));

            enqueueLocationChildNodes(node);
        }

        #region CollectorNode visitor

        public void VisitValueCollectorNode(ValueCollectorNode node)
        {
            if (node.IsCollected)
            {
                collectValueNode(node);
            }
            else
            {
                continueValueNode(node);
            }
        }

        public void VisitMemoryIndexCollectorNode(MemoryIndexCollectorNode node)
        {
            if (node.IsCollected)
            {
                collectMemoryIndexCollectorNode(node);
            }
            else
            {
                continueMemoryIndexCollectorNode(node);
            }
        }

        public void VisitUnknownIndexCollectorNode(UnknownIndexCollectorNode node)
        {
            if (node.IsCollected)
            {
                collectUnknownIndexCollectorNode(node);
            }
            else
            {
                continueUnknownIndexCollectorNode(node);
            }
        }

        public void VisitUndefinedCollectorNode(UndefinedCollectorNode node)
        {
            if (node.IsCollected)
            {
                collectUndefinedCollectorNode(node);
            }
            else
            {
                continueUndefinedCollectorNode(node);
            }
        }

        #endregion

        #region Processing

        /// <summary>
        /// Processes the source aliases.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="memoryAlias">The memory alias.</param>
        private void processSourceAliases(MemoryCollectorNode node, IMemoryAlias memoryAlias)
        {
            if (memoryAlias != null && memoryAlias.HasAliases)
            {
                if (node.IsMust)
                {
                    foreach (MemoryIndex alias in memoryAlias.MustAliases)
                    {
                        Snapshot.AddAlias(alias, node.TargetIndex, null);
                    }

                    foreach (MemoryIndex alias in memoryAlias.MayAliases)
                    {
                        Snapshot.AddAlias(alias, null, node.TargetIndex);
                    }

                    Snapshot.MustSetAliases(node.TargetIndex, memoryAlias.MustAliases, memoryAlias.MayAliases);
                }
                else
                {
                    HashSet<MemoryIndex> aliases = new HashSet<MemoryIndex>();
                    CollectionMemoryUtils.AddAll(aliases, memoryAlias.MustAliases);
                    CollectionMemoryUtils.AddAll(aliases, memoryAlias.MayAliases);

                    foreach (MemoryIndex alias in aliases)
                    {
                        Snapshot.AddAlias(alias, null, node.TargetIndex);
                    }

                    Snapshot.MaySetAliases(node.TargetIndex, aliases);
                }
            }
        }

        /// <summary>
        /// Processes the source array.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="arrayValue">The array value.</param>
        /// <param name="values">The values.</param>
        private void processSourceArray(MemoryCollectorNode node, AssociativeArray arrayValue, ICollection<Value> values)
        {
            if (arrayValue != null)
            {
                IArrayDescriptor sourceArray = Structure.GetDescriptor(arrayValue);

                if (node.AnyChildNode == null)
                {
                    node.CreateMemoryIndexAnyChild(sourceArray.UnknownIndex);
                }

                foreach (var item in sourceArray.Indexes)
                {
                    string name = item.Key;
                    MemoryIndex index = item.Value;

                    if (!node.NamedChildNodes.ContainsKey(name))
                    {
                        LocationCollectorNode newChild = node.CreateMemoryIndexChildFromAny(name, sourceArray.UnknownIndex);
                        newChild.IsMust = node.IsMust;
                    }
                }

                createArray(node, values);
            }
            else
            {
                testAndCreateImplicitArray(node, values);
            }
        }

        /// <summary>
        /// Processes the source objects.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="objects">The objects.</param>
        private void processSourceObjects(MemoryCollectorNode node, IObjectValueContainer objects)
        {
            if (objects != null && objects.Count > 0)
            {
                IObjectValueContainer targetObjects = Factories.StructuralContainersFactories.ObjectValueContainerFactory.CreateObjectValueContainer(Structure, objects);
                Structure.SetObjects(node.TargetIndex, targetObjects);
            }
        }

        /// <summary>
        /// Tests the and create implicit array.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="values">The values.</param>
        public void testAndCreateImplicitArray(MemoryCollectorNode node, ICollection<Value> values)
        {
            if (node.HasNewImplicitArray)
            {
                createArray(node, values);
            }
        }

        /// <summary>
        /// Creates the array.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="values">The values.</param>
        /// <returns>New created array</returns>
        public AssociativeArray createArray(MemoryCollectorNode node, ICollection<Value> values)
        {
            AssociativeArray createdArrayValue = Snapshot.CreateArray(node.TargetIndex);
            values.Add(createdArrayValue);

            if (node.AnyChildNode != null)
            {
                IArrayDescriptor descriptor = Structure.GetDescriptor(createdArrayValue);
                node.AnyChildNode.TargetIndex = descriptor.UnknownIndex;
            }

            return createdArrayValue;
        }

        /// <summary>
        /// Tests the and create implicit object.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="values">The values.</param>
        private void testAndCreateImplicitObject(MemoryCollectorNode node, HashSet<Value> values)
        {
            if (node.HasNewImplicitObject)
            {
                ObjectValue value = Snapshot.MemoryAssistant.CreateImplicitObject();
                values.Add(value);

                IObjectValueContainerBuilder objectValues = Structure.GetObjects(node.TargetIndex).Builder(Structure);
                objectValues.Add(value);
                Structure.SetObjects(node.TargetIndex, objectValues.Build(Structure));

                IObjectDescriptor oldDescriptor = Structure.GetDescriptor(value);
                IObjectDescriptorBuilder builder = oldDescriptor.Builder(Structure);

                foreach (var newChild in node.ImplicitObjectNode.UndefinedChildren)
                {
                    string childName = newChild.Item1;
                    MemoryCollectorNode childNode = newChild.Item2;

                    MemoryIndex index = ObjectIndex.Create(value, childName);
                    childNode.TargetIndex = index;

                    builder.AddIndex(childName, index);
                }

                IObjectDescriptor newDescriptor = builder.Build(Structure);
                Structure.SetDescriptor(value, newDescriptor);

                enqueueChildNodes(node.ImplicitObjectNode);
            }
        }

        /// <summary>
        /// Tests the and create undefined children.
        /// </summary>
        /// <param name="node">The node.</param>
        private void testAndCreateUndefinedChildren(MemoryCollectorNode node)
        {
            if (node.HasUndefinedChildren)
            {
                AssociativeArray arrayValue = Structure.GetArray(node.TargetIndex);

                IArrayDescriptor oldDescriptor = Structure.GetDescriptor(arrayValue);
                IArrayDescriptorBuilder builder = oldDescriptor.Builder(Structure);

                foreach (var newChild in node.UndefinedChildren)
                {
                    string childName = newChild.Item1;
                    MemoryCollectorNode childNode = newChild.Item2;

                    MemoryIndex index = node.TargetIndex.CreateIndex(childName);
                    childNode.TargetIndex = index;

                    builder.AddIndex(childName, index);
                }

                IArrayDescriptor newDescriptor = builder.Build(Structure);
                Structure.SetDescriptor(arrayValue, newDescriptor);
            }
        }

        #endregion

        /// <summary>
        /// Copies the entry values.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="values">The values.</param>
        /// <param name="removeUndefined">if set to <c>true</c> [remove undefined].</param>
        /// <param name="removeArray">if set to <c>true</c> [remove array].</param>
        public void copyEntryValues(MemoryEntry entry, ICollection<Value> values, bool removeUndefined, bool removeArray)
        {
            copyValuesVisitor.CopyEntryValues(entry, values, removeUndefined, removeArray);
        }

        private class CopyValuesVisitor : AbstractValueVisitor
        {
            private ICollection<Value> values;
            private bool removeUndefined;
            private bool removeArray;

            public void CopyEntryValues(MemoryEntry entry, ICollection<Value> values, bool removeUndefined, bool removeArray)
            {
                this.values = values;
                this.removeUndefined = removeUndefined;
                this.removeArray = removeArray;

                VisitMemoryEntry(entry);
            }

            public override void VisitValue(Value value)
            {
                values.Add(value);
            }

            public override void VisitUndefinedValue(UndefinedValue value)
            {
                if (!removeUndefined)
                {
                    base.VisitUndefinedValue(value);
                }
            }

            public override void VisitAssociativeArray(AssociativeArray value)
            {
                if (!removeArray)
                {
                    base.VisitAssociativeArray(value);
                }
            }
        }
    }
}
