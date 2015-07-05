using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    class MergeArrayStructureWorker
    {
        private IWriteableSnapshotStructure writeableTargetStructure;
        private TrackingMergeStructureWorker worker;

        private List<ContainerContext> sourceArrays;
        private bool arrayAlwaysDefined = true;
        private bool hasArray = false;
        private AssociativeArray targetArray;

        public MergeArrayStructureWorker(IWriteableSnapshotStructure writeableTargetStructure, TrackingMergeStructureWorker worker)
        {
            this.writeableTargetStructure = writeableTargetStructure;
            this.worker = worker;

            sourceArrays = new List<ContainerContext>();
        }

        public void collectSourceArray(MemoryIndex targetIndex, MergeOperation operation, MergeOperationContext operationContext, AssociativeArray sourceArray)
        {
            // Source array
            if (sourceArray != null)
            {
                // Becomes target array when not set
                if (targetArray == null && operationContext.Index.Equals(targetIndex))
                {
                    targetArray = sourceArray;
                }
                hasArray = true;

                // Save source array to merge descriptors
                IArrayDescriptor descriptor = operationContext.SnapshotContext.SourceStructure.GetDescriptor(sourceArray);
                sourceArrays.Add(new ContainerContext(operationContext.SnapshotContext, descriptor, operationContext.OperationType));

                // Equeue all array indexes when whole subtree should be merged
                if (operationContext.OperationType == MergeOperationType.WholeSubtree)
                {
                    foreach (var index in descriptor.Indexes)
                    {
                        operation.TreeNode.GetOrCreateChild(index.Key);
                    }
                    operation.TreeNode.GetOrCreateAny();
                }
            }
            else
            {
                // Source do not contain array - at least one source is empty
                arrayAlwaysDefined = false;
            }
        }

        public void SetTargetArray(AssociativeArray targetArray)
        {
            this.targetArray = targetArray;
        }

        public void MergeArraysAndClear(Snapshot targetSnapshot, MemoryIndex targetIndex, MergeOperation operation)
        {
            if (hasArray)
            {
                if (targetArray == null)
                {
                    targetArray = targetSnapshot.CreateArray();
                }

                IArrayDescriptor targetArrayDescriptor;
                if (!writeableTargetStructure.TryGetDescriptor(targetArray, out targetArrayDescriptor))
                {
                    // Target does not contain array - create and add new in target snapshot
                    targetArrayDescriptor = worker.Factories.StructuralContainersFactories.ArrayDescriptorFactory.CreateArrayDescriptor(writeableTargetStructure, targetArray, targetIndex);
                    writeableTargetStructure.SetDescriptor(targetArray, targetArrayDescriptor);
                    writeableTargetStructure.NewIndex(targetArrayDescriptor.UnknownIndex);
                    writeableTargetStructure.SetArray(targetIndex, targetArray);
                }

                // Create context and merge descriptors
                var arrayContext = new ArrayTargetContainerContext(writeableTargetStructure, targetArrayDescriptor);
                worker.CreateAndEnqueueOperations(arrayContext, operation.TreeNode, sourceArrays, arrayAlwaysDefined && !operation.IsUndefined);

                // Update current descriptor when changed
                IArrayDescriptor currentDescriptor = arrayContext.getCurrentDescriptor();
                if (currentDescriptor != targetArrayDescriptor)
                {
                    writeableTargetStructure.SetDescriptor(targetArray, currentDescriptor);
                }

                sourceArrays.Clear();
                hasArray = false;
                targetArray = null;
            }
            else if (targetArray != null)
            {
                worker.DeleteArray(targetIndex, targetArray);

                targetArray = null;
            }
            arrayAlwaysDefined = true;
        }
    }
}
