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
    class TreeIndexCollector : IPathSegmentVisitor
    {

        /*Dictionary<int, VariableStackNode> variableStackNodes = new Dictionary<int, VariableStackNode>();
        Dictionary<int, ControlStackNode> controlStackNodes = new Dictionary<int, ControlStackNode>();
        Dictionary<MemoryIndex, TemporaryNode> temporaryNodes = new Dictionary<MemoryIndex, TemporaryNode>();
        Dictionary<ObjectValue, CollectorNode> objectNodes = new Dictionary<ObjectValue, CollectorNode>();*/

        public Snapshot Snapshot { get; private set; }
        public IReadOnlySnapshotStructure Structure { get; private set; }
        public IReadOnlySnapshotData Data { get; private set; }
        public RootCollectorNode RootNode { get; private set; }

        HashSet<LocationCollectorNode> currentNodes = new HashSet<LocationCollectorNode>();
        HashSet<LocationCollectorNode> nextIterationNodes = new HashSet<LocationCollectorNode>();

        Dictionary<MemoryIndex, CollectorNode> processedIndex = new Dictionary<MemoryIndex, CollectorNode>();

        private MemoryPath currentPath;

        public TreeIndexCollector(Snapshot snapshot)
        {
            this.Snapshot = snapshot;
            this.Structure = snapshot.Structure.Readonly;
            this.Data = snapshot.Data.Readonly;

            RootNode = new RootCollectorNode();
        }

        public void ProcessPath(MemoryPath path)
        {
            currentPath = path;

            foreach (var segment in path.PathSegments)
            {
                segment.Accept(this);

                processAliases();

                currentNodes = nextIterationNodes;
                nextIterationNodes = new HashSet<LocationCollectorNode>();
            }

            foreach (CollectorNode node in currentNodes)
            {
                node.IsCollected = true;
            }
        }

        private void processAliases()
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
        
        public void CollectFieldSegmentFromValues(FieldPathSegment fieldSegment, CollectorNode node,
            IEnumerable<Value> values, bool isMust)
        {
            isMust = isMust && values.Count() == 1;
            CollectFieldValueVisitor visitor = new CollectFieldValueVisitor(fieldSegment, this, node, isMust);
            visitor.VisitValues(values);
        }

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

        public void CollectAlias(LocationCollectorNode node, MemoryIndex alias, bool isMust)
        {
            RootNode.CollectAlias(this, alias, isMust);
        }

        public MemoryEntry GetMemoryEntry(MemoryIndex memoryIndex)
        {
            MemoryEntry entry;
            if (!Data.TryGetMemoryEntry(memoryIndex, out entry))
            {
                entry = Snapshot.EmptyEntry;
            }
            return entry;
        }

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














        /*internal void ProcessSegmentInStructure(PathSegment segment, CollectorNode node, IReadonlyIndexContainer indexContainer, bool isMust = true)
        {
            throw new NotImplementedException();

            if (segment.IsAny)
            {
                MemoryIndex unknown = indexContainer.UnknownIndex;
                CollectorNode anyNode = node.GetOrCreateAny(unknown);
                addNode(unknown, anyNode);

                foreach (var index in indexContainer.Indexes)
                {
                    CollectorNode childNode = node.GetOrCreateChild(index.Key, index.Value);
                    childNode.SetMay();
                    addNode(index.Value, childNode);
                }
            }
            else if (segment.IsUnknown)
            {
                MemoryIndex unknown = indexContainer.UnknownIndex;
                CollectorNode anyNode = node.GetOrCreateAny(unknown);
                addNode(unknown, anyNode);
            }
            else if (segment.Names.Count == 1)
            {
                string name = segment.Names[0];
                MemoryIndex index;
                if (indexContainer.TryGetIndex(name, out index))
                {
                    CollectorNode childNode = node.GetOrCreateChild(name, index);

                    if (!isMust)
                    {
                        childNode.SetMay();
                    }

                    addNode(index, childNode);
                }
                else
                {

                }
            }
            else
            {
                foreach (string name in segment.Names)
                {
                    MemoryIndex index;
                    if (indexContainer.TryGetIndex(name, out index))
                    {
                        CollectorNode childNode = node.GetOrCreateChild(name, index);
                        childNode.SetMay();
                        addNode(index, childNode);
                    }
                    else
                    {

                    }
                }
            }
        }*/


    }
}
