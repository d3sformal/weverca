using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors
{
    /// <summary>
    /// This is the collecting class for lazy assign algorithm.
    /// 
    /// Instances are used for preparing all data which will be assigned. Instance will go thru whole 
    /// sub trees of all assigned arrays and builds tree representation of merged data. Produced subtree 
    /// is used within the assign algorithm to store assigned value into collected locations.
    /// </summary>
    class MemoryEntryCollector
    {
        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <value>
        /// The snapshot.
        /// </value>
        public Snapshot Snapshot { get; private set; }
        /// <summary>
        /// Gets the structure.
        /// </summary>
        /// <value>
        /// The structure.
        /// </value>
        public IReadOnlySnapshotStructure Structure { get; private set; }
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public IReadOnlySnapshotData Data { get; private set; }

        /// <summary>
        /// Gets or sets the root node.
        /// </summary>
        /// <value>
        /// The root node.
        /// </value>
        public MemoryEntryCollectorNode RootNode { get; set; }

        /// <summary>
        /// Gets the root memory entry.
        /// </summary>
        /// <value>
        /// The root memory entry.
        /// </value>
        public MemoryEntry RootMemoryEntry { get; private set; }

        /// <summary>
        /// The collecting queue
        /// </summary>
        public LinkedList<MemoryEntryCollectorNode> collectingQueue = new LinkedList<MemoryEntryCollectorNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryEntryCollector"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public MemoryEntryCollector(Snapshot snapshot)
        {
            this.Snapshot = snapshot;
            this.Structure = snapshot.Structure.Readonly;
            this.Data = snapshot.Data.Readonly;
        }

        /// <summary>
        /// Processes the root memory entry. This is the beginning of an computation which creates the root node from given memory entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void ProcessRootMemoryEntry(MemoryEntry entry)
        {
            RootNode = new MemoryEntryCollectorNode();
            RootNode.IsMust = true;
            RootNode.CollectValuesFromMemoryEntry(entry);
            RootNode.CollectChildren(this);

            processQueue();
            RootMemoryEntry = entry;

        }

        /// <summary>
        /// Processes the root indexes. This is the beginning of an computation which creates the root node from given sets of indexes.
        /// Used to collect all data from aliased locations.
        /// </summary>
        /// <param name="mustIndexes">The must indexes.</param>
        /// <param name="mayIndexes">The may indexes.</param>
        /// <param name="values">The values.</param>
        public void ProcessRootIndexes(HashSet<MemoryIndex> mustIndexes, HashSet<MemoryIndex> mayIndexes, IEnumerable<Value> values)
        {
            RootNode = new MemoryEntryCollectorNode();
            RootNode.SourceIndexes = new List<SourceIndex>();
            RootNode.IsMust = true;

            foreach (MemoryIndex index in mustIndexes)
            {
                RootNode.SourceIndexes.Add(new SourceIndex(index, Snapshot));
                RootNode.CollectAliases(index, Snapshot, true);
            }
            foreach (MemoryIndex index in mayIndexes)
            {
                RootNode.SourceIndexes.Add(new SourceIndex(index, Snapshot));
                RootNode.CollectAliases(index, Snapshot, false);
            }

            RootNode.CollectValuesFromSources(this);
            RootNode.VisitValues(values);
            RootNode.CollectChildren(this);


            processQueue();
            RootMemoryEntry = RootNode.GenerateMemeoryEntry(values);
        }

        /// <summary>
        /// Adds the node to the collecting queue.
        /// </summary>
        /// <param name="node">The node.</param>
        public void AddNode(MemoryEntryCollectorNode node)
        {
            collectingQueue.AddLast(node);
        }

        /// <summary>
        /// Process all nodes stored in the queue to build the tree.
        /// </summary>
        private void processQueue()
        {
            while (collectingQueue.Count > 0)
            {
                MemoryEntryCollectorNode node = collectingQueue.First.Value;
                collectingQueue.RemoveFirst();

                node.CollectAliasesFromSources(this);
                node.CollectValuesFromSources(this);
                node.CollectChildren(this);
            }
        }
    }


    class MemoryEntryCollectorNode : AbstractValueVisitor
    {
        private static MemoryEntryCollectorNode emptyNode = null;


        /// <summary>
        /// Produce an empty node with no inner data an no childs. This node can be used as 
        /// an unknown node in the assign algorithm.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>An empty node.</returns>
        public static MemoryEntryCollectorNode GetEmptyNode(Snapshot snapshot)
        {
            if (emptyNode != null)
            {
                return emptyNode;
            }
            else
            {
                emptyNode = new MemoryEntryCollectorNode();
                emptyNode.ScalarValues = new HashSet<Value>();
                emptyNode.ScalarValues.Add(snapshot.UndefinedValue);

                return emptyNode;
            }
        }



        /// <summary>
        /// Gets or sets a value indicating whether this instance is must.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is must; otherwise, <c>false</c>.
        /// </value>
        public bool IsMust { get; set; }

        /// <summary>
        /// Gets or sets the list of source indexes.
        /// </summary>
        /// <value>
        /// The list of source indexes.
        /// </value>
        public List<SourceIndex> SourceIndexes { get; set; }

        /// <summary>
        /// Gets the scalar values stored at this node.
        /// </summary>
        /// <value>
        /// The scalar values.
        /// </value>
        public HashSet<Value> ScalarValues { get; private set; }

        /// <summary>
        /// Gets the arrays.
        /// </summary>
        /// <value>
        /// The arrays.
        /// </value>
        public HashSet<AssociativeArray> Arrays { get; private set; }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        public HashSet<ObjectValue> Objects { get; private set; }

        /// <summary>
        /// Gets or sets the collection named children.
        /// </summary>
        /// <value>
        /// The named children.
        /// </value>
        public Dictionary<string, MemoryEntryCollectorNode> NamedChildren { get; set; }

        /// <summary>
        /// Gets or sets any child.
        /// </summary>
        /// <value>
        /// Any child.
        /// </value>
        public MemoryEntryCollectorNode AnyChild { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of all aliases to the merged location.
        /// </summary>
        /// <value>
        /// The references.
        /// </value>
        public ReferenceCollector References { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has aliases.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has aliases; otherwise, <c>false</c>.
        /// </value>
        public bool HasAliases { get { return References != null && References.HasAliases; } }

        private bool hasArray;
        private bool mustHaveArray;

        public MemoryEntry GenerateMemeoryEntry(IEnumerable<Value> values)
        {
            List<Value> entryValues = new List<Value>(values);
            
            if (ScalarValues != null)
            {
                CollectionMemoryUtils.AddAll(entryValues, ScalarValues);
            }
            if (Arrays != null)
            {
                CollectionMemoryUtils.AddAll(entryValues, Arrays);
            }
            if (Objects != null)
            {
                CollectionMemoryUtils.AddAll(entryValues, Objects);
            }

            return new MemoryEntry(entryValues);
        }

        public void CollectChildren(MemoryEntryCollector collector)
        {
            if (Arrays != null && Arrays.Count > 0)
            {
                // Create new collection for named children
                NamedChildren = new Dictionary<string, MemoryEntryCollectorNode>();

                // Any child node
                AnyChild = new MemoryEntryCollectorNode();
                AnyChild.SourceIndexes = new List<SourceIndex>();
                AnyChild.IsMust = false;
                collector.AddNode(AnyChild);

                // Collect child names and adds any child sources
                HashSet<string> names = new HashSet<string>();
                List<Tuple<Snapshot, IArrayDescriptor>> sourceDescriptors = new List<Tuple<Snapshot, IArrayDescriptor>>();
                foreach (AssociativeArray arrayValue in Arrays)
                {
                    var descriptors = getIArrayDescriptors(collector, arrayValue);
                    foreach (var tuple in descriptors)
                    {
                        sourceDescriptors.Add(tuple);

                        Snapshot sourceSnapshot = tuple.Item1;
                        IArrayDescriptor descriptor = tuple.Item2;

                        AnyChild.SourceIndexes.Add(new SourceIndex(descriptor.UnknownIndex, sourceSnapshot));
                        foreach (var item in descriptor.Indexes)
                        {
                            names.Add(item.Key);
                        }
                    }
                }

                // Test whether new array as MAY or MUST
                bool mustHaveChildren = this.IsMust && mustHaveArray;
                mustHaveChildren &= (ScalarValues == null || ScalarValues.Count == 0);
                mustHaveChildren &= (Objects == null || Objects.Count == 0);

                // Iterates collected names and stors them into the structure
                foreach (string name in names)
                {
                    MemoryEntryCollectorNode childNode = new MemoryEntryCollectorNode();
                    childNode.SourceIndexes = new List<SourceIndex>();
                    childNode.IsMust = mustHaveChildren;
                    collector.AddNode(childNode);
                    NamedChildren.Add(name, childNode);

                    // Collect sources for named child
                    foreach (var tuple in sourceDescriptors)
                    {
                        Snapshot sourceSnapshot = tuple.Item1;
                        IArrayDescriptor descriptor = tuple.Item2;

                        MemoryIndex index;
                        if (descriptor.TryGetIndex(name, out index))
                        {
                            childNode.SourceIndexes.Add(new SourceIndex(index, sourceSnapshot));
                        }
                        else
                        {
                            childNode.IsMust = false;
                            childNode.SourceIndexes.Add(new SourceIndex(descriptor.UnknownIndex, sourceSnapshot));
                        }
                    }
                }
            }
        }

        public void CollectValuesFromMemoryEntry(MemoryEntry entry)
        {
            mustHaveArray = true;
            this.VisitMemoryEntry(entry);
            mustHaveArray &= hasArray;
        }

        public void CollectValuesFromSources(MemoryEntryCollector collector)
        {
            mustHaveArray = true;
            foreach (SourceIndex source in SourceIndexes)
            {
                MemoryEntry entry = SnapshotDataUtils.GetMemoryEntry(source.Snapshot, source.Snapshot.Data.Readonly, source.Index);

                hasArray = false;
                this.VisitMemoryEntry(entry);
                mustHaveArray &= hasArray;
            }

            if (!IsMust)
            {
                if (ScalarValues == null)
                {
                    ScalarValues = new HashSet<Value>();
                }

                this.ScalarValues.Add(collector.Snapshot.UndefinedValue);
            }
        }

        public void CollectAliasesFromSources(MemoryEntryCollector collector)
        {
            bool invalidateMust = !IsMust;
            foreach (SourceIndex source in SourceIndexes)
            {
                IMemoryAlias aliases;
                if (source.Snapshot.Structure.Readonly.TryGetAliases(source.Index, out aliases))
                {
                    if (References == null)
                    {
                        References = new ReferenceCollector();
                    }

                    References.CollectMust(aliases.MustAliases);
                    References.CollectMay(aliases.MayAliases);

                    if (IsMust && SourceIndexes.Count == 1)
                    {
                        References.AddMustAlias(source.Index);
                    }
                    else
                    {
                        References.AddMayAlias(source.Index);
                    }
                }
                else
                {
                    invalidateMust = true;
                }
            }

            if (invalidateMust && References != null)
            {
                References.InvalidateMust();
            }
        }

        public void CollectAliases(MemoryIndex index, Snapshot snapshot, bool isMust)
        {
            if (References == null)
            {
                References = new ReferenceCollector();
            }

            IMemoryAlias aliases;
            if (snapshot.Structure.Readonly.TryGetAliases(index, out aliases))
            {
                References.CollectMay(aliases.MayAliases);

                if (isMust)
                {
                    References.CollectMust(aliases.MustAliases);
                }
                else
                {
                    References.CollectMay(aliases.MustAliases);
                }
            }

            if (isMust)
            {
                References.AddMustAlias(index);
            }
            else
            {
                References.AddMayAlias(index);
            }
        }

        #region Value Visitors

        public override void VisitValue(Value value)
        {
            if (ScalarValues == null)
            {
                ScalarValues = new HashSet<Value>();
            }

            ScalarValues.Add(value);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            if (Arrays == null)
            {
                Arrays = new HashSet<AssociativeArray>();
            }

            Arrays.Add(value);
            hasArray = true;
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            if (Objects == null)
            {
                Objects = new HashSet<ObjectValue>();
            }

            Objects.Add(value);
        }

        #endregion

        private IEnumerable<Tuple<Snapshot, IArrayDescriptor>> getIArrayDescriptors(
            MemoryEntryCollector collector, AssociativeArray array)
        {
            List<Tuple<Snapshot, IArrayDescriptor>> results = new List<Tuple<Snapshot, IArrayDescriptor>>();

            IArrayDescriptor descriptor;
            IEnumerable<Snapshot> snapshots;
            if (collector.Structure.TryGetDescriptor(array, out descriptor))
            {
                results.Add(new Tuple<Snapshot, IArrayDescriptor>(collector.Snapshot, descriptor));
            }
            else if (collector.Structure.TryGetCallArraySnapshot(array, out snapshots))
            {
                foreach (Snapshot snapshot in snapshots)
                {
                    IArrayDescriptor snapDescriptor = snapshot.Structure.Readonly.GetDescriptor(array);
                    results.Add(new Tuple<Snapshot, IArrayDescriptor>(snapshot, snapDescriptor));
                }
            }
            else
            {
                throw new Exception("Missing array descriptor");
            }

            return results;
        }

    }

    /// <summary>
    /// Represents combination of the memory index and snapshot. Used as the information where to get the data from.
    /// </summary>
    class SourceIndex
    {
        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public MemoryIndex Index { get; private set; }

        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <value>
        /// The snapshot.
        /// </value>
        public Snapshot Snapshot { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceIndex"/> class.
        /// </summary>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="snapshot">The snapshot.</param>
        public SourceIndex(MemoryIndex sourceIndex, Snapshot snapshot)
        {
            Index = sourceIndex;
            Snapshot = snapshot;
        }
    }
}
