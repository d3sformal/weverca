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
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors
{
    class MemoryEntryCollector
    {
        public Snapshot Snapshot { get; private set; }
        public IReadOnlySnapshotStructure Structure { get; private set; }
        public IReadOnlySnapshotData Data { get; private set; }

        public MemoryEntry RootMemoryEntry { get; private set; }

        public MemoryEntryCollectorNode RootNode { get; set; }

        public LinkedList<MemoryEntryCollectorNode> collectingQueue = new LinkedList<MemoryEntryCollectorNode>();

        public MemoryEntryCollector(Snapshot snapshot)
        {
            this.Snapshot = snapshot;
            this.Structure = snapshot.Structure.Readonly;
            this.Data = snapshot.Data.Readonly;
        }

        public void ProcessRootMemoryEntry(MemoryEntry entry)
        {
            RootMemoryEntry = entry;

            MemoryEntryCollectorNode rootNode = new MemoryEntryCollectorNode();
            rootNode.IsMust = true;
            rootNode.CollectValuesFromMemoryEntry(entry);
            rootNode.CollectChildren(this);
            RootNode = rootNode;


            while (collectingQueue.Count > 0)
            {
                MemoryEntryCollectorNode node = collectingQueue.First.Value;
                collectingQueue.RemoveFirst();

                node.CollectAliasesFromSources(this);
                node.CollectValuesFromSources(this);
                node.CollectChildren(this);
            }
        }

        public void AddNode(MemoryEntryCollectorNode node)
        {
            collectingQueue.AddLast(node);
        }

        internal void ProcessRootIndexes(HashSet<MemoryIndex> mustIndexes, HashSet<MemoryIndex> mayIndexes, IEnumerable<Value> values)
        {
            MemoryEntryCollectorNode rootNode = new MemoryEntryCollectorNode();
            rootNode.IsMust = true;
            RootNode = rootNode;

            foreach (MemoryIndex index in mustIndexes)
            {
                rootNode.SourceIndexes.Add(new SourceIndex(index, Snapshot));
                rootNode.CollectAliases(index, Snapshot, true);
            }
            foreach (MemoryIndex index in mayIndexes)
            {
                rootNode.SourceIndexes.Add(new SourceIndex(index, Snapshot));
                rootNode.CollectAliases(index, Snapshot, false);
            }

            rootNode.CollectValuesFromSources(this);
            rootNode.VisitValues(values);
            rootNode.CollectChildren(this);

            RootMemoryEntry = rootNode.GenerateMemeoryEntry(values);

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



        public bool hasArray;
        public HashSet<Value> ScalarValues { get; private set; }
        public HashSet<AssociativeArray> Arrays { get; private set; }
        public HashSet<ObjectValue> Objects { get; private set; }

        public MemoryEntryCollectorNode AnyChild { get; set; }
        public Dictionary<string, MemoryEntryCollectorNode> NamedChildren { get; set; }

        public ReferenceCollector References { get; set; }

        public bool HasAliases { get { return References != null && References.HasAliases; } }

        public List<SourceIndex> SourceIndexes { get; private set; }
        public bool IsMust { get; set; }

        private bool mustHaveArray;




        public MemoryEntryCollectorNode()
        {
            /*
            ScalarValues = new HashSet<Value>();
            Arrays = new HashSet<AssociativeArray>();
            Objects = new HashSet<ObjectValue>();
             */

            SourceIndexes = new List<SourceIndex>();
            NamedChildren = new Dictionary<string, MemoryEntryCollectorNode>();

        }

        public MemoryEntry GenerateMemeoryEntry(IEnumerable<Value> values)
        {
            List<Value> entryValues = new List<Value>(values);
            
            if (ScalarValues != null)
            {
                CollectionTools.AddAll(entryValues, ScalarValues);
            }
            if (Arrays != null)
            {
                CollectionTools.AddAll(entryValues, Arrays);
            }
            if (Objects != null)
            {
                CollectionTools.AddAll(entryValues, Objects);
            }

            return new MemoryEntry(entryValues);
        }

        public void CollectChildren(MemoryEntryCollector collector)
        {
            if (Arrays != null && Arrays.Count > 0)
            {
                AnyChild = new MemoryEntryCollectorNode();
                collector.AddNode(AnyChild);
                AnyChild.IsMust = false;

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

                bool mustHaveChildren = this.IsMust && mustHaveArray;
                mustHaveChildren &= (ScalarValues == null || ScalarValues.Count == 0);
                mustHaveChildren &= (Objects == null || Objects.Count == 0);

                foreach (string name in names)
                {
                    MemoryEntryCollectorNode childNode = new MemoryEntryCollectorNode();
                    NamedChildren.Add(name, childNode);
                    collector.AddNode(childNode);
                    childNode.IsMust = mustHaveChildren;

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
                MemoryEntry entry = source.Snapshot.Data.Readonly.GetMemoryEntry(source.Index);

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

    class SourceIndex
    {
        public MemoryIndex Index { get; private set; }
        public Snapshot Snapshot { get; private set; }

        public SourceIndex(MemoryIndex sourceIndex, Snapshot snapshot)
        {
            Index = sourceIndex;
            Snapshot = snapshot;
        }
    }
}
