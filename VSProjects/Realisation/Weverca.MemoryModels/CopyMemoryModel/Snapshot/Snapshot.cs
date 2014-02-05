using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using PHP.Core.AST;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class Logger
    {
        static readonly string logFile = @"C:\Users\Pavel\Desktop\weverca_log.txt";

        static Snapshot oldOne = null;

        static Logger() {
            System.IO.File.Delete(logFile);
        }

        public static void append(string message)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + message);
            }
        }

        public static void append(Snapshot snapshot)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n\r\n");
                w.WriteLine(snapshot.DumpSnapshotSimplified());
                w.WriteLine("-------------------------------");
            }

            oldOne = snapshot;
        }

        public static void append(Snapshot snapshot, String message)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write("\r\n" + snapshot.getSnapshotIdentification() + " > " + message);
            }
        }

        public static void append(SnapshotBase snapshotBase, String message)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(snapshotBase);
            append(snapshot, message);
        }

        public static void appendToSameLine(String message)
        {
            using (System.IO.StreamWriter w = System.IO.File.AppendText(logFile))
            {
                w.Write(message);
            }
        }
    }


    /// <summary>
    /// Implementation of the memory snapshot based on copy semantics.
    /// This implementation stores every possible value for the single memory location in one place.
    /// This approach guarantees write-read semantics - when user strongly writes to MAY alliased location
    /// he is still able to read only the strongly writed data without the ones from MAY target.
    /// Basic unit of the memory model is MemoryIndex which allows undirect links between aliases
    /// and collection indexes. Every index is just pointer into the memory location where the data is stored.
    /// So when the data is changed is not necessary to change every connected memory locations. See
    /// <see cref="Indexes" /> and <see cref="MemoryContainers" /> for more information. Data model itself is
    /// implemented in <see cref="SnapshotStructure" /> class.
    /// Algorithms for reading or modifying snapshots are splitted into two groups. Memory collectors represents
    /// algorithms to gathering indexes and memory workers provides implementation of read/write
    /// algorithm. For more informations see <see cref="IndexCollectors" /> and <see cref="MemoryWorkers" />.
    /// </summary>
    public class Snapshot : SnapshotBase, IReferenceHolder
    {
        private static int SNAP_ID = 0;
        private int snapId = SNAP_ID++;

        public static readonly string THIS_VARIABLE_IDENTIFIER = "this";
        public static readonly string RETURN_VARIABLE_IDENTIFIER = ".return";


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
        internal SnapshotStructure Structure { get; private set; }

        internal SnapshotData Data { get; private set; }
        internal SnapshotData Infos { get; private set; }

        public override SnapshotMode CurrentMode
        {
            get
            {
                return base.CurrentMode;
            }
            protected set
            {
                base.CurrentMode = value;

                switch (value)
                {
                    case SnapshotMode.MemoryLevel:
                        Structure.Data = Data;
                        Structure.Locked = false;
                        break;

                    case SnapshotMode.InfoLevel:
                        Structure.Data = Infos;
                        Structure.Locked = true;
                        break;

                    default:
                        throw new NotSupportedException("Current mode: " + CurrentMode);
                }
            }
        }

        /// <summary>
        /// Gets the call level of snapshot. Contains GLOBAL_CALL_LEVEL value for the global code.
        /// Every call is incremented parent's call level.
        /// </summary>
        /// <value>
        /// The call level.
        /// </value>
        internal int CallLevel { get; private set; }

        private TemporaryIndex returnIndex;

        /// <summary>
        /// Backup of the snapshot data after the start of transaction.
        /// </summary>
        SnapshotStructure oldStructure = null;
        SnapshotData oldData = null;
        SnapshotData oldInfos = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot"/> class. Sets the call information for global code
        /// and initializes empty data.
        /// </summary>
        public Snapshot()
        {
            CallLevel = GLOBAL_CALL_LEVEL;

            Data = SnapshotData.CreateEmpty(this);
            Infos = SnapshotData.CreateEmpty(this);
            Structure = SnapshotStructure.CreateGlobal(this, Data);
            Logger.append(this, "Constructed snapshot");
        }

        /// <summary>
        /// Creates the string representation of the data.
        /// </summary>
        public String DumpSnapshot()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(snapId.ToString() + "::" + Structure.DataId.ToString() + "\n");
            foreach (var index in Structure.IndexData)
            {
                builder.Append(index.ToString());
                builder.Append("\n");

                MemoryEntry entry;
                if (Data.TryGetMemoryEntry(index.Key, out entry))
                {
                    builder.Append(entry.ToString());
                }
                else
                {
                    builder.Append("MISSING ENTRY");
                }

                if (Infos.TryGetMemoryEntry(index.Key, out entry))
                {
                    builder.Append("\n  Info: ");
                    builder.Append(entry.ToString());
                }
                

                if (index.Value.Aliases != null)
                {
                    index.Value.Aliases.ToString(builder);
                }

                builder.Append("\n\n");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates the string representation of the data.
        /// </summary>
        public String DumpSnapshotSimplified()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(snapId.ToString() + "::" + Structure.DataId.ToString() + "\n");
            foreach (var index in Structure.IndexData)
            {
                if (index.Key is TemporaryIndex)
                {
                    continue;
                }

                builder.Append(index.Key.ToString());
                builder.Append("\t");
                MemoryEntry entry;
                if (Data.TryGetMemoryEntry(index.Key, out entry))
                {
                    builder.Append(entry.ToString());
                }
                else
                {
                    builder.Append("MISSING ENTRY");
                }

                if (Infos.TryGetMemoryEntry(index.Key, out entry))
                {
                    builder.Append("\t  Info: ");
                    builder.Append(entry.ToString());
                }

                if (index.Value.Aliases != null)
                {
                    index.Value.Aliases.ToString(builder);
                }

                builder.Append("\n");
            }

            return builder.ToString();
        }

        public String getSnapshotIdentification()
        {
            return snapId.ToString() + "::" + Structure.DataId.ToString();
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
            Logger.append(this, "Start mode: " + CurrentMode.ToString());

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    startDataTransaction();
                    break;

                case SnapshotMode.InfoLevel:
                    startInfoTransaction();
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }

        }

        private void startDataTransaction()
        {
            oldStructure = Structure;
            oldData = Data;

            Data = oldData.Copy(this);
            Structure = oldStructure.Copy(this, Data);

            Structure.Data = Data;
            Structure.Locked = false;
            oldStructure.Data = oldData;
        }

        private void startInfoTransaction()
        {
            oldInfos = Infos;
            Infos = oldInfos.Copy(this);

            Structure.Data = Infos;
            Structure.Locked = true;
            oldStructure.Data = oldInfos;
        }

        protected override bool commitTransaction()
        {
            Logger.append(this, "Commit " + Structure.DataEquals(oldStructure));
            Logger.append(this);
            
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    return !Structure.DataEquals(oldStructure);

                case SnapshotMode.InfoLevel:
                    return !Infos.DataEquals(oldInfos);

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }
        }

        protected override bool widenAndCommitTransaction()
        {
            Logger.append(this, "Commit and widen");
            bool result = !Structure.WidenNotEqual(oldStructure, Assistant);
            Logger.appendToSameLine(" " + result);
            Logger.append(this);

            return result;
        }

        #endregion

        #region Objects

        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            Logger.append(this, "Init object " + createdObject + " " + type);
            ObjectDescriptor descriptor = new ObjectDescriptor(createdObject, type, ObjectIndex.CreateUnknown(createdObject));
            Structure.NewIndex(descriptor.UnknownIndex);
            Structure.SetDescriptor(createdObject, descriptor);
        }

        protected IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            Logger.append(this, "Iterate object " + iteratedObject);
            ObjectDescriptor descriptor;
            if (Structure.TryGetDescriptor(iteratedObject, out descriptor))
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
            Logger.append(this, "Get object type " + objectValue);
            ObjectDescriptor descriptor;
            if (Structure.TryGetDescriptor(objectValue, out descriptor))
            {
                return descriptor.Type;
            }
            else
            {
                throw new Exception("Unknown object");
            }
        }



        protected override IEnumerable<FunctionValue> resolveStaticMethod(TypeValue value, QualifiedName methodName)
        {
            throw new NotImplementedException("Deprecated - should not be used in analysis");
        }

        internal IEnumerable<FunctionValue> resolveMethod(MemoryEntry entry, QualifiedName methodName)
        {
            HashSet<FunctionValue> functions = new HashSet<FunctionValue>();
            foreach (Value value in entry.PossibleValues)
            {
                HashSetTools.AddAll(functions, resolveMethod(value, methodName));
            }

            return functions;
        }

        internal IEnumerable<FunctionValue> resolveMethod(Value value, QualifiedName methodName)
        {
            IEnumerable<FunctionValue> objectMethods;
            TypeValue type;

            ObjectValue objectValue = value as ObjectValue;
            if (objectValue == null)
            {
                type = null;
                objectMethods = new FunctionValue[0];
            }
            else
            {
                type = objectType(objectValue);
                objectMethods = Weverca.MemoryModels.VirtualReferenceModel.TypeMethodResolver.ResolveMethods(type, this);
            }

            var resolvedMethods = Assistant.ResolveMethods(objectValue, type, methodName, objectMethods);

            return resolvedMethods;
        }

        #endregion

        #region Arrays

        protected override void initializeArray(AssociativeArray createdArray)
        {
            Logger.append(this, "Init array " + createdArray);
            TemporaryIndex arrayIndex = CreateTemporary();

            ArrayDescriptor descriptor = new ArrayDescriptor(createdArray, arrayIndex);
            Structure.NewIndex(descriptor.UnknownIndex);
            Structure.SetArray(arrayIndex, createdArray);
            Structure.SetDescriptor(createdArray, descriptor);
            Structure.SetMemoryEntry(arrayIndex, new MemoryEntry(createdArray));
        }

        protected IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            Logger.append(this, "Iterate array " + iteratedArray);
            ArrayDescriptor descriptor;
            if (Structure.TryGetDescriptor(iteratedArray, out descriptor))
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
            Logger.append(this, "extend");

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
            Logger.append(this, "call extend: " + snapshot.getSnapshotIdentification() + " level: " + snapshot.CallLevel + " this: " + thisObject);

            CallLevel = snapshot.CallLevel + 1;

            Data = snapshot.Data.Copy(this);
            Infos = snapshot.Infos.Copy(this);
            Structure = snapshot.Structure.CopyAndAddLocalLevel(this, Data);

            if (thisObject != null)
            {
                ReadWriteSnapshotEntryBase snapshotEntry = SnapshotEntry.CreateVariableEntry(new VariableIdentifier(THIS_VARIABLE_IDENTIFIER), GlobalContext.LocalOnly, CallLevel);
                snapshotEntry.WriteMemory(this, thisObject);
            }

            int level = CallLevel > GLOBAL_CALL_LEVEL ? CallLevel - 1 : GLOBAL_CALL_LEVEL;
            returnIndex = new TemporaryIndex(level);
            Structure.NewIndex(returnIndex);
            Structure.Temporary[level].Add(returnIndex);
        }

        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutputs)
        {
            Logger.append(this, "call merge");
            List<Snapshot> snapshots = new List<Snapshot>(callOutputs.Length);
            foreach (ISnapshotReadonly input in callOutputs)
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(input);
                Logger.append(this, snapshot.getSnapshotIdentification() + " call merge " + snapshot.CallLevel);
                snapshots.Add(snapshot);
            }

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    MergeWorker worker = new MergeWorker(this, snapshots, true);
                    worker.Merge();

                    Structure = worker.Structure;
                    Data = worker.Data;
                    break;

                case SnapshotMode.InfoLevel:
                    throw new NotImplementedException();
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }
        }

        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            Logger.append(this, "Fetch from global");
            foreach (VariableName name in variables)
            {
                Logger.append(this, "Fetch from global " + name);
                ReadWriteSnapshotEntryBase localEntry = getVariable(new VariableIdentifier(name), false);
                ReadWriteSnapshotEntryBase globalEntry = getVariable(new VariableIdentifier(name), true);

                localEntry.SetAliases(this, globalEntry);
            }
        }

        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            Logger.append(this, "Get global ");

            List<VariableName> names = new List<VariableName>();

            foreach (var variable in Structure.Variables.Global.Indexes)
            {
                names.Add(new VariableName(variable.Key));
            }

            return names;
        }

        #endregion

        #region Infos

        protected override void setInfo(Value value, params InfoValue[] info)
        {
            throw new NotImplementedException("Info values - waiting for final approach");
        }

        protected override void setInfo(VariableName variable, params InfoValue[] info)
        {
            throw new NotImplementedException("Info values - waiting for final approach");
        }

        protected override InfoValue[] readInfo(Value value)
        {
            throw new NotImplementedException("Info values - waiting for final approach");
        }

        protected override InfoValue[] readInfo(VariableName variable)
        {
            throw new NotImplementedException("Info values - waiting for final approach");
        }

        #endregion

        #region Functions and Classes

        protected override IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName)
        {
            Logger.append(this, "Resolve function " + functionName);
            if (Structure.IsFunctionDefined(functionName))
            {
                return Structure.GetFunction(functionName);
            }
            else
            {
                return new List<FunctionValue>(0);
            }
        }

        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            Logger.append(this, "Resolve type " + typeName);
            if (Structure.IsClassDefined(typeName))
            {
                return Structure.GetClass(typeName);
            }
            else
            {
                return new List<TypeValue>(0);
            }
        }

        protected override void declareGlobal(FunctionValue declaration)
        {
            QualifiedName name = new QualifiedName(declaration.Name);
            Logger.append(this, "Declare function - " + name);

            if (!Structure.IsFunctionDefined(name))
            {
                Structure.SetFunction(name, declaration);
            }
            else
            {
                Structure.SetFunction(name, declaration);
            }
        }

        protected override void declareGlobal(TypeValue declaration)
        {
            QualifiedName name = declaration.QualifiedName;
            Logger.append(this, "Declare class - " + name);

            if (!Structure.IsClassDefined(name))
            {
                Structure.SetClass(name, declaration);
            }
            else
            {
                Structure.SetClass(name, declaration);
            }
        }

        #endregion

        #region Snapshot Entry API

        protected override ReadWriteSnapshotEntryBase getVariable(VariableIdentifier variable, bool forceGlobalContext)
        {
            Logger.append(this, "Get variable - " + variable);
            if (forceGlobalContext)
            {
                return SnapshotEntry.CreateVariableEntry(variable, GlobalContext.GlobalOnly);
            }
            else
            {
                return SnapshotEntry.CreateVariableEntry(variable, GlobalContext.LocalOnly, CallLevel);
            }
        }

        protected override ReadWriteSnapshotEntryBase getControlVariable(VariableName name)
        {
            Logger.append(this, "Get control variable - " + name);
            return SnapshotEntry.CreateControlEntry(name, GlobalContext.GlobalOnly);
        }

        protected override ReadWriteSnapshotEntryBase createSnapshotEntry(MemoryEntry entry)
        {
            Logger.append(this, "Get entry snap - " + entry);
            return new DataSnapshotEntry(this, entry);
        }

        protected override ReadWriteSnapshotEntryBase getLocalControlVariable(VariableName name)
        {
            Logger.append(this, "Get local control variable - " + name);
            return SnapshotEntry.CreateControlEntry(name, GlobalContext.LocalOnly, CallLevel);
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
        /// <param name="callLevel">The call level of variable.</param>
        /// <returns>
        /// Index of newly created variable.
        /// </returns>
        internal MemoryIndex CreateLocalVariable(string variableName, int callLevel)
        {
            MemoryIndex variableIndex = VariableIndex.Create(variableName, callLevel);

            Structure.NewIndex(variableIndex);
            Structure.Variables[callLevel].Indexes.Add(variableName, variableIndex);

            CopyMemory(Structure.Variables[callLevel].UnknownIndex, variableIndex, false);

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

            Structure.NewIndex(variableIndex);
            Structure.Variables.Global.Indexes.Add(variableName, variableIndex);

            CopyMemory(Structure.Variables.Global.UnknownIndex, variableIndex, false);

            return variableIndex;
        }

        #endregion

        #region Controll Variables

        /// <summary>
        /// Creates the local controll variable. Stack level of the local variable is equal to CallLevel.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="callLevel">The call level of variable.</param>
        /// <returns>
        /// Index of newly created variable.
        /// </returns>
        internal MemoryIndex CreateLocalControll(string variableName, int callLevel)
        {
            MemoryIndex ctrlIndex = ControlIndex.Create(variableName, callLevel);

            Structure.NewIndex(ctrlIndex);
            Structure.ContolVariables[callLevel].Indexes.Add(variableName, ctrlIndex);

            CopyMemory(Structure.ContolVariables[callLevel].UnknownIndex, ctrlIndex, false);

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

            Structure.NewIndex(ctrlIndex);
            Structure.ContolVariables.Global.Indexes.Add(variableName, ctrlIndex);

            CopyMemory(Structure.Variables.Global.UnknownIndex, ctrlIndex, false);

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
            return Structure.Temporary.Local.Contains(temporaryIndex);
        }

        /// <summary>
        /// Creates the temporary variable.
        /// </summary>
        /// <returns>Index of newly created variable.</returns>
        internal TemporaryIndex CreateTemporary()
        {
            TemporaryIndex tmp = new TemporaryIndex(CallLevel);
            Structure.NewIndex(tmp);
            Structure.Temporary.Local.Add(tmp);
            return tmp;
        }

        /// <summary>
        /// Releases the temporary variable and clears the memory.
        /// </summary>
        /// <param name="temporaryIndex">Index of the temporary variable.</param>
        internal void ReleaseTemporary(TemporaryIndex temporaryIndex)
        {
            ReleaseMemory(temporaryIndex);
            Structure.Temporary.Local.Remove(temporaryIndex);
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
            MemoryEntry entry = Structure.GetMemoryEntry(index);

            if (entry.Count == 1)
            {
                ObjectValueContainer objects = Structure.GetObjects(index);
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
            MemoryEntry entry = Structure.GetMemoryEntry(index);
            ObjectValueContainer objects = Structure.GetObjects(index);

            return entry.Count == objects.Count;
        }

        /// <summary>
        /// Makes the must reference to the given object in the target index.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="targetIndex">Index of the target.</param>
        internal void MakeMustReferenceObject(ObjectValue objectValue, MemoryIndex targetIndex)
        {
            ObjectValueContainerBuilder objects = Structure.GetObjects(targetIndex).Builder();
            objects.Add(objectValue);
            Structure.SetObjects(targetIndex, objects.Build());
        }

        /// <summary>
        /// Makes the may reference object to the given object in the target index.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <param name="targetIndex">Index of the target.</param>
        internal void MakeMayReferenceObject(HashSet<ObjectValue> objects, MemoryIndex targetIndex)
        {
            ObjectValueContainerBuilder objectsContainer = Structure.GetObjects(targetIndex).Builder();

            foreach (ObjectValue objectValue in objects)
            {
                objectsContainer.Add(objectValue);
            }

            Structure.SetObjects(targetIndex, objectsContainer.Build());
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
                Structure.SetMemoryEntry(parentIndex, new MemoryEntry(value));
            }
            else
            {
                MemoryEntry oldEntry;

                List<Value> values;
                if (Structure.TryGetMemoryEntry(parentIndex, out oldEntry))
                {
                    values = new List<Value>(oldEntry.PossibleValues);
                }
                else
                {
                    values = new List<Value>();
                }

                values.Add(value);
                Structure.SetMemoryEntry(parentIndex, new MemoryEntry(values));
            }

            ObjectValueContainerBuilder objectValues = Structure.GetObjects(parentIndex).Builder();
            objectValues.Add(value);

            Structure.SetObjects(parentIndex, objectValues.Build());
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
            return CreateField(fieldName, Structure.GetDescriptor(objectValue), isMust, copyFromUnknown);
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
            Structure.NewIndex(fieldIndex);

            descriptor = descriptor.Builder()
                .add(fieldName, fieldIndex)
                .Build();
            Structure.SetDescriptor(descriptor.ObjectValue, descriptor);

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
            visitor.VisitMemoryEntry(Structure.GetMemoryEntry(index));

            ObjectValueContainer objects = Structure.GetObjects(index);
            MemoryEntry entry = new MemoryEntry(objects);

            Structure.SetMemoryEntry(index, entry);
        }

        /// <summary>
        /// Destroys the object.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="value">The value.</param>
        internal void DestroyObject(MemoryIndex parentIndex, ObjectValue value)
        {
            ObjectValueContainerBuilder objects = Structure.GetObjects(parentIndex).Builder();
            objects.Remove(value);
            Structure.SetObjects(parentIndex, objects.Build());
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
            if (Structure.HasArray(parentIndex))
            {
                throw new Exception("Variable " + parentIndex + " already has associated array value.");
            }

            AssociativeArray value = this.CreateArray();
            ArrayDescriptor oldDescriptor = Structure.GetDescriptor(value);
            closeTemporaryArray(oldDescriptor);

            ArrayDescriptor newDescriptor = oldDescriptor
                .Builder()
                .SetParentVariable(parentIndex)
                .SetUnknownField(parentIndex.CreateUnknownIndex())
                .Build();

            Structure.NewIndex(newDescriptor.UnknownIndex);
            Structure.SetArray(parentIndex, value);
            Structure.SetDescriptor(value, newDescriptor);
            return value;
        }

        private void closeTemporaryArray(ArrayDescriptor descriptor)
        {
            TemporaryIndex index = descriptor.ParentVariable as TemporaryIndex;

            if (index != null)
            {
                Structure.SetMemoryEntry(index, new MemoryEntry());
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
                Structure.SetMemoryEntry(parentIndex, new MemoryEntry(value));
            }
            else
            {
                List<Value> values;
                MemoryEntry oldEntry;
                if (Structure.TryGetMemoryEntry(parentIndex, out oldEntry))
                {
                    values = new List<Value>(oldEntry.PossibleValues);
                }
                else
                {
                    values = new List<Value>();
                }

                values.Add(value);
                Structure.SetMemoryEntry(parentIndex, new MemoryEntry(values));
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
            return CreateIndex(indexName, Structure.GetDescriptor(arrayValue), isMust, copyFromUnknown);
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
            Structure.NewIndex(indexIndex);

            descriptor = descriptor.Builder()
                .add(indexName, indexIndex)
                .Build();
            Structure.SetDescriptor(descriptor.ArrayValue, descriptor);

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
            visitor.VisitMemoryEntry(Structure.GetMemoryEntry(index));

            AssociativeArray array;
            MemoryEntry entry;
            if (Structure.TryGetArray(index, out array))
            {
                entry = new MemoryEntry(array);
            }
            else
            {
                entry = new MemoryEntry();
            }

            Structure.SetMemoryEntry(index, entry);
        }

        /// <summary>
        /// Destroys the array.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        internal void DestroyArray(MemoryIndex parentIndex)
        {
            AssociativeArray arrayValue;
            if (!Structure.TryGetArray(parentIndex, out arrayValue))
            {
                return;
            }

            ArrayDescriptor descriptor = Structure.GetDescriptor(arrayValue);
            foreach (var index in descriptor.Indexes)
            {
                ReleaseMemory(index.Value);
            }

            ReleaseMemory(descriptor.UnknownIndex);

            Structure.RemoveArray(parentIndex, arrayValue);
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
            MemoryEntry entry = Structure.GetMemoryEntry(index);
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
            if (Structure.TryGetMemoryEntry(index, out entry))
            {
                visitor.VisitMemoryEntry(entry);
            }
            Structure.SetMemoryEntry(index, new MemoryEntry(this.UndefinedValue));
        }

        /// <summary>
        /// Releases the memory in the given index and removes it from the data model.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void ReleaseMemory(MemoryIndex index)
        {
            DestroyMemory(index);
            DestroyAliases(index);

            Structure.RemoveIndex(index);
        }

        /// <summary>
        /// Extends the snapshot.
        /// </summary>
        /// <param name="input">The input.</param>
        private void extendSnapshot(ISnapshotReadonly input)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(input);

            Logger.append(this, snapshot.getSnapshotIdentification() + " extend");

            CallLevel = snapshot.CallLevel;
            returnIndex = snapshot.returnIndex;

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    Data = snapshot.Data.Copy(this);
                    Structure = snapshot.Structure.Copy(this, Data);
                    break;

                case SnapshotMode.InfoLevel:
                    Infos = snapshot.Infos.Copy(this);
                    Structure.Data = Infos;
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }
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
            int callLevel = 0;

            List<Snapshot> snapshots = new List<Snapshot>(inputs.Length);
            foreach (ISnapshotReadonly input in inputs)
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(input);
                Logger.append(this, snapshot.getSnapshotIdentification() + " merge");
                snapshots.Add(snapshot);

                if (snapshot.CallLevel > callLevel)
                {
                    callLevel = snapshot.CallLevel;
                }
            }

            CallLevel = callLevel;

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    MergeWorker worker = new MergeWorker(this, snapshots);
                    worker.Merge();
                    
                    Structure = worker.Structure;
                    Data = worker.Data;
                    break;

                case SnapshotMode.InfoLevel:
                    throw new NotImplementedException();
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }
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
            if (Structure.TryGetAliases(index, out oldAlias))
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

            Structure.SetAlias(index, alias.Build());
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
            if (Structure.TryGetAliases(index, out oldAlias))
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

            Structure.SetAlias(index, alias.Build());
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

            Structure.SetAlias(index, builder.Build());
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
            if (Structure.TryGetAliases(index, out oldAlias))
            {
                builder = oldAlias.Builder();
                convertAliasesToMay(index, builder);
            }
            else
            {
                builder = new MemoryAliasBuilder();
            }

            builder.AddMayAlias(mayAliases);
            Structure.SetAlias(index, builder.Build());
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
                MemoryAlias alias = Structure.GetAliases(mustIndex);

                MemoryAliasBuilder mustBuilder = Structure.GetAliases(mustIndex).Builder();
                mustBuilder.RemoveMustAlias(index);
                mustBuilder.AddMayAlias(index);
                Structure.SetAlias(index, mustBuilder.Build());
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
            if (Structure.TryGetAliases(sourceIndex, out aliases))
            {
                MemoryAliasBuilder builder = new MemoryAliasBuilder();
                foreach (MemoryIndex mustAlias in aliases.MustAliasses)
                {
                    MemoryAliasBuilder mustBuilder = Structure.GetAliases(mustAlias).Builder();
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
                    Structure.SetAlias(mustAlias, mustBuilder.Build());
                }

                foreach (MemoryIndex mayAlias in aliases.MayAliasses)
                {
                    MemoryAliasBuilder mayBuilder = Structure.GetAliases(mayAlias).Builder();

                    builder.AddMayAlias(mayAlias);
                    mayBuilder.AddMayAlias(targetIndex);

                    Structure.SetAlias(mayAlias, mayBuilder.Build());
                }

                Structure.SetAlias(targetIndex, builder.Build());
            }
        }

        /// <summary>
        /// Destroys the aliases of the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void DestroyAliases(MemoryIndex index)
        {
            MemoryAlias aliases;
            if (!Structure.TryGetAliases(index, out aliases))
            {
                return;
            }

            foreach (MemoryIndex mustIndex in aliases.MustAliasses)
            {
                MemoryAlias alias = Structure.GetAliases(mustIndex);
                if (alias.MustAliasses.Count == 1 && alias.MayAliasses.Count == 0)
                {
                    Structure.RemoveAlias(mustIndex);
                }
                else
                {
                    MemoryAliasBuilder builder = Structure.GetAliases(mustIndex).Builder();
                    builder.RemoveMustAlias(index);
                    Structure.SetAlias(mustIndex, builder.Build());
                }
            }

            foreach (MemoryIndex mayIndex in aliases.MayAliasses)
            {
                MemoryAlias alias = Structure.GetAliases(mayIndex);
                if (alias.MustAliasses.Count == 0 && alias.MayAliasses.Count == 1)
                {
                    Structure.RemoveAlias(mayIndex);
                }
                else
                {
                    MemoryAliasBuilder builder = Structure.GetAliases(mayIndex).Builder();
                    builder.RemoveMayAlias(index);
                    Structure.SetAlias(mayIndex, builder.Build());
                }
            }
        }

        #endregion

        #endregion
    }
}
