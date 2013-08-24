using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel
{
    public class Snapshot : SnapshotBase
    {
        private static readonly MemoryIndex undefinedVariable = new MemoryIndex();
        private static readonly MemoryEntry emptyEntry = new MemoryEntry(new Value[] { });

        private Dictionary<VariableName, MemoryIndex> variables = new Dictionary<VariableName, MemoryIndex>();
        private Dictionary<AssociativeArray, ArrayDescriptor> arrays = new Dictionary<AssociativeArray, ArrayDescriptor>();
        private Dictionary<ObjectValue, ObjectDescriptor> objects = new Dictionary<ObjectValue, ObjectDescriptor>();

        private Dictionary<MemoryIndex, MemoryEntry> memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>();
        private Dictionary<MemoryIndex, MemoryInfo> memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>();




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

        #region Variables


        protected override MemoryEntry readValue(VariableName sourceVar)
        {
            MemoryIndex index = getVariableOrUnknown(sourceVar);
            return getMemoryEntry(index);
        }

        protected override void assign(VariableName targetVar, MemoryEntry entry)
        {
            MemoryIndex index = getOrCreateVariable(targetVar);
            assignMemoryEntry(index, entry);
        }

        protected override AliasValue createAlias(VariableName sourceVar)
        {
            MemoryIndex index = getOrCreateVariable(sourceVar);
            return new MemoryAlias(index);
        }

        protected override void assignAlias(VariableName targetVar, AliasValue alias)
        {
            MemoryIndex index = getOrCreateVariable(targetVar);
            assignMemoryAlias(index, alias);
        }

        #endregion

        #region Fields

        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            throw new NotImplementedException();
            /*ObjectDescriptor descriptor = new ObjectDescriptor(type.Declaration);
            objects[createdObject] = descriptor;*/
        }

        protected override MemoryEntry getField(ObjectValue value, ContainerIndex index)
        {
            throw new NotImplementedException();
            /*ObjectDescriptor descriptor = objects[value];

            MemoryIndex memoryIndex = getFieldOrUnknown(descriptor, index);
            return getMemoryEntry(memoryIndex);*/
        }

        protected override void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            throw new NotImplementedException();
            /*ObjectDescriptor descriptor = objects[value];

            MemoryIndex memoryIndex = getOrCreateField(descriptor, index);
            assignMemoryEntry(memoryIndex, entry);*/
        }

        protected override void setFieldAlias(ObjectValue value, ContainerIndex index, AliasValue alias)
        {
            throw new NotImplementedException();
            /*MemoryAlias memoryAlias = alias as MemoryAlias;

            if (memoryAlias != null)
            {
                ObjectDescriptor descriptor = objects[value];

                MemoryIndex memoryIndex = getOrCreateField(descriptor, index);
                assignMemoryAlias(memoryIndex, memoryAlias);
            }*/
        }

        #endregion

        #region Indexes

        protected override void initializeArray(AssociativeArray createdArray)
        {
            ArrayDescriptor descriptor = new ArrayDescriptor(this);
            arrays[createdArray] = descriptor;
        }

        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            MemoryIndex fieldIndex = getIndexOrUnknown(value, index);
            return getMemoryEntry(fieldIndex);
        }

        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            MemoryIndex fieldIndex = getOrCreateIndex(value, index);
            assignMemoryEntry(fieldIndex, entry);
        }

        protected override void setIndexAlias(AssociativeArray value, ContainerIndex index, AliasValue alias)
        {
            MemoryIndex fieldIndex = getOrCreateIndex(value, index);
            assignMemoryAlias(fieldIndex, alias);
        }

        #endregion





        #region Flow Controll
        
        protected override void extend(ISnapshotReadonly[] inputs)
        {
            throw new NotImplementedException();
        }

        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutputs)
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

        #endregion






        #region Something from interface
        protected override void declareGlobal(FunctionValue declaration)
        {
            throw new NotImplementedException();
        }
        protected override void declareGlobal(TypeValue declaration)
        {
            throw new NotImplementedException();
        }
        protected override IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName)
        {
            throw new NotImplementedException();
        }
        protected override IEnumerable<FunctionValue> resolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            throw new NotImplementedException();
        }
        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            throw new NotImplementedException();
        }

        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            throw new NotImplementedException();
        }

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

        #endregion

        #region Containers

        /// <summary>
        /// Fetches memory entry for given memory index or return bool when there is no associated memory entry
        /// </summary>
        /// <param name="index"></param>
        /// <param name="memoryEntry"></param>
        /// <returns></returns>
        private bool tryGetMemoryEntry(MemoryIndex index, out MemoryEntry memoryEntry)
        {
            return memoryEntries.TryGetValue(index, out memoryEntry);
        }

        private MemoryEntry getMemoryEntry(MemoryIndex index)
        {
            MemoryEntry entry;
            if (tryGetMemoryEntry(index, out entry))
            {
                return entry;
            }
            else
            {
                Debug.Assert(false, "MemoryModel::Snapshot - Memory entry is not in the collection");
                return null;
            }
        }

        private MemoryEntry getMemoryEntryOrNull(MemoryIndex index)
        {
            MemoryEntry entry;
            if (tryGetMemoryEntry(index, out entry))
            {
                return entry;
            }
            else
            {
                return null;
            }
        }

        private MemoryIndex getVariableOrUnknown(VariableName variable)
        {
            MemoryIndex index;
            if (variables.TryGetValue(variable, out index))
            {
                return index;
            }
            else
            {
                return undefinedVariable;
            }
        }

        private MemoryIndex getOrCreateVariable(VariableName variable)
        {
            MemoryIndex index;
            if (variables.TryGetValue(variable, out index))
            {
                return index;
            }
            else
            {
                //This is the only place where the new variable is created
                return createVariable(variable);
            }
        }

        private MemoryIndex createVariable(VariableName variable)
        {
            MemoryIndex index = new MemoryIndex();
            MemoryInfo info = new MemoryInfo(index);

            variables[variable] = index;
            memoryInfos[index] = info;
            memoryEntries[index] = emptyEntry;

            return index;
        }


        private MemoryInfo getMemoryInfo(MemoryIndex index)
        {
            return memoryInfos[index];
        }

        #endregion

        #region Array Indexes

        private MemoryIndex getOrCreateIndex(AssociativeArray value, ContainerIndex containerIndex)
        {
            ArrayDescriptor descriptor = arrays[value];
            MemoryIndex index;
            if (descriptor.Indexes.TryGetValue(containerIndex, out index))
            {
                return index;
            }
            else
            {
                return createIndex(value, containerIndex);
            }
        }

        private MemoryIndex createIndex(AssociativeArray value, ContainerIndex containerIndex)
        {
            ArrayDescriptor descriptor = arrays[value];
            MemoryIndex index = new MemoryIndex();
            MemoryInfo info = new IndexMemoryInfo(index, value);
            MemoryEntry entry = emptyEntry;

            memoryInfos[index] = info;
            memoryEntries[index] = entry;

            arrays[value] = descriptor.Builder()
                .add(containerIndex, index)
                .Build();

            return index;
        }

        private MemoryIndex getIndexOrUnknown(AssociativeArray value, ContainerIndex containerIndex)
        {
            ArrayDescriptor descriptor = arrays[value];
            MemoryIndex index;
            if (descriptor.Indexes.TryGetValue(containerIndex, out index))
            {
                return index;
            }
            else
            {
                return undefinedVariable;
            }
        }

        #endregion

        #region Assign

        /// <summary>
        /// String assign of values from given memory entry into new memory index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="entry"></param>
        private void assignMemoryEntry(MemoryIndex index, MemoryEntry entry)
        {
            MemoryInfo info = getMemoryInfo(index);

            ValueVisitors.AssignValueVisitor visitor = new ValueVisitors.AssignValueVisitor(this, index);
            visitor.VisitMemoryEntry(entry);
            MemoryEntry copiedEntry = visitor.GetCopiedEntry();

            info.PostAssign(this, entry);
            destroyMemoryEntry(index);
            
            MemoryEntry newEntry = visitor.GetCopiedEntry();
            foreach (MemoryIndex aliasIndex in info.MustAliasses)
            {
                memoryEntries[aliasIndex] = newEntry;
            }
        }

        internal ObjectValue AssignObjectValue(MemoryIndex targetIndex, ObjectValue value)
        {
            throw new NotImplementedException();
        }

        internal AssociativeArray AssignArrayValue(MemoryIndex targetIndex, AssociativeArray value)
        {
            throw new NotImplementedException();
        }

        internal void WeakAssignMemoryEntry(MemoryIndex alias, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        private void destroyMemoryEntry(MemoryIndex index)
        {
            MemoryEntry entry = getMemoryEntry(index);

            ValueVisitors.DestroyemoryEntryVisitor visitor = new ValueVisitors.DestroyemoryEntryVisitor(this, index);
            visitor.VisitMemoryEntry(entry);
        }


        #endregion

        #region Alias

        private void assignMemoryAlias(MemoryIndex index, AliasValue alias)
        {
            MemoryAlias memoryAlias = alias as MemoryAlias;
            if (memoryAlias == null)
            {
                Debug.Assert(false, "Invalid alias type on assign alias");
            }

            MemoryInfo variableInfo = getMemoryInfo(index);
            MemoryInfo aliasGoupInfo = getMemoryInfo(memoryAlias.Index);

            if (memoryAlias.Index != index && variableInfo != aliasGoupInfo)
            {
                clearMustAliases(index);
                clearMayAliases(index);

                destroyMemoryEntry(index);

                MemoryInfo newGroupInfo = aliasGoupInfo.Builder()
                    .AddMustAlias(index)
                    .Build();

                foreach (MemoryIndex aliasIndex in newGroupInfo.MustAliasses)
                {
                    memoryInfos[aliasIndex] = newGroupInfo;
                }

                memoryInfos[index] = newGroupInfo;

                MemoryEntry aliasGroupEntry = getMemoryEntry(memoryAlias.Index);
                memoryEntries[index] = aliasGroupEntry;
            }
        }

        private void clearMayAliases(MemoryIndex index)
        {
            MemoryInfo oldvariableInfo = getMemoryInfo(index);
            
            if (oldvariableInfo.MayAliasses.Count > 1)
            {
                throw new NotImplementedException();
            }
        }

        private void clearMustAliases(MemoryIndex index)
        {
            MemoryInfo oldvariableInfo = getMemoryInfo(index);

            if (oldvariableInfo.MustAliasses.Count > 1)
            {
                MemoryInfo newVariableInfo = oldvariableInfo.Builder()
                    .RemoveMustAlias(index)
                    .Build();

                foreach (MemoryIndex aliasIndex in oldvariableInfo.MustAliasses)
                {
                    if (aliasIndex != index)
                    {
                        memoryInfos[aliasIndex] = newVariableInfo;
                    }
                }

                memoryEntries[index] = emptyEntry;
            }
        }

        #endregion



































        protected override IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            throw new NotImplementedException();
        }
    }
}
