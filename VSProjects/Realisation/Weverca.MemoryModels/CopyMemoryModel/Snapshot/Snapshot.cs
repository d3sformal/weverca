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
    public class Snapshot : SnapshotBase
    {
        HashSet<MemoryIndex> memoryIndexes = new HashSet<MemoryIndex>();

        internal MemoryIndex UnknownVariable;
        internal Dictionary<string, MemoryIndex> Variables = new Dictionary<string, MemoryIndex>();

        Dictionary<AssociativeArray, ArrayDescriptor> arrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
        Dictionary<ObjectValue, ObjectDescriptor> objectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();

        Dictionary<Value, MemoryInfo> memoryValueInfos = new Dictionary<Value, MemoryInfo>();

        Dictionary<MemoryIndex, MemoryEntry> memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>();
        Dictionary<MemoryIndex, MemoryAlias> memoryAliases = new Dictionary<MemoryIndex, MemoryAlias>();
        Dictionary<MemoryIndex, MemoryInfo> memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>();

        Dictionary<MemoryIndex, AssociativeArray> indexArrays = new Dictionary<MemoryIndex, AssociativeArray>();
        Dictionary<MemoryIndex, ObjectValue> indexObjects = new Dictionary<MemoryIndex, ObjectValue>();

        public Snapshot()
        {
            UnknownVariable = MemoryIndex.MakeIndexAnyVariable();

            memoryIndexes.Add(UnknownVariable);
            memoryEntries.Add(UnknownVariable, new MemoryEntry(this.UndefinedValue));
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
            ObjectDescriptor descriptor = new ObjectDescriptor(createdObject, type);
            objectDescriptors[createdObject] = descriptor;
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
        protected override void extend(ISnapshotReadonly[] inputs)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
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

        //OBSOLETE
        protected override bool variableExists(VariableName variable, bool forceGlobalContext)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override bool objectFieldExists(ObjectValue objectValue, ContainerIndex field)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override bool arrayIndexExists(AssociativeArray array, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        #endregion

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







        internal bool TryGetObject(MemoryIndex parentIndex, out ObjectValue objectValue)
        {
            return indexObjects.TryGetValue(parentIndex, out objectValue);
        }

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
                Debug.Fail("Missing object descriptor");
                return null;
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
                Debug.Fail("Missing array descriptor");
                return null;
            }
        }

        internal MemoryIndex CreateVariable(string variableName)
        {
            MemoryIndex variableIndex = MemoryIndex.MakeIndexVariable(variableName);
            
            memoryIndexes.Add(variableIndex);
            Variables.Add(variableName, variableIndex);

            CopyMemory(UnknownVariable, variableIndex, false);

            return variableIndex;
        }

        internal ObjectValue CreateObject(MemoryIndex parentIndex, bool isMust)
        {
            if (indexObjects.ContainsKey(parentIndex))
            {
                Debug.Fail("Variable " + parentIndex + " already has associated object value.");
            }

            ObjectValue value = this.CreateObject(null);
            ObjectDescriptor descriptor = GetDescriptor(value)
                .Builder()
                .SetParentVariable(parentIndex)
                .SetUnknownField(MemoryIndex.MakeIndexAnyField(parentIndex))
                .Build();

            memoryIndexes.Add(descriptor.UnknownField);
            CreateEmptyEntry(descriptor.UnknownField, false);

            if (isMust)
            {
                destroyMemory(parentIndex);
                memoryEntries[parentIndex] = new MemoryEntry(value);
            }
            else
            {
                List<Value> values = new List<Value>(memoryEntries[parentIndex].PossibleValues);
                values.Add(value);
                memoryEntries[parentIndex] = new MemoryEntry(values);
            }

            objectDescriptors[value] = descriptor;
            indexObjects[parentIndex] = value;

            return value;
        }


        internal MemoryIndex CreateField(string fieldName, ObjectValue objectValue, bool isMust, bool copyFromUnknown)
        {
            return CreateField(fieldName, GetDescriptor(objectValue), isMust, copyFromUnknown);
        }

        internal MemoryIndex CreateField(string fieldName, ObjectDescriptor descriptor, bool isMust, bool copyFromUnknown)
        {
            Debug.Assert(!descriptor.Fields.ContainsKey(fieldName), "Field " + fieldName + " is already defined");

            MemoryIndex fieldIndex = MemoryIndex.MakeIndexField(descriptor.ParentVariable, fieldName);

            memoryIndexes.Add(fieldIndex);

            descriptor = descriptor.Builder()
                .add(fieldName, fieldIndex)
                .Build();
            objectDescriptors[descriptor.ObjectValue] = descriptor;

            if (copyFromUnknown)
            {
                CopyMemory(descriptor.UnknownField, fieldIndex, isMust);
            }
            else
            {
                CreateEmptyEntry(fieldIndex, isMust);
            }

            return fieldIndex;
        }

        internal AssociativeArray CreateArray(MemoryIndex parentIndex, bool isMust)
        {
            if (indexArrays.ContainsKey(parentIndex))
            {
                Debug.Fail("Variable " + parentIndex + " already has associated arraz value.");
            }

            AssociativeArray value = this.CreateArray();
            ArrayDescriptor descriptor = GetDescriptor(value)
                .Builder()
                .SetParentVariable(parentIndex)
                .SetUnknownField(MemoryIndex.MakeIndexAnyIndex(parentIndex))
                .Build();

            memoryIndexes.Add(descriptor.UnknownIndex);
            CreateEmptyEntry(descriptor.UnknownIndex, false);

            if (isMust)
            {
                destroyMemory(parentIndex);
                memoryEntries[parentIndex] = new MemoryEntry(value);
            }
            else
            {
                List<Value> values = new List<Value>(memoryEntries[parentIndex].PossibleValues);
                values.Add(value);
                memoryEntries[parentIndex] = new MemoryEntry(values);
            }

            arrayDescriptors[value] = descriptor;
            indexArrays[parentIndex] = value;

            return value;
        }

        internal MemoryIndex CreateIndex(string indexName, AssociativeArray arrayValue, bool isMust, bool copyFromUnknown)
        {
            return CreateIndex(indexName, GetDescriptor(arrayValue), isMust, copyFromUnknown);
        }

        internal MemoryIndex CreateIndex(string indexName, ArrayDescriptor descriptor, bool isMust, bool copyFromUnknown)
        {
            Debug.Assert(!descriptor.Indexes.ContainsKey(indexName), "Index " + indexName + " is already defined");

            MemoryIndex indexIndex = MemoryIndex.MakeIndexIndex(descriptor.ParentVariable, indexName);

            memoryIndexes.Add(indexIndex);

            descriptor = descriptor.Builder()
                .add(indexName, indexIndex)
                .Build();
            arrayDescriptors[descriptor.ArrayValue] = descriptor;

            if (copyFromUnknown)
            {
                CopyMemory(descriptor.UnknownIndex, indexIndex, isMust);
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
                Debug.Fail("Missing memory entry for " + index);
                return null;
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

        private void destroyMemory(MemoryIndex parentIndex)
        {
            if (indexArrays.ContainsKey(parentIndex) || indexObjects.ContainsKey(parentIndex))
            {
                throw new NotImplementedException();
            }
        }


        internal void SetMemoryEntry(MemoryIndex targetIndex, MemoryEntry memoryEntry)
        {
            memoryEntries[targetIndex] = memoryEntry;
        }

        internal void SameObjectReference(ObjectValue sourceObject, ObjectValue targetObject, bool isMust)
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
        }

        internal ObjectValue GetObject(MemoryIndex index)
        {
            return indexObjects[index];
        }

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
    }
}
