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
    class LazyAssignWorker : LocationCollectorNodeVisitor
    {
        internal readonly Snapshot Snapshot;
        private MemoryEntryCollector memoryEntryCollector;
        private TreeIndexCollector treeCollector;

        internal readonly IWriteableSnapshotStructure Structure;
        internal readonly IWriteableSnapshotData Data;

        private LinkedList<LocationCollectorNode> collectorNodesQueue = new LinkedList<LocationCollectorNode>();
        private LinkedList<AssignOperation> operationQueue = new LinkedList<AssignOperation>();

        private AssignOperation currentOperation;
        private MemoryEntry assignedEntry;
        private bool forceStrongWrite;

        AssignValueLocationVisitor valueLocationVisitor;

        public LazyAssignWorker(Snapshot snapshot, MemoryEntryCollector memoryEntryCollector, 
            TreeIndexCollector treeCollector)
        {
            this.Snapshot = snapshot;
            this.memoryEntryCollector = memoryEntryCollector;
            this.treeCollector = treeCollector;

            this.Structure = snapshot.Structure.Writeable;
            this.Data = snapshot.CurrentData.Writeable;
        }

        public void Assign(MemoryEntry assignedEntry, bool forceStrongWrite)
        {
            valueLocationVisitor = new AssignValueLocationVisitor(Snapshot, assignedEntry, true);

            this.assignedEntry = assignedEntry;
            this.forceStrongWrite = forceStrongWrite;

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

            while (operationQueue.Count > 0)
            {
                AssignOperation operation = operationQueue.First.Value;
                operationQueue.RemoveFirst();

                operation.ProcessOperation();
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

        #region CollectorNode visitor

        public void VisitValueCollectorNode(ValueCollectorNode node)
        {
            if (node.IsCollected)
            {
                if (node.IsMust || forceStrongWrite)
                {
                    valueLocationVisitor.IsMust = true;
                    node.ValueLocation.ContainingIndex = node.TargetIndex;

                    node.ValueLocation.Accept(valueLocationVisitor);
                }
                else
                {
                    valueLocationVisitor.IsMust = false;
                    node.ValueLocation.ContainingIndex = node.TargetIndex;

                    node.ValueLocation.Accept(valueLocationVisitor);
                }
            }
            else
            {
                enqueueLocationChildNodes(node);
            }
        }

        public void VisitMemoryIndexCollectorNode(MemoryIndexCollectorNode node)
        {
            if (node.IsCollected)
            {
                if (node.IsMust || forceStrongWrite)
                {
                    AddOperation(new MemoryIndexMustAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, false));
                }
                else
                {
                    AddOperation(new MemoryIndexMayAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, false));
                }
            }
            else
            {
                HashSet<Value> values = new HashSet<Value>();

                testAndCreateImplicitArray(node, values);
                testAndCreateImplicitObject(node, values);
                testAndCreateUndefinedChildren(node);

                bool removeUndefined = node.IsMust && node.ContainsUndefinedValue;
                if (removeUndefined || values.Count > 0)
                {
                    MemoryEntry entry = Data.GetMemoryEntry(node.TargetIndex);

                    if (node.IsMust)
                    {
                        foreach (Value value in entry.PossibleValues)
                        {
                            if (!(value is UndefinedValue))
                            {
                                values.Add(value);
                            }
                        }
                    }
                    else
                    {
                        CollectionTools.AddAll(values, entry.PossibleValues);
                    }

                    Data.SetMemoryEntry(node.TargetIndex, Snapshot.CreateMemoryEntry(values));
                }

                enqueueLocationChildNodes(node);
            }
        }

        public void VisitUnknownIndexCollectorNode(UnknownIndexCollectorNode node)
        {

            if (node.IsCollected)
            {
                if (node.IsMust || forceStrongWrite)
                {
                    AddOperation(new UndefinedMustAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, false));
                }
                else
                {
                    AddOperation(new UnknownIndexMayAssign(this, node.SourceIndex, node.TargetIndex, memoryEntryCollector.RootNode, false));
                }
            }
            else
            {
                Structure.NewIndex(node.TargetIndex);

                HashSet<Value> values = new HashSet<Value>();

                IIndexDefinition definition = Structure.GetIndexDefinition(node.SourceIndex);

                if (definition.Aliases != null && definition.Aliases.HasAliases)
                {
                    if (node.IsMust)
                    {
                        foreach (MemoryIndex alias in definition.Aliases.MustAliases)
                        {
                            Snapshot.AddAlias(alias, node.TargetIndex, null);
                        }

                        foreach (MemoryIndex alias in definition.Aliases.MayAliases)
                        {
                            Snapshot.AddAlias(alias, null, node.TargetIndex);
                        }

                        Snapshot.MustSetAliases(node.TargetIndex, definition.Aliases.MustAliases, definition.Aliases.MayAliases);
                    }
                    else
                    {
                        HashSet<MemoryIndex> aliases = new HashSet<MemoryIndex>();
                        CollectionTools.AddAll(aliases, definition.Aliases.MustAliases);
                        CollectionTools.AddAll(aliases, definition.Aliases.MayAliases);

                        foreach (MemoryIndex alias in aliases)
                        {
                            Snapshot.AddAlias(alias, null, node.TargetIndex);
                        }

                        Snapshot.MaySetAliases(node.TargetIndex, aliases);
                    }
                }

                if (definition.Array != null)
                {
                    IArrayDescriptor sourceArray = Structure.GetDescriptor(definition.Array);

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

                if (definition.Objects != null && definition.Objects.Count > 0)
                {
                    IObjectValueContainer objects = Snapshot.Structure.CreateObjectValueContainer(definition.Objects);
                    Structure.SetObjects(node.TargetIndex, objects);
                }

                testAndCreateImplicitObject(node, values);
                testAndCreateUndefinedChildren(node);

                MemoryEntry entry = Data.GetMemoryEntry(node.SourceIndex);

                if (node.IsMust)
                {
                    foreach (Value value in entry.PossibleValues)
                    {
                        if (!(value is UndefinedValue || value is AssociativeArray))
                        {
                            values.Add(value);
                        }
                    }
                }
                else
                {
                    foreach (Value value in entry.PossibleValues)
                    {
                        if (!(value is AssociativeArray))
                        {
                            values.Add(value);
                        }
                    }
                }

                Data.SetMemoryEntry(node.TargetIndex, Snapshot.CreateMemoryEntry(values));

                enqueueLocationChildNodes(node);
            }
        }

        public void VisitUndefinedCollectorNode(UndefinedCollectorNode node)
        {
            Structure.NewIndex(node.TargetIndex);

            if (node.IsCollected)
            {
                if (node.IsMust || forceStrongWrite)
                {
                    AddOperation(new UndefinedMustAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, false));
                }
                else
                {
                    AddOperation(new UndefinedMayAssignOperation(this, node.TargetIndex, memoryEntryCollector.RootNode, false));
                }
            }
            else
            {
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
        }

        #endregion

        #region Processing

        public void testAndCreateImplicitArray(MemoryCollectorNode node, HashSet<Value> values)
        {
            if (node.HasNewImplicitArray)
            {
                createArray(node, values);
            }
        }

        public AssociativeArray createArray(MemoryCollectorNode node, HashSet<Value> values)
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

        internal void AddOperation(AssignOperation operation)
        {
            operationQueue.AddLast(operation);
        }
    }
}
