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
    /// <summary>
    /// Implementation of the memory snapshot based on copy semantics.
    /// 
    /// This implementation stores every possible value for the single memory location in one place.
    /// This approach guarantees write-read semantics - when user strongly writes to MAY alliased location
    /// he is still able to read only the strongly writed data without the ones from MAY target.
    /// 
    /// Basic unit of the memory model is MemoryIndex which allows undirect links between aliases
    /// and collection indexes. Every index is just pointer into the memory location where the data is stored.
    /// So when the data is changed is not necessary to change every connected memory locations. See
    /// <see cref="MemoryIndex" /> and <see cref="MemoryContainer" /> for more information. Data model itself is
    /// implemented in <see cref="SnapshotStructure" /> class.
    /// 
    /// Algorithms for reading or modifying snapshots are splitted into two groups. Memory collectors represents
    /// algorithms to gathering indexes and memory workers provides implementation of read/write
    /// algorithm. For more informations see <see cref="IndexCollectors" /> and <see cref="MemoryWorkers" />.
    /// </summary>
    public class Snapshot : SnapshotBase, IReferenceHolder
    {
        #region Variables and properties

        /// <summary>
        /// Global identifier counter for snapshot instances
        /// </summary>
        private static int SNAP_ID = 0;

        /// <summary>
        /// Unique identifier of snapshot instance
        /// </summary>
        private int snapId = SNAP_ID++;
        
        /// <summary>
        /// Defines the name of this variable for object calls
        /// </summary>
        public static readonly string THIS_VARIABLE_IDENTIFIER = "this";

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

        /// <summary>
        /// Gets the object with data values for memory locations.
        /// </summary>
        /// <value>
        /// The data object.
        /// </value>
        internal SnapshotData Data { get; private set; }

        /// <summary>
        /// Gets the object with data values for memory locations for the info phase of analysis.
        /// </summary>
        /// <value>
        /// The info object.
        /// </value>
        internal SnapshotData Infos { get; private set; }

        /// <summary>
        /// Overrides CurrentMode property setter to handle changing of mode settings
        /// </summary>
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

        /// <summary>
        /// Assistant helping memory models resolving memory operations.
        /// </summary>
        /// <value>
        /// The assistant.
        /// </value>
        internal MemoryAssistantBase MemoryAssistant { get { return base.Assistant; } }
        internal new MemoryAssistantBase  Assistant { get { return base.Assistant; } }

        /// <summary>
        /// Backup of the snapshot data after the start of transaction.
        /// </summary>
        SnapshotStructure oldStructure = null;
        SnapshotData oldData = null;
        SnapshotData oldInfos = null;
        int oldCallLevel;

        #endregion
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot"/> class. Sets the call information for global code
        /// and initializes empty data.
        /// </summary>
        public Snapshot()
        {
            CallLevel = GLOBAL_CALL_LEVEL;
            oldCallLevel = GLOBAL_CALL_LEVEL;

            Data = SnapshotData.CreateEmpty(this);
            Infos = SnapshotData.CreateEmpty(this);
            Structure = SnapshotStructure.CreateGlobal(this, Data);
            SnapshotLogger.append(this, "Constructed snapshot");
        }

        #region Text representations

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
        /// Generates snapshot representation in Weverca format
        /// </summary>
        public string GetRepresentation()
        {
            var result = new StringBuilder();

            if (CallLevel > GLOBAL_CALL_LEVEL)
            {
                result.AppendLine("===LOCALS===");
                result.AppendLine(Structure.Variables.GetLocalRepresentation(Data, Infos));
            }
            result.AppendLine("===GLOBALS===");
            result.AppendLine(Structure.Variables.GetGlobalRepresentation(Data, Infos));

            result.AppendLine("===GLOBAL CONTROLS===");
            result.AppendLine(Structure.ContolVariables.GetLocalRepresentation(Data, Infos));

            result.AppendLine("===LOCAL CONTROLS===");
            result.AppendLine(Structure.ContolVariables.GetGlobalRepresentation(Data, Infos));

            result.AppendLine("===ALIASES===");
            result.AppendLine(Structure.GetAliasesRepresentation());

            result.AppendLine("\n===ARRAYS===");
            result.AppendLine(Structure.GetArraysRepresentation(Data, Infos));

            result.AppendLine("\n===FIELDS===");
            result.AppendLine(Structure.GetFieldsRepresentation(Data, Infos));


            return result.ToString();
        }

        /// <summary>
        /// Creates the string representation of the data where values for each index are at the same line.
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

        /// <summary>
        /// Gets the snapshot identification which consists of snapshot and data ID separated by colons.
        /// </summary>
        public String getSnapshotIdentification()
        {
            return CallLevel + "." + snapId.ToString() + "::" + Structure.DataId.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GetRepresentation();
        }

        #endregion

        #region AbstractSnapshot Implementation

        #region Transaction

        /// <summary>
        /// Start snapshot transaction - changes can be proceeded only when transaction is started
        /// </summary>
        /// <exception cref="System.NotSupportedException">Current mode:  + CurrentMode</exception>
        protected override void startTransaction()
        {
            SnapshotLogger.append(this, "Start mode: " + CurrentMode.ToString());

            oldCallLevel = CallLevel;

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

        /// <summary>
        /// Starts the data transaction.
        /// </summary>
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

        /// <summary>
        /// Starts the information transaction.
        /// </summary>
        private void startInfoTransaction()
        {
            oldInfos = Infos;
            Infos = oldInfos.Copy(this);

            Structure.Data = Infos;
            Structure.Locked = true;
            oldStructure.Data = oldInfos;
        }

        /// <summary>
        /// Commit started transaction - must return true if the content of the snapshot is different
        /// than the content commited by the previous transaction, false otherwise
        /// NOTE:
        /// Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <param name="simplifyLimit">Limit number of memory entry possible values count when does simplifying MemoryEntries start</param>
        /// <returns>
        ///   <c>true</c> if there is semantic change in transaction, <c>false</c> otherwise
        /// </returns>
        /// <exception cref="System.NotSupportedException">Current mode:  + CurrentMode</exception>
        protected override bool commitTransaction(int simplifyLimit)
        {
            SnapshotLogger.append(this, "Commit " + Structure.DataEquals(oldStructure));
            SnapshotLogger.append(this);

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    return !Structure.DataEqualsAndSimplify(oldStructure, simplifyLimit, MemoryAssistant);

                case SnapshotMode.InfoLevel:
                    return !Infos.DataEqualsAndSimplify(oldInfos, simplifyLimit, MemoryAssistant);

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }
        }

        /// <summary>
        /// Widen current transaction and process commit.
        /// Commit started transaction - must return true if the content of the snapshot is different
        /// than the content commited by the previous transaction, false otherwise
        /// NOTE:
        /// Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <param name="simplifyLimit">Limit number of memory entry possible values count when does simplifying MemoryEntries start</param>
        /// <returns>
        ///   <c>true</c> if there is semantic change in transaction, <c>false</c> otherwise
        /// </returns>
        /// s
        protected override bool widenAndCommitTransaction(int simplifyLimit)
        {
            SnapshotLogger.append(this, "Commit and widen");
            bool result = !Structure.WidenNotEqual(oldStructure, simplifyLimit, MemoryAssistant);
            SnapshotLogger.appendToSameLine(" " + result);
            SnapshotLogger.append(this);

            return result;
        }

        #endregion

        #region Objects

        /// <summary>
        /// Initialize object of given type
        /// </summary>
        /// <param name="createdObject">Created object that has to be initialized</param>
        /// <param name="type">Desired type of initialized object</param>
        protected override void initializeObject(ObjectValue createdObject, TypeValue type)
        {
            SnapshotLogger.append(this, "Init object " + createdObject + " " + type);
            ObjectDescriptor descriptor = new ObjectDescriptor(createdObject, type, ObjectIndex.CreateUnknown(createdObject));
            Structure.NewIndex(descriptor.UnknownIndex);
            Structure.SetDescriptor(createdObject, descriptor);
        }

        /// <summary>
        /// Iterates the object.
        /// </summary>
        /// <param name="iteratedObject">The iterated object.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unknown object</exception>
        protected IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            SnapshotLogger.append(this, "Iterate object " + iteratedObject);
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

        /// <summary>
        /// Determine type of given object
        /// </summary>
        /// <param name="objectValue">Object which type is resolved</param>
        /// <returns>
        /// Type of given object
        /// </returns>
        /// <exception cref="System.Exception">Unknown object</exception>
        protected override TypeValue objectType(ObjectValue objectValue)
        {
            SnapshotLogger.append(this, "Get object type " + objectValue);
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
        
        /// <summary>
        /// Resolves all possible functions for given functionName
        /// NOTE:
        /// Multiple declarations for single functionName can happen for example because of branch merging
        /// </summary>
        /// <param name="value"></param>
        /// <param name="methodName"></param>
        /// <returns>
        /// Resolved functions
        /// </returns>
        protected override IEnumerable<FunctionValue> resolveStaticMethod(TypeValue value, QualifiedName methodName)
        {
            var objectMethods = Weverca.MemoryModels.VirtualReferenceModel.TypeMethodResolver.ResolveMethods(value, this);
            return Assistant.ResolveMethods(value, methodName, objectMethods);
        }

        /// <summary>
        /// Resolves the method.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        internal IEnumerable<FunctionValue> resolveMethod(MemoryEntry entry, QualifiedName methodName)
        {
            HashSet<FunctionValue> functions = new HashSet<FunctionValue>();
            foreach (Value value in entry.PossibleValues)
            {
                HashSetTools.AddAll(functions, resolveMethod(value, methodName));
            }

            return functions;
        }

        /// <summary>
        /// Resolves the method.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
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

            var resolvedMethods = MemoryAssistant.ResolveMethods(objectValue, type, methodName, objectMethods);

            return resolvedMethods;
        }

        #endregion

        #region Arrays

        /// <summary>
        /// Initialize array
        /// </summary>
        /// <param name="createdArray">Created array that has to be initialized</param>
        protected override void initializeArray(AssociativeArray createdArray)
        {
            SnapshotLogger.append(this, "Init array " + createdArray);
            TemporaryIndex arrayIndex = CreateTemporary();

            ArrayDescriptor descriptor = new ArrayDescriptor(createdArray, arrayIndex);
            Structure.NewIndex(descriptor.UnknownIndex);
            Structure.SetArray(arrayIndex, createdArray);
            Structure.SetDescriptor(createdArray, descriptor);
            Structure.SetMemoryEntry(arrayIndex, new MemoryEntry(createdArray));
        }

        /// <summary>
        /// Iterates the array.
        /// </summary>
        /// <param name="iteratedArray">The iterated array.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unknown associative array</exception>
        protected IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            SnapshotLogger.append(this, "Iterate array " + iteratedArray);
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

        /// <summary>
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contains till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged</param>
        protected override void extend(ISnapshotReadonly[] inputs)
        {
            SnapshotLogger.append(this, "extend");

            if (inputs.Length == 1)
            {
                extendSnapshot(inputs[0]);
            }
            else if (inputs.Length > 1)
            {
                mergeSnapshots(inputs);
            }
        }

        /// <summary>
        /// Extend snapshot as call from given callerContext
        /// </summary>
        /// <param name="callerContext"></param>
        /// <param name="thisObject"></param>
        /// <param name="arguments"></param>
        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(callerContext);
            SnapshotLogger.append(this, "call extend: " + snapshot.getSnapshotIdentification() + " level: " + snapshot.CallLevel + " this: " + thisObject);

            CallLevel = snapshot.CallLevel + 1;
            if (CallLevel != oldCallLevel && oldCallLevel != GLOBAL_CALL_LEVEL)
            {
                CallLevel = oldCallLevel;
            }

            Data = snapshot.Data.Copy(this);
            Infos = snapshot.Infos.Copy(this);
            Structure = snapshot.Structure.CopyAndAddLocalLevel(this, Data);

            if (thisObject != null)
            {
                ReadWriteSnapshotEntryBase snapshotEntry = SnapshotEntry.CreateVariableEntry(new VariableIdentifier(THIS_VARIABLE_IDENTIFIER), GlobalContext.LocalOnly, CallLevel);
                snapshotEntry.WriteMemory(this, thisObject);
            }

            int level = CallLevel > GLOBAL_CALL_LEVEL ? CallLevel - 1 : GLOBAL_CALL_LEVEL;
            //returnIndex = new TemporaryIndex(level);
            //Structure.NewIndex(returnIndex);
            //Structure.Temporary[level].Add(returnIndex);
        }

        /// <summary>
        /// Merge given call output with current context.
        /// WARNING: Call can change many objects via references (they don't has to be in global context)
        /// </summary>
        /// <param name="callOutputs">Output snapshots of call level</param>
        /// <exception cref="System.NotSupportedException">Current mode:  + CurrentMode</exception>
        protected override void mergeWithCallLevel(ISnapshotReadonly[] callOutputs)
        {
            SnapshotLogger.append(this, "call merge");
            List<Snapshot> snapshots = new List<Snapshot>(callOutputs.Length);
            foreach (ISnapshotReadonly input in callOutputs)
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(input);
                SnapshotLogger.append(this, snapshot.getSnapshotIdentification() + " call merge " + snapshot.CallLevel);
                snapshots.Add(snapshot);
            }

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    {
                        MergeWorker worker = new MergeWorker(this, snapshots, true);
                        worker.Merge();

                        Structure = worker.Structure;
                        Data = worker.Data;
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        MergeInfoWorker worker = new MergeInfoWorker(this, snapshots, true);
                        worker.Merge();

                        Structure = worker.Structure;
                        Infos = worker.Infos;
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }
        }

        /// <summary>
        /// Fetch variables from global context into current context
        /// </summary>
        /// <param name="variables">Variables that will be fetched</param>
        /// <example>global x,y;</example>
        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            SnapshotLogger.append(this, "Fetch from global");
            foreach (VariableName name in variables)
            {
                SnapshotLogger.append(this, "Fetch from global " + name);
                ReadWriteSnapshotEntryBase localEntry = getVariable(new VariableIdentifier(name), false);
                ReadWriteSnapshotEntryBase globalEntry = getVariable(new VariableIdentifier(name), true);

                localEntry.SetAliases(this, globalEntry);
            }
        }

        /// <summary>
        /// Get all variables defined in global scope
        /// </summary>
        /// <returns>
        /// Variables defined in global scope
        /// </returns>
        protected override IEnumerable<VariableName> getGlobalVariables()
        {
            SnapshotLogger.append(this, "Get global ");

            List<VariableName> names = new List<VariableName>();

            foreach (var variable in Structure.Variables.Global.Indexes)
            {
                names.Add(new VariableName(variable.Key));
            }

            return names;
        }

        #endregion

        #region Infos

        /// <summary>
        /// Set given info for value
        /// </summary>
        /// <param name="value">Value which info is stored</param>
        /// <param name="info">Info stored for value</param>
        /// <exception cref="System.NotImplementedException">Info values - waiting for final approach</exception>
        protected override void setInfo(Value value, params InfoValue[] info)
        {
            throw new NotImplementedException("Info values - waiting for final approach");
        }

        /// <summary>
        /// Set given info for variable
        /// </summary>
        /// <param name="variable">Variable which info is stored</param>
        /// <param name="info">Info stored for variable</param>
        /// <exception cref="System.NotImplementedException">Info values - waiting for final approach</exception>
        protected override void setInfo(VariableName variable, params InfoValue[] info)
        {
            throw new NotImplementedException("Info values - waiting for final approach");
        }

        /// <summary>
        /// Read info stored for given value
        /// </summary>
        /// <param name="value">Value which info is read</param>
        /// <returns>
        /// Stored info
        /// </returns>
        /// <exception cref="System.NotImplementedException">Info values - waiting for final approach</exception>
        protected override InfoValue[] readInfo(Value value)
        {
            throw new NotImplementedException("Info values - waiting for final approach");
        }

        /// <summary>
        /// Read info stored for given variable
        /// </summary>
        /// <param name="variable">Variable which info is read</param>
        /// <returns>
        /// Stored info
        /// </returns>
        /// <exception cref="System.NotImplementedException">Info values - waiting for final approach</exception>
        protected override InfoValue[] readInfo(VariableName variable)
        {
            throw new NotImplementedException("Info values - waiting for final approach");
        }

        #endregion

        #region Functions and Classes

        /// <summary>
        /// Resolves all possible functions for given functionName
        /// NOTE:
        /// Multiple declarations for single functionName can happen for example because of branch merging
        /// </summary>
        /// <param name="functionName">Name of resolved function</param>
        /// <returns>
        /// Resolved functions
        /// </returns>
        protected override IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName)
        {
            SnapshotLogger.append(this, "Resolve function " + functionName);
            if (Structure.IsFunctionDefined(functionName))
            {
                return Structure.GetFunction(functionName);
            }
            else
            {
                return new List<FunctionValue>(0);
            }
        }

        /// <summary>
        /// Resolves all possible types for given typeName
        /// NOTE:
        /// Multiple declarations for single typeName can happen for example because of branch merging
        /// </summary>
        /// <param name="typeName">Name of resolved type</param>
        /// <returns>
        /// Resolved types
        /// </returns>
        protected override IEnumerable<TypeValue> resolveType(QualifiedName typeName)
        {
            SnapshotLogger.append(this, "Resolve type " + typeName);
            if (Structure.IsClassDefined(typeName))
            {
                return Structure.GetClass(typeName);
            }
            else
            {
                return new List<TypeValue>(0);
            }
        }

        /// <summary>
        /// Declare given function into global context
        /// </summary>
        /// <param name="declaration">Declared function</param>
        protected override void declareGlobal(FunctionValue declaration)
        {
            QualifiedName name = new QualifiedName(declaration.Name);
            SnapshotLogger.append(this, "Declare function - " + name);

            if (!Structure.IsFunctionDefined(name))
            {
                Structure.SetFunction(name, declaration);
            }
            else
            {
                Structure.SetFunction(name, declaration);
            }
        }

        /// <summary>
        /// Declare given type into global context
        /// </summary>
        /// <param name="declaration">Declared type</param>
        protected override void declareGlobal(TypeValue declaration)
        {
            QualifiedName name = declaration.QualifiedName;
            SnapshotLogger.append(this, "Declare class - " + name);

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

        /// <summary>
        /// Create snapshot entry providing reading,... services for variable
        /// </summary>
        /// <param name="variable">Name of variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns>
        /// Readable snapshot entry for variable identifier
        /// </returns>
        /// <remarks>
        /// If global context is not forced, searches in local context (there can be
        /// fetched some variables from global context also),
        /// or in global context in snapshot belonging to global code
        /// </remarks>
        protected override ReadWriteSnapshotEntryBase getVariable(VariableIdentifier variable, bool forceGlobalContext)
        {
            SnapshotLogger.append(this, "Get variable - " + variable);
            if (forceGlobalContext)
            {
                return SnapshotEntry.CreateVariableEntry(variable, GlobalContext.GlobalOnly);
            }
            else
            {
                return SnapshotEntry.CreateVariableEntry(variable, GlobalContext.LocalOnly, CallLevel);
            }
        }

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="name">Variable determining control entry</param>
        /// <returns>
        /// Created control entry
        /// </returns>
        protected override ReadWriteSnapshotEntryBase getControlVariable(VariableName name)
        {
            SnapshotLogger.append(this, "Get control variable - " + name);
            return SnapshotEntry.CreateControlEntry(name, GlobalContext.GlobalOnly);
        }

        /// <summary>
        /// Creates snapshot entry containing given value. Created entry doesn't have
        /// explicit memory storage. But it still can be asked for saving indexes, fields, resolving aliases,... !!!
        /// </summary>
        /// <param name="entry">Value wrapped in snapshot entry</param>
        /// <returns>
        /// Created value entry
        /// </returns>
        protected override ReadWriteSnapshotEntryBase createSnapshotEntry(MemoryEntry entry)
        {
            SnapshotLogger.append(this, "Get entry snap - " + entry);
            return new DataSnapshotEntry(this, entry);
        }

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling in local context. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="name">Variable determining control entry</param>
        /// <returns>
        /// Created control entry
        /// </returns>
        protected override ReadWriteSnapshotEntryBase getLocalControlVariable(VariableName name)
        {
            SnapshotLogger.append(this, "Get local control variable - " + name);
            return SnapshotEntry.CreateControlEntry(name, GlobalContext.LocalOnly, CallLevel);
        }

        #endregion

        /// <summary>
        /// Returns the number of memory locations in the snapshot.
        /// Memory locations are top-level variables, all indices of arrays and all properties of objects.
        /// </summary>
        /// <returns>
        /// the number of variables in the snapshot
        /// </returns>
        override public int NumMemoryLocations()
        {
            return Structure.IndexData.Count();
        }

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
        /// <param name="removeUndefined">if set to <c>true</c> undefined value is removed from target memory entry.</param>
        /// <returns></returns>
        internal ObjectValue CreateObject(MemoryIndex parentIndex, bool isMust, bool removeUndefined = false)
        {
            ObjectValue value = MemoryAssistant.CreateImplicitObject();

            if (isMust)
            {
                DestroyMemory(parentIndex);
                Structure.SetMemoryEntry(parentIndex, new MemoryEntry(value));
            }
            else
            {
                MemoryEntry oldEntry;

                HashSet<Value> values;
                if (Structure.TryGetMemoryEntry(parentIndex, out oldEntry))
                {
                    values = new HashSet<Value>(oldEntry.PossibleValues);

                    if (removeUndefined)
                    {
                        values.Remove(this.UndefinedValue);
                    }
                }
                else
                {
                    values = new HashSet<Value>();
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

        /// <summary>
        /// Closes the temporary array which was created on array initialization to prevent orphan arrays.
        /// </summary>
        /// <param name="descriptor">The descriptor of temporary array.</param>
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
        /// <param name="removeUndefined">if set to <c>true</c> removes undefined value from target memory entry.</param>
        /// <returns>
        /// Memory index of newly created array index.
        /// </returns>
        internal AssociativeArray CreateArray(MemoryIndex parentIndex, bool isMust, bool removeUndefined = false)
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
                HashSet<Value> values;
                MemoryEntry oldEntry;
                if (Structure.TryGetMemoryEntry(parentIndex, out oldEntry))
                {
                    values = new HashSet<Value>(oldEntry.PossibleValues);
                }
                else
                {
                    values = new HashSet<Value>();
                }

                if (removeUndefined)
                {
                    values.Remove(this.UndefinedValue);
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
        /// <param name="copyFromUnknown">if set to <c>true</c> value of the field is initialized</param>
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
        /// <param name="copyFromUnknown">if set to <c>true</c> value of the field is initialized</param>
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

            SnapshotLogger.append(this, snapshot.getSnapshotIdentification() + " extend");

            CallLevel = snapshot.CallLevel;

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    Data = snapshot.Data.Copy(this);
                    Structure = snapshot.Structure.Copy(this, Data);
                    break;

                case SnapshotMode.InfoLevel:
                    Infos = snapshot.Infos.Copy(this);
                    Structure.Data = Infos;
                    assignCreatedAliases();
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }
        }

        private void assignCreatedAliases()
        {
            foreach (AliasData aliasData in Structure.CreatedAliases)
            {
                MemoryEntry entry = Structure.GetMemoryEntry(aliasData.SourceIndex);
                foreach (MemoryIndex mustAlias in aliasData.MustIndexes)
                {
                    if (mustAlias != null)
                    {
                        Structure.SetMemoryEntry(mustAlias, entry);
                    }
                }

                foreach (MemoryIndex mayAlias in aliasData.MayIndexes)
                {
                    if (mayAlias != null)
                    {
                        MemoryEntry aliasEntry = Structure.GetMemoryEntry(mayAlias);
                        HashSet<Value> values = new HashSet<Value>(aliasEntry.PossibleValues);
                        HashSetTools.AddAll(values, entry.PossibleValues);
                        Structure.SetMemoryEntry(mayAlias, new MemoryEntry(values));
                    }
                }
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
                SnapshotLogger.append(this, snapshot.getSnapshotIdentification() + " merge");
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
                    {
                        MergeWorker worker = new MergeWorker(this, snapshots);
                        worker.Merge();

                        Structure = worker.Structure;
                        Data = worker.Data;
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        MergeInfoWorker worker = new MergeInfoWorker(this, snapshots);
                        worker.Merge();

                        Structure = worker.Structure;
                        Infos = worker.Infos;
                    }
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
            Structure.AddCreatedAlias(new AliasData(mustAliases, mayAliases, index));
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
            Structure.AddCreatedAlias(new AliasData(new MemoryIndex[] { mustAlias }, new MemoryIndex[] { mayAlias }, index));
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
