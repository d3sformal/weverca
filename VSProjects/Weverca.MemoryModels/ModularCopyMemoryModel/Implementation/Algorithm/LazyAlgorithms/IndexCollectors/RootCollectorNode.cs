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
    /// <summary>
    /// Represents the root of the collecting tree. Contains the set of variables or objects 
    /// which were resulved during the interpretation of the path.
    /// </summary>
    class RootCollectorNode
    {
        public readonly Dictionary<int, ContainerCollectorNode> VariableStackNodes =
            new Dictionary<int, ContainerCollectorNode>();
        public readonly Dictionary<int, ContainerCollectorNode> ControlStackNodes =
            new Dictionary<int, ContainerCollectorNode>();
        public readonly Dictionary<MemoryIndex, MemoryIndexCollectorNode> TemporaryNodes =
            new Dictionary<MemoryIndex, MemoryIndexCollectorNode>();
        public readonly Dictionary<ObjectValue, ContainerCollectorNode> ObjectNodes =
            new Dictionary<ObjectValue, ContainerCollectorNode>();

        public bool HasRootNode { get; private set; }

        /// <summary>
        /// Collects the variable.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="variableSegment">The variable segment.</param>
        public void CollectVariable(TreeIndexCollector collector, VariablePathSegment variableSegment)
        {
            int currentCallLevel = collector.GetCurrentCallLevel();
            ContainerCollectorNode variableStackNode = GetOrCreateVariableStackNode(collector, currentCallLevel);

            variableStackNode.Collect(collector, variableSegment);
            HasRootNode = true;
        }

        /// <summary>
        /// Collects the control variables.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="controlPathSegment">The control path segment.</param>
        public void CollectControl(TreeIndexCollector collector, ControlPathSegment controlPathSegment)
        {
            int currentCallLevel = collector.GetCurrentCallLevel();
            ContainerCollectorNode controlStackNode = GetOrCreateControlStackNode(collector, currentCallLevel);

            controlStackNode.Collect(collector, controlPathSegment);
            HasRootNode = true;
        }

        /// <summary>
        /// Collects the object.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="objectValue">The object value.</param>
        /// <param name="fieldPathSegment">The field path segment.</param>
        public void CollectObject(TreeIndexCollector collector, ObjectValue objectValue, FieldPathSegment fieldPathSegment)
        {
            ContainerCollectorNode objectNode = GetOrCreateObjectNode(collector, objectValue);
            objectNode.Collect(collector, fieldPathSegment);
        }

        /// <summary>
        /// Collects the temporary variables.
        /// </summary>
        /// <param name="treeIndexCollector">The tree index collector.</param>
        /// <param name="temporaryPathSegment">The temporary path segment.</param>
        /// <exception cref="System.NotImplementedException">Temporary memory index is visited more than once</exception>
        public void CollectTemporary(TreeIndexCollector treeIndexCollector, TemporaryPathSegment temporaryPathSegment)
        {
            if (TemporaryNodes.ContainsKey(temporaryPathSegment.TemporaryIndex))
            {
                throw new NotImplementedException("Temporary memory index is visited more than once");
            }

            MemoryIndexCollectorNode node = new MemoryIndexCollectorNode(temporaryPathSegment.TemporaryIndex);
            node.IsMust = true;
            TemporaryNodes.Add(temporaryPathSegment.TemporaryIndex, node);
            treeIndexCollector.AddNode(node);
            HasRootNode = true;
        }

        /// <summary>
        /// Collects the alias.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="aliasIndex">Index of the alias.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public void CollectAlias(TreeIndexCollector collector, MemoryIndex aliasIndex, bool isMust)
        {
            CollectAliasMemoryIndexVisitor visitor = new CollectAliasMemoryIndexVisitor(collector, this, isMust);
            aliasIndex.Accept(visitor);
        }

        /// <summary>
        /// Gets the or create variable stack node.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns></returns>
        public ContainerCollectorNode GetOrCreateVariableStackNode(TreeIndexCollector collector, int callLevel)
        {
            ContainerCollectorNode variableStackNode;
            if (!VariableStackNodes.TryGetValue(callLevel, out variableStackNode))
            {
                IReadonlyIndexContainer indexContainer = collector.Structure
                    .GetReadonlyStackContext(callLevel).ReadonlyVariables;

                variableStackNode = new ContainerCollectorNode(indexContainer);
                VariableStackNodes.Add(callLevel, variableStackNode);
            }

            return variableStackNode;
        }

        /// <summary>
        /// Gets the or create control stack node.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns>Node representing stack context on specified level.</returns>
        public ContainerCollectorNode GetOrCreateControlStackNode(TreeIndexCollector collector, int callLevel) 
        {
            ContainerCollectorNode controlStackNode;
            if (!ControlStackNodes.TryGetValue(callLevel, out controlStackNode))
            {
                IReadonlyIndexContainer indexContainer = collector.Structure
                    .GetReadonlyStackContext(callLevel).ReadonlyControllVariables;
                controlStackNode = new ContainerCollectorNode(indexContainer);
                ControlStackNodes.Add(callLevel, controlStackNode);
            }

            return controlStackNode;
        }

        /// <summary>
        /// Gets the or create object node.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="objectValue">The object value.</param>
        /// <returns>Node representing given object</returns>
        public ContainerCollectorNode GetOrCreateObjectNode(TreeIndexCollector collector, ObjectValue objectValue)
        {
            ContainerCollectorNode objectNode;
            if (!ObjectNodes.TryGetValue(objectValue, out objectNode))
            {
                IObjectDescriptor descriptor = collector.Structure.GetDescriptor(objectValue);
                objectNode = new ContainerCollectorNode(descriptor);
                ObjectNodes.Add(objectValue, objectNode);
            }

            return objectNode;
        }

        private class CollectAliasMemoryIndexVisitor : MemoryIndexVisitor
        {
            RootCollectorNode rootNode;
            TreeIndexCollector collector;
            bool isMust;

            MemoryIndexCollectorNode lastCreatedNode = null;
            CollectorNode currentNode;
            CollectorNode parentNode;
            IndexSegment lastSegment;

            public CollectAliasMemoryIndexVisitor(TreeIndexCollector collector, RootCollectorNode rootNode, bool isMust)
            {
                this.rootNode = rootNode;
                this.collector = collector;
                this.isMust = isMust;
            }

            public void VisitObjectIndex(ObjectIndex index)
            {
                currentNode = rootNode.GetOrCreateObjectNode(collector, index.Object);

                processIndexName(index);
                processIndexPath(index);

                processAlias(index);
            }

            public void VisitVariableIndex(VariableIndex index)
            {
                currentNode = rootNode.GetOrCreateVariableStackNode(collector, index.CallLevel);

                processIndexName(index);
                processIndexPath(index);

                processAlias(index);
            }

            public void VisitTemporaryIndex(TemporaryIndex index)
            {
                MemoryIndexCollectorNode node;
                if (!rootNode.TemporaryNodes.TryGetValue(index.RootIndex, out node))
                {
                    MemoryIndexCollectorNode newNode = new MemoryIndexCollectorNode(index.RootIndex);
                    newNode.IsMust = isMust;
                    rootNode.TemporaryNodes.Add(index.RootIndex, newNode);

                    currentNode = newNode;
                }
                else
                {
                    currentNode = node;
                }

                processIndexPath(index);

                if (!processAlias(index))
                {
                    if (isMust)
                    {
                        rootNode.TemporaryNodes.Remove(index.RootIndex);

                        MemoryIndexCollectorNode newNode = new MemoryIndexCollectorNode(index.RootIndex);
                        newNode.IsMust = isMust;
                        rootNode.TemporaryNodes.Add(index.RootIndex, newNode);
                    }
                    else
                    {
                        MemoryIndexCollectorNode childNode = rootNode.TemporaryNodes[index.RootIndex];

                        collector.AddNode(childNode);
                        childNode.TargetIndex = index;
                        childNode.SourceIndex = index;
                        childNode.IsMust = false;
                    }
                }
            }

            public void VisitControlIndex(ControlIndex index)
            {
                currentNode = rootNode.GetOrCreateControlStackNode(collector, index.CallLevel);

                processIndexName(index);
                processIndexPath(index);

                processAlias(index);
            }

            private void processIndexName(NamedIndex index)
            {
                processIndexSegment(index.MemoryRoot);
            }


            private void processIndexPath(MemoryIndex index)
            {
                foreach (IndexSegment segment in index.MemoryPath)
                {
                    processIndexSegment(segment);
                }
            }

            private void processIndexSegment(IndexSegment segment)
            {
                lastSegment = segment;
                if (segment.IsAny)
                {
                    if (currentNode.AnyChildNode != null)
                    {
                        parentNode = currentNode;
                        currentNode = currentNode.AnyChildNode;
                    }
                    else
                    {
                        MemoryIndexCollectorNode newNode = new MemoryIndexCollectorNode(null);
                        newNode.IsMust = false;
                        currentNode.addAnyChild(newNode);

                        parentNode = currentNode;
                        currentNode = newNode;
                        lastCreatedNode = newNode;
                    }
                }
                else
                {
                    if (currentNode.NamedChildNodes != null && currentNode.NamedChildNodes.ContainsKey(segment.Name))
                    {
                        parentNode = currentNode;
                        currentNode = currentNode.NamedChildNodes[segment.Name];
                    }
                    else
                    {
                        MemoryIndexCollectorNode newNode = new MemoryIndexCollectorNode(null);
                        newNode.IsMust = isMust;
                        currentNode.addChild(newNode, segment.Name);

                        parentNode = currentNode;
                        currentNode = newNode;
                        lastCreatedNode = newNode;
                    }
                }
            }

            private bool processAlias(MemoryIndex index)
            {

                if (lastCreatedNode == null)
                {
                    if (parentNode != null && lastSegment != null)
                    {
                        if (lastSegment.IsAny)
                        {
                            if (testAndProcessReturnedAnyNode(parentNode, index))
                            {
                                MemoryIndexCollectorNode newNode = new MemoryIndexCollectorNode(index);
                                newNode.IsMust = false;
                                currentNode.addAnyChild(newNode);

                                collector.AddNode(newNode);
                            }
                        }
                        else
                        {
                            if (testAndProcessReturnedNode(lastSegment.Name, parentNode, isMust, index))
                            {
                                MemoryIndexCollectorNode newNode = new MemoryIndexCollectorNode(index);
                                newNode.IsMust = isMust;
                                currentNode.addChild(newNode, lastSegment.Name);

                                collector.AddNode(newNode);
                            }
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    lastCreatedNode.SetMemoryIndex(index);
                    collector.AddNode(lastCreatedNode);

                    return true;
                }
            }

            private bool testAndProcessReturnedNode(string name, CollectorNode node, bool isMust, MemoryIndex index)
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
                        collector.AddNode(childNode);
                        childNode.TargetIndex = index;
                        childNode.SourceIndex = index;
                        childNode.IsMust = false;

                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }

            private bool testAndProcessReturnedAnyNode(CollectorNode node, MemoryIndex index)
            {
                if (node.AnyChildNode != null)
                {
                    collector.AddNode(node.AnyChildNode);
                    node.AnyChildNode.TargetIndex = index;
                    node.AnyChildNode.SourceIndex = index;
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
