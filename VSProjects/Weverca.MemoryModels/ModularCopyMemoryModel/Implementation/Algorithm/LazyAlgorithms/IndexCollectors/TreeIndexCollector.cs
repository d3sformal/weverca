using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors
{
    /// <summary>
    /// Collector class which interprets given memory path as collection of matching 
    /// memory locations.
    /// 
    /// Collector builds the tree of memory indexes from the root of memory tree to all 
    /// collected locations. Tre can be used in assign algorithm to prepare the memory 
    /// structure - implicit objects, arrays, create missing target locations.
    /// </summary>
    class TreeIndexCollector : IPathSegmentVisitor
    {
        private HashSet<LocationCollectorNode> currentNodes = new HashSet<LocationCollectorNode>();
        private HashSet<LocationCollectorNode> nextIterationNodes = new HashSet<LocationCollectorNode>();
        private Dictionary<MemoryIndex, CollectorNode> processedIndex = new Dictionary<MemoryIndex, CollectorNode>();
        private MemoryPath currentPath;

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
        /// Gets the root node which contains the beginning of interpreted access path.
        /// </summary>
        /// <value>
        /// The root node.
        /// </value>
        public RootCollectorNode RootNode { get; private set; }


        /// <summary>
        /// Gets or sets a value indicating whether the aliases should be processed after 
        /// the end of collecting process. If true then all aliases to the collected locations 
        /// will be added to the collected tree.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the aliases should be processed after the end of collecting 
        ///   process; otherwise, <c>false</c>.
        /// </value>
        public bool PostProcessAliases { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeIndexCollector"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public TreeIndexCollector(Snapshot snapshot)
        {
            this.Snapshot = snapshot;
            this.Structure = snapshot.Structure.Readonly;
            this.Data = snapshot.Data.Readonly;

            RootNode = new RootCollectorNode();

            PostProcessAliases = false;
        }

        /// <summary>
        /// Process given access path and collects all matching indexes. This is an entry point 
        /// of the computation.
        /// </summary>
        /// <param name="path">The path.</param>
        public void ProcessPath(MemoryPath path)
        {
            currentPath = path;

            foreach (var segment in path.PathSegments)
            {
                processAliases();

                var swap = currentNodes;
                currentNodes = nextIterationNodes;
                nextIterationNodes = swap;
                nextIterationNodes.Clear();

                segment.Accept(this);
            }

            if (PostProcessAliases)
            {
                processAliases();
            }
            currentNodes = nextIterationNodes;

            foreach (CollectorNode node in currentNodes)
            {
                node.IsCollected = true;
            }
        }

        public void processAliases()
        {
            HashSet<CollectorNode> nodes = new HashSet<CollectorNode>(nextIterationNodes);

            foreach (LocationCollectorNode node in nodes)
            {
                node.CollectAliases(this);
            }
        }

        #region Segment visitors

        public void VisitVariable(VariablePathSegment variableSegment)
        {
            if (!RootNode.HasRootNode)
            {
                RootNode.CollectVariable(this, variableSegment);
            }
            else
            {
                throw new Exception("Malformed MemoryPath - duplicit root segment");
            }
        }

        public void VisitControl(ControlPathSegment controlPathSegment)
        {
            if (!RootNode.HasRootNode)
            {
                RootNode.CollectControl(this, controlPathSegment);
            }
            else
            {
                throw new Exception("Malformed MemoryPath - duplicit root segment");
            }
        }

        public void VisitTemporary(TemporaryPathSegment temporaryPathSegment)
        {
            if (!RootNode.HasRootNode)
            {
                RootNode.CollectTemporary(this, temporaryPathSegment);
            }
            else
            {
                throw new Exception("Malformed MemoryPath - duplicit root segment");
            }
        }

        public void VisitField(FieldPathSegment fieldSegment)
        {
            foreach (LocationCollectorNode node in currentNodes)
            {
                node.CollectField(this, fieldSegment);
            }
        }

        public void VisitIndex(IndexPathSegment indexSegment)
        {
            foreach (LocationCollectorNode node in currentNodes)
            {
                node.CollectIndex(this, indexSegment);
            }
        }

        #endregion

        #region Segment Processing

        /// <summary>
        /// Collects the segment from structure.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="node">The node.</param>
        /// <param name="indexContainer">The index container.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public void CollectSegmentFromStructure(PathSegment segment, CollectorNode node, 
            IReadonlyIndexContainer indexContainer, bool isMust)
        {
            isMust = isMust && node.IsMust;

            if (segment.IsAny)
            {
                collectMemoryIndexAnyNode(indexContainer.UnknownIndex, node);

                foreach (var item in indexContainer.Indexes)
                {
                    collectMemoryIndexExpandedNode(item.Key, item.Value, node);
                }
            }
            else if (segment.IsUnknown)
            {
                collectMemoryIndexAnyNode(indexContainer.UnknownIndex, node);
            }
            else if (segment.Names.Count == 1)
            {
                string name = segment.Names[0];
                collectMemoryIndexNode(name, node, indexContainer, isMust);
            }
            else
            {
                foreach (string name in segment.Names)
                {
                    collectMemoryIndexNode(name, node, indexContainer, false);
                }
            }
        }

        /// <summary>
        /// Collects the segment without structure.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="node">The node.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public void CollectSegmentWithoutStructure(PathSegment segment, CollectorNode node, bool isMust)
        {
            isMust = isMust && node.IsMust;

            if (segment.IsAny || segment.IsUnknown)
            {
                collectImplicitAnyNode(node);
            }
            else if (segment.Names.Count == 1)
            {
                String name = segment.Names[0];
                collectImplicitNode(name, node, isMust);
            }
            else
            {
                foreach (string name in segment.Names)
                {
                    collectImplicitNode(name, node, false);
                }
            }
        }

        /// <summary>
        /// Collects the field segment from values.
        /// </summary>
        /// <param name="fieldSegment">The field segment.</param>
        /// <param name="node">The node.</param>
        /// <param name="values">The values.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public void CollectFieldSegmentFromValues(FieldPathSegment fieldSegment, CollectorNode node,
            IEnumerable<Value> values, bool isMust)
        {
            isMust = isMust && values.Count() == 1;
            CollectFieldValueVisitor visitor = new CollectFieldValueVisitor(fieldSegment, this, node, isMust);
            visitor.VisitValues(values);
        }

        /// <summary>
        /// Collects the index segment from values.
        /// </summary>
        /// <param name="indexSegment">The index segment.</param>
        /// <param name="node">The node.</param>
        /// <param name="values">The values.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public void CollectIndexSegmentFromValues(IndexPathSegment indexSegment, CollectorNode node,
            IEnumerable<Value> values, bool isMust)
        {
            isMust = isMust && values.Count() == 1;

            CollectIndexValueVisitor visitor = new CollectIndexValueVisitor(indexSegment, this, node, isMust);
            visitor.VisitValues(values);
        }

        private void collectMemoryIndexAnyNode(MemoryIndex unknownIndex, CollectorNode node)
        {
            if (testAndProcessReturnedAnyNode(node))
            {
                LocationCollectorNode nextNode = node.CreateMemoryIndexAnyChild(unknownIndex);
                nextNode.IsMust = false;
                AddNode(nextNode);
            }
        }

        private void collectMemoryIndexExpandedNode(string name, MemoryIndex memoryIndex, CollectorNode node)
        {
            if (testAndProcessReturnedNode(name, node, false))
            {
                LocationCollectorNode nextNode = node.CreateMemoryIndexChild(name, memoryIndex);
                nextNode.IsMust = false;
                AddNode(nextNode);
            }
        }

        private void collectMemoryIndexNode(string name, CollectorNode node, 
            IReadonlyIndexContainer indexContainer, bool isMust)
        {
            if (testAndProcessReturnedNode(name, node, isMust))
            {
                LocationCollectorNode nextNode ;
                MemoryIndex memoryIndex;
                if (indexContainer.TryGetIndex(name, out memoryIndex))
                {
                    nextNode = node.CreateMemoryIndexChild(name, memoryIndex);
                }
                else
                {
                    nextNode = node.CreateMemoryIndexChildFromAny(name, indexContainer.UnknownIndex);
                }

                nextNode.IsMust = isMust;
                AddNode(nextNode);
            }
        }
        
        private void collectImplicitAnyNode(CollectorNode node)
        {
            if (testAndProcessReturnedAnyNode(node))
            {
                LocationCollectorNode nextNode = node.CreateUndefinedAnyChild();
                AddNode(nextNode);
            }
        }

        private void collectImplicitNode(string name, CollectorNode node, bool isMust)
        {
            if (testAndProcessReturnedNode(name, node, isMust))
            {
                LocationCollectorNode nextNode = node.CreateUndefinedChild(name);
                nextNode.IsMust = isMust;
                AddNode(nextNode);
            }
        }

        private bool testAndProcessReturnedNode(string name, CollectorNode node, bool isMust)
        {
            MemoryCollectorNode childNode;
            if (node.NamedChildNodes != null && node.NamedChildNodes.TryGetValue(name, out childNode))
            {
                if (isMust)
                {
                    node.NamedChildNodes.Remove(name);

                    return true;
                }
                else
                {
                    AddNode(childNode);
                    childNode.IsMust = false;

                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool testAndProcessReturnedAnyNode(CollectorNode node)
        {
            if (node.AnyChildNode != null)
            {
                AddNode(node.AnyChildNode);
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Public Helpers

        /// <summary>
        /// Collects the alias.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public void CollectAlias(LocationCollectorNode node, MemoryIndex alias, bool isMust)
        {
            RootNode.CollectAlias(this, alias, isMust);
        }

        /// <summary>
        /// Gets the memory entry.
        /// </summary>
        /// <param name="memoryIndex">Index of the memory.</param>
        /// <returns>Returns memory entry of the given index</returns>
        public MemoryEntry GetMemoryEntry(MemoryIndex memoryIndex)
        {
            MemoryEntry entry;
            if (!Data.TryGetMemoryEntry(memoryIndex, out entry))
            {
                entry = Snapshot.EmptyEntry;
            }
            return entry;
        }

        /// <summary>
        /// Gets the current call level.
        /// </summary>
        /// <returns>Call level of curent path</returns>
        /// <exception cref="System.InvalidOperationException">Unknown GlobalContext state:  + currentPath.Global</exception>
        public int GetCurrentCallLevel()
        {
            switch (currentPath.Global)
            {
                case GlobalContext.GlobalOnly:
                    return Snapshot.GLOBAL_CALL_LEVEL;

                case GlobalContext.LocalOnly:
                    return currentPath.CallLevel;

                default:
                    throw new InvalidOperationException("Unknown GlobalContext state: " + currentPath.Global);
            }
        }

        public void AddNode(LocationCollectorNode nextNode)
        {
            nextIterationNodes.Add(nextNode);
        }

        #endregion
    }
}
