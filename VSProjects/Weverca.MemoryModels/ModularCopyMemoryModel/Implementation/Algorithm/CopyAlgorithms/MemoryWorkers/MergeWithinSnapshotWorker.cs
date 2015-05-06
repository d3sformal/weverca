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
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers
{
    class MergeWithinSnapshotWorker
    {
        private Snapshot targetSnapshot;

        private LinkedList<MergeOperation> operationStack = new LinkedList<MergeOperation>();

        public MergeWithinSnapshotWorker(Snapshot snapshot)
        {
            this.targetSnapshot = snapshot;
        }

        int arrayCount = 0;

        public void MergeMemoryEntry(MemoryIndex targetIndex, MemoryEntry entry)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(entry);
            arrayCount = visitor.Arrays.Count;

            HashSet<Value> values = getValues(targetIndex, visitor, false);

            processMerge();

            targetSnapshot.CurrentData.Writeable.SetMemoryEntry(targetIndex, targetSnapshot.CreateMemoryEntry(values));
        }

        public void MergeIndexes(MemoryIndex targetIndex, MemoryIndex sourceIndex)
        {
            if (!sourceIndex.IsPrefixOf(targetIndex) && !targetIndex.IsPrefixOf(sourceIndex))
            {
                MergeOperation operation = new MergeOperation(targetIndex);
                operation.Add(targetIndex, targetSnapshot);
                operation.Add(sourceIndex, targetSnapshot);
                operation.IsRoot = true;
                addOperation(operation);

                arrayCount = 2;

                processMerge();
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

            foreach (var item in operation.Indexes)
            {
                Snapshot snapshot = item.Item2;
                MemoryIndex index = item.Item1;
                MemoryEntry entry = snapshot.CurrentData.Readonly.GetMemoryEntry(index);
                visitor.VisitMemoryEntry(entry);

                IMemoryAlias aliases;
                if (snapshot.Structure.Readonly.TryGetAliases(index, out aliases))
                {
                    references.CollectMust(aliases.MustAliases, snapshot.CallLevel);
                    references.CollectMay(aliases.MayAliases, snapshot.CallLevel);
                }
                else
                {
                    references.InvalidateMust();
                }
            }

            if (references.HasAliases && !operation.IsRoot)
            {
                if (!operation.IsUndefined && operation.Indexes.Count == 1 && references.HasMustAliases)
                {
                    if (targetSnapshot == operation.Indexes.First().Item2)
                    {
                        references.AddMustAlias(operation.Indexes.First().Item1);
                    }
                }
                else
                {
                    HashSet<MemoryIndex> referenceIndexes = new HashSet<MemoryIndex>();
                    foreach (var item in operation.Indexes)
                    {
                        MemoryIndex index = item.Item1;
                        Snapshot snapshot = item.Item2;

                        if (index != operation.TargetIndex && targetSnapshot == snapshot)
                        {
                            referenceIndexes.Add(index);
                        }
                    }

                    references.CollectMay(referenceIndexes, targetSnapshot.CallLevel);
                }
            }

            references.SetAliases(operation.TargetIndex, targetSnapshot, !operation.IsUndefined);

            HashSet<Value> values = getValues(operation.TargetIndex, visitor, operation.IsUndefined);

            if (operation.IsUndefined)
            {
                values.Add(targetSnapshot.UndefinedValue);
            }

            targetSnapshot.CurrentData.Writeable.SetMemoryEntry(operation.TargetIndex, targetSnapshot.CreateMemoryEntry(values));
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
                    targetSnapshot.MakeMustReferenceObject(objectValue, targetIndex);
                }
                else
                {
                    targetSnapshot.MakeMayReferenceObject(objects, targetIndex);
                }

            }
            else if (objects.Count > 1)
            {
                targetSnapshot.MakeMayReferenceObject(objects, targetIndex);

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
            if (!targetSnapshot.Structure.Readonly.TryGetArray(targetIndex, out arrayValue))
            {
                arrayValue = targetSnapshot.CreateArray(targetIndex);
            }
            IArrayDescriptor newDescriptor = targetSnapshot.Structure.Readonly.GetDescriptor(arrayValue);

            MergeOperation unknownMerge = new MergeOperation(newDescriptor.UnknownIndex);
            unknownMerge.SetUndefined();
            addOperation(unknownMerge);

            HashSet<string> indexNames = new HashSet<string>();
            foreach (AssociativeArray array in arrays)
            {
                foreach (Tuple<Snapshot, IArrayDescriptor> item in getIArrayDescriptor(array))
                {
                    Snapshot snapshot = item.Item1;
                    IArrayDescriptor descriptor = item.Item2;

                    unknownMerge.Add(descriptor.UnknownIndex, snapshot);

                    foreach (var index in descriptor.Indexes)
                    {
                        indexNames.Add(index.Key);
                    }
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
                    foreach (Tuple<Snapshot, IArrayDescriptor> item in getIArrayDescriptor(array))
                    {
                        Snapshot snapshot = item.Item1;
                        IArrayDescriptor descriptor = item.Item2;

                        MemoryIndex sourceIndex;
                        if (descriptor.TryGetIndex(indexName, out sourceIndex))
                        {
                            operation.Add(sourceIndex, snapshot);
                        }
                        else
                        {
                            operation.Add(descriptor.UnknownIndex, snapshot);
                        }
                    }
                }

                MemoryIndex arrayIndex;
                if (!newDescriptor.TryGetIndex(indexName, out arrayIndex))
                {
                    arrayIndex = targetSnapshot.CreateIndex(indexName, arrayValue, true, false);
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

        private IEnumerable<Tuple<Snapshot, IArrayDescriptor>> getIArrayDescriptor(AssociativeArray array)
        {
            List<Tuple<Snapshot, IArrayDescriptor>> results = new List<Tuple<Snapshot, IArrayDescriptor>>();

            IArrayDescriptor descriptor;
            IEnumerable<Snapshot> snapshots;
            if (targetSnapshot.Structure.Readonly.TryGetDescriptor(array, out descriptor))
            {
                results.Add(new Tuple<Snapshot, IArrayDescriptor>(targetSnapshot, descriptor));
            }
            else if (targetSnapshot.Structure.Readonly.TryGetCallArraySnapshot(array, out snapshots))
            {
                foreach (Snapshot snapshot in snapshots)
                {
                    IArrayDescriptor snapDescriptor = snapshot.Structure.Readonly.GetDescriptor(array);
                    results.Add(new Tuple<Snapshot, IArrayDescriptor>(snapshot, snapDescriptor));
                }


            }
            else
            {
                throw new Exception("Missing array descriptor");
            }

            return results;
        }
    }
}