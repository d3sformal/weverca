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
    class MergeWorker : IReferenceHolder, IMergeWorker
    {
        private Dictionary<MemoryIndex, MemoryAliasBuilder> memoryAliases = new Dictionary<MemoryIndex, MemoryAliasBuilder>();

        internal SnapshotStructure Structure { get; private set; }
        internal SnapshotData Data { get; private set; }

        internal Snapshot targetSnapshot;
        internal List<Snapshot> sourceSnapshots;

        private HashSet<ObjectValue> objects = new HashSet<ObjectValue>();

        private LinkedList<MergeOperation> operationStack = new LinkedList<MergeOperation>();

        private bool isCallMerge;

        public MergeWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots, bool isCallMerge = false)
        {
            Data = SnapshotData.CreateEmpty(targetSnapshot);
            Structure = SnapshotStructure.CreateEmpty(targetSnapshot);
            Structure.Data = Data;

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
                IndexContainer variables = new IndexContainer(VariableIndex.CreateUnknown(x));
                Structure.Variables[x] = variables;
                collectVariables[x] = new ContainerOperations(this, variables, variables.UnknownIndex, variables.UnknownIndex);

                IndexContainer control = new IndexContainer(ControlIndex.CreateUnknown(x));
                Structure.ContolVariables[x] = control;
                collectControl[x] = new ContainerOperations(this, control, control.UnknownIndex, control.UnknownIndex);

                Structure.Temporary[x] = new IndexSet<TemporaryIndex>();
                Structure.Arrays[x] = new IndexSet<AssociativeArray>();
            }

            foreach (Snapshot snapshot in sourceSnapshots)
            {
                collectObjects(snapshot);


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

                    collectVariables[targetLevel].CollectIndexes(snapshot, Structure.Variables[targetLevel].UnknownIndex, snapshot.Structure.Variables[sourceLevel]);
                    collectControl[targetLevel].CollectIndexes(snapshot, Structure.ContolVariables[targetLevel].UnknownIndex, snapshot.Structure.ContolVariables[sourceLevel]);
                    collectTemporary(snapshot, sourceLevel, targetLevel);
                }

                mergeDeclarations(Structure.FunctionDecl, snapshot.Structure.FunctionDecl);
                mergeDeclarations(Structure.ClassDecl, snapshot.Structure.ClassDecl);

                if (isCallMerge)
                {
                    foreach (AssociativeArray array in snapshot.Structure.Arrays.Local)
                    {
                        Structure.AddCallArray(array, snapshot);
                    }
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


            foreach (var alias in memoryAliases)
            {
                Structure.SetAlias(alias.Key, alias.Value.Build());
            }
        }

        private void mergeDeclarations<T>(DeclarationContainer<T> target, DeclarationContainer<T> source)
        {
            foreach (var name in source.GetNames())
            {
                foreach (var decl in source.GetValue(name))
                {
                    target.Add(name, decl);
                }
            }
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
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            ReferenceCollector references = new ReferenceCollector();

            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                MemoryEntry entry = snapshot.Structure.GetMemoryEntry(index);
                visitor.VisitMemoryEntry(entry);

                MemoryAlias aliases;
                if (snapshot.Structure.TryGetAliases(index, out aliases))
                {
                    references.CollectMust(aliases.MustAliasses, targetSnapshot.CallLevel);
                    references.CollectMay(aliases.MayAliasses, targetSnapshot.CallLevel);
                }
                else
                {
                    references.InvalidateMust();
                }

                //TODO - merge aliases, infos

            }

            references.SetAliases(operation.TargetIndex, this, !operation.IsUndefined);

            HashSet<Value> values = getValues(operation, visitor);

            if (operation.IsUndefined)
            {
                values.Add(targetSnapshot.UndefinedValue);
            }

            Structure.SetMemoryEntry(operation.TargetIndex, new MemoryEntry(values));
            Structure.SetObjects(operation.TargetIndex, new ObjectValueContainer(visitor.Objects));
        }

        private HashSet<Value> getValues(MergeOperation operation, CollectComposedValuesVisitor visitor)
        {
            HashSet<Value> values = visitor.Values;
            bool noScalarValue = visitor.Values.Count == 0;

            if (visitor.Arrays.Count > 0)
            {
                values.Add(mergeArrays(operation));
            }

            foreach (ObjectValue value in visitor.Objects)
            {
                values.Add(value);
            }

            return values;
        }

        private Value mergeArrays(MergeOperation operation)
        {
            ArrayDescriptorBuilder builder = new ArrayDescriptorBuilder();
            builder.SetParentVariable(operation.TargetIndex);
            builder.SetUnknownField(operation.TargetIndex.CreateUnknownIndex());

            ContainerOperations collectVariables = new ContainerOperations(this, builder, operation.TargetIndex, builder.UnknownIndex);

            AssociativeArray targetArray = null;
            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                AssociativeArray arrayValue;
                if (snapshot.Structure.TryGetArray(index, out arrayValue))
                {

                    if (targetArray == null)
                    {
                        if (index.Equals(operation.TargetIndex))
                        {
                            targetArray = arrayValue;
                        }
                    }

                    ArrayDescriptor descriptor = snapshot.Structure.GetDescriptor(arrayValue);
                    collectVariables.CollectIndexes(snapshot, index, descriptor);
                }
                else
                {
                    collectVariables.SetUndefined();
                }
            }

            if (targetArray == null)
            {
                targetArray = targetSnapshot.CreateArray();
            }
            builder.SetArrayValue(targetArray);

            collectVariables.MergeContainer();

            Structure.SetArray(operation.TargetIndex, targetArray);
            Structure.SetDescriptor(targetArray, builder.Build());

            return targetArray;
        }

        #region Temporary

        private void collectTemporary(Snapshot snapshot, int sourceLevel, int targetLevel)
        {
            foreach (TemporaryIndex temp in snapshot.Structure.Temporary[sourceLevel])
            {
                if (!Structure.Temporary[targetLevel].Contains(temp))
                {
                    Structure.Temporary[targetLevel].Add(temp);
                }
            }
        }

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
            foreach (ObjectValue objectValue in objects)
            {
                mergeObject(objectValue);
            }
        }

        private void mergeObject(ObjectValue objectValue)
        {
            ObjectDescriptorBuilder builder =
                new ObjectDescriptor(objectValue, null, ObjectIndex.CreateUnknown(objectValue))
                .Builder();

            ContainerOperations collectVariables = new ContainerOperations(this, builder, builder.UnknownIndex, builder.UnknownIndex);
            
            foreach (Snapshot snapshot in sourceSnapshots)
            {
                ObjectDescriptor descriptor;
                if (snapshot.Structure.TryGetDescriptor(objectValue, out descriptor))
                {
                    collectVariables.CollectIndexes(snapshot, builder.UnknownIndex, descriptor);
                    builder.Type = descriptor.Type;
                }
                else
                {
                    collectVariables.SetUndefined();
                }
            }

            collectVariables.MergeContainer();
            Structure.ObjectDescriptors.Add(objectValue, builder.Build());
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

        public void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            MemoryAliasBuilder alias;
            if (!memoryAliases.TryGetValue(index, out alias))
            {
                alias = new MemoryAliasBuilder();
            }

            if (mustAliases != null)
            {
                alias.AddMustAlias(mustAliases);
            }
            if (mayAliases != null)
            {
                alias.AddMayAlias(mayAliases);
            }

            foreach (MemoryIndex mustIndex in alias.MustAliasses)
            {
                if (alias.MayAliasses.Contains(mustIndex))
                {
                    alias.MayAliasses.Remove(mustIndex);
                }
            }

            memoryAliases[index] = alias;
        }

        public void AddAlias(MemoryIndex index, MemoryIndex mustAlias, MemoryIndex mayAlias)
        {
            MemoryAliasBuilder alias;
            if (!memoryAliases.TryGetValue(index, out alias))
            {
                alias = new MemoryAliasBuilder();
            }

            if (mustAlias != null)
            {
                alias.AddMustAlias(mustAlias);

                if (alias.MayAliasses.Contains(mustAlias))
                {
                    alias.MayAliasses.Remove(mustAlias);
                }
            }

            if (mayAlias != null && !alias.MustAliasses.Contains(mayAlias))
            {
                alias.AddMayAlias(mayAlias);
            }

            memoryAliases[index] = alias;
        }
    }

    class ContainerOperations
    {
        private IWriteableIndexContainer targetContainer;
        private IMergeWorker worker;
        private MergeOperation unknownOperation;
        private List<Tuple<ReadonlyIndexContainer, Snapshot>> sources = new List<Tuple<ReadonlyIndexContainer, Snapshot>>();
        private bool isUndefined = false;
        private MemoryIndex targetIndex;

        HashSet<string> undefinedIndexes = new HashSet<string>();

        public ContainerOperations(
            IMergeWorker worker, 
            IWriteableIndexContainer targetContainer, 
            MemoryIndex targetIndex, 
            MemoryIndex unknownIndex)
        {
            this.targetContainer = targetContainer;
            this.worker = worker;
            this.targetIndex = targetIndex;

            unknownOperation = new MergeOperation(unknownIndex);
            worker.addOperation(unknownOperation);
        }

        public void CollectIndexes(
            Snapshot sourceSnapshot,
            MemoryIndex sourceIndex,
            ReadonlyIndexContainer sourceContainer)
        {
            sources.Add(new Tuple<ReadonlyIndexContainer, Snapshot>(sourceContainer, sourceSnapshot));

            unknownOperation.Add(sourceContainer.UnknownIndex, sourceSnapshot);

            bool indexEquals = targetIndex.Equals(sourceIndex);

            foreach (var index in sourceContainer.Indexes)
            {
                MemoryIndex containerIndex;
                if (targetContainer.Indexes.TryGetValue(index.Key, out containerIndex))
                {
                    if (containerIndex == null && indexEquals)
                    {
                        targetContainer.Indexes[index.Key] = index.Value;
                        undefinedIndexes.Remove(index.Key);
                    }
                }
                else if (indexEquals)
                {
                    targetContainer.Indexes.Add(index.Key, index.Value);
                }
                else
                {
                    targetContainer.Indexes.Add(index.Key, null);
                    undefinedIndexes.Add(index.Key);
                }
            }
        }

        public void AddContainer(ReadonlyIndexContainer sourceContainer, Snapshot sourceSnapshot)
        {
            sources.Add(new Tuple<ReadonlyIndexContainer, Snapshot>(sourceContainer, sourceSnapshot));

            unknownOperation.Add(sourceContainer.UnknownIndex, sourceSnapshot);
        }

        public void MergeContainer()
        {
            foreach (string indexName in undefinedIndexes)
            {
                targetContainer.Indexes[indexName] = targetIndex.CreateIndex(indexName);
            }

            foreach (var index in targetContainer.Indexes)
            {
                MergeOperation operation = new MergeOperation(index.Value);
                worker.addOperation(operation);

                if (isUndefined)
                {
                    operation.SetUndefined();
                }

                foreach (var source in sources)
                {
                    ReadonlyIndexContainer container = source.Item1;
                    Snapshot snapshot = source.Item2;

                    MemoryIndex containerIndex;
                    if (container.Indexes.TryGetValue(index.Key, out containerIndex))
                    {
                        operation.Add(containerIndex, snapshot);
                    }
                    else
                    {
                        operation.Add(container.UnknownIndex, snapshot);
                    }
                }
            }
        }

        internal void SetUndefined()
        {
            isUndefined = true;
        }
    }
}
