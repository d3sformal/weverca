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

        private Dictionary<VariableName, MemoryIndex> variables = new Dictionary<VariableName, MemoryIndex>();
        private Dictionary<AssociativeArray, ArrayDescriptor> arrays = new Dictionary<AssociativeArray, ArrayDescriptor>();
        private Dictionary<ObjectValue, ObjectDescriptor> objects = new Dictionary<ObjectValue, ObjectDescriptor>();

        private Dictionary<MemoryIndex, MemoryEntry> memoryEntries = new Dictionary<MemoryIndex, MemoryEntry>();
        private Dictionary<MemoryIndex, MemoryInfo> memoryInfos = new Dictionary<MemoryIndex, MemoryInfo>();


        private Dictionary<VariableName, MemoryIndex> variablesOld;
        private Dictionary<AssociativeArray, ArrayDescriptor> arraysOld;
        private Dictionary<ObjectValue, ObjectDescriptor> objectsOld;

        private Dictionary<MemoryIndex, MemoryEntry> memoryEntriesOld;
        private Dictionary<MemoryIndex, MemoryInfo> memoryInfosOld;




        #region AbstractSnapshot Implementation

        #region Transaction

        protected override void startTransaction()
        {
            variablesOld = variables;
            arraysOld = arrays;
            objectsOld = objects;

            memoryEntriesOld = memoryEntries;
            memoryInfosOld = memoryInfos;
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
            MemoryAlias memoryAlias = alias as MemoryAlias;

            if (memoryAlias != null)
            {
                MemoryIndex index = getOrCreateVariable(targetVar);
                assignMemoryAlias(index, memoryAlias);
            }
        }

        #endregion

        #region Fields

        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            ObjectDescriptor descriptor = new ObjectDescriptor(type.Declaration);
            objects[createdObject] = descriptor;
        }

        protected override MemoryEntry getField(ObjectValue value, ContainerIndex index)
        {
            ObjectDescriptor descriptor = objects[value];

            MemoryIndex memoryIndex = getFieldOrUnknown(descriptor, index);
            return getMemoryEntry(memoryIndex);
        }

        protected override void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            ObjectDescriptor descriptor = objects[value];

            MemoryIndex memoryIndex = getOrCreateField(descriptor, index);
            assignMemoryEntry(memoryIndex, entry);
        }

        protected override void setFieldAlias(ObjectValue value, ContainerIndex index, AliasValue alias)
        {
            MemoryAlias memoryAlias = alias as MemoryAlias;

            if (memoryAlias != null)
            {
                ObjectDescriptor descriptor = objects[value];

                MemoryIndex memoryIndex = getOrCreateField(descriptor, index);
                assignMemoryAlias(memoryIndex, memoryAlias);
            }
        }

        #endregion

        #region Indexes

        protected override void initializeArray(AssociativeArray createdArray)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry getIndex(AssociativeArray value, ContainerIndex index)
        {
            throw new NotImplementedException();
        }

        protected override void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }

        protected override void setIndexAlias(AssociativeArray value, ContainerIndex index, AliasValue alias)
        {
            throw new NotImplementedException();
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

        #endregion

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


        private void assignMemoryEntry(MemoryIndex index, MemoryEntry entry)
        {
            MemoryInfo info = memoryInfos[index];

            foreach (MemoryIndex mayAlias in info.MayAliasses)
            {
                weakAssignMemoryEntry(mayAlias, entry);
            }

            destroyMemoryEntry(index);

            ValueVisitors.AssignValueVisitor visitor = new ValueVisitors.AssignValueVisitor(index, info);
            visitor.VisitMemoryEntry(entry);
            MemoryEntry copiedEntry = visitor.GetCopiedEntry();

            foreach (MemoryIndex mustAlias in info.MustAliasses)
            {
                memoryEntries[mustAlias] = copiedEntry;
            }
        }

        private void weakAssignMemoryEntry(MemoryIndex mayAlias, MemoryEntry entry)
        {
            throw new NotImplementedException();
        }


        private void assignMemoryAlias(MemoryIndex index, MemoryAlias memoryAlias)
        {
            throw new NotImplementedException();
        }

        private MemoryIndex getFieldOrUnknown(ObjectDescriptor descriptor, ContainerIndex index)
        {
            throw new NotImplementedException();
        }
        private MemoryIndex getOrCreateField(ObjectDescriptor descriptor, ContainerIndex index)
        {
            throw new NotImplementedException();
        }



        private void destroyMemoryEntry(MemoryIndex index)
        {
            throw new NotImplementedException();
        }






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
        protected override IEnumerable<PHP.Core.AST.MethodDecl> resolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            throw new NotImplementedException();
        }
        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            throw new NotImplementedException();
        }
        private MemoryIndex createVariable(VariableName variable)
        {
            throw new NotImplementedException();
        }


        public override MemoryEntry ThisObject
        {
            get { throw new NotImplementedException(); }
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
    }
}
