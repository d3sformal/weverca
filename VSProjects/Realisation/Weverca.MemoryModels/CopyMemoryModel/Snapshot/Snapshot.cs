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
    public class Snapshot : SnapshotBase, IReferenceHolder
    {
        public static readonly int GLOBAL_CALL_LEVEL = 0;

        HashSet<MemoryIndex> memoryIndexes = new HashSet<MemoryIndex>();


        Dictionary<AssociativeArray, ArrayDescriptor> arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
        Dictionary<ObjectValue, ObjectDescriptor> objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();

        Dictionary<Value, MemoryInfo> memoryValueInfos = new Dictionary<Value, MemoryInfo>();

        Dictionary<MemoryIndex, MemoryEntry> memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>();
        Dictionary<MemoryIndex, MemoryAlias> memoryAliases = new Dictionary<MemoryIndex, MemoryAlias>();
        Dictionary<MemoryIndex, MemoryInfo> memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>();

        Dictionary<MemoryIndex, AssociativeArray> indexArrays = new Dictionary<MemoryIndex, AssociativeArray>();
        Dictionary<MemoryIndex, ObjectValueContainer> indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>();


        internal DeclarationContainer<FunctionValue> FunctionDecl { get; private set; }
        internal DeclarationContainer<TypeValue> ClassDecl { get; private set; }

        internal MemoryStack<IndexSet<TemporaryIndex>> Temporary { get; private set; }
        internal MemoryStack<IndexContainer> Variables { get; private set; }
        internal MemoryStack<IndexContainer> ContolVariables { get; private set; }

        internal IEnumerable<KeyValuePair<ObjectValue, ObjectDescriptor>> Objects { get { return objectDescriptors; } }


        public IEnumerable<MemoryIndex> MemoryIndexes { get { return memoryIndexes; } }
        
        internal int CallLevel { get; private set; }
        internal MemoryEntry ThisObject { get; private set; }
        Snapshot callerContext;

        public Snapshot()
        {
            callerContext = null;
            ThisObject = null;
            CallLevel = GLOBAL_CALL_LEVEL;

            Variables = createMemoryStack(VariableIndex.CreateUnknown(CallLevel));
            ContolVariables = createMemoryStack(ControlIndex.CreateUnknown(CallLevel));
            FunctionDecl = new DeclarationContainer<FunctionValue>();
            ClassDecl = new DeclarationContainer<TypeValue>();

            Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(new IndexSet<TemporaryIndex>());
        }

        private MemoryStack<IndexContainer> createMemoryStack(MemoryIndex unknownIndex)
        {
            return new MemoryStack<IndexContainer>(createIndexContainer(unknownIndex));
        }

        private IndexContainer createIndexContainer(MemoryIndex unknownIndex)
        {
            IndexContainer container = new IndexContainer(unknownIndex);
            memoryIndexes.Add(unknownIndex);
            memoryEntries.Add(unknownIndex, new MemoryEntry(this.UndefinedValue));

            return container;
        }

        public String DumpSnapshot()
        {
            StringBuilder builder = new StringBuilder();

            foreach (MemoryIndex index in memoryIndexes)
            {
                builder.Append(index.ToString());
                builder.Append("\n");
                builder.Append(GetMemoryEntry(index).ToString());

                MemoryAlias alias;
                if (TryGetAliases(index, out alias))
                {
                    alias.ToString(builder);
                }

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

        protected override bool widenAndCommitTransaction()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Objects

        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
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

        protected override TypeValue objectType(ObjectValue objectValue)
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

        #region Merge Calls and Globals

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

        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(callerContext);

            this.callerContext = snapshot;
            CallLevel = snapshot.CallLevel + 1;
            ThisObject = thisObject;

            memoryIndexes = new HashSet<MemoryIndex>(snapshot.memoryIndexes);
            arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(snapshot.arrayDescriptors);
            objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(snapshot.objectDescriptors);

            memoryValueInfos = new Dictionary<Value, MemoryInfo>(snapshot.memoryValueInfos);

            memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>(snapshot.memoryEntries);
            memoryAliases = new Dictionary<MemoryIndex, MemoryAlias>(snapshot.memoryAliases);
            memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>(snapshot.memoryInfos);

            indexArrays = new Dictionary<MemoryIndex, AssociativeArray>(snapshot.indexArrays);
            indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>(snapshot.indexObjects);

            IndexSet<TemporaryIndex> localTemporary = new IndexSet<TemporaryIndex>();
            Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(snapshot.Temporary, localTemporary);

            IndexContainer localVar = createIndexContainer(VariableIndex.CreateUnknown(CallLevel));
            Variables = new MemoryStack<IndexContainer>(snapshot.Variables, localVar);

            IndexContainer localctrl = createIndexContainer(ControlIndex.CreateUnknown(CallLevel));
            ContolVariables = new MemoryStack<IndexContainer>(snapshot.ContolVariables, localVar);

            FunctionDecl = new DeclarationContainer<FunctionValue>(snapshot.FunctionDecl);
            ClassDecl = new DeclarationContainer<TypeValue>(snapshot.ClassDecl);
        }

        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutputs)
        {
            Snapshot parentCallerContext = null;

            List<Snapshot> snapshots = new List<Snapshot>(callOutputs.Length);
            foreach (ISnapshotReadonly input in callOutputs)
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(input);
                snapshots.Add(snapshot);

                if (parentCallerContext == null)
                {
                    parentCallerContext = snapshot.callerContext;
                }
                else if (snapshot.callerContext != parentCallerContext)
                {
                    throw new Exception("Call outputs don't have the same call context.");
                }
            }

            this.callerContext = parentCallerContext.callerContext;
            CallLevel = parentCallerContext.CallLevel;
            ThisObject = parentCallerContext.ThisObject;

            MergeWorker worker = new MergeWorker(this, snapshots);
            worker.Merge();

            memoryIndexes = new HashSet<MemoryIndex>(worker.memoryIndexes);
            arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(worker.arrayDescriptors);
            objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(worker.objectDescriptors);

            memoryValueInfos = new Dictionary<Value, MemoryInfo>(worker.memoryValueInfos);

            memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>(worker.memoryEntries);
            memoryAliases = worker.GetMemoryAliases();
            memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>(worker.memoryInfos);

            indexArrays = new Dictionary<MemoryIndex, AssociativeArray>(worker.indexArrays);
            indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>(worker.indexObjects);

            Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(worker.Temporary);
            Variables = new MemoryStack<IndexContainer>(worker.Variables);
            ContolVariables = new MemoryStack<IndexContainer>(worker.ContolVariables);

            FunctionDecl = new DeclarationContainer<FunctionValue>(worker.FunctionDecl);
            ClassDecl = new DeclarationContainer<TypeValue>(worker.ClassDecl);
        }

        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            foreach (VariableName name in variables)
            {
                ReadWriteSnapshotEntryBase localEntry = getVariable(new VariableIdentifier(name), false);
                ReadWriteSnapshotEntryBase globalEntry = getVariable(new VariableIdentifier(name), true);

                localEntry.SetAliases(this, globalEntry);
            }
        }

        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            List<VariableName> names = new List<VariableName>();

            foreach (var variable in Variables.Global.Indexes)
            {
                names.Add(new VariableName(variable.Key));
            }

            return names;
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

        #region Functions and Classes

        protected override IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName)
        {
            if (!FunctionDecl.ContainsKey(functionName))
            {
                return FunctionDecl.GetValue(functionName);
            }
            else
            {
                throw new Exception("Function " + functionName + " is not defined in this context.");
            }
        }

        protected override void declareGlobal(FunctionValue declaration)
        {
            QualifiedName name = new QualifiedName(declaration.Name);

            if (!FunctionDecl.ContainsKey(name))
            {
                FunctionDecl.Add(name, declaration);
            }
            else
            {
                throw new Exception("Function " + name + " is already defined in this context.");
            }
        }

        protected override void declareGlobal(TypeValue declaration)
        {
            QualifiedName name = declaration.QualifiedName;

            if (!ClassDecl.ContainsKey(name))
            {
                ClassDecl.Add(name, declaration);
            }
            else
            {
                throw new Exception("Class " + name + " is already defined in this context.");
            }
        }

        #endregion



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
        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Snapshot Entry API

        protected override ReadWriteSnapshotEntryBase getVariable(VariableIdentifier variable, bool forceGlobalContext)
        {
            GlobalContext global = forceGlobalContext ? GlobalContext.GlobalOnly : GlobalContext.LocalOnly;
            return SnapshotEntry.CreateVariableEntry(variable, global);
        }

        protected override ReadWriteSnapshotEntryBase getControlVariable(VariableName name)
        {
            return SnapshotEntry.CreateControlEntry(name, GlobalContext.GlobalOnly);
        }

        protected override ReadWriteSnapshotEntryBase createSnapshotEntry(MemoryEntry entry)
        {
            return new DataSnapshotEntry(entry);
        }

        protected override ReadWriteSnapshotEntryBase getLocalControlVariable(VariableName name)
        {
            return SnapshotEntry.CreateControlEntry(name, GlobalContext.LocalOnly);
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
            MemoryIndex variableIndex = VariableIndex.Create(variableName, CallLevel);

            memoryIndexes.Add(variableIndex);
            Variables.Local.Indexes.Add(variableName, variableIndex);

            CopyMemory(Variables.Local.UnknownIndex, variableIndex, false);

            return variableIndex;
        }

        internal MemoryIndex CreateControll(string variableName)
        {
            MemoryIndex ctrlIndex = ControlIndex.Create(variableName, CallLevel);

            memoryIndexes.Add(ctrlIndex);
            ContolVariables.Local.Indexes.Add(variableName, ctrlIndex);

            CopyMemory(Variables.Local.UnknownIndex, ctrlIndex, false);

            return ctrlIndex;
        }

        internal MemoryIndex CreateGlobalVariable(string variableName)
        {
            MemoryIndex variableIndex = VariableIndex.Create(variableName, GLOBAL_CALL_LEVEL);

            memoryIndexes.Add(variableIndex);
            Variables.Global.Indexes.Add(variableName, variableIndex);

            CopyMemory(Variables.Global.UnknownIndex, variableIndex, false);

            return variableIndex;
        }

        internal MemoryIndex CreateGlobalControll(string variableName)
        {
            MemoryIndex ctrlIndex = ControlIndex.Create(variableName, CallLevel);

            memoryIndexes.Add(ctrlIndex);
            ContolVariables.Global.Indexes.Add(variableName, ctrlIndex);

            CopyMemory(Variables.Global.UnknownIndex, ctrlIndex, false);

            return ctrlIndex;
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
            MemoryAlias aliases;
            if (TryGetAliases(sourceIndex, out aliases))
            {
                MemoryAliasBuilder builder = new MemoryAliasBuilder();
                foreach (MemoryIndex mustAlias in aliases.MustAliasses)
                {
                    MemoryAliasBuilder mustBuilder = GetAliases(mustAlias).Builder();
                    if (isMust)
                    {
                        builder.AddMustAlias(mustAlias);
                        mustBuilder.AddMustAlias(targetIndex);
                    }
                    else
                    {
                        builder.AddMayAlias(mustAlias);
                        mustBuilder.AddMayAlias(targetIndex);
                    }
                    memoryAliases[mustAlias] = mustBuilder.Build();
                }

                foreach (MemoryIndex mayAlias in aliases.MayAliasses)
                {
                    MemoryAliasBuilder mayBuilder = GetAliases(mayAlias).Builder();

                    builder.AddMayAlias(mayAlias);
                    mayBuilder.AddMayAlias(targetIndex);

                    memoryAliases[mayAlias] = mayBuilder.Build();
                }

                memoryAliases[targetIndex] = builder.Build();
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
            TemporaryIndex tmp = new TemporaryIndex(CallLevel);
            memoryIndexes.Add(tmp);
            Temporary.Local.Add(tmp);

            memoryEntries[tmp] = new MemoryEntry(this.UndefinedValue);

            return tmp;
        }

        internal void ReleaseTemporary(TemporaryIndex temporaryIndex)
        {
            ReleaseMemory(temporaryIndex);
            Temporary.Local.Remove(temporaryIndex);
        }

        internal void ReleaseMemory(MemoryIndex index)
        {
            DestroyMemory(index);
            DestroyAliases(index);

            memoryIndexes.Remove(index);
            memoryEntries.Remove(index);

            // TODO - remove temporary from alias structure
            //memoryAliases.Remove(temporaryIndex);
            //memoryInfos.Remove(temporaryIndex);
        }








        private void mergeSnapshots(ISnapshotReadonly[] inputs)
        {
            bool inputSet = false;
            Snapshot callerContext = null;
            int callLevel = 0;
            MemoryEntry thisObject = null;

            List<Snapshot> snapshots = new List<Snapshot>(inputs.Length);
            foreach (ISnapshotReadonly input in inputs)
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(input);
                snapshots.Add(snapshot);

                if (!inputSet)
                {
                    callerContext = snapshot.callerContext;
                    callLevel = snapshot.CallLevel;
                    thisObject = snapshot.ThisObject;

                    inputSet = true;
                }
                else if (snapshot.callerContext != callerContext)
                {
                    throw new Exception("Merged snapshots don't have the same call context.");
                }
                else if (snapshot.CallLevel != callLevel)
                {
                    throw new Exception("Merged snapshots don't have the same call level.");
                }
                else if (snapshot.ThisObject != thisObject)
                {
                    throw new Exception("Merged snapshots don't have the same this object value.");
                }
            }

            this.callerContext = callerContext;
            CallLevel = callLevel;
            ThisObject = thisObject;

            MergeWorker worker = new MergeWorker(this, snapshots);
            worker.Merge();

            memoryIndexes = new HashSet<MemoryIndex>(worker.memoryIndexes);
            arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(worker.arrayDescriptors);
            objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(worker.objectDescriptors);

            memoryValueInfos = new Dictionary<Value, MemoryInfo>(worker.memoryValueInfos);

            memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>(worker.memoryEntries);
            memoryAliases = worker.GetMemoryAliases();
            memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>(worker.memoryInfos);

            indexArrays = new Dictionary<MemoryIndex, AssociativeArray>(worker.indexArrays);
            indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>(worker.indexObjects);

            Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(worker.Temporary);
            Variables = new MemoryStack<IndexContainer>(worker.Variables);
            ContolVariables = new MemoryStack<IndexContainer>(worker.ContolVariables);

            FunctionDecl = new DeclarationContainer<FunctionValue>(worker.FunctionDecl);
            ClassDecl = new DeclarationContainer<TypeValue>(worker.ClassDecl);
        }

        private void extendSnapshot(ISnapshotReadonly input)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(input);

            this.callerContext = snapshot.callerContext;
            CallLevel = snapshot.CallLevel;
            ThisObject = snapshot.ThisObject;

            memoryIndexes = new HashSet<MemoryIndex>(snapshot.memoryIndexes);
            arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(snapshot.arrayDescriptors);
            objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(snapshot.objectDescriptors);

            memoryValueInfos = new Dictionary<Value, MemoryInfo>(snapshot.memoryValueInfos);

            memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>(snapshot.memoryEntries);
            memoryAliases = new Dictionary<MemoryIndex, MemoryAlias>(snapshot.memoryAliases);
            memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>(snapshot.memoryInfos);

            indexArrays = new Dictionary<MemoryIndex, AssociativeArray>(snapshot.indexArrays);
            indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>(snapshot.indexObjects);

            Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(snapshot.Temporary);

            Variables = new MemoryStack<IndexContainer>(snapshot.Variables);
            ContolVariables = new MemoryStack<IndexContainer>(snapshot.ContolVariables);

            FunctionDecl = new DeclarationContainer<FunctionValue>(snapshot.FunctionDecl);
            ClassDecl = new DeclarationContainer<TypeValue>(snapshot.ClassDecl);
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

        public void MustSetAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            DestroyAliases(index);

            MemoryAliasBuilder builder = new MemoryAliasBuilder();
            builder.AddMustAlias(mustAliases);
            builder.AddMayAlias(mayAliases);

            memoryAliases[index] = builder.Build();
        }

        public void MaySetAliases(MemoryIndex index, HashSet<MemoryIndex> mayAliases)
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
            memoryAliases[index] = builder.Build();
        }

        public void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
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

            memoryAliases[index] = alias.Build();
        }

        public void AddAlias(MemoryIndex index, MemoryIndex mustAlias, MemoryIndex mayAlias)
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

            memoryAliases[index] = alias.Build();
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
                MemoryAlias alias = GetAliases(mustIndex);
                if (alias.MustAliasses.Count == 1 && alias.MayAliasses.Count == 0)
                {
                    memoryAliases.Remove(mustIndex);
                }
                else
                {
                    MemoryAliasBuilder builder = GetAliases(mustIndex).Builder();
                    builder.RemoveMustAlias(index);
                    memoryAliases[mustIndex] = builder.Build();
                }
            }

            foreach (MemoryIndex mayIndex in aliases.MayAliasses)
            {
                MemoryAlias alias = GetAliases(mayIndex);
                if (alias.MustAliasses.Count == 0 && alias.MayAliasses.Count == 1)
                {
                    memoryAliases.Remove(mayIndex);
                }
                else
                {
                    MemoryAliasBuilder builder = GetAliases(mayIndex).Builder();
                    builder.RemoveMayAlias(index);
                    memoryAliases[mayIndex] = builder.Build();
                }
            }
        }

        private void convertAliasesToMay(MemoryIndex index, MemoryAliasBuilder builder)
        {
            foreach (MemoryIndex mustIndex in builder.MustAliasses)
            {
                MemoryAlias alias = GetAliases(mustIndex);

                MemoryAliasBuilder mustBuilder = GetAliases(mustIndex).Builder();
                mustBuilder.RemoveMustAlias(index);
                mustBuilder.AddMayAlias(index);
                memoryAliases[mustIndex] = mustBuilder.Build();
            }

            builder.AddMayAlias(builder.MustAliasses);
            builder.MustAliasses.Clear();
        }

        internal MemoryAlias GetAliases(MemoryIndex index)
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
    }
}
