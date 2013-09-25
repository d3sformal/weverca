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

        protected override bool tryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext)
        {
            throw new NotImplementedException();
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

        protected override void assignAlias(VariableName targetVar, IEnumerable<AliasValue> aliases)
        {
            //TODO signature change
            if (aliases.Count() != 1)
                throw new NotImplementedException();

            var alias = aliases.First();

            MemoryIndex index = getOrCreateVariable(targetVar);
            assignMemoryAlias(index, alias);
        }

        protected override bool variableExists(VariableName variable, bool forceGlobalContext)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Fields

        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            ObjectDescriptor descriptor = new ObjectDescriptor(type);
            objects[createdObject] = descriptor;
        }

        protected override MemoryEntry getField(ObjectValue value, ContainerIndex index)
        {
            MemoryIndex fieldIndex = getFieldOrUnknown(value, index);
            return getMemoryEntry(fieldIndex);
        }

        protected override bool tryGetField(ObjectValue objectValue, ContainerIndex field, out MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            MemoryIndex fieldIndex = getOrCreateField(value, index);
            assignMemoryEntry(fieldIndex, entry);
        }

        protected override void setFieldAlias(ObjectValue value, ContainerIndex index, IEnumerable<AliasValue> aliases)
        {
            //TODO signature change
            if (aliases.Count() != 1)
                throw new NotImplementedException();

            var alias = aliases.First();

            MemoryIndex fieldIndex = getOrCreateField(value, index);
            assignMemoryAlias(fieldIndex, alias);
        }

        protected override bool objectFieldExists(ObjectValue objectValue, ContainerIndex field)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            ObjectDescriptor descriptor = objects[iteratedObject];
            return descriptor.Fields.Keys;
        }

        #endregion

        #region Indexes

        protected override void initializeArray(AssociativeArray createdArray)
        {
            ArrayDescriptor descriptor = new ArrayDescriptor();
            arrays[createdArray] = descriptor;
        }

        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            MemoryIndex fieldIndex = getIndexOrUnknown(value, index);
            return getMemoryEntry(fieldIndex);
        }

        protected override bool tryGetIndex(AssociativeArray array, ContainerIndex index, out MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            MemoryIndex fieldIndex = getOrCreateIndex(value, index);
            assignMemoryEntry(fieldIndex, entry);
        }

        protected override void setIndexAlias(AssociativeArray value, ContainerIndex index, IEnumerable<AliasValue> aliases)
        {
            //TODO signature change
            if (aliases.Count() != 1)
                throw new NotImplementedException();

            var alias = aliases.First();

            MemoryIndex fieldIndex = getOrCreateIndex(value, index);
            assignMemoryAlias(fieldIndex, alias);
        }

        protected override bool arrayIndexExists(AssociativeArray array, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            ArrayDescriptor array = arrays[iteratedArray];
            return array.Indexes.Keys;
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
            MemoryInfo info = new MemoryInfo(index);
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

        #region Object Fields

        private MemoryIndex getFieldOrUnknown(ObjectValue value, ContainerIndex containerIndex)
        {
            ObjectDescriptor descriptor = objects[value];
            MemoryIndex index;
            if (descriptor.Fields.TryGetValue(containerIndex, out index))
            {
                return index;
            }
            else
            {
                return undefinedVariable;
            }
        }

        private MemoryIndex getOrCreateField(ObjectValue value, ContainerIndex containerIndex)
        {
            ObjectDescriptor descriptor = objects[value];
            MemoryIndex index;
            if (descriptor.Fields.TryGetValue(containerIndex, out index))
            {
                return index;
            }
            else
            {
                return createField(value, containerIndex);
            }
        }

        private MemoryIndex createField(ObjectValue value, ContainerIndex containerIndex)
        {
            ObjectDescriptor descriptor = objects[value];
            MemoryIndex index = new MemoryIndex();
            MemoryInfo info = new MemoryInfo(index);
            MemoryEntry entry = emptyEntry;

            memoryInfos[index] = info;
            memoryEntries[index] = entry;

            objects[value] = descriptor.Builder()
                .add(containerIndex, index)
                .Build();

            return index;
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
            MemoryEntry oldMemoryEntry = getMemoryEntry(index);

            if (oldMemoryEntry != entry)
            {
                ValueVisitors.AssignValueVisitor visitor = new ValueVisitors.AssignValueVisitor(this, index);
                visitor.VisitMemoryEntry(entry);
   
                destroyMemoryEntry(index);
            
                MemoryEntry newEntry = visitor.GetCopiedEntry();
                foreach (MemoryIndex aliasIndex in info.MustAliasses)
                {
                    memoryEntries[aliasIndex] = newEntry;
                }
            }
        }

        /// <summary>
        /// Assigns reference of object into the selected index
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal ObjectValue AssignObjectValue(MemoryIndex targetIndex, ObjectValue value)
        {
            ObjectDescriptor oldDescriptor = objects[value];
            MemoryInfo targetInfo = getMemoryInfo(targetIndex);

            //Creates new MUST reference with all MUST objects
            var newDescriptorBuilder = oldDescriptor.Builder();
            foreach (MemoryIndex aliasIndex in targetInfo.MustAliasses)
            {
                newDescriptorBuilder.addMustReference(aliasIndex);
            }

            objects[value] = newDescriptorBuilder.Build();
            return value;
        }

        /// <summary>
        /// Provides deep copy of given array and stores it in the selected index
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        internal AssociativeArray AssignArrayValue(MemoryIndex targetIndex, AssociativeArray sourceValue)
        {
            ArrayDescriptor sourceDescriptor = arrays[sourceValue];

            AssociativeArray targetValue = CreateArray();
            ArrayDescriptorBuilder descriptorBuilder = arrays[targetValue].Builder();
            
            //Copy all indexes
            foreach (var sourceContent in sourceDescriptor.Indexes)
            {
                //Creates new index
                MemoryIndex newIndex = createIndex(targetValue, sourceContent.Key);
                descriptorBuilder.add(sourceContent.Key, newIndex);

                MemoryInfo sourceInfo = getMemoryInfo(sourceContent.Value);
                if (sourceInfo.MustAliasses.Count == 1)
                {
                    //If tehere is no MUST alias just provides deep copy
                    MemoryEntry sourceEntry = getMemoryEntry(sourceContent.Value);
                    ValueVisitors.AssignValueVisitor visitor = new ValueVisitors.AssignValueVisitor(this, newIndex);
                    visitor.VisitMemoryEntry(sourceEntry);

                    memoryEntries[newIndex] = visitor.GetCopiedEntry();
                }
                else
                {
                    //If there is some MUST alias in source - create alias reference
                    memoryEntries[newIndex] = getMemoryEntry(sourceContent.Value);

                    MemoryInfo newInfo = sourceInfo
                        .Builder()
                        .AddMustAlias(newIndex)
                        .Build();

                    foreach (MemoryIndex aliasIndex in newInfo.MustAliasses)
                    {
                        memoryInfos[aliasIndex] = newInfo;
                    }
                }

                //Add new index into MAY alias structure
                foreach (MemoryIndex aliasIndex in sourceInfo.MayAliasses)
                {
                    memoryInfos[aliasIndex] = getMemoryInfo(aliasIndex)
                        .Builder()
                        .AddMayAlias(newIndex)
                        .Build();
                }

                if (sourceInfo.MayAliasses.Count > 0 && sourceInfo.MustAliasses.Count == 1)
                {
                    //There is some MAY an no MUST alias - create MAY alias between source and new index
                    //Add new to source info
                    memoryInfos[sourceContent.Value] = sourceInfo
                        .Builder()
                        .AddMayAlias(newIndex)
                        .Build();

                    //Add source to new info
                    memoryInfos[newIndex] = memoryInfos[sourceContent.Value]
                        .Builder()
                        .AddMayAlias(sourceContent.Value)
                        .Build();
                }
            }

            descriptorBuilder.AddParentVariable(targetIndex);
            arrays[targetValue] = descriptorBuilder.Build();

            return targetValue;
        }

        internal void WeakAssignMemoryEntry(MemoryIndex alias, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }




        #endregion

        #region Destroy

        /// <summary>
        /// Kills memory on given index - removes from alias structure and destroy its memory entry if is not used
        /// </summary>
        /// <param name="index"></param>
        private void destroyMemoryIndex(MemoryIndex index)
        {
            MemoryInfo info = getMemoryInfo(index)
                .Builder()
                .RemoveMustAlias(index)
                .Build();

            //Rremove myself from MAY aliases
            foreach (MemoryIndex aliasIndex in info.MayAliasses)
            {
                MemoryInfo aliasInfo = getMemoryInfo(aliasIndex)
                    .Builder()
                    .RemoveMayAlias(index)
                    .Build();

                memoryInfos[aliasIndex] = aliasInfo;
            }

            if (info.MustAliasses.Count > 0)
            {
                //If there are some MUST aliases - just update infos
                foreach (MemoryIndex aliasIndex in info.MustAliasses)
                {
                    memoryInfos[aliasIndex] = info;
                }
            }
            else
            {
                //If there is not MUST alias - unlink
                destroyMemoryEntry(index);
            }

            memoryInfos.Remove(index);
            memoryEntries.Remove(index);
        }

        /// <summary>
        /// Kills all values which is sored in given memory entry
        /// </summary>
        /// <param name="index"></param>
        private void destroyMemoryEntry(MemoryIndex index)
        {
            MemoryEntry entry = getMemoryEntry(index);

            ValueVisitors.DestroyemoryEntryVisitor visitor = new ValueVisitors.DestroyemoryEntryVisitor(this, index);
            visitor.VisitMemoryEntry(entry);
        }

        /// <summary>
        /// Kills array value - destroys all array indexes
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        internal void DestroyArrayValue(MemoryIndex index, AssociativeArray value)
        {
            ArrayDescriptor descriptor = arrays[value];

            foreach (var content in descriptor.Indexes)
            {
                destroyMemoryIndex(content.Value);
            }

            arrays.Remove(value);
        }

        /// <summary>
        /// Kills object value reference - removes from alias reference structure and destroy all fields if is not used
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        internal void DestroyObjectValue(MemoryIndex index, ObjectValue value)
        {
            ObjectDescriptor descriptor = objects[value]
                .Builder()
                .removeMustReference(index)
                .Build();

            //Removes object from alias references
            foreach (ObjectValue aliasValue in descriptor.MayReferences)
            {
                ObjectDescriptor aliasDescriptor = objects[aliasValue]
                    .Builder()
                    .removeMayReference(value)
                    .Build();

                objects[aliasValue] = aliasDescriptor;
            }

            if (descriptor.MustReferences.Count == 0)
            {
                //There is no more reference
                foreach (var content in descriptor.Fields)
                {
                    destroyMemoryIndex(content.Value);
                }

                objects.Remove(value);
            }
            else
            {
                //Just update value descriptor
                objects[value] = descriptor;
            }
        }

        #endregion

        #region Alias

        /// <summary>
        /// Assigns the memory alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="alias">The alias.</param>
        private void assignMemoryAlias(MemoryIndex index, AliasValue alias)
        {
            MemoryAlias memoryAlias = alias as MemoryAlias;
            if (memoryAlias == null)
            {
                Debug.Assert(false, "Invalid alias type on assign alias");
            }

            MemoryInfo variableInfo = getMemoryInfo(index);
            MemoryInfo aliasGoupInfo = getMemoryInfo(memoryAlias.Index);

            //Is there alias between index and alias index?
            if (memoryAlias.Index != index && variableInfo != aliasGoupInfo)
            {
                //Destroys aliases
                clearMustAliases(index);
                clearMayAliases(index);

                destroyMemoryEntry(index);

                //Indexes conected in MUST alias relation shares memory space - INFO and ENTRY

                //Memory info update
                MemoryInfo newGroupInfo = aliasGoupInfo.Builder()
                    .AddMustAlias(index)
                    .Build();

                memoryInfos[index] = newGroupInfo;
                foreach (MemoryIndex aliasIndex in newGroupInfo.MustAliasses)
                {
                    memoryInfos[aliasIndex] = newGroupInfo;
                }

                //Shared memory entry
                MemoryEntry entry = getMemoryEntry(memoryAlias.Index);
                memoryEntries[index] = entry;
                ValueVisitors.AssignedAliasVisitor visitor = new ValueVisitors.AssignedAliasVisitor(this, index);
                visitor.VisitMemoryEntry(entry);

                //Update MAY aliases
                foreach (MemoryIndex aliasIndex in newGroupInfo.MayAliasses)
                {
                    memoryInfos[aliasIndex] = getMemoryInfo(aliasIndex)
                        .Builder()
                        .AddMayAlias(index)
                        .Build();
                }
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

        /// <summary>
        /// Clears the must aliases.
        /// </summary>
        /// <param name="index">The index.</param>
        private void clearMustAliases(MemoryIndex index)
        {
            MemoryInfo oldvariableInfo = getMemoryInfo(index);

            if (oldvariableInfo.MustAliasses.Count > 1)
            {
                //Removes from MUST group info
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

                //Removes from entry
                MemoryEntry entry = getMemoryEntry(index);
                ValueVisitors.RemoveAliasVisitor visitor = new ValueVisitors.RemoveAliasVisitor(this, index);
                visitor.VisitMemoryEntry(entry);

                memoryEntries[index] = emptyEntry;
            }
        }

        internal void AliasAssignedObjectValue(MemoryIndex index, ObjectValue value)
        {
            objects[value] = objects[value]
                .Builder()
                .addMustReference(index)
                .Build();
        }

        internal void AliasAssignedArrayValue(MemoryIndex index, AssociativeArray value)
        {
            arrays[value] = arrays[value]
                .Builder()
                .AddParentVariable(index)
                .Build();
        }

        internal void AliasRemovedArrayValue(MemoryIndex index, AssociativeArray value)
        {
            arrays[value] = arrays[value]
                .Builder()
                .RemoveParentVariable(index)
                .Build();
        }

        internal void AliasRemovedObjectValue(MemoryIndex index, ObjectValue value)
        {
            objects[value] = objects[value]
                .Builder()
                .removeMustReference(index)
                .Build();
        }

        #endregion


        protected override AliasValue createIndexAlias(AssociativeArray array, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        protected override AliasValue createFieldAlias(ObjectValue objectValue, ContainerIndex field)
        {
            throw new NotImplementedException();
        }
    }
}
