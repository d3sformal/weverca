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
    interface LocationCollectorNodeVisitor
    {
        void VisitValueCollectorNode(ValueCollectorNode valueCollectorNode);
        
        void VisitMemoryIndexCollectorNode(MemoryIndexCollectorNode memoryIndexCollectorNode);

        void VisitUnknownIndexCollectorNode(UnknownIndexCollectorNode unknownIndexCollectorNode);

        void VisitUndefinedCollectorNode(UndefinedCollectorNode undefinedCollectorNode);
    }

    abstract class CollectorNode
    {
        public CollectorNode Parent { get; private set; }

        public List<LocationCollectorNode> ChildNodes { get; private set; }

        public MemoryCollectorNode AnyChildNode { get; private set; }

        public Dictionary<string, MemoryCollectorNode> NamedChildNodes { get; private set; }

        public List<Tuple<string, MemoryCollectorNode>> UndefinedChildren { get; private set; }

        public List<ValueCollectorNode> ValueNodes { get; private set; }

        public bool IsMust { get; set; }

        public bool IsCollected { get; set; }

        public bool HasUndefinedChildren { get { return UndefinedChildren.Count > 0; } }

        public CollectorNode()
        {
            ChildNodes = new List<LocationCollectorNode>();
            NamedChildNodes = new Dictionary<string, MemoryCollectorNode>();
            UndefinedChildren = new List<Tuple<string, MemoryCollectorNode>>();
            ValueNodes = new List<ValueCollectorNode>();
        }

        #region Children operations
        
        public void addChild(MemoryCollectorNode node, string name)
        {
            if (!NamedChildNodes.ContainsKey(name))
            {
                NamedChildNodes.Add(name, node);
                ChildNodes.Add(node);
                node.Parent = this;
            }
            else
            {
                throw new NotImplementedException("Visiting memory location more than once");
            }
        }

        public void addAnyChild(MemoryCollectorNode node)
        {
            if (AnyChildNode == null)
            {
                AnyChildNode = node;
                ChildNodes.Add(node);
                node.Parent = this;
            }
            else
            {
                throw new NotImplementedException("Visiting memory location more than once");
            }
        }

        public void addValueChild(ValueCollectorNode node)
        {
            ValueNodes.Add(node);
            ChildNodes.Add(node);
            node.Parent = this;
        }

        public virtual LocationCollectorNode CreateMemoryIndexChild(string name, MemoryIndex memoryIndex)
        {
            MemoryIndexCollectorNode node = new MemoryIndexCollectorNode(memoryIndex);
            addChild(node, name);
            return node;
        }

        public virtual LocationCollectorNode CreateMemoryIndexChildFromAny(string name, MemoryIndex sourceIndex)
        {
            UnknownIndexCollectorNode node = new UnknownIndexCollectorNode(sourceIndex);
            addChild(node, name);
            this.UndefinedChildren.Add(new Tuple<string, MemoryCollectorNode>(name, node));
            return node;
        }

        public virtual LocationCollectorNode CreateMemoryIndexAnyChild(MemoryIndex unknownIndex)
        {
            MemoryIndexCollectorNode node = new MemoryIndexCollectorNode(unknownIndex);
            addAnyChild(node);
            return node;
        }

        public virtual LocationCollectorNode CreateUndefinedChild(string name)
        {
            UndefinedCollectorNode node = new UndefinedCollectorNode();
            addChild(node, name);
            this.UndefinedChildren.Add(new Tuple<string, MemoryCollectorNode>(name, node));
            return node;
        }

        public virtual LocationCollectorNode CreateUndefinedAnyChild()
        {
            UndefinedCollectorNode node = new UndefinedCollectorNode();
            addAnyChild(node);
            return node;
        }

        public virtual LocationCollectorNode CreateValueChild(ValueLocation location)
        {
            ValueCollectorNode node = new ValueCollectorNode(location);
            addValueChild(node);
            return node;
        }

        #endregion
    }

    class ContainerCollectorNode : CollectorNode
    {
        IReadonlyIndexContainer indexContainer;

        public ContainerCollectorNode(IReadonlyIndexContainer indexContainer)
        {
            this.indexContainer = indexContainer;
            IsMust = true;
        }

        public void Collect(TreeIndexCollector collector, PathSegment pathSegment)
        {
            collector.CollectSegmentFromStructure(pathSegment, this, indexContainer, true);
        }
    }

    class ImplicitObjectCollectorNode : CollectorNode
    {
        public ObjectValue ObjectValue { get; set; }
    }

    abstract class LocationCollectorNode : CollectorNode
    {
        public MemoryIndex TargetIndex { get; set; }

        public abstract void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment);

        public abstract void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment);

        public abstract void CollectAliases(TreeIndexCollector collector);

        public abstract void Accept(LocationCollectorNodeVisitor visitor);
    }

    class ValueCollectorNode : LocationCollectorNode
    {
        public ValueLocation ValueLocation { get; private set; }

        public ValueCollectorNode(ValueLocation valueLocation)
        {
            this.ValueLocation = valueLocation;
        }

        public override void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment)
        {
            FieldLocationVisitor visitor = new FieldLocationVisitor(collector, fieldSegment, this);
            ValueLocation.Accept(visitor);
        }

        public override void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment)
        {
            IndexLocationVisitor visitor = new IndexLocationVisitor(collector, indexSegment, this);
            ValueLocation.Accept(visitor);
        }

        private class IndexLocationVisitor : ProcessValueAsLocationVisitor
        {
            private TreeIndexCollector collector;
            private IndexPathSegment indexSegment;
            private ValueCollectorNode node;

            public IndexLocationVisitor(TreeIndexCollector collector, IndexPathSegment indexSegment,
                ValueCollectorNode node)
                : base(collector.Snapshot.MemoryAssistant)
            {
                this.collector = collector;
                this.indexSegment = indexSegment;
                this.node = node;
            }

            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                collector.CollectIndexSegmentFromValues(indexSegment, node, values, isMust);
            }
        }

        private class FieldLocationVisitor : ProcessValueAsLocationVisitor
        {
            private TreeIndexCollector collector;
            private FieldPathSegment fieldSegment;
            private ValueCollectorNode node;

            public FieldLocationVisitor(TreeIndexCollector collector, FieldPathSegment fieldSegment,
                ValueCollectorNode node)
                : base(collector.Snapshot.MemoryAssistant)
            {
                this.collector = collector;
                this.fieldSegment = fieldSegment;
                this.node = node;
            }

            public override void ProcessValues(MemoryIndex parentIndex, IEnumerable<Value> values, bool isMust)
            {
                collector.CollectFieldSegmentFromValues(fieldSegment, node, values, isMust);
            }
        }

        public override void Accept(LocationCollectorNodeVisitor visitor)
        {
            visitor.VisitValueCollectorNode(this);
        }

        public override void CollectAliases(TreeIndexCollector collector)
        {
            // Do nothing - values can not have any alias
        }
    }

    abstract class MemoryCollectorNode : LocationCollectorNode
    {
        public MemoryIndex SourceIndex { get; set; }

        public bool ContainsUndefinedValue { get; set; }

        public bool HasNewImplicitArray { get; set; }

        public bool HasNewImplicitObject { get; set; }

        public ImplicitObjectCollectorNode ImplicitObjectNode { get; set; }

        public void AddNewImplicitObjectNode(bool isMust)
        {
            if (!this.HasNewImplicitObject)
            {
                HasNewImplicitObject = true;
                ImplicitObjectNode = new ImplicitObjectCollectorNode();
                ImplicitObjectNode.IsMust = this.IsMust && isMust;
            }
        }

        protected void collectFieldFromMemoryIndex(TreeIndexCollector collector, FieldPathSegment fieldSegment, MemoryIndex index)
        {
            MemoryEntry entry = collector.GetMemoryEntry(index);
            this.ContainsUndefinedValue = entry.ContainsUndefinedValue;
            bool processOtherValues = false;

            IObjectValueContainer objects = collector.Structure.GetObjects(index);
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
                    AddNewImplicitObjectNode(false);
                    collector.CollectSegmentWithoutStructure(fieldSegment, ImplicitObjectNode, false);
                }
            }
            else if (entry.ContainsUndefinedValue)
            {
                processOtherValues = entry.Count > 1;

                AddNewImplicitObjectNode(!processOtherValues);
                collector.CollectSegmentWithoutStructure(fieldSegment, ImplicitObjectNode, !processOtherValues);
            }
            else
            {
                processOtherValues = true;
            }

            if (processOtherValues)
            {
                collector.CollectFieldSegmentFromValues(fieldSegment, this, entry.PossibleValues, true);
            }
        }

        protected void CollectIndexFromMemoryIndex(TreeIndexCollector collector, IndexPathSegment indexSegment, MemoryIndex index)
        {
            MemoryEntry entry = collector.GetMemoryEntry(index);
            this.ContainsUndefinedValue = entry.ContainsUndefinedValue;
            bool processOtherValues = false;

            AssociativeArray arrayValue;
            if (collector.Structure.TryGetArray(index, out arrayValue))
            {
                processOtherValues = entry.Count > 1 || entry.ContainsUndefinedValue && entry.Count > 2;

                IArrayDescriptor descriptor = collector.Structure.GetDescriptor(arrayValue);
                collector.CollectSegmentFromStructure(indexSegment, this, descriptor, !processOtherValues);
            }
            else if (entry.ContainsUndefinedValue)
            {
                processOtherValues = entry.Count > 1;

                this.HasNewImplicitArray = true;
                collector.CollectSegmentWithoutStructure(indexSegment, this, !processOtherValues);
            }
            else
            {
                processOtherValues = true;
            }

            if (processOtherValues)
            {
                collector.CollectIndexSegmentFromValues(indexSegment, this, entry.PossibleValues, true);
            }
        }

        protected void CollectAliasesFromMemoryIndex(TreeIndexCollector collector, MemoryIndex index)
        {
            IMemoryAlias aliases;
            if (collector.Structure.TryGetAliases(index, out aliases))
            {
                foreach (MemoryIndex alias in aliases.MustAliases)
                {
                    collector.CollectAlias(this, alias, IsMust);
                }
                foreach (MemoryIndex alias in aliases.MayAliases)
                {
                    collector.CollectAlias(this, alias, false);
                }
            }
        }
    }

    class MemoryIndexCollectorNode : MemoryCollectorNode
    {
        MemoryIndex memoryIndex;

        public MemoryIndexCollectorNode(MemoryIndex memoryIndex)
        {
            SetMemoryIndex(memoryIndex);
        }

        internal void SetMemoryIndex(MemoryIndex memoryIndex)
        {
            this.memoryIndex = memoryIndex;
            TargetIndex = memoryIndex;
            SourceIndex = memoryIndex;
        }

        public override void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment)
        {
            collectFieldFromMemoryIndex(collector, fieldSegment, memoryIndex);
        }

        public override void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment)
        {
            CollectIndexFromMemoryIndex(collector, indexSegment, memoryIndex);
        }

        public override void CollectAliases(TreeIndexCollector collector)
        {
            CollectAliasesFromMemoryIndex(collector, memoryIndex);
        }

        public override void Accept(LocationCollectorNodeVisitor visitor)
        {
            visitor.VisitMemoryIndexCollectorNode(this);
        }
    }

    class UnknownIndexCollectorNode : MemoryCollectorNode
    {
        MemoryIndex sourceMemoryIndex;

        public UnknownIndexCollectorNode(MemoryIndex sourceMemoryIndex)
        {
            this.sourceMemoryIndex = sourceMemoryIndex;
            SourceIndex = sourceMemoryIndex;
        }

        public override void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment)
        {
            collectFieldFromMemoryIndex(collector, fieldSegment, sourceMemoryIndex);
        }

        public override void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment)
        {
            CollectIndexFromMemoryIndex(collector, indexSegment, sourceMemoryIndex);
        }

        public override void CollectAliases(TreeIndexCollector collector)
        {
            CollectAliasesFromMemoryIndex(collector, sourceMemoryIndex);
        }

        public override LocationCollectorNode CreateMemoryIndexChild(string name, MemoryIndex memoryIndex)
        {
            UnknownIndexCollectorNode node = new UnknownIndexCollectorNode(memoryIndex);
            addChild(node, name);
            this.UndefinedChildren.Add(new Tuple<string, MemoryCollectorNode>(name, node));
            return node;
        }

        public override LocationCollectorNode CreateMemoryIndexAnyChild(MemoryIndex unknownIndex)
        {
            UnknownIndexCollectorNode node = new UnknownIndexCollectorNode(unknownIndex);
            addAnyChild(node);
            return node;
        }

        public override void Accept(LocationCollectorNodeVisitor visitor)
        {
            visitor.VisitUnknownIndexCollectorNode(this);
        }
    }

    class UndefinedCollectorNode : MemoryCollectorNode
    {

        public override void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment)
        {
            AddNewImplicitObjectNode(true);
            collector.CollectSegmentWithoutStructure(fieldSegment, ImplicitObjectNode, true);
        }

        public override void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment)
        {
            HasNewImplicitArray = true;
            collector.CollectSegmentWithoutStructure(indexSegment, this, true);
        }

        public override void CollectAliases(TreeIndexCollector collector)
        {
            // Do nothing - undefined index can not have any alias
        }

        public override void Accept(LocationCollectorNodeVisitor visitor)
        {
            visitor.VisitUndefinedCollectorNode(this);
        }
    }
}
