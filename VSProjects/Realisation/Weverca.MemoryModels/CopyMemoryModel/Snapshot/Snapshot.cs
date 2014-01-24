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
    /// <summary>
    /// Implementation of the memory snapshot based on copy semantics.
    /// This implementation stores every possible value for the single memory location in one place.
    /// This approach guarantees write-read semantics - when user strongly writes to MAY alliased location
    /// he is still able to read only the strongly writed data without the ones from MAY target.
    /// Basic unit of the memory model is MemoryIndex which allows undirect links between aliases
    /// and collection indexes. Every index is just pointer into the memory location where the data is stored.
    /// So when the data is changed is not necessary to change every connected memory locations. See
    /// <see cref="Indexes" /> and <see cref="MemoryContainers" /> for more information. Data model itself is
    /// implemented in <see cref="SnapshotData" /> class.
    /// Algorithms for reading or modifying snapshots are splitted into two groups. Memory collectors represents
    /// algorithms to gathering indexes and memory workers provides implementation of read/write
    /// algorithm. For more informations see <see cref="IndexCollectors" /> and <see cref="MemoryWorkers" />.
    /// </summary>
    public class Snapshot : SnapshotBase, IReferenceHolder
    {
        private static int SNAP_ID = 0;
        private int snapId = SNAP_ID++;


        /// <summary>
        /// Identifier for the bottom of the call stack. At this level the global variables are stored.
        /// </summary>
        public static readonly int GLOBAL_CALL_LEVEL = 0;

        /// <summary>
        /// Gets the container with data of the snapshot.
        /// </summary>
        /// <value>
        /// The data container object.
        /// </value>
        internal SnapshotData Data { get; private set; }

        /// <summary>
        /// Gets the call level of snapshot. Contains GLOBAL_CALL_LEVEL value for the global code.
        /// Every call is incremented parent's call level.
        /// </summary>
        /// <value>
        /// The call level.
        /// </value>
        internal int CallLevel { get; private set; }

        /// <summary>
        /// Gets the this object for actual call level of the snapshot. Null for global code or function calls.
        /// </summary>
        /// <value>
        /// The this object.
        /// </value>
        internal MemoryEntry ThisObject { get; private set; }

        /// <summary>
        /// Snapshot where the call was made. CallLevel value of this is incremented value of callerContext.
        /// Null for global code.
        /// </summary>
        Snapshot callerContext;

        /// <summary>
        /// Backup of the snapshot data after the start of transaction.
        /// </summary>
        SnapshotData oldData = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot"/> class. Sets the call information for global code
        /// and initializes empty data.
        /// </summary>
        public Snapshot()
        {
            callerContext = null;
            ThisObject = null;
            CallLevel = GLOBAL_CALL_LEVEL;

            Data = new SnapshotData(this);
        }

        /// <summary>
        /// Creates the string representation of the data.
        /// </summary>
        public String DumpSnapshot()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(snapId.ToString() + "::"+ Data.ToString() +"\n");
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

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return DumpSnapshot();
        }

        /// <summary>
        /// Implementation of BaseSnapshot class.
        /// See <see cref="SnapshotBase"/> for more informations. 
        /// </summary>
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
            return Data.WidenNotEqual(oldData, Assistant);
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
            TemporaryIndex arrayIndex = CreateTemporary();

            ArrayDescriptor descriptor = new ArrayDescriptor(createdArray, arrayIndex);
            Data.NewIndex(descriptor.UnknownIndex);
            Data.SetDescriptor(createdArray, descriptor);
            Data.SetMemoryEntry(arrayIndex, new MemoryEntry(createdArray));
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
            if (Data.IsFunctionDefined(functionName))
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

        #region OBSOLETE

        //OBSOLETE
        protected override AliasValue createAlias(VariableName sourceVar)
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
        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<FunctionValue> resolveStaticMethod(TypeValue value, QualifiedName methodName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Snapshot Logic Implementation

        #region Variables

        #region Named variables

        /// <summary>
        /// Creates the local variable. Stack level of the local variable is equal to CallLevel.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>Index of newly created variable.</returns>
        internal MemoryIndex CreateLocalVariable(string variableName)
        {
            MemoryIndex variableIndex = VariableIndex.Create(variableName, CallLevel);

            Data.NewIndex(variableIndex);
            Data.Variables.Local.Indexes.Add(variableName, variableIndex);

            CopyMemory(Data.Variables.Local.UnknownIndex, variableIndex, false);

            return variableIndex;
        }

        /// <summary>
        /// Creates the global variable. Stack level of the local variable is equal to GLOBAL_CALL_LEVEL value.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>Index of newly created variable.</returns>
        internal MemoryIndex CreateGlobalVariable(string variableName)
        {
            MemoryIndex variableIndex = VariableIndex.Create(variableName, GLOBAL_CALL_LEVEL);

            Data.NewIndex(variableIndex);
            Data.Variables.Global.Indexes.Add(variableName, variableIndex);

            CopyMemory(Data.Variables.Global.UnknownIndex, variableIndex, false);

            return variableIndex;
        }

        #endregion

        #region Controll Variables

        /// <summary>
        /// Creates the local controll variable. Stack level of the local variable is equal to CallLevel.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>Index of newly created variable.</returns>
        internal MemoryIndex CreateLocalControll(string variableName)
        {
            MemoryIndex ctrlIndex = ControlIndex.Create(variableName, CallLevel);

            Data.NewIndex(ctrlIndex);
            Data.ContolVariables.Local.Indexes.Add(variableName, ctrlIndex);

            CopyMemory(Data.Variables.Local.UnknownIndex, ctrlIndex, false);

            return ctrlIndex;
        }

        /// <summary>
        /// Creates the global controll variable. Stack level of the local variable is equal to GLOBAL_CALL_LEVEL value.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>Index of newly created variable.</returns>
        internal MemoryIndex CreateGlobalControll(string variableName)
        {
            MemoryIndex ctrlIndex = ControlIndex.Create(variableName, CallLevel);

            Data.NewIndex(ctrlIndex);
            Data.ContolVariables.Global.Indexes.Add(variableName, ctrlIndex);

            CopyMemory(Data.Variables.Global.UnknownIndex, ctrlIndex, false);

            return ctrlIndex;
        }

        #endregion

        #region Temporary Variables

        /// <summary>
        /// Determines whether a temporary variable is set for the specified temporary index.
        /// </summary>
        /// <param name="temporaryIndex">Index of the temporary variable.</param>
        internal bool IsTemporarySet(TemporaryIndex temporaryIndex)
        {
            return Data.Temporary.Local.Contains(temporaryIndex);
        }

        /// <summary>
        /// Creates the temporary variable.
        /// </summary>
        /// <returns>Index of newly created variable.</returns>
        internal TemporaryIndex CreateTemporary()
        {
            TemporaryIndex tmp = new TemporaryIndex(CallLevel);
            Data.NewIndex(tmp);
            Data.Temporary.Local.Add(tmp);
            return tmp;
        }

        /// <summary>
        /// Releases the temporary variable and clears the memory.
        /// </summary>
        /// <param name="temporaryIndex">Index of the temporary variable.</param>
        internal void ReleaseTemporary(TemporaryIndex temporaryIndex)
        {
            ReleaseMemory(temporaryIndex);
            Data.Temporary.Local.Remove(temporaryIndex);
        }

        #endregion

        #endregion

        #region Objects

        /// <summary>
        /// Determines whether the specified index has must reference to some objects.
        /// </summary>
        /// <param name="index">The index.</param>
        internal bool HasMustReference(MemoryIndex index)
        {
            MemoryEntry entry = Data.GetMemoryEntry(index);

            if (entry.Count == 1)
            {
                ObjectValueContainer objects = Data.GetObjects(index);
                if (objects.Count == 1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified index contains only references.
        /// </summary>
        /// <param name="index">The index.</param>
        internal bool ContainsOnlyReferences(MemoryIndex index)
        {
            MemoryEntry entry = Data.GetMemoryEntry(index);
            ObjectValueContainer objects = Data.GetObjects(index);

            return entry.Count == objects.Count;
        }

        /// <summary>
        /// Makes the must reference to the given object in the target index.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="targetIndex">Index of the target.</param>
        internal void MakeMustReferenceObject(ObjectValue objectValue, MemoryIndex targetIndex)
        {
            ObjectValueContainerBuilder objects = Data.GetObjects(targetIndex).Builder();
            objects.Add(objectValue);
            Data.SetObjects(targetIndex, objects.Build());
        }

        /// <summary>
        /// Makes the may reference object to the given object in the target index.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <param name="targetIndex">Index of the target.</param>
        internal void MakeMayReferenceObject(HashSet<ObjectValue> objects, MemoryIndex targetIndex)
        {
            ObjectValueContainerBuilder objectsContainer = Data.GetObjects(targetIndex).Builder();

            foreach (ObjectValue objectValue in objects)
            {
                objectsContainer.Add(objectValue);
            }

            Data.SetObjects(targetIndex, objectsContainer.Build());
        }

        /// <summary>
        /// Creates the new instance of the default object in given parent index.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="isMust">if set to <c>true</c> object is must and values in parent indexes are removed.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates new field in the given object.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="objectValue">The object value.</param>
        /// <param name="isMust">if set to <c>true</c> value must be created.</param>
        /// <param name="copyFromUnknown">if set to <c>true</c> value of the field is initialized
        /// from the unknown field of object.</param>
        /// <returns>Memory index of the newly created field.</returns>
        internal MemoryIndex CreateField(string fieldName, ObjectValue objectValue, bool isMust, bool copyFromUnknown)
        {
            return CreateField(fieldName, Data.GetDescriptor(objectValue), isMust, copyFromUnknown);
        }

        /// <summary>
        /// Creates new field in the given object.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="descriptor">The descriptor of object.</param>
        /// <param name="isMust">if set to <c>true</c> value must be created.</param>
        /// <param name="copyFromUnknown">if set to <c>true</c> value of the field is initialized
        /// from the unknown field of object.</param>
        /// <returns>Memory index of the newly created field.</returns>
        /// <exception cref="System.Exception">Field with given name is already defined</exception>
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

        /// <summary>
        /// Removes all values from the given memory object except object values.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void ClearForObjects(MemoryIndex index)
        {
            DestroyArrayVisitor visitor = new DestroyArrayVisitor(this, index);
            visitor.VisitMemoryEntry(Data.GetMemoryEntry(index));

            ObjectValueContainer objects = Data.GetObjects(index);
            MemoryEntry entry = new MemoryEntry(objects);

            Data.SetMemoryEntry(index, entry);
        }

        /// <summary>
        /// Destroys the object.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="value">The value.</param>
        internal void DestroyObject(MemoryIndex parentIndex, ObjectValue value)
        {
            ObjectValueContainerBuilder objects = Data.GetObjects(parentIndex).Builder();
            objects.Remove(value);
            Data.SetObjects(parentIndex, objects.Build());
        }

        #endregion

        #region Arrays

        /// <summary>
        /// Creates new array in given parent index.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <returns>Memory index of newly created array index.</returns>
        /// <exception cref="System.Exception">Variable  with given index already has associated array value.</exception>
        internal AssociativeArray CreateArray(MemoryIndex parentIndex)
        {
            if (Data.HasArray(parentIndex))
            {
                throw new Exception("Variable " + parentIndex + " already has associated array value.");
            }

            AssociativeArray value = this.CreateArray();
            ArrayDescriptor oldDescriptor = Data.GetDescriptor(value);
            closeTemporaryArray(oldDescriptor);
            
            ArrayDescriptor newDescriptor = oldDescriptor
                .Builder()
                .SetParentVariable(parentIndex)
                .SetUnknownField(parentIndex.CreateUnknownIndex())
                .Build();

            Data.NewIndex(newDescriptor.UnknownIndex);
            Data.SetDescriptor(value, newDescriptor);
            Data.SetArray(parentIndex, value);
            return value;
        }

        private void closeTemporaryArray(ArrayDescriptor descriptor)
        {
            TemporaryIndex index = descriptor.ParentVariable as TemporaryIndex;

            if (index != null)
            {
                Data.SetMemoryEntry(index, new MemoryEntry());
                ReleaseTemporary(index);
            }

            ReleaseMemory(descriptor.UnknownIndex);
        }

        /// <summary>
        /// Creates new array in given parent index.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="isMust">if set to <c>true</c> is must and values in parent indexes are removed.</param>
        /// <returns>Memory index of newly created array index.</returns>
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

        /// <summary>
        /// Creates new index in the given array.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="arrayValue">The array value.</param>
        /// <param name="isMust">if set to <c>true</c> value must be created.</param>
        /// <param name="copyFromUnknown">if set to <c>true</c> value of the field is initialized
        /// <returns>Memory index of the newly created array index.</returns>
        internal MemoryIndex CreateIndex(string indexName, AssociativeArray arrayValue, bool isMust, bool copyFromUnknown)
        {
            return CreateIndex(indexName, Data.GetDescriptor(arrayValue), isMust, copyFromUnknown);
        }

        /// <summary>
        /// Creates new index in the given array.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="descriptor">The descriptor of array.</param>
        /// <param name="isMust">if set to <c>true</c> value must be created.</param>
        /// <param name="copyFromUnknown">if set to <c>true</c> value of the field is initialized
        /// <returns>Memory index of the newly created array index.</returns>
        /// <exception cref="System.Exception">Index  with given name is already defined</exception>
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

        /// <summary>
        /// Removes all values from the given memory object except array values.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void ClearForArray(MemoryIndex index)
        {
            DestroyObjectsVisitor visitor = new DestroyObjectsVisitor(this, index);
            visitor.VisitMemoryEntry(Data.GetMemoryEntry(index));

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

        /// <summary>
        /// Destroys the array.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
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

        #endregion

        #region Memory

        /// <summary>
        /// Determines whether the specified index contains undefined value.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        internal bool IsUndefined(MemoryIndex index)
        {
            MemoryEntry entry = Data.GetMemoryEntry(index);
            return entry.PossibleValues.Contains(this.UndefinedValue);
        }

        /// <summary>
        /// Copies the memory from the source index onto the target index.
        /// </summary>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="isMust">if set to <c>true</c> copy is must othervise undefined value is added into the whole memory tree.</param>
        private void CopyMemory(MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust)
        {
            CopyWithinSnapshotWorker worker = new CopyWithinSnapshotWorker(this, isMust);
            worker.Copy(sourceIndex, targetIndex);
        }

        /// <summary>
        /// Destroys the memory of given index and sets the memory of given index to undefined.
        /// </summary>
        /// <param name="index">The index.</param>
        public void DestroyMemory(MemoryIndex index)
        {
            DestroyMemoryVisitor visitor = new DestroyMemoryVisitor(this, index);

            MemoryEntry entry;
            if (Data.TryGetMemoryEntry(index, out entry))
            {
                visitor.VisitMemoryEntry(entry);
            }
            Data.SetMemoryEntry(index, new MemoryEntry(this.UndefinedValue));
        }

        /// <summary>
        /// Releases the memory in the given index and removes it from the data model.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void ReleaseMemory(MemoryIndex index)
        {
            DestroyMemory(index);
            DestroyAliases(index);

            Data.RemoveIndex(index);
        }
        
        /// <summary>
        /// Extends the snapshot.
        /// </summary>
        /// <param name="input">The input.</param>
        private void extendSnapshot(ISnapshotReadonly input)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(input);

            this.callerContext = snapshot.callerContext;
            CallLevel = snapshot.CallLevel;
            ThisObject = snapshot.ThisObject;

            Data = new SnapshotData(this, snapshot.Data);
        }

        /// <summary>
        /// Merges the snapshots.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <exception cref="System.Exception">
        /// Merged snapshots don't have the same call context.
        /// or
        /// Merged snapshots don't have the same call level.
        /// or
        /// Merged snapshots don't have the same this object value.
        /// </exception>
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

        #endregion

        #region Aliases

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAliases">The must aliases.</param>
        /// <param name="mayAliases">The may aliases.</param>
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

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAlias">The must alias.</param>
        /// <param name="mayAlias">The may alias.</param>
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

        /// <summary>
        /// Must the set aliases of the given index operation. Clears old alias entry if is set and set new alias to given index.
        /// Alias entry of the given alias indexes are not changed.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAliases">The must aliases.</param>
        /// <param name="mayAliases">The may aliases.</param>
        public void MustSetAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            DestroyAliases(index);

            MemoryAliasBuilder builder = new MemoryAliasBuilder();
            builder.AddMustAlias(mustAliases);
            builder.AddMayAlias(mayAliases);

            Data.SetAlias(index, builder.Build());
        }

        /// <summary>
        /// May the set aliases of the given index operation. All must aliases of given index are converted into may.
        /// Alias entry of the given alias indexes are not changed.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mayAliases">The may aliases.</param>
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

        /// <summary>
        /// Converts the aliases of given index to may.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="builder">The builder.</param>
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

        /// <summary>
        /// Copies the aliases of the source index to target.
        /// </summary>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="isMust">if set to <c>true</c> operation is must otherwise all must aliases are copied as may.</param>
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

                Data.SetAlias(targetIndex, builder.Build());
            }
        }

        /// <summary>
        /// Destroys the aliases of the given index.
        /// </summary>
        /// <param name="index">The index.</param>
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

        #endregion

        #endregion
    }
}
