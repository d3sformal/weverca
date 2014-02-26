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
    /// <summary>
    /// Implementation of merge algorithm. This algorithm is used to merge every memory location between several snapshots
    /// from different branches of PHP program. Output data contains structure with all possible variables, arrays, objects
    /// and may and must aliases. Data for each memory location contains all possible values. When some memory location
    /// is not specified on some snapshot the nearest unknown location is used as source of data.
    /// </summary>
    class MergeWorker : IReferenceHolder, IMergeWorker
    {
        private Dictionary<MemoryIndex, MemoryAliasBuilder> memoryAliases = new Dictionary<MemoryIndex, MemoryAliasBuilder>();

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
        internal SnapshotStructure Structure { get; private set; }

        /// <summary>
        /// Gets the collection of data of merge operation.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        internal SnapshotData Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeWorker"/> class.
        /// </summary>
        /// <param name="targetSnapshot">The target snapshot.</param>
        /// <param name="sourceSnapshots">The source snapshots.</param>
        /// <param name="isCallMerge">if set to <c>true</c> [is call merge].</param>
        public MergeWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots, bool isCallMerge = false)
        {
            Data = SnapshotData.CreateEmpty(targetSnapshot);
            Structure = SnapshotStructure.CreateEmpty(targetSnapshot);
            Structure.Data = Data;

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
                IndexContainer variables = new IndexContainer(VariableIndex.CreateUnknown(x));
                Structure.Variables[x] = variables;
                collectVariables[x] = new ContainerOperations(this, variables, variables.UnknownIndex, variables.UnknownIndex);

                IndexContainer control = new IndexContainer(ControlIndex.CreateUnknown(x));
                Structure.ContolVariables[x] = control;
                collectControl[x] = new ContainerOperations(this, control, control.UnknownIndex, control.UnknownIndex);

                Structure.Temporary[x] = new IndexSet<TemporaryIndex>();
                Structure.Arrays[x] = new IndexSet<AssociativeArray>();
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
                    collectVariables[targetLevel].CollectIndexes(snapshot, Structure.Variables[targetLevel].UnknownIndex, snapshot.Structure.Variables[sourceLevel]);
                    collectControl[targetLevel].CollectIndexes(snapshot, Structure.ContolVariables[targetLevel].UnknownIndex, snapshot.Structure.ContolVariables[sourceLevel]);
                    collectTemporary(snapshot, sourceLevel, targetLevel);
                }

                mergeDeclarations(Structure.FunctionDecl, snapshot.Structure.FunctionDecl);
                mergeDeclarations(Structure.ClassDecl, snapshot.Structure.ClassDecl);

                // When is it call merge remember which arrays was forgotten in order to support arrays in returns
                if (isCallMerge)
                {
                    foreach (AssociativeArray array in snapshot.Structure.Arrays.Local)
                    {
                        Structure.AddCallArray(array, snapshot);
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
                Structure.SetAlias(alias.Key, alias.Value.Build());
            }
        }

        /// <summary>
        /// Merges single source declaration container with the target.
        /// Adds all delcarations from source container to the target.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
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
            }

            references.SetAliases(operation.TargetIndex, this, !operation.IsUndefined);

            //repares the set of values - array values are traversed
            HashSet<Value> values = getValues(operation, visitor);

            // If some index in operation can be undefined add undefined value into result
            if (operation.IsUndefined)
            {
                values.Add(targetSnapshot.UndefinedValue);
            }

            Structure.SetMemoryEntry(operation.TargetIndex, new MemoryEntry(values));
            Structure.SetObjects(operation.TargetIndex, new ObjectValueContainer(visitor.Objects));
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
            ArrayDescriptorBuilder builder = new ArrayDescriptorBuilder();
            builder.SetParentVariable(operation.TargetIndex);
            builder.SetUnknownField(operation.TargetIndex.CreateUnknownIndex());

            ContainerOperations collectVariables = new ContainerOperations(this, builder, operation.TargetIndex, builder.UnknownIndex);

            // Collecting possible indexes of merged array
            AssociativeArray targetArray = null;
            foreach (var operationData in operation.Indexes)
            {
                MemoryIndex index = operationData.Item1;
                Snapshot snapshot = operationData.Item2;

                AssociativeArray arrayValue;
                if (snapshot.Structure.TryGetArray(index, out arrayValue))
                {
                    // Target array value will be the firs one
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

            collectVariables.MergeContainers();

            Structure.SetArray(operation.TargetIndex, targetArray);
            Structure.SetDescriptor(targetArray, builder.Build());

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
            foreach (TemporaryIndex temp in snapshot.Structure.Temporary[sourceLevel])
            {
                if (!Structure.Temporary[targetLevel].Contains(temp))
                {
                    Structure.Temporary[targetLevel].Add(temp);
                }
            }
        }

        /// <summary>
        /// Creates merge operations for all temporary indexes in the target structure.
        /// </summary>
        /// <param name="index">The index.</param>
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

        /// <summary>
        /// Inserts all objects from source structure into target structure
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
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

            collectVariables.MergeContainers();
            Structure.ObjectDescriptors.Add(objectValue, builder.Build());
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

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAlias">The must alias.</param>
        /// <param name="mayAlias">The may alias.</param>
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
}
