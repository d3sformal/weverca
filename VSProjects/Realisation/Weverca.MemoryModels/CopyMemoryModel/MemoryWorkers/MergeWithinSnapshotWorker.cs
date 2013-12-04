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

        private LinkedList<MergeWithinSnapshotOperation> operationStack = new LinkedList<MergeWithinSnapshotOperation>();

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

            snapshot.Data.SetMemoryEntry(targetIndex, new MemoryEntry(values));
        }

        public void MergeIndexes(MemoryIndex targetIndex, MemoryIndex sourceIndex)
        {
            MergeWithinSnapshotOperation operation = new MergeWithinSnapshotOperation(targetIndex);
            operation.Add(targetIndex);
            operation.Add(sourceIndex);
            operation.IsRoot = true;
            addOperation(operation);

            arrayCount = 2;

            processMerge();
        }

        private void processMerge()
        {
            while (operationStack.Count > 0)
            {
                MergeWithinSnapshotOperation operation = getOperation();

                processMergeOperation(operation);
            }
        }

        private void processMergeOperation(MergeWithinSnapshotOperation operation)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            ReferenceCollector references = new ReferenceCollector();

            foreach (var index in operation.Indexes)
            {
                MemoryEntry entry = snapshot.GetMemoryEntry(index);
                visitor.VisitMemoryEntry(entry);

                MemoryAlias aliases;
                if (snapshot.Data.TryGetAliases(index, out aliases))
                {
                    references.CollectMust(aliases.MustAliasses, snapshot.CallLevel);
                    references.CollectMay(aliases.MayAliasses, snapshot.CallLevel);
                }
                else
                {
                    references.InvalidateMust();
                }

                //TODO - merge aliases, infos
            }

            if (references.HasAliases && !operation.IsRoot)
            {
                if (!operation.IsUndefined && operation.Indexes.Count == 1 && references.HasMustAliases)
                {
                    references.AddMustAlias(operation.Indexes.First());
                }
                else
                {
                    references.CollectMay(operation.Indexes, snapshot.CallLevel);
                }
            }

            references.SetAliases(operation.TargetIndex, snapshot, !operation.IsUndefined);

            HashSet<Value> values = getValues(operation.TargetIndex, visitor, operation.IsUndefined);

            if (operation.IsUndefined)
            {
                values.Add(snapshot.UndefinedValue);
            }

            snapshot.Data.SetMemoryEntry(operation.TargetIndex, new MemoryEntry(values));
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
            if (!snapshot.Data.TryGetArray(targetIndex, out arrayValue))
            {
                arrayValue = snapshot.CreateArray(targetIndex);
            }
            ArrayDescriptor newDescriptor = snapshot.Data.GetDescriptor(arrayValue);
            
            MergeWithinSnapshotOperation unknownMerge = new MergeWithinSnapshotOperation(newDescriptor.UnknownIndex);
            unknownMerge.SetUndefined();
            addOperation(unknownMerge);

            HashSet<string> indexNames = new HashSet<string>();
            foreach (var array in arrays)
            {
                ArrayDescriptor descriptor = snapshot.Data.GetDescriptor(array);
                unknownMerge.Add(descriptor.UnknownIndex);

                foreach (var index in descriptor.Indexes)
                {
                    indexNames.Add(index.Key);
                }
            }

            foreach (string indexName in indexNames)
            {
                MergeWithinSnapshotOperation operation = new MergeWithinSnapshotOperation();

                if (includeUndefined)
                {
                    operation.SetUndefined();
                }

                foreach (var array in arrays)
                {
                    ArrayDescriptor descriptor = snapshot.Data.GetDescriptor(array);
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

        private void addOperation(MergeWithinSnapshotOperation operation)
        {
            operationStack.AddLast(operation);
        }

        private MergeWithinSnapshotOperation getOperation()
        {
            MergeWithinSnapshotOperation operation = operationStack.First.Value;
            operationStack.RemoveFirst();

            return operation;
        }
    }
}
