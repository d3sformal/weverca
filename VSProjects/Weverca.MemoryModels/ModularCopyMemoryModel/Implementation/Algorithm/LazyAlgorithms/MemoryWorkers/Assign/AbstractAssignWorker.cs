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
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign
{
    abstract class AbstractAssignWorker : LocationCollectorNodeVisitor
    {
        internal readonly Snapshot Snapshot;
        protected TreeIndexCollector treeCollector;

        internal readonly IWriteableSnapshotStructure Structure;
        internal readonly IWriteableSnapshotData Data;
        internal readonly MemoryIndexModificationList PathModifications;

        protected LinkedList<LocationCollectorNode> collectorNodesQueue = new LinkedList<LocationCollectorNode>();
        private CopyValuesVisitor copyValuesVisitor = new CopyValuesVisitor();

        public AbstractAssignWorker(Snapshot snapshot,
            TreeIndexCollector treeCollector, MemoryIndexModificationList pathModifications)
        {
            this.Snapshot = snapshot;
            this.treeCollector = treeCollector;
            this.PathModifications = pathModifications;

            this.Structure = snapshot.Structure.Writeable;
            this.Data = snapshot.CurrentData.Writeable;
        }

        public void ProcessCollector()
        {
            foreach (var item in treeCollector.RootNode.VariableStackNodes)
            {
                processVariables(item.Key, item.Value);
            }

            foreach (var item in treeCollector.RootNode.ControlStackNodes)
            {
                processControls(item.Key, item.Value);
            }

            foreach (var item in treeCollector.RootNode.TemporaryNodes)
            {
                processTemporary(item.Value);
            }

            foreach (var item in treeCollector.RootNode.ObjectNodes)
            {
                processObject(item.Key, item.Value);
            }

            while (collectorNodesQueue.Count > 0)
            {
                LocationCollectorNode node = collectorNodesQueue.First.Value;
                collectorNodesQueue.RemoveFirst();
                node.Accept(this);
            }
        }

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

        private void processTemporary(MemoryIndexCollectorNode memoryIndexNode)
        {
            collectorNodesQueue.AddLast(memoryIndexNode);
        }
        
        private void enqueueChildNodes(CollectorNode node)
        {
            foreach (var childNode in node.ChildNodes)
            {
                collectorNodesQueue.AddLast(childNode);
            }
        }

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

        protected abstract void collectValueNode(ValueCollectorNode node);
        protected abstract void collectMemoryIndexCollectorNode(MemoryIndexCollectorNode node);
        protected abstract void collectUnknownIndexCollectorNode(UnknownIndexCollectorNode node);
        protected abstract void collectUndefinedCollectorNode(UndefinedCollectorNode node);


        protected void continueValueNode(ValueCollectorNode node)
        {
            enqueueLocationChildNodes(node);
        }
        protected void continueMemoryIndexCollectorNode(MemoryIndexCollectorNode node)
        {
            HashSet<Value> values = new HashSet<Value>();

            testAndCreateImplicitArray(node, values);
            testAndCreateImplicitObject(node, values);
            testAndCreateUndefinedChildren(node);

            bool removeUndefined = node.IsMust && node.ContainsUndefinedValue;
            if (removeUndefined || values.Count > 0)
            {
                MemoryEntry entry = Data.GetMemoryEntry(node.TargetIndex);
                copyEntryValues(entry, values, removeUndefined, false);
                Data.SetMemoryEntry(node.TargetIndex, Snapshot.CreateMemoryEntry(values));
            }

            enqueueLocationChildNodes(node);
        }
        protected void continueUnknownIndexCollectorNode(UnknownIndexCollectorNode node)
        {
            Structure.NewIndex(node.TargetIndex);
            PathModifications[node.TargetIndex].AddDatasource(node.SourceIndex, Snapshot);

            IIndexDefinition definition = Structure.GetIndexDefinition(node.SourceIndex);
            HashSet<Value> values = new HashSet<Value>();

            processSourceAliases(node, definition.Aliases);
            processSourceArray(node, definition.Array, values);
            processSourceObjects(node, definition.Objects);
            testAndCreateImplicitObject(node, values);
            testAndCreateUndefinedChildren(node);

            MemoryEntry entry = Data.GetMemoryEntry(node.SourceIndex);
            copyEntryValues(entry, values, node.IsMust, true);
            Data.SetMemoryEntry(node.TargetIndex, Snapshot.CreateMemoryEntry(values));

            enqueueLocationChildNodes(node);
        }
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
                    CollectionTools.AddAll(aliases, memoryAlias.MustAliases);
                    CollectionTools.AddAll(aliases, memoryAlias.MayAliases);

                    foreach (MemoryIndex alias in aliases)
                    {
                        Snapshot.AddAlias(alias, null, node.TargetIndex);
                    }

                    Snapshot.MaySetAliases(node.TargetIndex, aliases);
                }
            }
        }

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

        private void processSourceObjects(MemoryCollectorNode node, IObjectValueContainer objects)
        {
            if (objects != null && objects.Count > 0)
            {
                IObjectValueContainer targetObjects = Snapshot.Structure.CreateObjectValueContainer(objects);
                Structure.SetObjects(node.TargetIndex, targetObjects);
            }
        }

        public void testAndCreateImplicitArray(MemoryCollectorNode node, ICollection<Value> values)
        {
            if (node.HasNewImplicitArray)
            {
                createArray(node, values);
            }
        }

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
