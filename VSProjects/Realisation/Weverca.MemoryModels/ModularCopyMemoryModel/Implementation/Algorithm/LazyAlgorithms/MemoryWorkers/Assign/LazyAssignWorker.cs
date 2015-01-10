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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign
{
    class LazyAssignWorker
    {
        private Snapshot snapshot;
        private TreeIndexCollector treeCollector;

        private IWriteableSnapshotStructure structure;
        private IWriteableSnapshotData data;

        private LinkedList<AssignOperation> operationQueue = new LinkedList<AssignOperation>();

        public LazyAssignWorker(Snapshot snapshot, TreeIndexCollector treeCollector)
        {
            this.snapshot = snapshot;
            this.treeCollector = treeCollector;

            this.structure = snapshot.Structure.Writeable;
            this.data = snapshot.CurrentData.Writeable;
        }

        public void Assign(MemoryEntry value, bool forceStrongWrite)
        {
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

            while (operationQueue.Count > 0)
            {
                AssignOperation operation = operationQueue.First.Value;
                operationQueue.RemoveFirst();

                processOperation(operation);
            }
        }

        private void processControls(int stackLevel, CollectorNode node)
        {
            if (node.Operations.HasNewChildren)
            {
                IWriteableIndexContainer writeableVariableContainer = structure.GetWriteableStackContext(stackLevel).WriteableControllVariables;
                foreach (var newChild in node.Operations.NewChildren)
                {
                    string childName = newChild.Key;
                    CollectorNode childNode = newChild.Value;

                    MemoryIndex index = ControlIndex.Create(childName, stackLevel);
                    childNode.Operations.NewCreateNodeOperation(index);

                    writeableVariableContainer.AddIndex(childName, index);
                }
            }

            IReadonlyIndexContainer readonlyVariableContainer = structure.GetReadonlyStackContext(stackLevel).ReadonlyVariables;
            enqueueOperations(node, readonlyVariableContainer);
        }

        private void processVariables(int stackLevel, CollectorNode node)
        {
            if (node.Operations.HasNewChildren)
            {
                IWriteableIndexContainer writeableVariableContainer = structure.GetWriteableStackContext(stackLevel).WriteableVariables;
                foreach (var newChild in node.Operations.NewChildren)
                {
                    string childName = newChild.Key;
                    CollectorNode childNode = newChild.Value;

                    MemoryIndex index = VariableIndex.Create(childName, stackLevel);
                    childNode.Operations.NewCreateNodeOperation(index);

                    writeableVariableContainer.AddIndex(childName, index);
                }
            }

            IReadonlyIndexContainer readonlyVariableContainer = structure.GetReadonlyStackContext(stackLevel).ReadonlyVariables;
            enqueueOperations(node, readonlyVariableContainer);
        }

        private void processObject(ObjectValue objectValue, ContainerCollectorNode node)
        {
            IObjectDescriptor descriptor;
            if (node.Operations.HasNewChildren)
            {
                IObjectDescriptor oldDescriptor = structure.GetDescriptor(objectValue);
                IObjectDescriptorBuilder builder = oldDescriptor.Builder(structure);
                foreach (var newChild in node.Operations.NewChildren)
                {
                    string childName = newChild.Key;
                    CollectorNode childNode = newChild.Value;

                    MemoryIndex index = ObjectIndex.Create(objectValue, childName);
                    childNode.Operations.NewCreateNodeOperation(index);

                    builder.AddIndex(childName, index);
                }

                IObjectDescriptor newDescriptor = builder.Build(structure);
                structure.SetDescriptor(objectValue, newDescriptor);

                descriptor = newDescriptor;
            }
            else
            {
                descriptor = structure.GetDescriptor(objectValue);
            }
            enqueueOperations(node, descriptor);
        }

        private void processTemporary(MemoryIndexNode memoryIndexNode)
        {
            operationQueue.AddLast(new AssignOperation(memoryIndexNode));
        }

        private void processOperation(AssignOperation operation)
        {
            CollectorNode node = operation.CollectorNode;

            /*if (node.Operations.IsMemoryIndexNotDefined)
            {
                structure.NewIndex(node.Operations.TargetIndex);
                // create new MI
            }

            if (node.Operations.IsNewImplicitObjectCreation)
            {
                // New implicit object
            }

            if (node.Operations.IsNewArrayCreation)
            {
                // create new array
            }

            if (node.Operations.IsCollectedNode)
            { 
                // provide assign
            }

            // Complete memory entry
             * */
        }

        private void enqueueOperations(CollectorNode node, IReadonlyIndexContainer container)
        {
            foreach (CollectorNode childNode in node.ChildNodes)
            {
                operationQueue.AddLast(new AssignOperation(childNode));
            }
        }

    }
}
