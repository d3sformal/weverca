using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors
{
    /// <summary>
    /// Defines visitor interface to perform operation on the collector nodes.
    /// </summary>
    interface LocationCollectorNodeVisitor
    {
        /// <summary>
        /// Visits the value collector node.
        /// </summary>
        /// <param name="valueCollectorNode">The value collector node.</param>
        void VisitValueCollectorNode(ValueCollectorNode valueCollectorNode);

        /// <summary>
        /// Visits the memory index collector node.
        /// </summary>
        /// <param name="memoryIndexCollectorNode">The memory index collector node.</param>
        void VisitMemoryIndexCollectorNode(MemoryIndexCollectorNode memoryIndexCollectorNode);

        /// <summary>
        /// Visits the unknown index collector node.
        /// </summary>
        /// <param name="unknownIndexCollectorNode">The unknown index collector node.</param>
        void VisitUnknownIndexCollectorNode(UnknownIndexCollectorNode unknownIndexCollectorNode);

        /// <summary>
        /// Visits the undefined collector node.
        /// </summary>
        /// <param name="undefinedCollectorNode">The undefined collector node.</param>
        void VisitUndefinedCollectorNode(UndefinedCollectorNode undefinedCollectorNode);
    }

    /// <summary>
    /// Abstract class with common functionality for nodes of the collecting tree
    /// </summary>
    abstract class CollectorNode
    {
        /// <summary>
        /// Gets the list of all child nodes.
        /// </summary>
        /// <value>
        /// The child nodes.
        /// </value>
        public List<LocationCollectorNode> ChildNodes { get; private set; }

        /// <summary>
        /// Gets any child node.
        /// </summary>
        /// <value>
        /// Any child node.
        /// </value>
        public MemoryCollectorNode AnyChildNode { get; private set; }

        /// <summary>
        /// Gets the list of child nodes with names.
        /// </summary>
        /// <value>
        /// The named child nodes.
        /// </value>
        public Dictionary<string, MemoryCollectorNode> NamedChildNodes { get; private set; }

        /// <summary>
        /// Gets the list of child nodes which are undefined.
        /// </summary>
        /// <value>
        /// The undefined children.
        /// </value>
        public List<Tuple<string, MemoryCollectorNode>> UndefinedChildren { get; private set; }

        /// <summary>
        /// Gets the vlist of child value nodes.
        /// </summary>
        /// <value>
        /// The value nodes.
        /// </value>
        public List<ValueCollectorNode> ValueNodes { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is must.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is must; otherwise, <c>false</c>.
        /// </value>
        public bool IsMust { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is collected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is collected; otherwise, <c>false</c>.
        /// </value>
        public bool IsCollected { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has undefined children.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has undefined children; otherwise, <c>false</c>.
        /// </value>
        public bool HasUndefinedChildren { get { return UndefinedChildren != null && UndefinedChildren.Count > 0; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorNode"/> class.
        /// </summary>
        public CollectorNode()
        {
            ChildNodes = new List<LocationCollectorNode>();
        }

        #region Children operations

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="System.NotImplementedException">Visiting memory location more than once</exception>
        public void addChild(MemoryCollectorNode node, string name)
        {
            if (NamedChildNodes == null)
            {
                NamedChildNodes = new Dictionary<string, MemoryCollectorNode>();
                UndefinedChildren = new List<Tuple<string, MemoryCollectorNode>>();
            }

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

        /// <summary>
        /// Adds any child.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <exception cref="System.NotImplementedException">Visiting memory location more than once</exception>
        public void addAnyChild(MemoryCollectorNode node)
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

        /// <summary>
        /// Adds the value child.
        /// </summary>
        /// <param name="node">The node.</param>
        public void addValueChild(ValueCollectorNode node)
        {
            if (ValueNodes == null)
            {
                ValueNodes = new List<ValueCollectorNode>();
            }

            ValueNodes.Add(node);
            ChildNodes.Add(node);
        }

        /// <summary>
        /// Creates the memory index child.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="memoryIndex">Index of the memory.</param>
        /// <returns>Child node</returns>
        public virtual LocationCollectorNode CreateMemoryIndexChild(string name, MemoryIndex memoryIndex)
        {
            MemoryIndexCollectorNode node = new MemoryIndexCollectorNode(memoryIndex);
            addChild(node, name);
            return node;
        }

        /// <summary>
        /// Creates the memory index child from any.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <returns>Child node</returns>
        public virtual MemoryCollectorNode CreateMemoryIndexChildFromAny(string name, MemoryIndex sourceIndex)
        {
            UnknownIndexCollectorNode node = new UnknownIndexCollectorNode(sourceIndex);
            addChild(node, name);
            this.UndefinedChildren.Add(new Tuple<string, MemoryCollectorNode>(name, node));
            return node;
        }

        /// <summary>
        /// Creates the memory index any child.
        /// </summary>
        /// <param name="unknownIndex">Index of the unknown.</param>
        /// <returns>Child node</returns>
        public virtual LocationCollectorNode CreateMemoryIndexAnyChild(MemoryIndex unknownIndex)
        {
            MemoryIndexCollectorNode node = new MemoryIndexCollectorNode(unknownIndex);
            addAnyChild(node);
            return node;
        }

        /// <summary>
        /// Creates the undefined child.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Child node</returns>
        public virtual LocationCollectorNode CreateUndefinedChild(string name)
        {
            UndefinedCollectorNode node = new UndefinedCollectorNode();
            addChild(node, name);
            this.UndefinedChildren.Add(new Tuple<string, MemoryCollectorNode>(name, node));
            return node;
        }

        /// <summary>
        /// Creates the undefined any child.
        /// </summary>
        /// <returns>Child node</returns>
        public virtual LocationCollectorNode CreateUndefinedAnyChild()
        {
            UndefinedCollectorNode node = new UndefinedCollectorNode();
            addAnyChild(node);
            return node;
        }

        /// <summary>
        /// Creates the value child.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>Child node</returns>
        public virtual LocationCollectorNode CreateValueChild(ValueLocation location)
        {
            ValueCollectorNode node = new ValueCollectorNode(location);
            addValueChild(node);
            return node;
        }

        #endregion
    }

    /// <summary>
    /// Node of the collecting tree which represents some container at the root of the memory tree.
    /// </summary>
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

    /// <summary>
    /// Node of the collecting tree which represents missing object which has to be implicitly created.
    /// </summary>
    class ImplicitObjectCollectorNode : CollectorNode
    {
        /// <summary>
        /// Gets or sets the object value.
        /// </summary>
        /// <value>
        /// The object value.
        /// </value>
        public ObjectValue ObjectValue { get; set; }
    }

    /// <summary>
    /// Abstract class with common functionality for nodes representing memory location or value.
    /// </summary>
    abstract class LocationCollectorNode : CollectorNode
    {
        /// <summary>
        /// Gets or sets the memory index represented by this node.
        /// </summary>
        /// <value>
        /// The tarhet memory index.
        /// </value>
        public MemoryIndex TargetIndex { get; set; }

        /// <summary>
        /// Collects all child nodes by field access.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="fieldSegment">The field segment.</param>
        public abstract void CollectField(TreeIndexCollector collector, FieldPathSegment fieldSegment);

        /// <summary>
        /// Collects all nodes by index access.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="indexSegment">The index segment.</param>
        public abstract void CollectIndex(TreeIndexCollector collector, IndexPathSegment indexSegment);

        /// <summary>
        /// Collects all aliases to target node.
        /// </summary>
        /// <param name="collector">The collector.</param>
        public abstract void CollectAliases(TreeIndexCollector collector);

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public abstract void Accept(LocationCollectorNodeVisitor visitor);
    }

    /// <summary>
    /// Node of the collecting tree which represents a value.
    /// </summary>
    class ValueCollectorNode : LocationCollectorNode
    {
        /// <summary>
        /// Gets the value stored within this node
        /// </summary>
        /// <value>
        /// The value location.
        /// </value>
        public ValueLocation ValueLocation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCollectorNode"/> class.
        /// </summary>
        /// <param name="valueLocation">The value location.</param>
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

    /// <summary>
    /// Abstract class with common functionality for nodes representing collected indexes within memory tree.
    /// </summary>
    abstract class MemoryCollectorNode : LocationCollectorNode
    {
        /// <summary>
        /// Gets or sets the index which contains the source data for this node. MAy be an undefined index.
        /// </summary>
        /// <value>
        /// The source index.
        /// </value>
        public MemoryIndex SourceIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the source index contains undefined value.
        /// </summary>
        /// <value>
        /// <c>true</c> if the source index contains undefined value; otherwise, <c>false</c>.
        /// </value>
        public bool ContainsUndefinedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has new implicit array.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has new implicit array; otherwise, <c>false</c>.
        /// </value>
        public bool HasNewImplicitArray { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has new implicit object.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has new implicit object; otherwise, <c>false</c>.
        /// </value>
        public bool HasNewImplicitObject { get; set; }

        /// <summary>
        /// Gets or sets the implicit object node.
        /// </summary>
        /// <value>
        /// The implicit object node.
        /// </value>
        public ImplicitObjectCollectorNode ImplicitObjectNode { get; set; }

        /// <summary>
        /// Adds the new implicit object node.
        /// </summary>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public void AddNewImplicitObjectNode(bool isMust)
        {
            if (!this.HasNewImplicitObject)
            {
                HasNewImplicitObject = true;
                ImplicitObjectNode = new ImplicitObjectCollectorNode();
                ImplicitObjectNode.IsMust = this.IsMust && isMust;
            }
        }

        /// <summary>
        /// Collects the index of the field from memory.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="fieldSegment">The field segment.</param>
        /// <param name="index">The index.</param>
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

        /// <summary>
        /// Collects the index of the index from memory.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="indexSegment">The index segment.</param>
        /// <param name="index">The index.</param>
        protected void CollectIndexFromMemoryIndex(TreeIndexCollector collector, IndexPathSegment indexSegment, MemoryIndex index)
        {
            MemoryEntry entry = collector.GetMemoryEntry(index);
            this.ContainsUndefinedValue = entry.ContainsUndefinedValue;
            bool processOtherValues = false;

            AssociativeArray arrayValue;
            if (collector.Structure.TryGetArray(index, out arrayValue))
            {
                if (entry.ContainsUndefinedValue)
                {
                    processOtherValues = entry.Count > 2;
                }
                else
                {
                    processOtherValues = entry.Count > 1;
                }

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

        /// <summary>
        /// Collects the index of the aliases from memory.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="index">The index.</param>
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

    /// <summary>
    /// Node of the collecting tree which represents collected index.
    /// </summary>
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

    /// <summary>
    /// Node of the collecting tree which represents collected unknown index.
    /// </summary>
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

    /// <summary>
    /// Node of the collecting tree which represents a missing memory index. Assign will use all 
    /// informatins to create the new index.
    /// </summary>
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
