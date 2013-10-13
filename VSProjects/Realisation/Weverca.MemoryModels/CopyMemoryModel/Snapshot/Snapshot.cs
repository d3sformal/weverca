using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class Snapshot : SnapshotBase
    {
        HashSet<MemoryIndex> memoryIndexes = new HashSet<MemoryIndex>();

        internal Dictionary<VariableName, MemoryIndex> variables = new Dictionary<VariableName, MemoryIndex>();

        internal Dictionary<AssociativeArray, ArrayDescriptor> arrays = new Dictionary<AssociativeArray, ArrayDescriptor>();
        internal Dictionary<ObjectValue, ObjectDescriptor> objects = new Dictionary<ObjectValue, ObjectDescriptor>();

        internal Dictionary<Value, MemoryInfo> memoryEntries = new Dictionary<Value, MemoryInfo>();

        internal Dictionary<MemoryIndex, MemoryEntry> memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>();
        internal Dictionary<MemoryIndex, MemoryAlias> memoryEntries = new Dictionary<MemoryIndex, MemoryAlias>();
        internal Dictionary<MemoryIndex, MemoryInfo> memoryEntries = new Dictionary<MemoryIndex, MemoryInfo>();


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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
        protected override void assignAlias(VariableName targetVar, IEnumerable<AliasValue> aliases)
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
        protected override void setFieldAlias(ObjectValue value, ContainerIndex index, IEnumerable<AliasValue> aliases)
        {
            throw new NotImplementedException();
        }

        //OBSOLETE
        protected override void setIndexAlias(AssociativeArray value, ContainerIndex index, IEnumerable<AliasValue> aliases)
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
    }
}
