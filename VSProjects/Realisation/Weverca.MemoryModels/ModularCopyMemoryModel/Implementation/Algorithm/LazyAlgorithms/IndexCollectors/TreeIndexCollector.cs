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

        HashSet<CollectorNode> currentNodes = new HashSet<CollectorNode>();
        HashSet<CollectorNode> nextIterationNodes = new HashSet<CollectorNode>();

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

                currentNodes = nextIterationNodes;
                nextIterationNodes = new HashSet<CollectorNode>();
            }

            foreach (CollectorNode node in currentNodes)
            {
                if (!node.HasOperations)
                {
                    node.Operations = new CollectorOperations(node);
                }

                node.Operations.NewCollectedOperation();
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
            foreach (CollectorNode node in currentNodes)
            {
                node.CollectField(this, fieldSegment);
            }
        }

        public void VisitIndex(IndexPathSegment indexSegment)
        {
            foreach (CollectorNode node in currentNodes)
            {
                node.CollectIndex(this, indexSegment);
            }
        }

        #endregion

        #region Segment Processing

        public void CollectSegmentFromStructure(PathSegment segment, CollectorNode node, 
            CollectorOperations operations, IReadonlyIndexContainer indexContainer, bool isMust)
        {
            isMust = isMust && node.IsMust;

            if (segment.IsAny)
            {
                collectMemoryIndexAnyNode(indexContainer.UnknownIndex, operations, node);

                foreach (var item in indexContainer.Indexes)
                {
                    collectMemoryIndexExpandedNode(item.Key, item.Value, operations, node);
                }
            }
            else if (segment.IsUnknown)
            {
                collectMemoryIndexAnyNode(indexContainer.UnknownIndex, operations, node);
            }
            else if (segment.Names.Count == 1)
            {
                string name = segment.Names[0];
                collectMemoryIndexNode(name, operations, node, indexContainer, isMust);
            }
            else
            {
                foreach (string name in segment.Names)
                {
                    collectMemoryIndexNode(name, operations, node, indexContainer, false);
                }
            }
        }

        public void CollectSegmentWithoutStructure(PathSegment segment, CollectorNode node, 
            CollectorOperations operations, bool isMust)
        {
            isMust = isMust && node.IsMust;

            if (segment.IsAny || segment.IsUnknown)
            {
                collectImplicitAnyNode(operations, node);
            }
            else if (segment.Names.Count == 1)
            {
                String name = segment.Names[0];
                collectImplicitNode(name, operations, node, isMust);
            }
            else
            {
                foreach (string name in segment.Names)
                {
                    collectImplicitNode(name, operations, node, false);
                }
            }
        }
        
        public void CollectFieldSegmentFromValues(FieldPathSegment fieldSegment, CollectorNode node,
            CollectorOperations operations, IEnumerable<Value> values, bool isMust)
        {
            isMust = isMust && values.Count() == 1;
            CollectFieldValueVisitor visitor = new CollectFieldValueVisitor(fieldSegment, this, node, isMust);
            visitor.VisitValues(values);
        }

        public void CollectIndexSegmentFromValues(IndexPathSegment indexSegment, CollectorNode node,
            CollectorOperations operations, IEnumerable<Value> values, bool isMust)
        {
            isMust = isMust && values.Count() == 1;

            CollectIndexValueVisitor visitor = new CollectIndexValueVisitor(indexSegment, this, node, isMust);
            visitor.VisitValues(values);
        }



        private void collectMemoryIndexAnyNode(MemoryIndex unknownIndex, CollectorOperations operations, CollectorNode node)
        {
            CollectorNode nextNode = node.CreateMemoryIndexAnyChild(unknownIndex);
            nextNode.IsMust = false;
            AddNode(nextNode);

            operations.NewAnyNodeAccess();
        }

        private void collectMemoryIndexExpandedNode(string name, MemoryIndex memoryIndex, 
            CollectorOperations operations, CollectorNode node)
        {
            CollectorNode nextNode = node.CreateMemoryIndexChild(name, memoryIndex);
            nextNode.IsMust = false;
            AddNode(nextNode);

            operations.NewChildAccess(name);
        }

        private void collectMemoryIndexNode(string name, CollectorOperations operations, CollectorNode node, 
            IReadonlyIndexContainer indexContainer, bool isMust)
        {
            MemoryIndex memoryIndex;
            CollectorNode nextNode;
            if (indexContainer.TryGetIndex(name, out memoryIndex))
            {
                nextNode = node.CreateMemoryIndexChild(name, memoryIndex);
                operations.NewChildAccess(name);
            }
            else
            {
                nextNode = node.CreateMemoryIndexChildFromAny(name, indexContainer.UnknownIndex);
                operations.NewImplicitChildFromAny(name, indexContainer.UnknownIndex);
            }

            nextNode.IsMust = isMust;
            AddNode(nextNode);
        }

        private void collectImplicitAnyNode(CollectorOperations operations, CollectorNode node)
        {
            CollectorNode nextNode = node.CreateUndefinedAnyChild();
            AddNode(nextNode);

            operations.NewAnyChildAccess();
        }

        private void collectImplicitNode(string name, CollectorOperations operations, CollectorNode node, bool isMust)
        {
            CollectorNode nextNode = node.CreateUndefinedChild(name);
            nextNode.IsMust = isMust;
            AddNode(nextNode);

            operations.NewChildAccess(name);
        }

        #endregion

        #region Public Helpers

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

        public void AddNode(CollectorNode nextNode)
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
