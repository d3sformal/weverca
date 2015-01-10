using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors
{
    enum VisitType
    {
        Must, May
    }

    abstract class CollectorNode
    {
        public CollectorNode Parent { get; private set; }

        public CollectorNode AnyChildNode { get; private set; }

        public Dictionary<string, CollectorNode> NamedChildNodes { get; private set; }

        public List<CollectorNode> ValueNodes { get; private set; }

        public List<CollectorNode> ChildNodes { get; private set; }

        public bool IsMust { get; set; }

        public bool HasOperations { get; protected set; }

        CollectorOperations operations;
        public CollectorOperations Operations
        {
            get { return operations; }
            set
            {
                operations = value;
                HasOperations = true;
            }
        }

        public CollectorNode()
        {
            NamedChildNodes = new Dictionary<string, CollectorNode>();
            ValueNodes = new List<CollectorNode>();
            ChildNodes = new List<CollectorNode>();
        }

        public abstract void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment);

        public abstract void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment);


        private void addChild(CollectorNode node, string name)
        {
            if (!NamedChildNodes.ContainsKey(name))
            {
                NamedChildNodes.Add(name, node);
                ChildNodes.Add(node);
            }
            else
            {
                throw new NotImplementedException("Visiting memory location more than once");
            }
        }

        private void addAnyChild(CollectorNode node)
        {
            if (AnyChildNode == null)
            {
                AnyChildNode = node;
                ChildNodes.Add(node);
            }
            else
            {
                throw new NotImplementedException("Visiting memory location more than once");
            }
        }

        private void addValueChild(CollectorNode node)
        {
            ValueNodes.Add(node);
            ChildNodes.Add(node);
        }

        #region Creating Children

        public CollectorNode CreateMemoryIndexChild(string name, MemoryIndex memoryIndex)
        {
            CollectorNode node = new MemoryIndexNode(memoryIndex);
            addChild(node, name);
            return node;
        }

        public CollectorNode CreateMemoryIndexChildFromAny(string name, MemoryIndex memoryIndex)
        {
            CollectorNode node = new MemoryIndexNode(memoryIndex);
            addChild(node, name);
            return node;
        }

        public CollectorNode CreateMemoryIndexAnyChild(MemoryIndex unknownIndex)
        {
            CollectorNode node = new MemoryIndexNode(unknownIndex);
            addAnyChild(node);
            return node;
        }

        public CollectorNode CreateUndefinedChild(string name)
        {
            CollectorNode node = new UndefinedNode();
            addChild(node, name);
            return node;
        }

        public CollectorNode CreateUndefinedAnyChild()
        {
            CollectorNode node = new UndefinedNode();
            addAnyChild(node);
            return node;
        }

        public CollectorNode CreateValueChild(ValueLocation location)
        {
            CollectorNode node = new ValueNode(location);
            addValueChild(node);
            return node;
        }

        #endregion

        public MemoryIndex MemoryIndex { get; set; }

        public bool CreateNewNode { get; set; }
    }

    class RootCollectorNode
    {
        public readonly Dictionary<int, ContainerCollectorNode> VariableStackNodes = 
            new Dictionary<int, ContainerCollectorNode>();
        public readonly Dictionary<int, ContainerCollectorNode> ControlStackNodes = 
            new Dictionary<int, ContainerCollectorNode>();
        public readonly Dictionary<MemoryIndex, MemoryIndexNode> TemporaryNodes = 
            new Dictionary<MemoryIndex, MemoryIndexNode>();
        public readonly Dictionary<ObjectValue, ContainerCollectorNode> ObjectNodes = 
            new Dictionary<ObjectValue, ContainerCollectorNode>();

        public bool HasRootNode { get; private set; }

        public void CollectVariable(TreeIndexCollector collector, VariablePathSegment variableSegment)
        {
            int currentCallLevel = collector.GetCurrentCallLevel();
            ContainerCollectorNode variableStackNode;
            if (!VariableStackNodes.TryGetValue(currentCallLevel, out variableStackNode))
            {
                IReadonlyIndexContainer indexContainer = collector.Structure
                    .GetReadonlyStackContext(currentCallLevel).ReadonlyVariables;

                variableStackNode = new ContainerCollectorNode(indexContainer);
                variableStackNode.Operations = new VariableOperations(variableStackNode);

                VariableStackNodes.Add(currentCallLevel, variableStackNode);
            }

            variableStackNode.Collect(collector, variableSegment);
            HasRootNode = true;
        }

        public void CollectControl(TreeIndexCollector collector, ControlPathSegment controlPathSegment)
        {
            int currentCallLevel = collector.GetCurrentCallLevel();
            ContainerCollectorNode controlStackNode;
            if (!ControlStackNodes.TryGetValue(currentCallLevel, out controlStackNode))
            {
                IReadonlyIndexContainer indexContainer = collector.Structure
                    .GetReadonlyStackContext(currentCallLevel).ReadonlyControllVariables;

                controlStackNode = new ContainerCollectorNode(indexContainer);
                controlStackNode.Operations = new ControlOperations(controlStackNode);

                ControlStackNodes.Add(currentCallLevel, controlStackNode);
            }

            controlStackNode.Collect(collector, controlPathSegment);
            HasRootNode = true;
        }

        public void CollectTemporary(TreeIndexCollector treeIndexCollector, TemporaryPathSegment temporaryPathSegment)
        {
            if (TemporaryNodes.ContainsKey(temporaryPathSegment.TemporaryIndex))
            {
                throw new NotImplementedException("Temporary memory index is visited more than once");
            }

            MemoryIndexNode node = new MemoryIndexNode(temporaryPathSegment.TemporaryIndex);
            node.IsMust = true;
            TemporaryNodes.Add(temporaryPathSegment.TemporaryIndex, node);
            treeIndexCollector.AddNode(node);
            HasRootNode = true;
        }

        public void CollectObject(TreeIndexCollector collector, ObjectValue objectValue, FieldPathSegment fieldPathSegment)
        {
            ContainerCollectorNode objectNode;
            if (!ObjectNodes.TryGetValue(objectValue, out objectNode))
            {
                IObjectDescriptor descriptor = collector.Structure.GetDescriptor(objectValue);
                objectNode = new ContainerCollectorNode(descriptor);
                objectNode.Operations = new CollectorOperations(objectNode);

                ObjectNodes.Add(objectValue, objectNode);
            }

            objectNode.Collect(collector, fieldPathSegment);
        }
    }

    class ContainerCollectorNode : CollectorNode
    {
        IReadonlyIndexContainer indexContainer;

        public ContainerCollectorNode(IReadonlyIndexContainer indexContainer)
        {
            this.indexContainer = indexContainer;
        }

        public void Collect(TreeIndexCollector collector, PathSegment pathSegment)
        {
            collector.CollectSegmentFromStructure(pathSegment, this, Operations, indexContainer, true);
        }

        public override void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment)
        {
            throw new InvalidOperationException();
        }

        public override void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment)
        {
            throw new InvalidOperationException();
        }
    }

    class MemoryIndexNode : CollectorNode
    {
        MemoryIndex memoryIndex;

        public MemoryIndexNode(MemoryIndex memoryIndex)
        {
            this.memoryIndex = memoryIndex;
        }

        public override void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment)
        {
            FieldOperations operations = new FieldOperations(this);
            Operations = operations;

            MemoryEntry entry = collector.GetMemoryEntry(memoryIndex);
            bool processOtherValues = false;

            IObjectValueContainer objects = collector.Structure.GetObjects(memoryIndex);
            if (objects.Count > 0)
            {
                processOtherValues = entry.Count > objects.Count
                    || entry.ContainsUndefinedValue && entry.Count > objects.Count + 1;

                bool isMustObject = objects.Count == 1 && !processOtherValues && !entry.ContainsUndefinedValue;
                foreach (var objectValue in objects)
                {
                    collector.RootNode.CollectObject(collector, objectValue, fieldSegment);
                }

                if (entry.ContainsUndefinedValue)
                {
                    operations.NewImplicitObject();
                    collector.CollectSegmentWithoutStructure(fieldSegment, this, operations, false);
                }
            }
            else if (entry.ContainsUndefinedValue)
            {
                processOtherValues = entry.Count > 1;

                operations.NewImplicitObject();
                collector.CollectSegmentWithoutStructure(fieldSegment, this, operations, !processOtherValues);
            }
            else
            {
                processOtherValues = true;
            }

            if (processOtherValues)
            {
                collector.CollectFieldSegmentFromValues(fieldSegment, this, operations, entry.PossibleValues, true);
            }
        }

        public override void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment)
        {
            IndexOperations operations = new IndexOperations(this);
            Operations = operations;

            MemoryEntry entry = collector.GetMemoryEntry(memoryIndex);
            bool processOtherValues = false;

            AssociativeArray arrayValue;
            if (collector.Structure.TryGetArray(memoryIndex, out arrayValue))
            {
                processOtherValues = entry.Count > 2 || entry.ContainsUndefinedValue && entry.Count > 1;

                IArrayDescriptor descriptor = collector.Structure.GetDescriptor(arrayValue);
                collector.CollectSegmentFromStructure(indexSegment, this, operations, descriptor, !processOtherValues);
            }
            else if (entry.ContainsUndefinedValue)
            {
                processOtherValues = entry.Count > 1;

                operations.NewImplicitArray();
                collector.CollectSegmentWithoutStructure(indexSegment, this, operations, !processOtherValues);
            }
            else
            {
                processOtherValues = true;
            }

            if (processOtherValues)
            {
                collector.CollectIndexSegmentFromValues(indexSegment, this, operations, entry.PossibleValues, true);
            }
        }
    }

    class UndefinedNode : CollectorNode
    {
        public override void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment)
        {
            FieldOperations operations = new FieldOperations(this);
            Operations = operations;

            operations.NewImplicitObject();
            collector.CollectSegmentWithoutStructure(fieldSegment, this, operations, true);
        }

        public override void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment)
        {
            IndexOperations operations = new IndexOperations(this);
            Operations = operations;

            operations.NewImplicitArray();
            collector.CollectSegmentWithoutStructure(indexSegment, this, operations, true);
        }
    }

    class ValueNode : CollectorNode
    {
        ValueLocation valueLocation;

        public ValueNode(ValueLocation valueLocation)
        {
            this.valueLocation = valueLocation;
        }

        public override void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment)
        {
            FieldOperations operations = new FieldOperations(this);
            Operations = operations;

            FieldLocationVisitor visitor = new FieldLocationVisitor(collector, fieldSegment, this, operations);
            valueLocation.Accept(visitor);
        }

        public override void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment)
        {
            IndexOperations operations = new IndexOperations(this);
            Operations = operations;

            IndexLocationVisitor visitor = new IndexLocationVisitor(collector, indexSegment, this, operations);
            valueLocation.Accept(visitor);
        }

        private class IndexLocationVisitor : ProcessValueAsLocationVisitor
        {
            private TreeIndexCollector collector;
            private IndexPathSegment indexSegment;
            private CollectorNode node;
            private IndexOperations operations;

            public IndexLocationVisitor(TreeIndexCollector collector, IndexPathSegment indexSegment,
                CollectorNode node, IndexOperations operations)
                : base(collector.Snapshot.MemoryAssistant)
            {
                this.collector = collector;
                this.indexSegment = indexSegment;
                this.node = node;
                this.operations = operations;
            }

            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                collector.CollectIndexSegmentFromValues(indexSegment, node, operations, values, isMust);
            }
        }

        private class FieldLocationVisitor : ProcessValueAsLocationVisitor
        {
            private TreeIndexCollector collector;
            private FieldPathSegment fieldSegment;
            private CollectorNode node;
            private FieldOperations operations;

            public FieldLocationVisitor(TreeIndexCollector collector, FieldPathSegment fieldSegment, 
                CollectorNode node, FieldOperations operations)
                : base(collector.Snapshot.MemoryAssistant)
            {
                this.collector = collector;
                this.fieldSegment = fieldSegment;
                this.node = node;
                this.operations = operations;
            }

            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                collector.CollectFieldSegmentFromValues(fieldSegment, node, operations, values, isMust);
            }
        }
    }
}
