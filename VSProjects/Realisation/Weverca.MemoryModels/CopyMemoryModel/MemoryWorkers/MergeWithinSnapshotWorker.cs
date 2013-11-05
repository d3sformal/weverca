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
    class MergeWithinSnapshotWorker
    {
        private Snapshot snapshot;

        private LinkedList<MergeOperation> operationStack = new LinkedList<MergeOperation>();

        public MergeWithinSnapshotWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        int arrayCount = 0;

        public void MergeMemoryEntry(MemoryIndex targetIndex, MemoryEntry entry)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(entry);
            arrayCount = visitor.Arrays.Count;

            HashSet<Value> values = getValues(targetIndex, visitor, false);

            processMerge();

            snapshot.SetMemoryEntry(targetIndex, new MemoryEntry(values));
        }

        public void MergeIndexes(MemoryIndex targetIndex, MemoryIndex sourceIndex)
        {
            MergeOperation operation = new MergeOperation(targetIndex);
            operation.Add(targetIndex);
            operation.Add(sourceIndex);
            addOperation(operation);

            arrayCount = 2;

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

            foreach (var index in operation.Indexes)
            {
                MemoryEntry entry = snapshot.GetMemoryEntry(index);
                visitor.VisitMemoryEntry(entry);

                //TODO - merge aliases, infos
            }

            HashSet<Value> values = getValues(operation.TargetIndex, visitor, operation.IsUndefined);

            if (operation.IsUndefined)
            {
                values.Add(snapshot.UndefinedValue);
            }

            snapshot.SetMemoryEntry(operation.TargetIndex, new MemoryEntry(values));
        }

        private HashSet<Value> getValues(MemoryIndex targetIndex, CollectComposedValuesVisitor visitor, bool includeUndefined)
        {
            HashSet<Value> values = visitor.Values;
            bool noScalarValue = visitor.Values.Count == 0;

            if (visitor.Arrays.Count > 0)
            {
                bool mustBeArray = !includeUndefined 
                    && noScalarValue 
                    && visitor.Objects.Count == 0 
                    && visitor.Arrays.Count == arrayCount;

                values.Add(mergeArrays(targetIndex, visitor.Arrays, !mustBeArray));
            }

            if (visitor.Objects.Count > 0)
            {
                bool mustBeObject = !includeUndefined && noScalarValue && visitor.Arrays.Count == 0;
                mergeObjects(targetIndex, visitor.Objects, mustBeObject, values);
            }

            return values;
        }

        private void mergeObjects(MemoryIndex targetIndex, HashSet<ObjectValue> objects, bool mustBeObject, HashSet<Value> values)
        {
            if (objects.Count == 1)
            {
                ObjectValue objectValue = objects.First();
                values.Add(objectValue);

                if (mustBeObject)
                {
                    snapshot.MakeMustReferenceObject(objectValue, targetIndex);
                }
                else
                {
                    snapshot.MakeMayReferenceObject(objects, targetIndex);
                }

            }
            else if (objects.Count > 1)
            {
                snapshot.MakeMayReferenceObject(objects, targetIndex);

                foreach (ObjectValue objectValue in objects)
                {
                    values.Add(objectValue);
                }
            }
        }

        private AssociativeArray mergeArrays(
            MemoryIndex targetIndex,
            HashSet<AssociativeArray> arrays,
            bool includeUndefined)
        {
            AssociativeArray arrayValue;
            if (!snapshot.TryGetArray(targetIndex, out arrayValue))
            {
                arrayValue = snapshot.CreateArray(targetIndex);
            }
            ArrayDescriptor newDescriptor = snapshot.GetDescriptor(arrayValue);
            
            MergeOperation unknownMerge = new MergeOperation(newDescriptor.UnknownIndex);
            unknownMerge.SetUndefined();
            addOperation(unknownMerge);

            HashSet<string> indexNames = new HashSet<string>();
            foreach (var array in arrays)
            {
                ArrayDescriptor descriptor = snapshot.GetDescriptor(array);
                unknownMerge.Add(descriptor.UnknownIndex);

                foreach (var index in descriptor.Indexes)
                {
                    indexNames.Add(index.Key);
                }
            }

            foreach (string indexName in indexNames)
            {
                MergeOperation operation = new MergeOperation();

                if (includeUndefined)
                {
                    operation.SetUndefined();
                }

                foreach (var array in arrays)
                {
                    ArrayDescriptor descriptor = snapshot.GetDescriptor(array);
                    MemoryIndex sourceIndex;
                    if (descriptor.Indexes.TryGetValue(indexName, out sourceIndex))
                    {
                        operation.Add(sourceIndex);
                    }
                    else
                    {
                        operation.Add(descriptor.UnknownIndex);
                    }
                }

                MemoryIndex arrayIndex;
                if (!newDescriptor.Indexes.TryGetValue(indexName, out arrayIndex))
                {
                    arrayIndex = snapshot.CreateIndex(indexName, arrayValue, true, false);
                }
                operation.SetTargetIndex(arrayIndex);
                addOperation(operation);
            }

            return arrayValue;
        }

        private void addOperation(MergeOperation operation)
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

    class MergeOperation
    {
        public readonly HashSet<MemoryIndex> Indexes = new HashSet<MemoryIndex>();

        public bool IsUndefined { get; private set; }
        public MemoryIndex TargetIndex { get; private set; }

        public MergeOperation(MemoryIndex targetIndex)
        {
            IsUndefined = false;
            TargetIndex = targetIndex;
        }

        public MergeOperation()
        {
            IsUndefined = false;
        }

        internal void Add(MemoryIndex memoryIndex)
        {
            Indexes.Add(memoryIndex);
        }

        internal void SetUndefined()
        {
            IsUndefined = true;
        }

        internal void SetTargetIndex(MemoryIndex targetIndex)
        {
            TargetIndex = targetIndex;
        }
    }

    class CollectComposedValuesVisitor : AbstractValueVisitor
    {
        public readonly HashSet<AssociativeArray> Arrays = new HashSet<AssociativeArray>();
        public readonly HashSet<ObjectValue> Objects = new HashSet<ObjectValue>();
        public readonly HashSet<Value> Values = new HashSet<Value>();

        public Snapshot Snapshot { get; set; }

        public override void VisitValue(Value value)
        {
            Values.Add(value);
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            Objects.Add(value);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            Arrays.Add(value);
        }
    }
}
