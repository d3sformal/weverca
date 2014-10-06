/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers
{
    /// <summary>
    /// Implementation of merge algorithm for second phase of analysis. This merge algorithm does not change the structure
    /// of memory model. Algorithm uses existing structure to traverse its memory tree and tries to find appropriate
    /// memoru location in merged snapshot. Output data for each memory location contains all possible values. When some
    /// memory location is not specified on some snapshot the nearest unknown location is used as source of data.
    /// Indexes which is not in target structure will be ignored.
    /// </summary>
    class MergeInfoWorker : IMergeWorker
    {
        private Dictionary<MemoryIndex, IMemoryAliasBuilder> memoryAliases = new Dictionary<MemoryIndex, IMemoryAliasBuilder>();
        private Snapshot targetSnapshot;
        private List<Snapshot> sourceSnapshots;
        private HashSet<ObjectValue> objects = new HashSet<ObjectValue>();
        private LinkedList<MergeOperation> operationStack = new LinkedList<MergeOperation>();
        private bool isCallMerge;

        /// <summary>
        /// Gets the structure of memory model which is used to merge info values into.
        /// </summary>
        /// <value>
        /// The structure.
        /// </value>
        public ISnapshotStructureProxy Structure { get; private set; }

        /// <summary>
        /// Gets the data container with result of merge.
        /// </summary>
        /// <value>
        /// The infos.
        /// </value>
        public ISnapshotDataProxy Infos { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeInfoWorker"/> class.
        /// </summary>
        /// <param name="targetSnapshot">The target snapshot.</param>
        /// <param name="sourceSnapshots">The source snapshots.</param>
        /// <param name="isCallMerge">if set to <c>true</c> [is call merge].</param>
        public MergeInfoWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots, bool isCallMerge = false)
        {
            Infos = Snapshot.SnapshotDataFactory.CreateEmptyInstance(targetSnapshot);
            Structure = targetSnapshot.Structure;
            Structure.Locked = true;

            this.targetSnapshot = targetSnapshot;
            this.sourceSnapshots = sourceSnapshots;
            this.isCallMerge = isCallMerge;
        }

        /// <summary>
        /// Main method of merge algorithm.
        /// 
        /// iI first phase prepares new empty data collection. Then collects all root memory locations and
        /// prepares their operations. As the final step process all merge operations which traverses the
        /// memory tree and merges data from all source indexes.
        /// </summary>
        internal void Merge()
        {
            ContainerOperations[] collectVariables = new ContainerOperations[targetSnapshot.CallLevel + 1];
            ContainerOperations[] collectControl = new ContainerOperations[targetSnapshot.CallLevel + 1];
            MergeOperation returnOperation = new MergeOperation();

            for (int x = 0; x <= targetSnapshot.CallLevel; x++)
            {
                IReadonlyIndexContainer variables = Structure.Readonly.GetReadonlyStackContext(x).ReadonlyVariables;
                collectVariables[x] = new ContainerOperations(this, variables, variables.UnknownIndex, variables.UnknownIndex);

                IReadonlyIndexContainer control = Structure.Readonly.GetReadonlyStackContext(x).ReadonlyControllVariables;
                collectControl[x] = new ContainerOperations(this, control, control.UnknownIndex, control.UnknownIndex);
            }

            foreach (Snapshot snapshot in sourceSnapshots)
            {
                for (int sourceLevel = 0, targetLevel = 0; targetLevel <= targetSnapshot.CallLevel; sourceLevel++, targetLevel++)
                {
                    if (sourceLevel == snapshot.CallLevel && snapshot.CallLevel != targetSnapshot.CallLevel)
                    {
                        if (isCallMerge)
                        {
                            break;
                        }
                        else
                        {
                            targetLevel = targetSnapshot.CallLevel;
                        }
                    }

                    IReadonlyIndexContainer variables = snapshot.Structure.Readonly.GetReadonlyStackContext(sourceLevel).ReadonlyVariables;
                    collectVariables[targetLevel].AddContainer(variables, snapshot);

                    IReadonlyIndexContainer control = snapshot.Structure.Readonly.GetReadonlyStackContext(sourceLevel).ReadonlyControllVariables;
                    collectControl[targetLevel].AddContainer(control, snapshot);
                }
            }

            mergeObjects();

            for (int x = 0; x <= targetSnapshot.CallLevel; x++)
            {
                collectVariables[x].MergeContainers();
                collectControl[x].MergeContainers();
                mergeTemporary(x);
            }

            processMerge();
        }

        /// <summary>
        /// Process all merge operations. Continues processing until operation stack is empty.
        /// </summary>
        private void processMerge()
        {
            while (operationStack.Count > 0)
            {
                MergeOperation operation = getOperation();

                processMergeOperation(operation);
            }
        }

        /// <summary>
        /// Processes single merge operation - prepares all valid values and alias informations from the source indexes.
        /// When the source indexes contains some array values prepares operation for every descendant index and merge the
        /// array into one which will be stored in the target memory entry.
        /// </summary>
        /// <param name="operation">The operation.</param>
        private void processMergeOperation(MergeOperation operation)
        {
            HashSet<Value> values = new HashSet<Value>();
            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                MemoryEntry entry = snapshot.Infos.Readonly.GetMemoryEntry(index);
                CollectionTools.AddAll(values, entry.PossibleValues);
            }

            if (Structure.Readonly.HasArray(operation.TargetIndex))
            {
                mergeArrays(operation);
            }

            if (operation.IsUndefined)
            {
                values.Add(targetSnapshot.UndefinedValue);
            }

            Infos.Writeable.SetMemoryEntry(operation.TargetIndex, targetSnapshot.CreateMemoryEntry(values));
        }

        /// <summary>
        /// Prepares operation for every descendant index of target array.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns>Array where the input arrays is merged into.</returns>
        private void mergeArrays(MergeOperation operation)
        {
            AssociativeArray targetArray = Structure.Readonly.GetArray(operation.TargetIndex);
            IArrayDescriptor targetDescriptor = Structure.Readonly.GetDescriptor(targetArray);
            ContainerOperations collectIndexes = new ContainerOperations(this, targetDescriptor.Builder(), operation.TargetIndex, targetDescriptor.UnknownIndex);

            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                AssociativeArray arrayValue;
                if (snapshot.Structure.Readonly.TryGetArray(index, out arrayValue))
                {
                    IArrayDescriptor descriptor = snapshot.Structure.Readonly.GetDescriptor(arrayValue);
                    collectIndexes.AddContainer(descriptor, snapshot);
                }
                else
                {
                    collectIndexes.SetUndefined();
                }
            }

            collectIndexes.MergeContainers();
        }

        #region Temporary

        /// <summary>
        /// Creates merge operations for all temporary indexes in the target structure.
        /// </summary>
        /// <param name="index">The index.</param>
        private void mergeTemporary(int index)
        {
            foreach (var temp in Structure.Readonly.GetReadonlyStackContext(index).ReadonlyTemporaryVariables)
            {
                MergeOperation operation = new MergeOperation(temp);
                addOperation(operation);

                foreach (Snapshot snapshot in sourceSnapshots)
                {
                    if (snapshot.Structure.Readonly.IsDefined(temp))
                    {
                        operation.Add(temp, snapshot);
                    }
                    else
                    {
                        operation.SetUndefined();
                    }
                }
            }
        }

        #endregion

        #region Objects

        private void collectObjects(Snapshot snapshot)
        {
            foreach (var obj in snapshot.Structure.Readonly.ObjectDescriptors)
            {
                IObjectDescriptor descriptor;
                if (!Structure.Readonly.TryGetDescriptor(obj.Key, out descriptor))
                {
                    objects.Add(obj.Key);
                }
            }
        }

        /// <summary>
        /// For all fields of objects in the target structure prepares the merge operations.
        /// </summary>
        private void mergeObjects()
        {
            foreach (var objectValue in targetSnapshot.Structure.Readonly.ObjectDescriptors)
            {
                mergeObject(objectValue.Key);
            }
        }

        /// <summary>
        /// Creates the merge operation for all fields of specified object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        private void mergeObject(ObjectValue objectValue)
        {
            IObjectDescriptor targetDescriptor = Structure.Readonly.GetDescriptor(objectValue);
            ContainerOperations collectVariables = new ContainerOperations(this, targetDescriptor.Builder(), targetDescriptor.UnknownIndex, targetDescriptor.UnknownIndex);

            foreach (Snapshot snapshot in sourceSnapshots)
            {
                IObjectDescriptor descriptor;
                if (snapshot.Structure.Readonly.TryGetDescriptor(objectValue, out descriptor))
                {
                    collectVariables.AddContainer(descriptor, snapshot);
                }
                else
                {
                    collectVariables.SetUndefined();
                }
            }

            collectVariables.MergeContainers();
        }

        #endregion

        /// <summary>
        /// Adds operation into stack of merge worker.
        /// </summary>
        /// <param name="operation">The operation.</param>
        public void addOperation(MergeOperation operation)
        {
            operationStack.AddLast(operation);
        }

        /// <summary>
        /// Gets the operation at the top of operation stack. Operation is removed from stack.
        /// </summary>
        /// <returns>First operation in the memory stack</returns>
        private MergeOperation getOperation()
        {
            MergeOperation operation = operationStack.First.Value;
            operationStack.RemoveFirst();

            return operation;
        }
    }
}