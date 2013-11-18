using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    public class Snapshot : SnapshotBase
    {
        HashSet<MemoryIndex> memoryIndexes = new HashSet<MemoryIndex>();


        Dictionary<AssociativeArray, ArrayDescriptor> arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
        Dictionary<ObjectValue, ObjectDescriptor> objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();

        Dictionary<Value, MemoryInfo> memoryValueInfos = new Dictionary<Value, MemoryInfo>();

        Dictionary<MemoryIndex, MemoryEntry> memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>();
        Dictionary<MemoryIndex, MemoryAlias> memoryAliases = new Dictionary<MemoryIndex, MemoryAlias>();
        Dictionary<MemoryIndex, MemoryInfo> memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>();

        Dictionary<MemoryIndex, AssociativeArray> indexArrays = new Dictionary<MemoryIndex, AssociativeArray>();
        Dictionary<MemoryIndex, ObjectValueContainer> indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>();




        internal HashSet<TemporaryIndex> temporary = new HashSet<TemporaryIndex>();
        public IndexContainer Variables { get; private set; }
        public IndexContainer ContollVariables { get; private set; }

        internal IEnumerable<KeyValuePair<ObjectValue, ObjectDescriptor>> Objects { get { return objectDescriptors; } }
        internal IEnumerable<TemporaryIndex> Temporary { get { return temporary; } }


        public IEnumerable<MemoryIndex> MemoryIndexes { get { return memoryIndexes; } }


        public Snapshot()
        {
            Variables = new IndexContainer(VariableIndex.CreateUnknown());

            memoryIndexes.Add(Variables.UnknownIndex);
            memoryEntries.Add(Variables.UnknownIndex, new MemoryEntry(this.UndefinedValue));
        }

        public String DumpSnapshot()
        {
            StringBuilder builder = new StringBuilder();

            foreach (MemoryIndex index in memoryIndexes)
            {
                builder.Append(index.ToString());
                builder.Append("\n");
                builder.Append(GetMemoryEntry(index).ToString());
                builder.Append("\n\n");
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            return DumpSnapshot();
        }

        #region AbstractSnapshot Implementation



        #region Transaction

        protected override void startTransaction()
        {

        }

        protected override bool commitTransaction()
        {
            return true;
        }

        #endregion

        #region Objects

        protected override void initializeObject(ObjectValue createdObject, TypeValueBase type)
        {
            ObjectDescriptor descriptor = new ObjectDescriptor(createdObject, type, ObjectIndex.CreateUnknown(createdObject));
            objectDescriptors[createdObject] = descriptor;

            memoryIndexes.Add(descriptor.UnknownIndex);
            CreateEmptyEntry(descriptor.UnknownIndex, false);
        }

        protected override IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            throw new NotImplementedException();
        }

        protected override TypeValueBase objectType(ObjectValue objectValue)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Arrays

        protected override void initializeArray(AssociativeArray createdArray)
        {
            ArrayDescriptor descriptor = new ArrayDescriptor(createdArray);
            arrayDescriptors[createdArray] = descriptor;
        }

        protected override IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Functions and Calls

        protected override void extend(ISnapshotReadonly[] inputs)
        {
            if (inputs.Length == 1)
            {
                extendSnapshot(inputs[0]);
            }
            else if (inputs.Length > 1)
            {
                mergeSnapshots(inputs);
            }
        }

        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutputs)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Infos

        protected override void setInfo(Value value, params InfoValue[] info)
        {
            throw new NotImplementedException();
        }

        protected override void setInfo(VariableName variable, params InfoValue[] info)
        {
            throw new NotImplementedException();
        }

        protected override InfoValue[] readInfo(Value value)
        {
            throw new NotImplementedException();
        }

        protected override InfoValue[] readInfo(VariableName variable)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Globals

        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            throw new NotImplementedException();
        }

        protected override void declareGlobal(FunctionValue declaration)
        {
            throw new NotImplementedException();
        }

        protected override void declareGlobal(TypeValueBase declaration)
        {
            throw new NotImplementedException();
        }

        #endregion



        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region OBSOLETE

        //OBSOLETE
        protected override AliasValue createAlias(VariableName sourceVar)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override AliasValue createIndexAlias(AssociativeArray array, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override AliasValue createFieldAlias(ObjectValue objectValue, ContainerIndex field)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override void assign(VariableName targetVar, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override MemoryEntry readValue(VariableName sourceVar)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override bool tryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override MemoryEntry getField(ObjectValue value, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override bool tryGetField(ObjectValue objectValue, ContainerIndex field, out MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override bool tryGetIndex(AssociativeArray array, ContainerIndex index, out MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override IEnumerable<FunctionValue> resolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override IEnumerable<TypeValueBase> resolveType(QualifiedName typeName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Snapshot Entry API

        protected override ReadWriteSnapshotEntryBase getVariable(VariableIdentifier variable, bool forceGlobalContext)
        {
            return new SnapshotEntry(variable);
        }

        protected override ReadWriteSnapshotEntryBase getControlVariable(VariableName name)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase createSnapshotEntry(MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase getLocalControlVariable(VariableName name)
        {
            throw new NotImplementedException();
        }

        #endregion


        internal bool TryGetArray(MemoryIndex parentIndex, out AssociativeArray arrayValue)
        {
            return indexArrays.TryGetValue(parentIndex, out arrayValue);
        }

        internal ObjectDescriptor GetDescriptor(ObjectValue objectValue)
        {
            ObjectDescriptor descriptor;
            if (objectDescriptors.TryGetValue(objectValue, out descriptor))
            {
                return descriptor;
            }
            else
            {
                throw new Exception("Missing object descriptor");
            }
        }

        internal ArrayDescriptor GetDescriptor(AssociativeArray arrayValue)
        {
            ArrayDescriptor descriptor;
            if (arrayDescriptors.TryGetValue(arrayValue, out descriptor))
            {
                return descriptor;
            }
            else
            {
                throw new Exception("Missing array descriptor");
            }
        }

        internal MemoryIndex CreateVariable(string variableName)
        {
            MemoryIndex variableIndex = VariableIndex.Create(variableName);

            memoryIndexes.Add(variableIndex);
            Variables.Indexes.Add(variableName, variableIndex);

            CopyMemory(Variables.UnknownIndex, variableIndex, false);

            return variableIndex;
        }

        internal ObjectValue CreateObject(MemoryIndex parentIndex, bool isMust)
        {
            ObjectValue value = this.CreateObject(null);

            if (isMust)
            {
                DestroyMemory(parentIndex);
                memoryEntries[parentIndex] = new MemoryEntry(value);
            }
            else
            {
                MemoryEntry oldEntry;

                List<Value> values;
                if (memoryEntries.TryGetValue(parentIndex, out oldEntry))
                {
                    values = new List<Value>(oldEntry.PossibleValues);
                }
                else
                {
                    values = new List<Value>();
                }

                values.Add(value);
                memoryEntries[parentIndex] = new MemoryEntry(values);
            }

            ObjectValueContainerBuilder objectValues = GetObjects(parentIndex).Builder();
            objectValues.Add(value);

            indexObjects[parentIndex] = objectValues.Build();

            return value;
        }


        internal MemoryIndex CreateField(string fieldName, ObjectValue objectValue, bool isMust, bool copyFromUnknown)
        {
            return CreateField(fieldName, GetDescriptor(objectValue), isMust, copyFromUnknown);
        }

        internal MemoryIndex CreateField(string fieldName, ObjectDescriptor descriptor, bool isMust, bool copyFromUnknown)
        {
            if (descriptor.Indexes.ContainsKey(fieldName))
            {
                throw new Exception("Field " + fieldName + " is already defined");
            }

            MemoryIndex fieldIndex = ObjectIndex.Create(descriptor.ObjectValue, fieldName);

            memoryIndexes.Add(fieldIndex);

            descriptor = descriptor.Builder()
                .add(fieldName, fieldIndex)
                .Build();
            objectDescriptors[descriptor.ObjectValue] = descriptor;

            if (copyFromUnknown)
            {
                CopyMemory(descriptor.UnknownIndex, fieldIndex, isMust);
            }
            else
            {
                CreateEmptyEntry(fieldIndex, isMust);
            }

            return fieldIndex;
        }

        internal AssociativeArray CreateArray(MemoryIndex parentIndex)
        {
            if (indexArrays.ContainsKey(parentIndex))
            {
                throw new Exception("Variable " + parentIndex + " already has associated arraz value.");
            }

            AssociativeArray value = this.CreateArray();
            ArrayDescriptor descriptor = GetDescriptor(value)
                .Builder()
                .SetParentVariable(parentIndex)
                .SetUnknownField(parentIndex.CreateUnknownIndex())
                .Build();

            memoryIndexes.Add(descriptor.UnknownIndex);
            CreateEmptyEntry(descriptor.UnknownIndex, false);

            arrayDescriptors[value] = descriptor;
            indexArrays[parentIndex] = value;

            return value;
        }

        internal AssociativeArray CreateArray(MemoryIndex parentIndex, bool isMust)
        {
            AssociativeArray value = CreateArray(parentIndex);

            if (isMust)
            {
                //TODO - nahlasit warning pri neprazdnem poli, i v MAY
                /* $x = 1;
                 * $x[1] = 2;
                 */
                DestroyMemory(parentIndex);
                memoryEntries[parentIndex] = new MemoryEntry(value);
            }
            else
            {
                List<Value> values;
                MemoryEntry oldEntry;
                if (memoryEntries.TryGetValue(parentIndex, out oldEntry))
                {
                    values = new List<Value>(oldEntry.PossibleValues);
                }
                else
                {
                    values = new List<Value>();
                }

                values.Add(value);
                memoryEntries[parentIndex] = new MemoryEntry(values);
            }

            return value;
        }

        internal MemoryIndex CreateIndex(string indexName, AssociativeArray arrayValue, bool isMust, bool copyFromUnknown)
        {
            return CreateIndex(indexName, GetDescriptor(arrayValue), isMust, copyFromUnknown);
        }

        internal MemoryIndex CreateIndex(string indexName, ArrayDescriptor descriptor, bool isMust, bool copyFromUnknown)
        {
            if (descriptor.Indexes.ContainsKey(indexName))
            {
                throw new Exception("Index " + indexName + " is already defined");
            }

            MemoryIndex indexIndex = descriptor.ParentVariable.CreateIndex(indexName);

            memoryIndexes.Add(indexIndex);

            descriptor = descriptor.Builder()
                .add(indexName, indexIndex)
                .Build();
            arrayDescriptors[descriptor.ArrayValue] = descriptor;

            if (copyFromUnknown)
            {
                CopyMemory(descriptor.UnknownIndex, indexIndex, false);
            }

            return indexIndex;
        }

        internal MemoryEntry GetMemoryEntry(MemoryIndex index)
        {
            MemoryEntry memoryEntry;
            if (memoryEntries.TryGetValue(index, out memoryEntry))
            {
                return memoryEntry;
            }
            else
            {
                throw new Exception("Missing memory entry for " + index);
            }
        }

        private void CreateEmptyEntry(MemoryIndex index, bool isMust)
        {
            Debug.Assert(!memoryEntries.ContainsKey(index), "Index " + index + " already has its memory entry");

            MemoryEntry memoryEntry;
            if (isMust)
            {
                memoryEntry = new MemoryEntry();
            }
            else
            {
                memoryEntry = new MemoryEntry(UndefinedValue);
            }

            memoryEntries[index] = memoryEntry;
        }

        private void CopyMemory(MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust)
        {
            CopyWithinSnapshotWorker worker = new CopyWithinSnapshotWorker(this, isMust);
            worker.Copy(sourceIndex, targetIndex);
        }

        public void DestroyMemory(MemoryIndex parentIndex)
        {
            DestroyMemoryVisitor visitor = new DestroyMemoryVisitor(this, parentIndex);

            if (memoryEntries.ContainsKey(parentIndex))
            {
                visitor.VisitMemoryEntry(GetMemoryEntry(parentIndex));
            }

            memoryEntries[parentIndex] = new MemoryEntry(this.UndefinedValue);
        }


        internal void SetMemoryEntry(MemoryIndex targetIndex, MemoryEntry memoryEntry)
        {
            memoryEntries[targetIndex] = memoryEntry;
        }

        /*internal void SameObjectReference(ObjectValue sourceObject, ObjectValue targetObject, bool isMust)
        {
            ObjectDescriptorBuilder sourceDescriptor = GetDescriptor(sourceObject).Builder();
            ObjectDescriptorBuilder targetDescriptor = GetDescriptor(targetObject).Builder();

            targetDescriptor.Types.AddRange(sourceDescriptor.Types);

            foreach (MemoryIndex index in sourceDescriptor.MustReferences)
            {
                ObjectValue value = GetObject(index);
                ObjectDescriptorBuilder descriptor = GetDescriptor(value).Builder();

                if (isMust)
                {
                    targetDescriptor.addMustReference(index);
                    descriptor.addMustReference(targetDescriptor.ParentVariable);
                }
                else
                {
                    targetDescriptor.addMayReference(index);
                    descriptor.addMayReference(targetDescriptor.ParentVariable);
                }

                objectDescriptors[value] = descriptor.Build();
            }

            foreach (MemoryIndex index in sourceDescriptor.MayReferences)
            {
                ObjectValue value = GetObject(index);
                ObjectDescriptorBuilder descriptor = GetDescriptor(value).Builder();

                targetDescriptor.addMayReference(index);
                descriptor.addMayReference(targetDescriptor.ParentVariable);

                objectDescriptors[value] = descriptor.Build();
            }

            if (isMust)
            {
                targetDescriptor.addMustReference(sourceDescriptor.ParentVariable);
                sourceDescriptor.addMustReference(targetDescriptor.ParentVariable);
            }
            else
            {
                targetDescriptor.addMayReference(sourceDescriptor.ParentVariable);
                sourceDescriptor.addMayReference(targetDescriptor.ParentVariable);
            }

            objectDescriptors[sourceObject] = sourceDescriptor.Build();
            objectDescriptors[targetObject] = targetDescriptor.Build();
        }*/

        internal void CopyAliases(MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust)
        {
            if (memoryAliases.ContainsKey(sourceIndex))
            {
                throw new NotImplementedException();
            }
        }

        internal void CopyInfos(MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust)
        {
            if (memoryInfos.ContainsKey(sourceIndex))
            {
                throw new NotImplementedException();
            }
        }

        internal void SetDescriptor(AssociativeArray arrayValue, ArrayDescriptorBuilder newDescriptor)
        {
            arrayDescriptors[arrayValue] = newDescriptor.Build();
        }

        internal void SetDescriptor(ObjectValue objectValue, ObjectDescriptorBuilder newDescriptor)
        {
            objectDescriptors[objectValue] = newDescriptor.Build();
        }

        public ObjectValueContainer GetObjects(MemoryIndex parentIndex)
        {
            ObjectValueContainer objectValues;
            if (indexObjects.TryGetValue(parentIndex, out objectValues))
            {
                return objectValues;
            }
            else
            {
                return new ObjectValueContainer();
            }
        }

        internal bool HasObjects(MemoryIndex parentIndex)
        {
            return indexObjects.ContainsKey(parentIndex);
        }

        internal void SetObjects(MemoryIndex index, ObjectValueContainerBuilder objectsValues)
        {
            indexObjects[index] = objectsValues.Build();
        }

        internal void ClearForArray(MemoryIndex index)
        {
            DestroyObjectsVisitor visitor = new DestroyObjectsVisitor(this, index);
            visitor.VisitMemoryEntry(GetMemoryEntry(index));

            AssociativeArray array;
            MemoryEntry entry;
            if (TryGetArray(index, out array))
            {
                entry = new MemoryEntry(array);
            }
            else
            {
                entry = new MemoryEntry();
            }

            SetMemoryEntry(index, entry);
        }

        internal void ClearForObjects(MemoryIndex index)
        {
            DestroyArrayVisitor visitor = new DestroyArrayVisitor(this, index);
            visitor.VisitMemoryEntry(GetMemoryEntry(index));

            ObjectValueContainer objects = GetObjects(index);
            MemoryEntry entry = new MemoryEntry(objects);

            SetMemoryEntry(index, entry);
        }

        internal bool IsUndefined(MemoryIndex index)
        {
            MemoryEntry entry = GetMemoryEntry(index);
            return entry.PossibleValues.Contains(this.UndefinedValue);
        }

        internal void DestroyObject(MemoryIndex parentIndex, ObjectValue value)
        {
            ObjectValueContainerBuilder objects = GetObjects(parentIndex).Builder();
            objects.Remove(value);
            SetObjects(parentIndex, objects);
        }

        internal void DestroyArray(MemoryIndex parentIndex)
        {
            AssociativeArray arrayValue;
            if (!indexArrays.TryGetValue(parentIndex, out arrayValue))
            {
                return;
            }

            ArrayDescriptor descriptor = GetDescriptor(arrayValue);
            foreach (var index in descriptor.Indexes)
            {
                ReleaseMemory(index.Value);
            }

            ReleaseMemory(descriptor.UnknownIndex);

            arrayDescriptors.Remove(arrayValue);
            indexArrays.Remove(parentIndex);
        }

        internal void MakeMustReferenceObject(ObjectValue objectValue, MemoryIndex targetIndex)
        {
            ObjectValueContainerBuilder objects = GetObjects(targetIndex).Builder();
            objects.Add(objectValue);
            SetObjects(targetIndex, objects);
        }

        internal void MakeMayReferenceObject(HashSet<ObjectValue> objects, MemoryIndex targetIndex)
        {
            ObjectValueContainerBuilder objectsContainer = GetObjects(targetIndex).Builder();

            foreach (ObjectValue objectValue in objects)
            {
                objectsContainer.Add(objectValue);
            }

            SetObjects(targetIndex, objectsContainer);
        }

        internal TemporaryIndex CreateTemporary()
        {
            TemporaryIndex tmp = new TemporaryIndex();
            memoryIndexes.Add(tmp);
            temporary.Add(tmp);

            memoryEntries[tmp] = new MemoryEntry(this.UndefinedValue);

            return tmp;
        }

        internal void ReleaseTemporary(TemporaryIndex temporaryIndex)
        {
            ReleaseMemory(temporaryIndex);
            temporary.Remove(temporaryIndex);
        }

        internal void ReleaseMemory(MemoryIndex index)
        {
            DestroyMemory(index);

            memoryIndexes.Remove(index);
            memoryEntries.Remove(index);

            // TODO - remove temporary from alias structure
            //memoryAliases.Remove(temporaryIndex);
            //memoryInfos.Remove(temporaryIndex);
        }








        private void mergeSnapshots(ISnapshotReadonly[] inputs)
        {
            List<Snapshot> snapshots = new List<Snapshot>(inputs.Length);
            foreach (ISnapshotReadonly input in inputs)
            {
                snapshots.Add(SnapshotEntry.ToSnapshot(input));
            }

            MergeWorker worker = new MergeWorker(this, snapshots);
            worker.Merge();

            memoryIndexes = new HashSet<MemoryIndex>(worker.memoryIndexes);
            arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(worker.arrayDescriptors);
            objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(worker.objectDescriptors);

            memoryValueInfos = new Dictionary<Value, MemoryInfo>(worker.memoryValueInfos);

            memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>(worker.memoryEntries);
            memoryAliases = new Dictionary<MemoryIndex, MemoryAlias>(worker.memoryAliases);
            memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>(worker.memoryInfos);

            indexArrays = new Dictionary<MemoryIndex, AssociativeArray>(worker.indexArrays);
            indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>(worker.indexObjects);

            temporary = new HashSet<TemporaryIndex>(worker.temporary);

            Variables = new IndexContainer(worker.Variables);
        }

        private void extendSnapshot(ISnapshotReadonly input)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(input);

            memoryIndexes = new HashSet<MemoryIndex>(snapshot.memoryIndexes);
            arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(snapshot.arrayDescriptors);
            objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(snapshot.objectDescriptors);

            memoryValueInfos = new Dictionary<Value, MemoryInfo>(snapshot.memoryValueInfos);

            memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>(snapshot.memoryEntries);
            memoryAliases = new Dictionary<MemoryIndex, MemoryAlias>(snapshot.memoryAliases);
            memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>(snapshot.memoryInfos);

            indexArrays = new Dictionary<MemoryIndex, AssociativeArray>(snapshot.indexArrays);
            indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>(snapshot.indexObjects);

            temporary = new HashSet<TemporaryIndex>(snapshot.temporary);

            Variables = new IndexContainer(snapshot.Variables);
            ContollVariables = new IndexContainer(snapshot.ContollVariables);
        }


        internal bool Exists(MemoryIndex memoryIndex)
        {
            return memoryIndexes.Contains(memoryIndex);
        }


        internal bool TryGetDescriptor(ObjectValue objectValue, out ObjectDescriptor descriptor)
        {
            return objectDescriptors.TryGetValue(objectValue, out descriptor);
        }

        internal bool HasMustReference(MemoryIndex parentIndex)
        {
            MemoryEntry entry = GetMemoryEntry(parentIndex);

            if (entry.Count == 1)
            {
                ObjectValueContainer objects = GetObjects(parentIndex);
                if (objects.Count == 1)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool ContainsOnlyReferences(MemoryIndex parentIndex)
        {
            MemoryEntry entry = GetMemoryEntry(parentIndex);
            ObjectValueContainer objects = GetObjects(parentIndex);

            return entry.Count == objects.Count;
        }

        internal void MustSetAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            DestroyAliases(index);

            MemoryAliasBuilder builder = new MemoryAliasBuilder();
            builder.AddMustAlias(mustAliases);
            builder.AddMayAlias(mayAliases);

            memoryAliases.Add(index, builder.Build());
        }

        internal void MaySetAliases(MemoryIndex index, HashSet<MemoryIndex> mayAliases)
        {
            MemoryAliasBuilder builder;
            MemoryAlias oldAlias;
            if (TryGetAliases(index, out oldAlias))
            {
                builder = oldAlias.Builder();
                convertAliasesToMay(index, builder);
            }
            else
            {
                builder = new MemoryAliasBuilder();
            }

            builder.AddMayAlias(mayAliases);
            memoryAliases.Add(index, builder.Build());
        }

        internal void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            MemoryAliasBuilder alias;
            MemoryAlias oldAlias;
            if (TryGetAliases(index, out oldAlias))
            {
                alias = oldAlias.Builder();
            }
            else
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

            memoryAliases.Add(index, alias.Build());
        }

        internal void DestroyAliases(MemoryIndex index)
        {
            MemoryAlias aliases;
            if (!TryGetAliases(index, out aliases))
            {
                return;
            }

            foreach (MemoryIndex mustIndex in aliases.MustAliasses)
            {
                MemoryAlias alias = getAliases(mustIndex);
                if (alias.MustAliasses.Count == 1 && alias.MayAliasses.Count == 0)
                {
                    memoryAliases.Remove(mustIndex);
                }
                else
                {
                    MemoryAliasBuilder builder = getAliases(mustIndex).Builder();
                    builder.RemoveMustAlias(index);
                    memoryAliases[mustIndex] = builder.Build();
                }
            }

            foreach (MemoryIndex mayIndex in aliases.MayAliasses)
            {
                MemoryAlias alias = getAliases(mayIndex);
                if (alias.MustAliasses.Count == 0 && alias.MayAliasses.Count == 1)
                {
                    memoryAliases.Remove(mayIndex);
                }
                else
                {
                    MemoryAliasBuilder builder = getAliases(mayIndex).Builder();
                    builder.RemoveMayAlias(index);
                    memoryAliases[mayIndex] = builder.Build();
                }
            }
        }

        private void convertAliasesToMay(MemoryIndex index, MemoryAliasBuilder builder)
        {
            foreach (MemoryIndex mustIndex in builder.MustAliasses)
            {
                MemoryAlias alias = getAliases(mustIndex);

                MemoryAliasBuilder mustBuilder = getAliases(mustIndex).Builder();
                builder.RemoveMustAlias(index);
                builder.AddMayAlias(index);
                memoryAliases[mustIndex] = builder.Build();
            }

            builder.AddMayAlias(builder.MustAliasses);
            builder.MustAliasses.Clear();
        }

        private MemoryAlias getAliases(MemoryIndex index)
        {
            MemoryAlias aliases;
            if (memoryAliases.TryGetValue(index, out aliases))
            {
                return aliases;
            }
            else 
            {
                throw new Exception("Missing alias value for " + index);
            }
        }

        internal bool TryGetAliases(MemoryIndex index, out MemoryAlias aliases)
        {
            return memoryAliases.TryGetValue(index, out aliases);
        }

        protected override bool widenAndCommitTransaction()
        {
            throw new NotImplementedException();
        }
    }
}
