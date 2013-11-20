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
    public class MergeWorker : IReferenceHolder
    {
        internal HashSet<MemoryIndex> memoryIndexes = new HashSet<MemoryIndex>();

        internal Dictionary<AssociativeArray, ArrayDescriptor> arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
        internal Dictionary<ObjectValue, ObjectDescriptor> objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();

        internal Dictionary<Value, MemoryInfo> memoryValueInfos = new Dictionary<Value, MemoryInfo>();

        internal Dictionary<MemoryIndex, MemoryEntry> memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>();
        internal Dictionary<MemoryIndex, MemoryAliasBuilder> memoryAliases = new Dictionary<MemoryIndex, MemoryAliasBuilder>();
        internal Dictionary<MemoryIndex, MemoryInfo> memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>();

        internal Dictionary<MemoryIndex, AssociativeArray> indexArrays = new Dictionary<MemoryIndex, AssociativeArray>();
        internal Dictionary<MemoryIndex, ObjectValueContainer> indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>();

        internal HashSet<TemporaryIndex> temporary = new HashSet<TemporaryIndex>();
        internal IndexContainer Variables { get; private set; }
        internal IndexContainer ContollVariables { get; private set; }

        internal Snapshot targetSnapshot;
        internal List<Snapshot> sourceSnapshots;

        private HashSet<ObjectValue> objects = new HashSet<ObjectValue>();

        private LinkedList<MergeOperation> operationStack = new LinkedList<MergeOperation>();

        public MergeWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots)
        {
            this.targetSnapshot = targetSnapshot;
            this.sourceSnapshots = sourceSnapshots;

            Variables = new IndexContainer(VariableIndex.CreateUnknown());
        }

        internal void Merge()
        {
            ContainerOperations collectVariables = new ContainerOperations(this, Variables, Variables.UnknownIndex, Variables.UnknownIndex);

            foreach (Snapshot snapshot in sourceSnapshots)
            {
                collectObjects(snapshot);
                collectTemporary(snapshot);
                collectVariables.CollectIndexes(snapshot, Variables.UnknownIndex, snapshot.Variables);
            }

            mergeObjects();
            mergeTemporary();
            collectVariables.MergeContainer();

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
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            ReferenceCollector references = new ReferenceCollector();

            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                MemoryEntry entry = snapshot.GetMemoryEntry(index);
                visitor.VisitMemoryEntry(entry);

                MemoryAlias aliases;
                if (snapshot.TryGetAliases(index, out aliases))
                {
                    references.CollectMust(aliases.MustAliasses);
                    references.CollectMay(aliases.MayAliasses);
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

            memoryEntries.Add(operation.TargetIndex, new MemoryEntry(values));
            memoryIndexes.Add(operation.TargetIndex);
            indexObjects.Add(operation.TargetIndex, new ObjectValueContainer(visitor.Objects));
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
            ArrayDescriptorBuilder builder = new ArrayDescriptor().Builder();
            builder.SetParentVariable(operation.TargetIndex);
            builder.SetUnknownField(operation.TargetIndex.CreateUnknownIndex());

            ContainerOperations collectVariables = new ContainerOperations(this, builder, operation.TargetIndex, builder.UnknownIndex);

            AssociativeArray targetArray = null;
            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                AssociativeArray arrayValue;
                if (snapshot.TryGetArray(index, out arrayValue))
                {

                    if (targetArray == null)
                    {
                        if (index.Equals(operation.TargetIndex))
                        {
                            targetArray = arrayValue;
                        }
                    }

                    ArrayDescriptor descriptor = snapshot.GetDescriptor(arrayValue);
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

            arrayDescriptors.Add(targetArray, builder.Build());
            indexArrays.Add(operation.TargetIndex, targetArray);

            return targetArray;
        }

        #region Temporary

        private void collectTemporary(Snapshot snapshot)
        {
            foreach (TemporaryIndex temp in snapshot.Temporary)
            {
                if (!temporary.Contains(temp))
                {
                    temporary.Add(temp);
                }
            }
        }

        private void mergeTemporary()
        {
            foreach (var temp in temporary)
            {
                MergeOperation operation = new MergeOperation(temp);
                addOperation(operation);

                foreach (Snapshot snapshot in sourceSnapshots)
                {
                    if (snapshot.Exists(temp))
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
            foreach (var obj in snapshot.Objects)
            {
                if (!objectDescriptors.ContainsKey(obj.Key))
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
                if (snapshot.TryGetDescriptor(objectValue, out descriptor))
                {
                    collectVariables.CollectIndexes(snapshot, builder.UnknownIndex, descriptor);
                    collectTypes(descriptor.Types, builder.Types);
                }
                else
                {
                    collectVariables.SetUndefined();
                }
            }

            collectVariables.MergeContainer();
            objectDescriptors.Add(objectValue, builder.Build());
        }

        private void collectTypes(IEnumerable<TypeValueBase> sourceTypes, HashSet<TypeValueBase> targetTypes)
        {
            foreach (var type in sourceTypes)
            {
                targetTypes.Add(type);
            }
        }

        #endregion

        internal void addOperation(MergeOperation operation)
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

        internal Dictionary<MemoryIndex, MemoryAlias> GetMemoryAliases()
        {
            Dictionary<MemoryIndex, MemoryAlias> aliases = new Dictionary<MemoryIndex, MemoryAlias>();

            foreach (var alias in memoryAliases)
            {
                aliases[alias.Key] = alias.Value.Build();
            }

            return aliases;
        }
    }

    class ContainerOperations
    {
        private IWriteableIndexContainer targetContainer;
        private MergeWorker worker;
        private MergeOperation unknownOperation;
        private List<Tuple<ReadonlyIndexContainer, Snapshot>> sources = new List<Tuple<ReadonlyIndexContainer, Snapshot>>();
        private bool isUndefined = false;
        private MemoryIndex targetIndex;

        HashSet<string> undefinedIndexes = new HashSet<string>();

        public ContainerOperations(
            MergeWorker worker, 
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

    
    class CollectValuesVisitor : AbstractValueVisitor
    {
        public readonly HashSet<Value> Values = new HashSet<Value>();

        public Snapshot Snapshot { get; set; }

        public override void VisitValue(Value value)
        {
            Values.Add(value);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {

        }
    }
}
