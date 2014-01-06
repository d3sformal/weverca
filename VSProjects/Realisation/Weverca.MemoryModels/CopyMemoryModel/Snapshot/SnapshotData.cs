using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class SnapshotData
    {
        static int DATA_ID = 0;
        int dataId = DATA_ID++;

        public override string ToString()
        {
            return dataId.ToString();
        }

        internal Dictionary<AssociativeArray, ArrayDescriptor> ArrayDescriptors { get; private set; }
        internal Dictionary<ObjectValue, ObjectDescriptor> ObjectDescriptors { get; private set; }

        internal Dictionary<MemoryIndex, IndexData> IndexData { get; private set; }

        internal DeclarationContainer<FunctionValue> FunctionDecl { get; private set; }
        internal DeclarationContainer<TypeValue> ClassDecl { get; private set; }

        internal MemoryStack<IndexSet<TemporaryIndex>> Temporary { get; private set; }
        internal MemoryStack<IndexContainer> Variables { get; private set; }
        internal MemoryStack<IndexContainer> ContolVariables { get; private set; }

        internal int CallLevel { get; private set; }

        internal Snapshot Snapshot { get; private set; }

        public SnapshotData(Snapshot snapshot)
        {
            Snapshot = snapshot;
            CallLevel = snapshot.CallLevel;

            ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
            ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();
            IndexData = new Dictionary<MemoryIndex, IndexData>();
            FunctionDecl = new DeclarationContainer<FunctionValue>();
            ClassDecl = new DeclarationContainer<TypeValue>();

            Variables = createMemoryStack(VariableIndex.CreateUnknown(CallLevel));
            ContolVariables = createMemoryStack(ControlIndex.CreateUnknown(CallLevel));
            FunctionDecl = new DeclarationContainer<FunctionValue>();
            ClassDecl = new DeclarationContainer<TypeValue>();

            Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(new IndexSet<TemporaryIndex>());
        }

        public SnapshotData(Snapshot snapshot, SnapshotData data)
        {
            Snapshot = snapshot;
            CallLevel = snapshot.CallLevel;

            ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(data.ArrayDescriptors);
            ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(data.ObjectDescriptors);
            IndexData = new Dictionary<MemoryIndex, IndexData>(data.IndexData);
            FunctionDecl = new DeclarationContainer<FunctionValue>(data.FunctionDecl);
            ClassDecl = new DeclarationContainer<TypeValue>(data.ClassDecl);

            Variables = new MemoryStack<IndexContainer>(data.Variables);
            ContolVariables = new MemoryStack<IndexContainer>(data.ContolVariables);

            Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(data.Temporary);
        }

        private MemoryStack<IndexContainer> createMemoryStack(MemoryIndex unknownIndex)
        {
            return new MemoryStack<IndexContainer>(createIndexContainer(unknownIndex));
        }

        private IndexContainer createIndexContainer(MemoryIndex unknownIndex)
        {
            IndexContainer container = new IndexContainer(unknownIndex);
            NewIndex(unknownIndex);

            return container;
        }

        public bool WidenNotEqual(SnapshotData compare, MemoryAssistantBase assistant)
        {
            bool funcCount = this.FunctionDecl.Count == compare.FunctionDecl.Count;
            bool classCount = this.ClassDecl.Count == compare.ClassDecl.Count;

            if (!widenNotEqualData(compare, assistant))
            {
                return false;
            }

            if (classCount && funcCount)
            {
                if (!this.FunctionDecl.DataEquals(compare.FunctionDecl))
                {
                    return false;
                }

                if (!this.ClassDecl.DataEquals(compare.ClassDecl))
                {
                    return false;
                }
            }

            return true;
        }

        public bool DataEquals(SnapshotData oldValue)
        {
            bool funcCount = this.FunctionDecl.Count == oldValue.FunctionDecl.Count;
            bool classCount = this.ClassDecl.Count == oldValue.ClassDecl.Count;
            bool indexCount = this.IndexData.Count == oldValue.IndexData.Count;

            if (indexCount && classCount && funcCount)
            {
                if (!compareData(oldValue))
                {
                    return false;
                }

                if (!this.FunctionDecl.DataEquals(oldValue.FunctionDecl))
                {
                    return false;
                }

                if (!this.ClassDecl.DataEquals(oldValue.ClassDecl))
                {
                    return false;
                }
            }

            return true;
        }

        private bool widenNotEqualData(SnapshotData oldValue, MemoryAssistantBase assistant)
        {
            HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();
            HashSetTools.AddAll(indexes, this.IndexData.Keys);
            HashSetTools.AddAll(indexes, oldValue.IndexData.Keys);

            bool areEqual = true;
            foreach (MemoryIndex index in indexes)
            {
                IndexData newData = null;
                IndexData oldData = null;
                if (this.IndexData.TryGetValue(index, out newData)
                    && oldValue.IndexData.TryGetValue(index, out oldData))
                {
                    if (!newData.DataEquals(oldData))
                    {
                        MemoryEntry newEntry = assistant.Widen(oldData.MemoryEntry, newData.MemoryEntry);
                        this.SetMemoryEntry(index, newEntry);

                        areEqual = false;
                    }
                }
                else if (newData != null && oldData == null)
                {
                    MemoryEntry newEntry = assistant.Widen(new MemoryEntry(Snapshot.UndefinedValue), newData.MemoryEntry);
                    this.SetMemoryEntry(index, newEntry);

                    areEqual = false;
                }
                else
                {
                    areEqual = false;
                }
            }

            return areEqual;
        }

        private bool compareData(SnapshotData compare)
        {
            foreach (var index in this.IndexData)
            {
                IndexData data;
                if (compare.IndexData.TryGetValue(index.Key, out data))
                {
                    if (!data.DataEquals(index.Value))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        #region Indexes

        internal void NewIndex(MemoryIndex index)
        {
            IndexData data = new IndexData(new MemoryEntry(Snapshot.UndefinedValue), null, null, null);

            IndexData.Add(index, data);
        }

        internal bool IsDefined(MemoryIndex index)
        {
            return IndexData.ContainsKey(index);
        }

        internal void RemoveIndex(MemoryIndex index)
        {
            IndexData.Remove(index);
        }

        internal bool TryGetIndexData(MemoryIndex index, out IndexData data)
        {
            return IndexData.TryGetValue(index, out data);
        }

        internal IndexData GetIndexData(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                return data;
            }
            throw new Exception("Missing alias value for " + index);
        }

        #endregion

        #region Objects

        internal ObjectDescriptor GetDescriptor(ObjectValue objectValue)
        {
            ObjectDescriptor descriptor;
            if (ObjectDescriptors.TryGetValue(objectValue, out descriptor))
            {
                return descriptor;
            }
            else
            {
                throw new Exception("Missing object descriptor");
            }
        }

        internal bool TryGetDescriptor(ObjectValue objectValue, out ObjectDescriptor descriptor)
        {
            return ObjectDescriptors.TryGetValue(objectValue, out descriptor);
        }

        internal void SetDescriptor(ObjectValue objectValue, ObjectDescriptor descriptor)
        {
            ObjectDescriptors[objectValue] = descriptor;
        }

        internal bool HasObjects(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                return data.Objects != null;
            }
            else
            {
                return false;
            }
        }

        public ObjectValueContainer GetObjects(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                if (data.Objects != null)
                {
                    return data.Objects;
                }
            }

            return new ObjectValueContainer();
        }

        internal void SetObjects(MemoryIndex index, ObjectValueContainer objects)
        {
            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Objects = objects;

            IndexData[index] = builder.Build();
        }

        #endregion

        #region Arrays

        internal bool TryGetDescriptor(AssociativeArray arrayValue, out ArrayDescriptor descriptor)
        {
            return ArrayDescriptors.TryGetValue(arrayValue, out descriptor);
        }

        internal ArrayDescriptor GetDescriptor(AssociativeArray arrayValue)
        {
            ArrayDescriptor descriptor;
            if (ArrayDescriptors.TryGetValue(arrayValue, out descriptor))
            {
                return descriptor;
            }
            else
            {
                throw new Exception("Missing array descriptor");
            }
        }

        internal void SetDescriptor(AssociativeArray arrayvalue, ArrayDescriptor descriptor)
        {
            ArrayDescriptors[arrayvalue] = descriptor;
        }

        internal bool HasArray(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                return data.Array != null;
            }
            else
            {
                return false;
            }
        }

        internal bool TryGetArray(MemoryIndex index, out AssociativeArray arrayValue)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                arrayValue = data.Array;
                return data.Array != null;
            }
            else
            {
                arrayValue = null;
                return false;
            }
        }

        internal void SetArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Array = arrayValue;

            IndexData[index] = builder.Build();
        }

        internal void RemoveArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            ArrayDescriptors.Remove(arrayValue);

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Array = null;

            IndexData[index] = builder.Build();
        }

        #endregion

        #region Functions

        internal bool IsFunctionDefined(PHP.Core.QualifiedName functionName)
        {
            return FunctionDecl.ContainsKey(functionName);
        }

        internal IEnumerable<FunctionValue> GetFunction(PHP.Core.QualifiedName functionName)
        {
            return FunctionDecl.GetValue(functionName);
        }

        internal void SetFunction(QualifiedName name, FunctionValue declaration)
        {
            FunctionDecl.Add(name, declaration);
        }

        #endregion

        #region Classes

        internal bool IsClassDefined(PHP.Core.QualifiedName name)
        {
            return ClassDecl.ContainsKey(name);
        }

        internal void SetClass(PHP.Core.QualifiedName name, TypeValue declaration)
        {
            ClassDecl.Add(name, declaration);
        }

        #endregion

        #region Aliasses

        internal bool TryGetAliases(MemoryIndex index, out MemoryAlias aliases)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                aliases = data.Aliases;
                return data.Aliases != null;
            }
            else
            {
                aliases = null;
                return false;
            }
        }

        internal MemoryAlias GetAliases(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                if (data.Aliases != null)
                {
                    return data.Aliases;
                }
            }
            throw new Exception("Missing alias value for " + index);
        }

        internal void SetAlias(MemoryIndex index, MemoryAlias alias)
        {
            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Aliases = alias;

            IndexData[index] = builder.Build();
        }

        internal void RemoveAlias(MemoryIndex index)
        {
            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Aliases = null;

            IndexData[index] = builder.Build();
        }

        #endregion

        #region MemoryEntries

        internal MemoryEntry GetMemoryEntry(MemoryIndex index)
        {
            MemoryEntry memoryEntry;
            if (TryGetMemoryEntry(index, out memoryEntry))
            {
                return memoryEntry;
            }
            else
            {
                throw new Exception("Missing memory entry for " + index);
            }
        }

        internal bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                entry = data.MemoryEntry;
                return data.MemoryEntry != null;
            }
            else
            {
                entry = null;
                return false;
            }
        }

        internal void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry)
        {
            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.MemoryEntry = memoryEntry;

            IndexData[index] = builder.Build();
        }
        #endregion
    }
}
