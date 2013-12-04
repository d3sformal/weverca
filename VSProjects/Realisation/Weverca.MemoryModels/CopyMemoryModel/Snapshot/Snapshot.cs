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

        /*HashSet<MemoryIndex> memoryIndexes = new HashSet<MemoryIndex>();


        Dictionary<AssociativeArray, ArrayDescriptor> arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
        Dictionary<ObjectValue, ObjectDescriptor> objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();
        
        Dictionary<MemoryIndex, MemoryEntry> memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>();
        Dictionary<MemoryIndex, MemoryAlias> memoryAliases = new Dictionary<MemoryIndex, MemoryAlias>();

        Dictionary<MemoryIndex, AssociativeArray> indexArrays = new Dictionary<MemoryIndex, AssociativeArray>();
        Dictionary<MemoryIndex, ObjectValueContainer> indexObjects = new Dictionary<MemoryIndex, ObjectValueContainer>();


        internal DeclarationContainer<FunctionValue> FunctionDecl { get; private set; }
        internal DeclarationContainer<TypeValue> ClassDecl { get; private set; }

        internal MemoryStack<IndexSet<TemporaryIndex>> Temporary { get; private set; }
        internal MemoryStack<IndexContainer> Variables { get; private set; }
        internal MemoryStack<IndexContainer> ContolVariables { get; private set; }

        internal IEnumerable<KeyValuePair<ObjectValue, ObjectDescriptor>> Objects { get { return objectDescriptors; } }


        public IEnumerable<MemoryIndex> MemoryIndexes { get { return memoryIndexes; } }*/

        SnapshotData oldData = null;
        internal SnapshotData Data { get; private set; } 
        
        internal int CallLevel { get; private set; }
        internal MemoryEntry ThisObject { get; private set; }
        Snapshot callerContext;

        public Snapshot()
        {
            callerContext = null;
            ThisObject = null;
            CallLevel = GLOBAL_CALL_LEVEL;

            Data = new SnapshotData(this);
        }

        public String DumpSnapshot()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var index in Data.IndexData)
            {
                builder.Append(index.ToString());
                builder.Append("\n");
                builder.Append(index.Value.MemoryEntry.ToString());

                if (index.Value.Aliases != null)
                {
                    index.Value.Aliases.ToString(builder);
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
            oldData = Data;
            Data = new SnapshotData(this, oldData);
        }

        protected override bool commitTransaction()
        {
            return Data.Equals(oldData);
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
            Data.NewIndex(descriptor.UnknownIndex);
            Data.SetDescriptor(createdObject, descriptor);
        }

        protected override IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            ObjectDescriptor descriptor;
            if (Data.TryGetDescriptor(iteratedObject, out descriptor))
            {
                List<ContainerIndex> indexes = new List<ContainerIndex>();
                foreach (var index in descriptor.Indexes)
                {
                    indexes.Add(this.CreateIndex(index.Key));
                }
                return indexes;
            }
            else
            {
                throw new Exception("Unknown object");
            }
        }

        protected override TypeValue objectType(ObjectValue objectValue)
        {
            ObjectDescriptor descriptor;
            if (Data.TryGetDescriptor(objectValue, out descriptor))
            {
                return descriptor.Type;
            }
            else
            {
                throw new Exception("Unknown object");
            }
        }

        #endregion

        #region Arrays

        protected override void initializeArray(AssociativeArray createdArray)
        {
            ArrayDescriptor descriptor = new ArrayDescriptor(createdArray);
            Data.SetDescriptor(createdArray, descriptor);
        }

        protected override IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            ArrayDescriptor descriptor;
            if (Data.TryGetDescriptor(iteratedArray, out descriptor))
            {
                List<ContainerIndex> indexes = new List<ContainerIndex>();
                foreach (var index in descriptor.Indexes)
                {
                    indexes.Add(this.CreateIndex(index.Key));
                }
                return indexes;
            }
            else
            {
                throw new Exception("Unknown associative array");
            }
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

            Data = new SnapshotData(this, snapshot.Data);
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

            Data = worker.Data;
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

            foreach (var variable in Data.Variables.Global.Indexes)
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
            if (!Data.IsFunctionDefined(functionName))
            {
                return Data.GetFunction(functionName);
            }
            else
            {
                throw new Exception("Function " + functionName + " is not defined in this context.");
            }
        }

        protected override void declareGlobal(FunctionValue declaration)
        {
            QualifiedName name = new QualifiedName(declaration.Name);

            if (!Data.IsFunctionDefined(name))
            {
                Data.SetFunction(name, declaration);
            }
            else
            {
                throw new Exception("Function " + name + " is already defined in this context.");
            }
        }

        protected override void declareGlobal(TypeValue declaration)
        {
            QualifiedName name = declaration.QualifiedName;

            if (!Data.IsClassDefined(name))
            {
                Data.SetClass(name, declaration);
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

        internal MemoryIndex CreateLocalVariable(string variableName)
        {
            MemoryIndex variableIndex = VariableIndex.Create(variableName, CallLevel);

            Data.NewIndex(variableIndex);
            Data.Variables.Local.Indexes.Add(variableName, variableIndex);

            CopyMemory(Data.Variables.Local.UnknownIndex, variableIndex, false);

            return variableIndex;
        }

        internal MemoryIndex CreateControll(string variableName)
        {
            MemoryIndex ctrlIndex = ControlIndex.Create(variableName, CallLevel);

            Data.NewIndex(ctrlIndex);
            Data.ContolVariables.Local.Indexes.Add(variableName, ctrlIndex);

            CopyMemory(Data.Variables.Local.UnknownIndex, ctrlIndex, false);

            return ctrlIndex;
        }

        internal MemoryIndex CreateGlobalVariable(string variableName)
        {
            MemoryIndex variableIndex = VariableIndex.Create(variableName, GLOBAL_CALL_LEVEL);

            Data.NewIndex(variableIndex);
            Data.Variables.Global.Indexes.Add(variableName, variableIndex);

            CopyMemory(Data.Variables.Global.UnknownIndex, variableIndex, false);

            return variableIndex;
        }

        internal MemoryIndex CreateGlobalControll(string variableName)
        {
            MemoryIndex ctrlIndex = ControlIndex.Create(variableName, CallLevel);

            Data.NewIndex(ctrlIndex);
            Data.ContolVariables.Global.Indexes.Add(variableName, ctrlIndex);

            CopyMemory(Data.Variables.Global.UnknownIndex, ctrlIndex, false);

            return ctrlIndex;
        }

        internal ObjectValue CreateObject(MemoryIndex parentIndex, bool isMust)
        {
            ObjectValue value = this.CreateObject(null);

            if (isMust)
            {
                DestroyMemory(parentIndex);
                Data.SetMemoryEntry(parentIndex, new MemoryEntry(value));
            }
            else
            {
                MemoryEntry oldEntry;

                List<Value> values;
                if (Data.TryGetMemoryEntry(parentIndex, out oldEntry))
                {
                    values = new List<Value>(oldEntry.PossibleValues);
                }
                else
                {
                    values = new List<Value>();
                }

                values.Add(value);
                Data.SetMemoryEntry(parentIndex, new MemoryEntry(values));
            }

            ObjectValueContainerBuilder objectValues = Data.GetObjects(parentIndex).Builder();
            objectValues.Add(value);

            Data.SetObjects(parentIndex, objectValues.Build());
            return value;
        }


        internal MemoryIndex CreateField(string fieldName, ObjectValue objectValue, bool isMust, bool copyFromUnknown)
        {
            return CreateField(fieldName, Data.GetDescriptor(objectValue), isMust, copyFromUnknown);
        }

        internal MemoryIndex CreateField(string fieldName, ObjectDescriptor descriptor, bool isMust, bool copyFromUnknown)
        {
            if (descriptor.Indexes.ContainsKey(fieldName))
            {
                throw new Exception("Field " + fieldName + " is already defined");
            }

            MemoryIndex fieldIndex = ObjectIndex.Create(descriptor.ObjectValue, fieldName);
            Data.NewIndex(fieldIndex);

            descriptor = descriptor.Builder()
                .add(fieldName, fieldIndex)
                .Build();
            Data.SetDescriptor(descriptor.ObjectValue, descriptor);

            if (copyFromUnknown)
            {
                CopyMemory(descriptor.UnknownIndex, fieldIndex, isMust);
            }

            return fieldIndex;
        }

        internal AssociativeArray CreateArray(MemoryIndex parentIndex)
        {
            if (Data.HasArray(parentIndex))
            {
                throw new Exception("Variable " + parentIndex + " already has associated arraz value.");
            }

            AssociativeArray value = this.CreateArray();
            ArrayDescriptor descriptor = Data.GetDescriptor(value)
                .Builder()
                .SetParentVariable(parentIndex)
                .SetUnknownField(parentIndex.CreateUnknownIndex())
                .Build();

            Data.NewIndex(descriptor.UnknownIndex);
            Data.SetDescriptor(value, descriptor);
            Data.SetArray(parentIndex, value);
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
                Data.SetMemoryEntry(parentIndex, new MemoryEntry(value));
            }
            else
            {
                List<Value> values;
                MemoryEntry oldEntry;
                if (Data.TryGetMemoryEntry(parentIndex, out oldEntry))
                {
                    values = new List<Value>(oldEntry.PossibleValues);
                }
                else
                {
                    values = new List<Value>();
                }

                values.Add(value);
                Data.SetMemoryEntry(parentIndex, new MemoryEntry(values));
            }

            return value;
        }

        internal MemoryIndex CreateIndex(string indexName, AssociativeArray arrayValue, bool isMust, bool copyFromUnknown)
        {
            return CreateIndex(indexName, Data.GetDescriptor(arrayValue), isMust, copyFromUnknown);
        }

        internal MemoryIndex CreateIndex(string indexName, ArrayDescriptor descriptor, bool isMust, bool copyFromUnknown)
        {
            if (descriptor.Indexes.ContainsKey(indexName))
            {
                throw new Exception("Index " + indexName + " is already defined");
            }

            MemoryIndex indexIndex = descriptor.ParentVariable.CreateIndex(indexName);
            Data.NewIndex(indexIndex);

            descriptor = descriptor.Builder()
                .add(indexName, indexIndex)
                .Build();
            Data.SetDescriptor(descriptor.ArrayValue, descriptor);

            if (copyFromUnknown)
            {
                CopyMemory(descriptor.UnknownIndex, indexIndex, false);
            }

            return indexIndex;
        }

        internal MemoryEntry GetMemoryEntry(MemoryIndex index)
        {
            MemoryEntry memoryEntry;
            if (Data.TryGetMemoryEntry(index, out memoryEntry))
            {
                return memoryEntry;
            }
            else
            {
                throw new Exception("Missing memory entry for " + index);
            }
        }

        private void CopyMemory(MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust)
        {
            CopyWithinSnapshotWorker worker = new CopyWithinSnapshotWorker(this, isMust);
            worker.Copy(sourceIndex, targetIndex);
        }

        public void DestroyMemory(MemoryIndex parentIndex)
        {
            DestroyMemoryVisitor visitor = new DestroyMemoryVisitor(this, parentIndex);

            MemoryEntry entry;
            if (Data.TryGetMemoryEntry(parentIndex, out entry))
            {
                visitor.VisitMemoryEntry(entry);
            }
            Data.SetMemoryEntry(parentIndex, new MemoryEntry(this.UndefinedValue));
        }

        internal void CopyAliases(MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust)
        {
            MemoryAlias aliases;
            if (Data.TryGetAliases(sourceIndex, out aliases))
            {
                MemoryAliasBuilder builder = new MemoryAliasBuilder();
                foreach (MemoryIndex mustAlias in aliases.MustAliasses)
                {
                    MemoryAliasBuilder mustBuilder = Data.GetAliases(mustAlias).Builder();
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
                    Data.SetAlias(mustAlias, mustBuilder.Build());
                }

                foreach (MemoryIndex mayAlias in aliases.MayAliasses)
                {
                    MemoryAliasBuilder mayBuilder = Data.GetAliases(mayAlias).Builder();

                    builder.AddMayAlias(mayAlias);
                    mayBuilder.AddMayAlias(targetIndex);

                    Data.SetAlias(mayAlias, mayBuilder.Build());
                }

                /*MemoryAliasBuilder sourceBuilder = aliases.Builder();
                if (isMust && aliases.MustAliasses.Count > 0)
                {
                    sourceBuilder.AddMustAlias(targetIndex);
                    builder.AddMustAlias(sourceIndex);
                }
                else
                {
                    sourceBuilder.AddMayAlias(targetIndex);
                    builder.AddMayAlias(sourceIndex);
                }*/

                Data.SetAlias(targetIndex, builder.Build());
                //Data.SetAlias(sourceIndex, sourceBuilder.Build());
            }
        }

        internal void ClearForArray(MemoryIndex index)
        {
            DestroyObjectsVisitor visitor = new DestroyObjectsVisitor(this, index);
            visitor.VisitMemoryEntry(GetMemoryEntry(index));

            AssociativeArray array;
            MemoryEntry entry;
            if (Data.TryGetArray(index, out array))
            {
                entry = new MemoryEntry(array);
            }
            else
            {
                entry = new MemoryEntry();
            }

            Data.SetMemoryEntry(index, entry);
        }

        internal void ClearForObjects(MemoryIndex index)
        {
            DestroyArrayVisitor visitor = new DestroyArrayVisitor(this, index);
            visitor.VisitMemoryEntry(GetMemoryEntry(index));

            ObjectValueContainer objects = Data.GetObjects(index);
            MemoryEntry entry = new MemoryEntry(objects);

            Data.SetMemoryEntry(index, entry);
        }

        internal bool IsUndefined(MemoryIndex index)
        {
            MemoryEntry entry = GetMemoryEntry(index);
            return entry.PossibleValues.Contains(this.UndefinedValue);
        }

        internal void DestroyObject(MemoryIndex parentIndex, ObjectValue value)
        {
            ObjectValueContainerBuilder objects = Data.GetObjects(parentIndex).Builder();
            objects.Remove(value);
            Data.SetObjects(parentIndex, objects.Build());
        }

        internal void DestroyArray(MemoryIndex parentIndex)
        {
            AssociativeArray arrayValue;
            if (!Data.TryGetArray(parentIndex, out arrayValue))
            {
                return;
            }

            ArrayDescriptor descriptor = Data.GetDescriptor(arrayValue);
            foreach (var index in descriptor.Indexes)
            {
                ReleaseMemory(index.Value);
            }

            ReleaseMemory(descriptor.UnknownIndex);

            Data.RemoveArray(parentIndex, arrayValue);
        }

        internal void MakeMustReferenceObject(ObjectValue objectValue, MemoryIndex targetIndex)
        {
            ObjectValueContainerBuilder objects = Data.GetObjects(targetIndex).Builder();
            objects.Add(objectValue);
            Data.SetObjects(targetIndex, objects.Build());
        }

        internal void MakeMayReferenceObject(HashSet<ObjectValue> objects, MemoryIndex targetIndex)
        {
            ObjectValueContainerBuilder objectsContainer = Data.GetObjects(targetIndex).Builder();

            foreach (ObjectValue objectValue in objects)
            {
                objectsContainer.Add(objectValue);
            }

            Data.SetObjects(targetIndex, objectsContainer.Build());
        }

        internal TemporaryIndex CreateTemporary()
        {
            TemporaryIndex tmp = new TemporaryIndex(CallLevel);
            Data.NewIndex(tmp);
            Data.Temporary.Local.Add(tmp);
            return tmp;
        }

        internal void ReleaseTemporary(TemporaryIndex temporaryIndex)
        {
            ReleaseMemory(temporaryIndex);
            Data.Temporary.Local.Remove(temporaryIndex);
        }

        internal void ReleaseMemory(MemoryIndex index)
        {
            DestroyMemory(index);
            DestroyAliases(index);

            Data.RemoveIndex(index);

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

            Data = worker.Data;
        }

        private void extendSnapshot(ISnapshotReadonly input)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(input);

            this.callerContext = snapshot.callerContext;
            CallLevel = snapshot.CallLevel;
            ThisObject = snapshot.ThisObject;

            Data = new SnapshotData(this, snapshot.Data);
        }

        internal bool HasMustReference(MemoryIndex parentIndex)
        {
            MemoryEntry entry = GetMemoryEntry(parentIndex);

            if (entry.Count == 1)
            {
                ObjectValueContainer objects = Data.GetObjects(parentIndex);
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
            ObjectValueContainer objects = Data.GetObjects(parentIndex);

            return entry.Count == objects.Count;
        }

        public void MustSetAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            DestroyAliases(index);

            MemoryAliasBuilder builder = new MemoryAliasBuilder();
            builder.AddMustAlias(mustAliases);
            builder.AddMayAlias(mayAliases);

            Data.SetAlias(index, builder.Build());
        }

        public void MaySetAliases(MemoryIndex index, HashSet<MemoryIndex> mayAliases)
        {
            MemoryAliasBuilder builder;
            MemoryAlias oldAlias;
            if (Data.TryGetAliases(index, out oldAlias))
            {
                builder = oldAlias.Builder();
                convertAliasesToMay(index, builder);
            }
            else
            {
                builder = new MemoryAliasBuilder();
            }

            builder.AddMayAlias(mayAliases);
            Data.SetAlias(index, builder.Build());
        }

        public void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            MemoryAliasBuilder alias;
            MemoryAlias oldAlias;
            if (Data.TryGetAliases(index, out oldAlias))
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

            Data.SetAlias(index, alias.Build());
        }

        public void AddAlias(MemoryIndex index, MemoryIndex mustAlias, MemoryIndex mayAlias)
        {
            MemoryAliasBuilder alias;
            MemoryAlias oldAlias;
            if (Data.TryGetAliases(index, out oldAlias))
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
            
            Data.SetAlias(index, alias.Build());
        }

        internal void DestroyAliases(MemoryIndex index)
        {
            MemoryAlias aliases;
            if (!Data.TryGetAliases(index, out aliases))
            {
                return;
            }

            foreach (MemoryIndex mustIndex in aliases.MustAliasses)
            {
                MemoryAlias alias = Data.GetAliases(mustIndex);
                if (alias.MustAliasses.Count == 1 && alias.MayAliasses.Count == 0)
                {
                    Data.RemoveAlias(mustIndex);
                }
                else
                {
                    MemoryAliasBuilder builder = Data.GetAliases(mustIndex).Builder();
                    builder.RemoveMustAlias(index);
                    Data.SetAlias(mustIndex, builder.Build());
                }
            }

            foreach (MemoryIndex mayIndex in aliases.MayAliasses)
            {
                MemoryAlias alias = Data.GetAliases(mayIndex);
                if (alias.MustAliasses.Count == 0 && alias.MayAliasses.Count == 1)
                {
                    Data.RemoveAlias(mayIndex);
                }
                else
                {
                    MemoryAliasBuilder builder = Data.GetAliases(mayIndex).Builder();
                    builder.RemoveMayAlias(index);
                    Data.SetAlias(mayIndex, builder.Build());
                }
            }
        }

        private void convertAliasesToMay(MemoryIndex index, MemoryAliasBuilder builder)
        {
            foreach (MemoryIndex mustIndex in builder.MustAliasses)
            {
                MemoryAlias alias = Data.GetAliases(mustIndex);

                MemoryAliasBuilder mustBuilder = Data.GetAliases(mustIndex).Builder();
                mustBuilder.RemoveMustAlias(index);
                mustBuilder.AddMayAlias(index);
                Data.SetAlias(index, mustBuilder.Build());
            }

            builder.AddMayAlias(builder.MustAliasses);
            builder.MustAliasses.Clear();
        }

        internal bool IsTemporarySet(TemporaryIndex temporaryIndex)
        {
            return Data.Temporary.Local.Contains(temporaryIndex);
        }
    }
}
