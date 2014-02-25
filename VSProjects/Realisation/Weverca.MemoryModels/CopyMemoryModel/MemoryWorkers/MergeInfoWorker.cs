using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class MergeInfoWorker : IMergeWorker
    {
        private Dictionary<MemoryIndex, MemoryAliasBuilder> memoryAliases = new Dictionary<MemoryIndex, MemoryAliasBuilder>();

        internal SnapshotStructure Structure { get; private set; }
        internal SnapshotData Infos { get; private set; }

        internal Snapshot targetSnapshot;
        internal List<Snapshot> sourceSnapshots;

        private HashSet<ObjectValue> objects = new HashSet<ObjectValue>();

        private LinkedList<MergeOperation> operationStack = new LinkedList<MergeOperation>();

        private bool isCallMerge;

        public MergeInfoWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots, bool isCallMerge = false)
        {
            Infos = SnapshotData.CreateEmpty(targetSnapshot);
            Structure = targetSnapshot.Structure;
            Structure.Data = Infos;
            Structure.Locked = true;

            this.targetSnapshot = targetSnapshot;
            this.sourceSnapshots = sourceSnapshots;
            this.isCallMerge = isCallMerge;
        }

        internal void Merge()
        {
            ContainerOperations[] collectVariables = new ContainerOperations[targetSnapshot.CallLevel + 1];
            ContainerOperations[] collectControl = new ContainerOperations[targetSnapshot.CallLevel + 1];
            MergeOperation returnOperation = new MergeOperation();

            for (int x = 0; x <= targetSnapshot.CallLevel; x++)
            {
                IndexContainer variables = Structure.Variables[x];
                collectVariables[x] = new ContainerOperations(this, variables, variables.UnknownIndex, variables.UnknownIndex);

                IndexContainer control = Structure.ContolVariables[x];
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

                    collectVariables[targetLevel].AddContainer(snapshot.Structure.Variables[sourceLevel], snapshot);
                    collectControl[targetLevel].AddContainer(snapshot.Structure.ContolVariables[sourceLevel], snapshot);
                }
            }

            mergeObjects();

            for (int x = 0; x <= targetSnapshot.CallLevel; x++)
            {
                collectVariables[x].MergeContainer();
                collectControl[x].MergeContainer();
                mergeTemporary(x);
            }

            processMerge();
        }

        private void processMerge()
        {
            while (operationStack.Count > 0)
            {
                MergeOperation operation = getOperation();

                processMergeOperation(operation);
            }
        }

        private void processMergeOperation(MergeOperation operation)
        {
            HashSet<Value> values = new HashSet<Value>();
            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                MemoryEntry entry = snapshot.Infos.GetMemoryEntry(index);
                HashSetTools.AddAll(values, entry.PossibleValues);
            }

            if (Structure.HasArray(operation.TargetIndex))
            {
                mergeArrays(operation);
            }

            if (operation.IsUndefined)
            {
                values.Add(targetSnapshot.UndefinedValue);
            }

            Structure.SetMemoryEntry(operation.TargetIndex, new MemoryEntry(values));
        }

        private void mergeArrays(MergeOperation operation)
        {
            AssociativeArray targetArray = Structure.GetArray(operation.TargetIndex);
            ArrayDescriptor targetDescriptor = Structure.GetDescriptor(targetArray);
            ContainerOperations collectIndexes = new ContainerOperations(this, targetDescriptor.Builder(), operation.TargetIndex, targetDescriptor.UnknownIndex);

            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                AssociativeArray arrayValue;
                if (snapshot.Structure.TryGetArray(index, out arrayValue))
                {
                    ArrayDescriptor descriptor = snapshot.Structure.GetDescriptor(arrayValue);
                    collectIndexes.AddContainer(descriptor, snapshot);
                }
                else
                {
                    collectIndexes.SetUndefined();
                }
            }

            collectIndexes.MergeContainer();
        }

        #region Temporary

        private void mergeTemporary(int index)
        {
            foreach (var temp in Structure.Temporary[index])
            {
                MergeOperation operation = new MergeOperation(temp);
                addOperation(operation);

                foreach (Snapshot snapshot in sourceSnapshots)
                {
                    if (snapshot.Structure.IsDefined(temp))
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
            foreach (var obj in snapshot.Structure.ObjectDescriptors)
            {
                if (!Structure.ObjectDescriptors.ContainsKey(obj.Key))
                {
                    objects.Add(obj.Key);
                }
            }
        }

        private void mergeObjects()
        {
            foreach (var objectValue in targetSnapshot.Structure.ObjectDescriptors)
            {
                mergeObject(objectValue.Key);
            }
        }

        private void mergeObject(ObjectValue objectValue)
        {
            ObjectDescriptor targetDescriptor = Structure.GetDescriptor(objectValue);
            ContainerOperations collectVariables = new ContainerOperations(this, targetDescriptor.Builder(), targetDescriptor.UnknownIndex, targetDescriptor.UnknownIndex);

            foreach (Snapshot snapshot in sourceSnapshots)
            {
                ObjectDescriptor descriptor;
                if (snapshot.Structure.TryGetDescriptor(objectValue, out descriptor))
                {
                    collectVariables.AddContainer(descriptor, snapshot);
                }
                else
                {
                    collectVariables.SetUndefined();
                }
            }

            collectVariables.MergeContainer();
        }

        #endregion

        public void addOperation(MergeOperation operation)
        {
            operationStack.AddLast(operation);
        }

        private MergeOperation getOperation()
        {
            MergeOperation operation = operationStack.First.Value;
            operationStack.RemoveFirst();

            return operation;
        }
    }
}
