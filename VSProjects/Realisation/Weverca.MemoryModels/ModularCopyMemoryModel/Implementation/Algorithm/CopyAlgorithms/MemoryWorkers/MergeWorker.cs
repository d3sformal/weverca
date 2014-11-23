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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers
{
    /// <summary>
    /// Implementation of merge algorithm. This algorithm is used to merge every memory location between several snapshots
    /// from different branches of PHP program. Output data contains structure with all possible variables, arrays, objects
    /// and may and must aliases. Data for each memory location contains all possible values. When some memory location
    /// is not specified on some snapshot the nearest unknown location is used as source of data.
    /// </summary>
    class MergeWorker : IReferenceHolder, IMergeWorker
    {
        private Dictionary<MemoryIndex, IMemoryAliasBuilder> memoryAliases = new Dictionary<MemoryIndex, IMemoryAliasBuilder>();

        private Snapshot targetSnapshot;

        private List<Snapshot> sourceSnapshots;

        private HashSet<ObjectValue> objects = new HashSet<ObjectValue>();

        private LinkedList<MergeOperation> operationStack = new LinkedList<MergeOperation>();

        private bool isCallMerge;

        /// <summary>
        /// Gets the result structure of merge operation.
        /// </summary>
        /// <value>
        /// The structure.
        /// </value>
        internal ISnapshotStructureProxy Structure { get; private set; }

        /// <summary>
        /// Gets the collection of data of merge operation.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        internal ISnapshotDataProxy Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeWorker"/> class.
        /// </summary>
        /// <param name="targetSnapshot">The target snapshot.</param>
        /// <param name="sourceSnapshots">The source snapshots.</param>
        /// <param name="isCallMerge">if set to <c>true</c> [is call merge].</param>
        public MergeWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots, bool isCallMerge = false)
        {
            Data = Snapshot.SnapshotDataFactory.CreateEmptyInstance(targetSnapshot);
            Structure = Snapshot.SnapshotStructureFactory.CreateEmptyInstance(targetSnapshot);

            this.targetSnapshot = targetSnapshot;
            this.sourceSnapshots = sourceSnapshots;
            this.isCallMerge = isCallMerge;
        }

        /// <summary>
        /// Main method of merge algorithm.
        /// 
        /// in first phase prepares new empty structure and data collections. Then collects all root memory locations
        /// and prepares their operations. As the final step process all merge operations which traverses the memory tree
        /// and creates new memory locations in target structure with the data from all source indexes.
        /// </summary>
        internal void Merge()
        {
            ContainerOperations[] collectVariables = new ContainerOperations[targetSnapshot.CallLevel + 1];
            ContainerOperations[] collectControl = new ContainerOperations[targetSnapshot.CallLevel + 1];
            MergeOperation returnOperation = new MergeOperation();

            // Prepares empty structure for target snapshot
            for (int x = 0; x <= targetSnapshot.CallLevel; x++)
            {
                Structure.Writeable.AddLocalLevel();

                IWriteableIndexContainer variables = Structure.Writeable.GetWriteableStackContext(x).WriteableVariables;
                collectVariables[x] = new ContainerOperations(this, variables, variables.UnknownIndex, variables.UnknownIndex);

                IWriteableIndexContainer control = Structure.Writeable.GetWriteableStackContext(x).WriteableControllVariables;
                collectControl[x] = new ContainerOperations(this, control, control.UnknownIndex, control.UnknownIndex);
            }

            // Collects all objects and root locations from the source objects
            foreach (Snapshot snapshot in sourceSnapshots)
            {
                collectObjects(snapshot);

                for (int sourceLevel = 0, targetLevel = 0; targetLevel <= targetSnapshot.CallLevel; sourceLevel++, targetLevel++)
                {
                    // Local levels of snaphot has to be merged together no matter to call level of each snapshot.
                    if (sourceLevel == snapshot.CallLevel && snapshot.CallLevel != targetSnapshot.CallLevel)
                    {
                        if (isCallMerge)
                        {
                            // When this is the call merge the local level is forgotten
                            break;
                        }
                        else
                        {
                            targetLevel = targetSnapshot.CallLevel;
                        }
                    }

                    // Gets all root locations
                    IWriteableIndexContainer targetVariables = Structure.Writeable.GetWriteableStackContext(targetLevel).WriteableVariables;
                    IReadonlyIndexContainer sourceVariables = snapshot.Structure.Readonly.GetReadonlyStackContext(sourceLevel).ReadonlyVariables;
                    collectVariables[targetLevel].CollectIndexes(snapshot, targetVariables.UnknownIndex, sourceVariables);

                    IWriteableIndexContainer targetControlls = Structure.Writeable.GetWriteableStackContext(targetLevel).WriteableControllVariables;
                    IReadonlyIndexContainer sourceControlls = snapshot.Structure.Readonly.GetReadonlyStackContext(sourceLevel).ReadonlyControllVariables;
                    collectControl[targetLevel].CollectIndexes(snapshot, targetControlls.UnknownIndex, sourceControlls);
                    collectTemporary(snapshot, sourceLevel, targetLevel);
                }

                foreach (var name in snapshot.Structure.Readonly.GetFunctions())
                {
                    foreach (var decl in snapshot.Structure.Readonly.GetFunction(name))
                    {
                        Structure.Writeable.AddFunctiondeclaration(name, decl);
                    }
                }

                foreach (var name in snapshot.Structure.Readonly.GetClasses())
                {
                    foreach (var decl in snapshot.Structure.Readonly.GetClass(name))
                    {
                        Structure.Writeable.AddClassDeclaration(name, decl);
                    }
                }

                // When is it call merge remember which arrays was forgotten in order to support arrays in returns
                if (isCallMerge)
                {
                    foreach (AssociativeArray array in snapshot.Structure.Readonly.ReadonlyLocalContext.ReadonlyArrays)
                    {
                        Structure.Writeable.AddCallArray(array, snapshot);
                    }
                }
            }

            mergeObjects();

            // Prepares operations for all root locations
            for (int x = 0; x <= targetSnapshot.CallLevel; x++)
            {
                collectVariables[x].MergeContainers();
                collectControl[x].MergeContainers();
                mergeTemporary(x);
            }

            processMerge();

            // Build aliases
            foreach (var alias in memoryAliases)
            {
                Structure.Writeable.SetAlias(alias.Key, alias.Value.Build());
            }
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
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            ReferenceCollector references = new ReferenceCollector();

            // Collect all data from source indexes
            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                MemoryEntry entry = snapshot.CurrentData.Readonly.GetMemoryEntry(index);
                visitor.VisitMemoryEntry(entry);

                IMemoryAlias aliases;
                if (snapshot.Structure.Readonly.TryGetAliases(index, out aliases))
                {
                    references.CollectMust(aliases.MustAliases, targetSnapshot.CallLevel);
                    references.CollectMay(aliases.MayAliases, targetSnapshot.CallLevel);
                }
                else
                {
                    references.InvalidateMust();
                }
            }

            references.SetAliases(operation.TargetIndex, this, !operation.IsUndefined);

            //repares the set of values - array values are traversed
            HashSet<Value> values = getValues(operation, visitor);

            // If some index in operation can be undefined add undefined value into result
            if (operation.IsUndefined)
            {
                values.Add(targetSnapshot.UndefinedValue);
            }

            Data.Writeable.SetMemoryEntry(operation.TargetIndex, targetSnapshot.CreateMemoryEntry(values));
            Structure.Writeable.SetObjects(operation.TargetIndex, Structure.CreateObjectValueContainer(visitor.Objects));
        }

        /// <summary>
        /// Gets all values which can be in target memory entry. If sources contains some arrays merge this arrays together
        /// and traverse their indexes.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="visitor">The visitor.</param>
        /// <returns>Values for the target memory entry.</returns>
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

        /// <summary>
        /// Prepares operation for every descendant index and merge the array into one which will be
        /// stored in the target memory entry.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns>Array where the input arrays is merged into.</returns>
        private Value mergeArrays(MergeOperation operation)
        {
            IArrayDescriptorBuilder builder = Structure.CreateArrayDescriptor(null, operation.TargetIndex).Builder();
            builder.SetUnknownIndex(operation.TargetIndex.CreateUnknownIndex());

            ContainerOperations collectVariables = new ContainerOperations(this, builder, operation.TargetIndex, builder.UnknownIndex);

            // Collecting possible indexes of merged array
            AssociativeArray targetArray = null;
            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                AssociativeArray arrayValue;
                if (snapshot.Structure.Readonly.TryGetArray(index, out arrayValue))
                {
                    // Target array value will be the firs one
                    if (targetArray == null)
                    {
                        if (index.Equals(operation.TargetIndex))
                        {
                            targetArray = arrayValue;
                        }
                    }

                    IArrayDescriptor descriptor = snapshot.Structure.Readonly.GetDescriptor(arrayValue);
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

            collectVariables.MergeContainers();

            Structure.Writeable.SetArray(operation.TargetIndex, targetArray);
            Structure.Writeable.SetDescriptor(targetArray, builder.Build());

            return targetArray;
        }

        #region Temporary

        /// <summary>
        /// Collects the temporary variables into target structure.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="sourceLevel">The source level.</param>
        /// <param name="targetLevel">The target level.</param>
        private void collectTemporary(Snapshot snapshot, int sourceLevel, int targetLevel)
        {
            IWriteableSet<MemoryIndex> temporary = Structure.Writeable.GetWriteableStackContext(targetLevel).WriteableTemporaryVariables;
            foreach (TemporaryIndex temp in snapshot.Structure.Readonly.GetReadonlyStackContext(sourceLevel).ReadonlyTemporaryVariables)
            {
                if (!temporary.Contains(temp))
                {
                    temporary.Add(temp);
                }
            }
        }

        /// <summary>
        /// Creates merge operations for all temporary indexes in the target structure.
        /// </summary>
        /// <param name="index">The index.</param>
        private void mergeTemporary(int index)
        {
            IWriteableSet<MemoryIndex> temporary = Structure.Writeable.GetWriteableStackContext(index).WriteableTemporaryVariables;
            foreach (var temp in temporary)
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

        /// <summary>
        /// Inserts all objects from source structure into target structure
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
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
            foreach (ObjectValue objectValue in objects)
            {
                mergeObject(objectValue);
            }
        }

        /// <summary>
        /// Creates the merge operation for all fields of specified object.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        private void mergeObject(ObjectValue objectValue)
        {
            IObjectDescriptorBuilder builder =
                Structure.CreateObjectDescriptor(objectValue, null, ObjectIndex.CreateUnknown(objectValue))
                .Builder();

            ContainerOperations collectVariables = new ContainerOperations(this, builder, builder.UnknownIndex, builder.UnknownIndex);

            foreach (Snapshot snapshot in sourceSnapshots)
            {
                IObjectDescriptor descriptor;
                if (snapshot.Structure.Readonly.TryGetDescriptor(objectValue, out descriptor))
                {
                    collectVariables.CollectIndexes(snapshot, builder.UnknownIndex, descriptor);
                    builder.SetType(descriptor.Type);
                }
                else
                {
                    collectVariables.SetUndefined();
                }
            }

            collectVariables.MergeContainers();
            Structure.Writeable.SetDescriptor(objectValue, builder.Build());
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

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAliases">The must aliases.</param>
        /// <param name="mayAliases">The may aliases.</param>
        public void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            IMemoryAliasBuilder alias;
            if (!memoryAliases.TryGetValue(index, out alias))
            {
                alias = Structure.CreateMemoryAlias(index).Builder();
            }

            if (mustAliases != null)
            {
                alias.MustAliases.AddAll(mustAliases);
            }
            if (mayAliases != null)
            {
                alias.MayAliases.AddAll(mayAliases);
            }

            foreach (MemoryIndex mustIndex in alias.MustAliases)
            {
                if (alias.MayAliases.Contains(mustIndex))
                {
                    alias.MayAliases.Remove(mustIndex);
                }
            }

            memoryAliases[index] = alias;
        }

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAlias">The must alias.</param>
        /// <param name="mayAlias">The may alias.</param>
        public void AddAlias(MemoryIndex index, MemoryIndex mustAlias, MemoryIndex mayAlias)
        {
            IMemoryAliasBuilder alias;
            if (!memoryAliases.TryGetValue(index, out alias))
            {
                alias = Structure.CreateMemoryAlias(index).Builder();
            }

            if (mustAlias != null)
            {
                alias.MustAliases.Add(mustAlias);

                if (alias.MayAliases.Contains(mustAlias))
                {
                    alias.MayAliases.Remove(mustAlias);
                }
            }

            if (mayAlias != null && !alias.MustAliases.Contains(mayAlias))
            {
                alias.MayAliases.Add(mayAlias);
            }

            memoryAliases[index] = alias;
        }
    }
}