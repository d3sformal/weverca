using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class SnapshotStructure
    {
        static int DATA_ID = 0;
        int dataId = DATA_ID++;

        public int DataId { get { return dataId; } }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var index in IndexData)
            {
                builder.Append(index.ToString());
                builder.Append("\n");
                //builder.Append(index.Value.MemoryEntry.ToString());

                if (index.Value.Aliases != null)
                {
                    index.Value.Aliases.ToString(builder);
                }

                builder.Append("\n\n");
            }

            return builder.ToString();
        }

        internal Dictionary<AssociativeArray, ArrayDescriptor> ArrayDescriptors { get; private set; }
        internal Dictionary<ObjectValue, ObjectDescriptor> ObjectDescriptors { get; private set; }

        internal Dictionary<MemoryIndex, IndexData> IndexData { get; private set; }

        internal DeclarationContainer<FunctionValue> FunctionDecl { get; private set; }
        internal DeclarationContainer<TypeValue> ClassDecl { get; private set; }

        internal MemoryStack<IndexSet<TemporaryIndex>> Temporary { get; private set; }
        internal VariableStack Variables { get; private set; }
        internal VariableStack ContolVariables { get; private set; }
        internal MemoryStack<IndexSet<AssociativeArray>> Arrays { get; private set; }
        internal Dictionary<AssociativeArray, IndexSet<Snapshot>> CallArrays { get; private set; }

        internal List<AliasData> CreatedAliases { get; private set; }

        internal Snapshot Snapshot { get; private set; }
        public SnapshotData Data { get; set; }
        public bool Locked { get; set; }

        private SnapshotStructure(Snapshot snapshot)
        {
            Snapshot = snapshot;

            CreatedAliases = new List<AliasData>();
        }

        public static SnapshotStructure CreateEmpty(Snapshot snapshot)
        {
            SnapshotStructure data = new SnapshotStructure(snapshot);

            data.ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
            data.ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();
            data.IndexData = new Dictionary<MemoryIndex, IndexData>();

            data.Variables = new VariableStack(snapshot.CallLevel);
            data.ContolVariables = new VariableStack(snapshot.CallLevel);
            data.FunctionDecl = new DeclarationContainer<FunctionValue>();
            data.ClassDecl = new DeclarationContainer<TypeValue>();

            data.Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(snapshot.CallLevel);
            data.Arrays = new MemoryStack<IndexSet<AssociativeArray>>(snapshot.CallLevel);
            data.CallArrays = new Dictionary<AssociativeArray, IndexSet<Snapshot>>();

            return data;
        }

        public static SnapshotStructure CreateGlobal(Snapshot snapshot, SnapshotData snapshotData)
        {
            SnapshotStructure data = new SnapshotStructure(snapshot);
            data.Data = snapshotData;

            data.ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>();
            data.ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>();
            data.IndexData = new Dictionary<MemoryIndex, IndexData>();

            data.Variables = data.createMemoryStack(VariableIndex.CreateUnknown(Snapshot.GLOBAL_CALL_LEVEL));
            data.ContolVariables = data.createMemoryStack(ControlIndex.CreateUnknown(Snapshot.GLOBAL_CALL_LEVEL));
            data.FunctionDecl = new DeclarationContainer<FunctionValue>();
            data.ClassDecl = new DeclarationContainer<TypeValue>();

            data.Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(new IndexSet<TemporaryIndex>());
            data.Arrays = new MemoryStack<IndexSet<AssociativeArray>>(new IndexSet<AssociativeArray>());
            data.CallArrays = new Dictionary<AssociativeArray, IndexSet<Snapshot>>();

            return data;
        }

        public SnapshotStructure Copy(Snapshot snapshot, SnapshotData snapshotData)
        {
            SnapshotStructure data = new SnapshotStructure(snapshot);
            data.Data = snapshotData;

            data.ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(ArrayDescriptors);
            data.ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(ObjectDescriptors);
            data.IndexData = new Dictionary<MemoryIndex, IndexData>(IndexData);
            data.FunctionDecl = new DeclarationContainer<FunctionValue>(FunctionDecl);
            data.ClassDecl = new DeclarationContainer<TypeValue>(ClassDecl);

            data.Variables = new VariableStack(Variables);
            data.ContolVariables = new VariableStack(ContolVariables);

            data.Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(Temporary);
            data.Arrays = new MemoryStack<IndexSet<AssociativeArray>>(Arrays);
            data.CallArrays = new Dictionary<AssociativeArray, IndexSet<Snapshot>>(CallArrays);

            return data;
        }

        public SnapshotStructure CopyAndAddLocalLevel(Snapshot snapshot, SnapshotData snapshotData)
        {
            SnapshotStructure data = new SnapshotStructure(snapshot);
            data.Data = snapshotData;

            data.ArrayDescriptors = new Dictionary<AssociativeArray, ArrayDescriptor>(ArrayDescriptors);
            data.ObjectDescriptors = new Dictionary<ObjectValue, ObjectDescriptor>(ObjectDescriptors);
            data.IndexData = new Dictionary<MemoryIndex, IndexData>(IndexData);
            data.FunctionDecl = new DeclarationContainer<FunctionValue>(FunctionDecl);
            data.ClassDecl = new DeclarationContainer<TypeValue>(ClassDecl);

            data.Variables = new VariableStack(Variables, data.createIndexContainer(VariableIndex.CreateUnknown(Variables.Length)));
            data.ContolVariables = new VariableStack(ContolVariables, data.createIndexContainer(ControlIndex.CreateUnknown(ContolVariables.Length)));

            data.Temporary = new MemoryStack<IndexSet<TemporaryIndex>>(Temporary, new IndexSet<TemporaryIndex>());
            data.Arrays = new MemoryStack<IndexSet<AssociativeArray>>(Arrays, new IndexSet<AssociativeArray>());
            data.CallArrays = new Dictionary<AssociativeArray, IndexSet<Snapshot>>(CallArrays);

            return data;
        }

        private VariableStack createMemoryStack(MemoryIndex unknownIndex)
        {
            return new VariableStack(createIndexContainer(unknownIndex));
        }

        private IndexContainer createIndexContainer(MemoryIndex unknownIndex)
        {
            IndexContainer container = new IndexContainer(unknownIndex);
            NewIndex(unknownIndex);

            return container;
        }

        public bool WidenNotEqual(SnapshotStructure compare, MemoryAssistantBase assistant)
        {
            lockedTest();

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
            else
            {
                return false;
            }

            return true;
        }

        public bool DataEquals(SnapshotStructure oldValue)
        {
            bool funcCount = this.FunctionDecl.Count == oldValue.FunctionDecl.Count;
            bool classCount = this.ClassDecl.Count == oldValue.ClassDecl.Count;
            bool indexCount = this.IndexData.Count == oldValue.IndexData.Count;

            if (!compareData(oldValue))
            {
                return false;
            }

            if (classCount && funcCount)
            {
                if (!this.FunctionDecl.DataEquals(oldValue.FunctionDecl))
                {
                    return false;
                }

                if (!this.ClassDecl.DataEquals(oldValue.ClassDecl))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool widenNotEqualData(SnapshotStructure oldValue, MemoryAssistantBase assistant)
        {
            HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();
            HashSetTools.AddAll(indexes, this.IndexData.Keys);
            HashSetTools.AddAll(indexes, oldValue.IndexData.Keys);

            IndexData emptyData = new CopyMemoryModel.IndexData(null, null, null);

            bool areEqual = true;
            foreach (MemoryIndex index in indexes)
            {
                IndexData newData = null;
                if (!this.IndexData.TryGetValue(index, out newData))
                {
                    newData = emptyData;
                }

                IndexData oldData = null;
                if (!oldValue.IndexData.TryGetValue(index, out oldData))
                {
                    oldData = emptyData;
                }

                Data.DataWiden(oldValue.Data, index, assistant);

                if (!Data.DataEquals(oldValue.Data, index))
                {
                    areEqual = false;
                }
            }

            return areEqual;
        }

        private bool compareData(SnapshotStructure oldValue)
        {
            HashSet<MemoryIndex> usedIndexes = new HashSet<MemoryIndex>();
            HashSetTools.AddAll(usedIndexes, this.IndexData.Keys);
            HashSetTools.AddAll(usedIndexes, oldValue.IndexData.Keys);

            IndexData emptyStructure = new CopyMemoryModel.IndexData(null, null, null);

            foreach (MemoryIndex index in usedIndexes)
            {
                if (index is TemporaryIndex)
                {
                    continue;
                }

                IndexData newStructure = null;
                if (!this.IndexData.TryGetValue(index, out newStructure))
                {
                    newStructure = emptyStructure;
                }

                IndexData oldStructure = null;
                if (!oldValue.IndexData.TryGetValue(index, out oldStructure))
                {
                    oldStructure = emptyStructure;
                }

                if (!newStructure.DataEquals(oldStructure))
                {
                    return false;
                }

                if (!Data.DataEquals(oldValue.Data, index))
                {
                    return false;
                }
            }

            return true;
        }

        #region Indexes

        internal void NewIndex(MemoryIndex index)
        {
            lockedTest();

            IndexData data = new IndexData(null, null, null);

            IndexData.Add(index, data);
            Data.SetMemoryEntry(index, new MemoryEntry(Snapshot.UndefinedValue));
        }

        internal bool IsDefined(MemoryIndex index)
        {
            return IndexData.ContainsKey(index);
        }

        internal void RemoveIndex(MemoryIndex index)
        {
            lockedTest();

            IndexData.Remove(index);
            Data.RemoveMemoryEntry(index);
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
            lockedTest();

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
            lockedTest();

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
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
            lockedTest();

            ArrayDescriptors[arrayvalue] = descriptor;
        }

        internal AssociativeArray GetArray(MemoryIndex index)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                if (data.Array != null)
                {
                    return data.Array;
                }
            }

            throw new Exception("Missing array for index " + index);
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

        internal bool TryGetCallArraySnapshot(AssociativeArray array, out IEnumerable<Snapshot> snapshots)
        {
            IndexSet<Snapshot> snapshotSet = null;
            if (CallArrays.TryGetValue(array, out snapshotSet))
            {
                snapshots = snapshotSet;
                return true;
            }
            else
            {
                snapshots = null;
                return false;
            }
        }

        internal void AddCallArray(AssociativeArray array, CopyMemoryModel.Snapshot snapshot)
        {
            IndexSet<Snapshot> snapshots;
            if (!CallArrays.TryGetValue(array, out snapshots))
            {
                snapshots = new IndexSet<Snapshot>();
                CallArrays[array] = snapshots;
            }

            snapshots.Add(snapshot);
        }

        internal void SetArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            lockedTest();

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Array = arrayValue;

            IndexData[index] = builder.Build();

            ArrayDescriptor descriptor;
            if (TryGetDescriptor(arrayValue, out descriptor))
            {
                if (descriptor.ParentVariable != null)
                {
                    Arrays[descriptor.ParentVariable.CallLevel].Remove(arrayValue);
                }
            }
            Arrays[index.CallLevel].Add(arrayValue);
        }

        internal void RemoveArray(MemoryIndex index, AssociativeArray arrayValue)
        {
            lockedTest();

            ArrayDescriptors.Remove(arrayValue);

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Array = null;

            IndexData[index] = builder.Build();
            Arrays[index.CallLevel].Remove(arrayValue);
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
            lockedTest();

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
            lockedTest();

            ClassDecl.Add(name, declaration);
        }

        internal IEnumerable<TypeValue> GetClass(PHP.Core.QualifiedName className)
        {
            return ClassDecl.GetValue(className);
        }

        #endregion

        #region Aliasses

        internal void AddCreatedAlias(AliasData aliasData)
        {
            CreatedAliases.Add(aliasData);
        }

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
            lockedTest();

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Aliases = alias;

            IndexData[index] = builder.Build();
        }

        internal void RemoveAlias(MemoryIndex index)
        {
            lockedTest();

            IndexData data;
            if (!IndexData.TryGetValue(index, out data))
            {
                data = new IndexData(null, null, null);
            }

            IndexDataBuilder builder = data.Builder();
            builder.Aliases = null;

            IndexData[index] = builder.Build();
        }

        #endregion

        #region MemoryEntries

        internal MemoryEntry GetMemoryEntry(MemoryIndex index)
        {
            return Data.GetMemoryEntry(index);
        }

        internal bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry)
        {
            return Data.TryGetMemoryEntry(index, out entry);
        }

        internal void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry)
        {
            IndexData data;
            if (IndexData.TryGetValue(index, out data))
            {
                Data.SetMemoryEntry(index, memoryEntry);
            }
            else if (!Locked)
            {
                IndexData[index] = new IndexData(null, null, null);
                Data.SetMemoryEntry(index, memoryEntry);
            }

            
        }
        #endregion

        private void lockedTest()
        {
            if (Locked)
            {
                throw new Exception("Snapshot structure is locked in this mode. Mode: " + Snapshot.CurrentMode);
            }
        }

        internal string GetArraysRepresentation(SnapshotData data, SnapshotData infos)
        {
            StringBuilder result = new StringBuilder();

            foreach (var item in ArrayDescriptors)
            {
                IndexContainer.GetRepresentation(item.Value, data, infos, result);
                result.AppendLine();
            }
            
            return result.ToString();
        }

        internal string GetFieldsRepresentation(SnapshotData data, SnapshotData infos)
        {
            StringBuilder result = new StringBuilder();

            foreach (var item in ObjectDescriptors)
            {
                IndexContainer.GetRepresentation(item.Value, data, infos, result);
                result.AppendLine();
            }

            return result.ToString();
        }

        internal string GetaliasesRepresentation()
        {
            StringBuilder result = new StringBuilder();

            foreach (var item in IndexData)
            {
                var aliases = item.Value.Aliases;
                if (aliases != null && (aliases.MayAliasses.Count > 0 || aliases.MustAliasses.Count > 0))
                {
                    MemoryIndex index = item.Key;
                    result.AppendFormat("{0}: {{ ", index);

                    result.Append(" MUST: ");
                    foreach (var alias in aliases.MustAliasses)
                    {
                        result.Append(alias);
                        result.Append(", ");
                    }
                    result.Length -= 2;

                    result.Append(" | MAY: ");
                    foreach (var alias in aliases.MayAliasses)
                    {
                        result.Append(alias);
                        result.Append(", ");
                    }
                    result.Length -= 2;
                    result.AppendLine(" }");
                }
            }

            return result.ToString();
        }
    }
}

