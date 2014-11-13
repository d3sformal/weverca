/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;

namespace Weverca.MemoryModels.ModularCopyMemoryModel
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
    /// MemoryIndex and MemoryContainer for more information. Data model itself is implemented
    /// in <see cref="Structure" /> class.
    /// 
    /// Algorithms for reading or modifying snapshots are splitted into two groups. Memory collectors represents
    /// algorithms to gathering indexes and memory workers provides implementation of read/write
    /// algorithm. For more informations see ÏndexCollectors and MemoryWorkers.
    /// </summary>
    public class Snapshot : SnapshotBase, IReferenceHolder
    {
        #region Variables and properties

        /// <summary>
        /// Gets the logger object which should be used to log snapshot operations.
        /// </summary>
        public static ILogger Logger = new MemoryModelLogger();

        /// <summary>
        /// Gets the benchmark object to mesure snapshot operations.
        /// </summary>
        public static IBenchmark Benchmark = new MemoryModelBenchmark();

        /// <summary>
        /// Gets the snapshot data factory.
        /// </summary>
        /// <value>
        /// The snapshot data factory.
        /// </value>
        public static ISnapshotDataFactory SnapshotDataFactory
            = new Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Data.TrackingSnapshotDataFactory();

        /// <summary>
        /// Gets the snapshot structure factory.
        /// </summary>
        /// <value>
        /// The snapshot structure factory.
        /// </value>
        public static ISnapshotStructureFactory SnapshotStructureFactory
            = new Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.TrackingSnapshotStructureFactory();

        #region Algorithm Factories

        /// <summary>
        /// The algorithm factories for the memory phase.
        /// </summary>
        public static AlgorithmFactories MemoryAlgorithmFactories
			= Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithmFactories.Factories;

        /// <summary>
        /// The algorithm factories for the info phase.
        /// </summary>
        public static AlgorithmFactories InfoAlgorithmFactories
            = Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithmFactories.Factories;

        /// <summary>
        /// Gets the algorithm factories for the current memory mode.
        /// </summary>
        /// <value>
        /// The algorithm factories.
        /// </value>
        public AlgorithmFactories AlgorithmFactories
        {
            get
            {
                switch (CurrentMode)
                {
                    case SnapshotMode.MemoryLevel:
                        return MemoryAlgorithmFactories;

                    case SnapshotMode.InfoLevel:
                        return InfoAlgorithmFactories;

                    default:
                        throw new NotSupportedException("Current mode: " + CurrentMode);
                }
            }
        }

        /// <summary>
        /// Gets the memory entry for undefined memory indexes.
        /// </summary>
        /// <value>
        /// The memory entry for undefined memory indexes.
        /// </value>
        /// <exception cref="System.NotSupportedException">Current mode:  + CurrentMode</exception>
        public MemoryEntry EmptyEntry {
            get
            {
                switch (CurrentMode)
                {
                    case SnapshotMode.MemoryLevel:
                        return new MemoryEntry(this.UndefinedValue);

                    case SnapshotMode.InfoLevel:
                        return new MemoryEntry();

                    default:
                        throw new NotSupportedException("Current mode: " + CurrentMode);
                }
            }
        }
        
        #endregion

        /// <summary>
        /// Global identifier counter for snapshot instances
        /// </summary>
        private static int SNAP_ID = 0;

        /// <summary>
        /// Defines the name of this variable for object calls
        /// </summary>
        public static readonly string THIS_VARIABLE_IDENTIFIER = "this";

        /// <summary>
        /// Identifier for the bottom of the call stack. At this level the global variables are stored.
        /// </summary>
        public static readonly int GLOBAL_CALL_LEVEL = 0;

        /// <summary>
        /// Unique identifier of snapshot instance
        /// </summary>
        public int SnapshotId { get; private set; }

        /// <summary>
        /// Gets the container with data of the snapshot.
        /// </summary>
        /// <value>
        /// The data container object.
        /// </value>
        public ISnapshotStructureProxy Structure { get; private set; }

        /// <summary>
        /// Gets the object with data values for memory locations.
        /// </summary>
        /// <value>
        /// The data object.
        /// </value>
        public ISnapshotDataProxy Data { get; private set; }

        /// <summary>
        /// Gets the object with data values for memory locations for the info phase of analysis.
        /// </summary>
        /// <value>
        /// The info object.
        /// </value>
        public ISnapshotDataProxy Infos { get; private set; }

        /// <summary>
        /// Gets the current container with data values for this mode.
        /// </summary>
        /// <value>
        /// The current data.
        /// </value>
        public ISnapshotDataProxy CurrentData { get; private set; }

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
                        CurrentData = Data;
                        Structure.Locked = false;
                        break;

                    case SnapshotMode.InfoLevel:
                        CurrentData = Infos;
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
        public int CallLevel { 
            get; 
            private set; }

        /// <summary>
        /// Assistant helping memory models resolving memory operations.
        /// </summary>
        /// <value>
        /// The assistant.
        /// </value>
        public MemoryAssistantBase MemoryAssistant { get { return base.Assistant; } }

        /// <summary>
        /// Assistant helping memory models resolving memory operations
        /// </summary>
        public new MemoryAssistantBase Assistant { get { return base.Assistant; } }

        /// <summary>
        /// Backup of the snapshot data after the start of transaction.
        /// </summary>
        ISnapshotStructureProxy oldStructure = null;
        ISnapshotDataProxy oldMemory = null;
        ISnapshotDataProxy oldInfos = null;
        int oldCallLevel;

        /// <summary>
        /// Gets the collection of created aliases in this snapshot.
        /// </summary>
        /// <value>
        /// The created aliases.
        /// </value>
        public IEnumerable<IMemoryAlias> CreatedAliases
        {
            get { return createdAliases; }
        }
        private List<IMemoryAlias> createdAliases;

        /// <summary>
        /// Gets the number of transactions.
        /// </summary>
        /// <value>
        /// The number of transactions.
        /// </value>
        public int NumberOfTransactions { get; private set; }
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot"/> class. Sets the call information for global code
        /// and initializes empty data.
        /// </summary>
        public Snapshot()
        {
            SnapshotId = SNAP_ID++;

            CallLevel = GLOBAL_CALL_LEVEL;
            oldCallLevel = GLOBAL_CALL_LEVEL;

            Data = SnapshotDataFactory.CreateEmptyInstance(this);
            Infos = SnapshotDataFactory.CreateEmptyInstance(this);
            Structure = SnapshotStructureFactory.CreateGlobalContextInstance(this);
            createdAliases = new List<IMemoryAlias>();
            NumberOfTransactions = 0;

            Benchmark.InitializeSnapshot(this);
            Logger.Log(this, "Constructed snapshot");
        }

        /// <summary>
        /// Gets the snapshot identification which consists of snapshot and data ID separated by colons.
        /// </summary>
        /// <returns>String version snapshot identification which consists of snapshot and data ID separated by colons.</returns>
        public String getSnapshotIdentification()
        {
            return CallLevel + "." + SnapshotId.ToString() + "::" + Structure.Readonly.StructureId.ToString();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            IPrintAlgorithm algorithm = AlgorithmFactories.PrintAlgorithmFactory.CreateInstance();
            return algorithm.SnapshotToString(this);
        }

        #region AbstractSnapshot Implementation

        #region Transaction

        /// <summary>
        /// Start snapshot transaction - changes can be proceeded only when transaction is started
        /// </summary>
        /// <exception cref="System.NotSupportedException">Current mode:  + CurrentMode</exception>
        protected override void startTransaction()
        {
            Benchmark.StartTransaction(this);
            Logger.Log(this, "Start mode: {0}", CurrentMode);

            oldCallLevel = CallLevel;
            NumberOfTransactions++;

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
            oldMemory = Data;
            createdAliases.Clear();

            Data = SnapshotDataFactory.CopyInstance(this, oldMemory);
            Structure = SnapshotStructureFactory.CopyInstance(this, oldStructure);
            CurrentData = Data;

            Structure.Locked = false;
        }

        /// <summary>
        /// Starts the information transaction.
        /// </summary>
        private void startInfoTransaction()
        {
            oldInfos = Infos;
            Infos = SnapshotDataFactory.CopyInstance(this, oldInfos);
            CurrentData = Infos;

            Structure.Locked = false;
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
            Logger.Log(this, "Commit");

            ICommitAlgorithm algorithm = AlgorithmFactories.CommitAlgorithmFactory.CreateInstance();
            ISnapshotDataProxy currentData;
            ISnapshotDataProxy oldData;

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    currentData = Data;
                    oldData = oldMemory;
                    break;

                case SnapshotMode.InfoLevel:
                    currentData = Infos;
                    oldData = oldInfos;
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }

            Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.COMMIT);

            algorithm.SetStructure(Structure, oldStructure);
            algorithm.SetData(currentData, oldData);
            algorithm.CommitAndSimplify(this, simplifyLimit);
            bool differs = algorithm.IsDifferent();

            Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.COMMIT);

            Logger.LogToSameLine(" " + differs);
            Logger.Log(this);
            Logger.Log("\n---------------------------------\n");

            Benchmark.FinishTransaction(this);

            oldStructure = null;
            oldMemory = null;
            oldInfos = null;

            return differs;
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
        protected override bool widenAndCommitTransaction(int simplifyLimit)
        {
            Logger.Log(this, "Commit and widen");

            ICommitAlgorithm algorithm = AlgorithmFactories.CommitAlgorithmFactory.CreateInstance();
            ISnapshotDataProxy currentData;
            ISnapshotDataProxy oldData;

            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    currentData = Data;
                    oldData = oldMemory;
                    break;

                case SnapshotMode.InfoLevel:
                    currentData = Infos;
                    oldData = oldInfos;
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }

            Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.WIDEN_COMMIT);

            algorithm.SetStructure(Structure, oldStructure);
            algorithm.SetData(currentData, oldData);
            algorithm.CommitAndWiden(this, simplifyLimit);
            bool differs = algorithm.IsDifferent();

            Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.WIDEN_COMMIT);

            Logger.LogToSameLine(" " + differs);
            Logger.Log(this);
            Logger.Log("\n---------------------------------\n");

            Benchmark.FinishTransaction(this);

            oldStructure = null;
            oldMemory = null;
            oldInfos = null;

            return differs;
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
            Logger.Log(this, "Init object " + createdObject + " " + type);
            IObjectDescriptor descriptor = Structure.CreateObjectDescriptor(createdObject, type, ObjectIndex.CreateUnknown(createdObject));
            Structure.Writeable.NewIndex(descriptor.UnknownIndex);
            Structure.Writeable.SetDescriptor(createdObject, descriptor);
        }

        /// <summary>
        /// Iterates the object.
        /// </summary>
        /// <param name="iteratedObject">The iterated object.</param>
        /// <returns>List of all fields of the given object.</returns>
        /// <exception cref="System.Exception">Unknown object</exception>
        protected IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            Logger.Log(this, "Iterate object " + iteratedObject);
            IObjectDescriptor descriptor;
            if (Structure.Readonly.TryGetDescriptor(iteratedObject, out descriptor))
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
            Logger.Log(this, "Get object type " + objectValue);
            IObjectDescriptor descriptor;
            if (Structure.Readonly.TryGetDescriptor(objectValue, out descriptor))
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
        /// <param name="value">The value.</param>
        /// <param name="methodName">Name of the method.</param>
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
        /// <returns>List of methods with the given name for objects within the given memory entry.</returns>
        internal IEnumerable<FunctionValue> resolveMethod(MemoryEntry entry, QualifiedName methodName)
        {
            HashSet<FunctionValue> functions = new HashSet<FunctionValue>();
            foreach (Value value in entry.PossibleValues)
            {
                CollectionTools.AddAll(functions, resolveMethod(value, methodName));
            }

            return functions;
        }

        /// <summary>
        /// Resolves the method.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>List of methods with the given name for given object.</returns>
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
            Logger.Log(this, "Init array " + createdArray);
            TemporaryIndex arrayIndex = CreateTemporary();

            IArrayDescriptor descriptor = Structure.CreateArrayDescriptor(createdArray, arrayIndex);
            Structure.Writeable.NewIndex(descriptor.UnknownIndex);
            Structure.Writeable.SetArray(arrayIndex, createdArray);
            Structure.Writeable.SetDescriptor(createdArray, descriptor);

            Data.Writeable.SetMemoryEntry(arrayIndex, new MemoryEntry(createdArray));
        }

        /// <summary>
        /// Iterates the array.
        /// </summary>
        /// <param name="iteratedArray">The iterated array.</param>
        /// <returns>List of indexes of the array.</returns>
        /// <exception cref="System.Exception">Unknown associative array</exception>
        protected IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            Logger.Log(this, "Iterate array " + iteratedArray);
            IArrayDescriptor descriptor;
            if (Structure.Readonly.TryGetDescriptor(iteratedArray, out descriptor))
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
        protected override void extend(params ISnapshotReadonly[] inputs)
        {
            Logger.Log(this, "extend");

            CallLevel = SnapshotEntry.ToSnapshot (inputs [0]).CallLevel;

            extendWithoutComputingCallLevel(inputs);
        }

        /// <inheritdoc />
        protected override void extendAtSubprogramEntry(ISnapshotReadonly[] inputs, ProgramPointBase[] extendedPoints)
        {
            Logger.Log(this, "extend");

            CallLevel = maxCallLevel(inputs);

            extendWithoutComputingCallLevel(inputs);
        }

        /// <inheritdoc />
        protected override void extendAtCatchEntry(ISnapshotReadonly[] inputs, CatchBlockDescription catchDescription) 
        {
            Logger.Log(this, "extend");

            CallLevel = SnapshotEntry.ToSnapshot (catchDescription.TargetPoint.OutSnapshot).CallLevel;

            extendWithoutComputingCallLevel(inputs);
        }

        /// <summary>
        /// Extend snapshot as call from given callerContext
        /// </summary>
        /// <param name="callerContext">The caller context.</param>
        /// <param name="thisObject">The this object.</param>
        /// <param name="arguments">The arguments.</param>
        protected override void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(callerContext);
            Logger.Log(this, "call extend: " + snapshot.getSnapshotIdentification() + " level: " + snapshot.CallLevel + " this: " + thisObject);


            CallLevel = snapshot.CallLevel + 1;

            if (oldCallLevel != CallLevel && oldCallLevel != GLOBAL_CALL_LEVEL) 
            {
                // The called function is shared and we are calling it repeatedly
                // Pick the call level from the previous call of extendAsCall
                CallLevel = oldCallLevel;
            }

            // Call levels of the caller should be always the same
            Debug.Assert (oldCallLevel == GLOBAL_CALL_LEVEL || oldCallLevel == CallLevel);

            IMergeAlgorithm algorithm;
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    algorithm = MemoryAlgorithmFactories.MergeAlgorithmFactory.CreateInstance();
                    Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.EXTEND_AS_CALL);
                    algorithm.ExtendAsCall(this, snapshot, thisObject);

                    Structure = algorithm.GetMergedStructure();
                    Data = algorithm.GetMergedData();
                    CurrentData = Data;
                    Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.EXTEND_AS_CALL);
                    break;

                case SnapshotMode.InfoLevel:
                    algorithm = InfoAlgorithmFactories.MergeAlgorithmFactory.CreateInstance();
                    Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.EXTEND_AS_CALL);
                    algorithm.ExtendAsCall(this, snapshot, thisObject);

                    Infos = algorithm.GetMergedData();
                    CurrentData = Infos;
                    Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.EXTEND_AS_CALL);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }

            if (thisObject != null)
            {
                ReadWriteSnapshotEntryBase snapshotEntry = 
                    SnapshotEntry.CreateVariableEntry(new VariableIdentifier(Snapshot.THIS_VARIABLE_IDENTIFIER), GlobalContext.LocalOnly, this.CallLevel);
                snapshotEntry.WriteMemory(this, thisObject);
            }
        }

        /// <summary>
        /// Merge given call output with current context.
        /// WARNING: Call can change many objects via references (they don't has to be in global context)
        /// </summary>
        /// <param name="callOutputs">Output snapshots of call level</param>
        /// <exception cref="System.NotSupportedException">Current mode:  + CurrentMode</exception>
        protected override void mergeWithCallLevel(ProgramPointBase callerPoint, ISnapshotReadonly[] callOutputs)
        {
            Logger.Log(this, "call merge");

            CallLevel = ((Snapshot)callerPoint.OutSnapshot).CallLevel;
            var tempCallLevel = CallLevel;
            // In case of shared functions, the call level of the caller can be bigger than call levels of the callee.
            // Se extendAsCall method.
            if (((Snapshot)callerPoint.OutSnapshot).CallLevel > ((Snapshot)callOutputs[0]).CallLevel ) 
            {
                extend (callerPoint.OutSnapshot);  // Refresh the original content of the call stack
                CallLevel = ((Snapshot)callOutputs[0]).CallLevel; // To be able to do merging, temporary decrease the call level (will be increased after merging)
            }

            List<Snapshot> snapshots = new List<Snapshot>(callOutputs.Length);
            foreach (ISnapshotReadonly input in callOutputs)
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(input);
                Logger.Log(this, snapshot.getSnapshotIdentification() + " call merge " + snapshot.CallLevel);
                snapshots.Add(snapshot);
            }

            IMergeAlgorithm algorithm;
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    algorithm = MemoryAlgorithmFactories.MergeAlgorithmFactory.CreateInstance();
                    Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.MERGE_WITH_CALL);
                    algorithm.MergeWithCall(this, snapshots);

                    Structure = algorithm.GetMergedStructure();
                    Data = algorithm.GetMergedData();
                    CurrentData = Data;
                    Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.MERGE_WITH_CALL);
                    break;

                case SnapshotMode.InfoLevel:
                    algorithm = InfoAlgorithmFactories.MergeAlgorithmFactory.CreateInstance();
                    Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.MERGE_WITH_CALL);
                    algorithm.MergeWithCall(this, snapshots);

                    Infos = algorithm.GetMergedData();
                    CurrentData = Infos;
                    Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.MERGE_WITH_CALL);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }

            CallLevel = tempCallLevel;
        }

        /// <summary>
        /// Fetch variables from global context into current context
        /// </summary>
        /// <param name="variables">Variables that will be fetched</param>
        /// <example>global x,y;</example>
        protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            Logger.Log(this, "Fetch from global");
            foreach (VariableName name in variables)
            {
                Logger.Log(this, "Fetch from global " + name);
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
            Logger.Log(this, "Get global ");

            List<VariableName> names = new List<VariableName>();

            foreach (var variable in 
                Structure.Readonly.ReadonlyGlobalContext
                .ReadonlyVariables.Indexes)
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
            Logger.Log(this, "Resolve function " + functionName);
            if (Structure.Readonly.IsFunctionDefined(functionName))
            {
                return Structure.Readonly.GetFunction(functionName);
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
            Logger.Log(this, "Resolve type " + typeName);
            if (Structure.Readonly.IsClassDefined(typeName))
            {
                return Structure.Readonly.GetClass(typeName);
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
            Logger.Log(this, "Declare function - " + name);

            var structure = Structure.Writeable;

            if (!structure.IsFunctionDefined(name))
            {
                structure.SetFunction(name, declaration);
            }
            else
            {
                structure.SetFunction(name, declaration);
            }
        }

        /// <summary>
        /// Declare given type into global context
        /// </summary>
        /// <param name="declaration">Declared type</param>
        protected override void declareGlobal(TypeValue declaration)
        {
            QualifiedName name = declaration.QualifiedName;
            Logger.Log(this, "Declare class - " + name);

            var structure = Structure.Writeable;

            if (!structure.IsClassDefined(name))
            {
                structure.SetClass(name, declaration);
            }
            else
            {
                structure.SetClass(name, declaration);
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
            Logger.Log(this, "Get variable - " + variable);
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
            Logger.Log(this, "Get control variable - " + name);
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
            if (entry.Count == 1)
            {
                Value value = entry.PossibleValues.First();
                AssociativeArray array = value as AssociativeArray;
                IArrayDescriptor descriptor;
                if (array != null && Structure.Readonly.TryGetDescriptor(array, out descriptor))
                {
                    TemporaryIndex index = descriptor.ParentIndex as TemporaryIndex;

                    if (index != null)
                    {
                        MemoryEntry indexEntry = Data.Readonly.GetMemoryEntry(index);
                        if (indexEntry.Count == 1 && indexEntry.PossibleValues.First() == value)
                        {
                            return new SnapshotEntry(MemoryPath.MakePathTemporary(index));
                        }
                    }
                }
            }

            Logger.Log(this, "Get entry snap - " + entry);
            return new SnapshotDataEntry(this, entry);
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
            Logger.Log(this, "Get local control variable - " + name.Value);
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
            return Structure.Readonly.GetNumberOfIndexes();
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
            IWriteableSnapshotStructure structure = Structure.Writeable;

            structure.NewIndex(variableIndex);
            structure.GetWriteableStackContext(callLevel).WriteableVariables.AddIndex(variableName, variableIndex);
            CopyMemory(structure.GetWriteableStackContext(callLevel).WriteableVariables.UnknownIndex, variableIndex, false);

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

            IWriteableSnapshotStructure structure = Structure.Writeable;

            structure.NewIndex(variableIndex);
            structure.WriteableGlobalContext.WriteableVariables.AddIndex(variableName, variableIndex);
            CopyMemory(structure.WriteableGlobalContext.WriteableVariables.UnknownIndex, variableIndex, false);

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

            IWriteableSnapshotStructure structure = Structure.Writeable;

            structure.NewIndex(ctrlIndex);
            structure.GetWriteableStackContext(callLevel).WriteableControllVariables.AddIndex(variableName, ctrlIndex);
            CopyMemory(structure.GetWriteableStackContext(callLevel).WriteableControllVariables.UnknownIndex, ctrlIndex, false);

            return ctrlIndex;
        }

        /// <summary>
        /// Creates the global controll variable. Stack level of the local variable is equal to GLOBAL_CALL_LEVEL value.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>Index of newly created variable.</returns>
        internal MemoryIndex CreateGlobalControll(string variableName)
        {
            MemoryIndex ctrlIndex = ControlIndex.Create(variableName, GLOBAL_CALL_LEVEL);

            IWriteableSnapshotStructure structure = Structure.Writeable;

            structure.NewIndex(ctrlIndex);
            structure.WriteableGlobalContext.WriteableControllVariables.AddIndex(variableName, ctrlIndex);
            CopyMemory(structure.WriteableGlobalContext.WriteableControllVariables.UnknownIndex, ctrlIndex, false);

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
            return Structure.Readonly.ReadonlyLocalContext.
                ReadonlyTemporaryVariables.Contains(temporaryIndex);
        }

        /// <summary>
        /// Creates the temporary variable.
        /// </summary>
        /// <returns>Index of newly created variable.</returns>
        internal TemporaryIndex CreateTemporary()
        {
            TemporaryIndex tmp = new TemporaryIndex(CallLevel);
            Structure.Writeable.NewIndex(tmp);
            Structure.Writeable.WriteableLocalContext
                .WriteableTemporaryVariables.Add(tmp);

            return tmp;
        }

        /// <summary>
        /// Releases the temporary variable and clears the memory.
        /// </summary>
        /// <param name="temporaryIndex">Index of the temporary variable.</param>
        internal void ReleaseTemporary(TemporaryIndex temporaryIndex)
        {
            ReleaseMemory(temporaryIndex);
            Structure.Writeable.WriteableLocalContext
                .WriteableTemporaryVariables.Remove(temporaryIndex);
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
            IObjectValueContainer objects = Structure.Readonly
                .GetObjects(index);
            if (objects != null && objects.Count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified index contains only references.
        /// </summary>
        /// <param name="index">The index.</param>
        internal bool ContainsOnlyReferences(MemoryIndex index)
        {
            MemoryEntry entry = Data.Readonly.GetMemoryEntry(index);
            IObjectValueContainer objects = Structure.Readonly
                .GetObjects(index);

            return entry.Count == objects.Count;
        }

        /// <summary>
        /// Makes the must reference to the given object in the target index.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="targetIndex">Index of the target.</param>
        internal void MakeMustReferenceObject(ObjectValue objectValue, MemoryIndex targetIndex)
        {
            IObjectValueContainerBuilder objects = Structure.Readonly
                .GetObjects(targetIndex).Builder();
            objects.Add(objectValue);
            Structure.Writeable.SetObjects(targetIndex, objects.Build());
        }

        /// <summary>
        /// Makes the may reference object to the given object in the target index.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <param name="targetIndex">Index of the target.</param>
        internal void MakeMayReferenceObject(HashSet<ObjectValue> objects, MemoryIndex targetIndex)
        {
            IObjectValueContainerBuilder objectsContainer = Structure.Readonly
                .GetObjects(targetIndex).Builder();

            foreach (ObjectValue objectValue in objects)
            {
                objectsContainer.Add(objectValue);
            }

            Structure.Writeable.SetObjects(targetIndex, objectsContainer.Build());
        }

        /// <summary>
        /// Creates the new instance of the default object in given parent index.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="isMust">if set to <c>true</c> object is must and values in parent indexes are removed.</param>
        /// <param name="removeUndefined">if set to <c>true</c> undefined value is removed from target memory entry.</param>
        /// <returns>New instance of the default object in given parent index.</returns>
        internal ObjectValue CreateObject(MemoryIndex parentIndex, bool isMust, bool removeUndefined = false)
        {
            ObjectValue value = MemoryAssistant.CreateImplicitObject();

            if (isMust)
            {
                DestroyMemory(parentIndex);
                CurrentData.Writeable.SetMemoryEntry(parentIndex, new MemoryEntry(value));
            }
            else
            {
                MemoryEntry oldEntry;

                HashSet<Value> values;
                if (CurrentData.Readonly.TryGetMemoryEntry(parentIndex, out oldEntry))
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
                CurrentData.Writeable.SetMemoryEntry(parentIndex, this.CreateMemoryEntry(values));
            }

            IObjectValueContainerBuilder objectValues = Structure.Readonly.GetObjects(parentIndex).Builder();
            objectValues.Add(value);

            Structure.Writeable.SetObjects(parentIndex, objectValues.Build());
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
            return CreateField(fieldName, Structure.Readonly.GetDescriptor(objectValue), isMust, copyFromUnknown);
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
        internal MemoryIndex CreateField(string fieldName, IObjectDescriptor descriptor, bool isMust, bool copyFromUnknown)
        {
            if (descriptor.ContainsIndex(fieldName))
            {
                throw new Exception("Field " + fieldName + " is already defined");
            }

            MemoryIndex fieldIndex = ObjectIndex.Create(descriptor.ObjectValue, fieldName);
            Structure.Writeable.NewIndex(fieldIndex);

            IObjectDescriptorBuilder builder = descriptor.Builder();
            builder.AddIndex(fieldName, fieldIndex);
            descriptor = builder.Build();

            Structure.Writeable.SetDescriptor(descriptor.ObjectValue, descriptor);

            if (copyFromUnknown)
            {
                CopyMemory(descriptor.UnknownIndex, fieldIndex, isMust);
            }

            return fieldIndex;
        }

        /// <summary>
        /// Destroys the object.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="value">The value.</param>
        internal void DestroyObject(MemoryIndex parentIndex, ObjectValue value)
        {
            IObjectValueContainerBuilder objects = Structure.Readonly
                .GetObjects(parentIndex).Builder();
            objects.Remove(value);
            Structure.Writeable.SetObjects(parentIndex, objects.Build());
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
            if (Structure.Readonly.HasArray(parentIndex))
            {
                throw new Exception("Variable " + parentIndex + " already has associated array value.");
            }

            AssociativeArray value = this.CreateArray();
            IArrayDescriptor oldDescriptor = Structure.Readonly.GetDescriptor(value);
            closeTemporaryArray(oldDescriptor);

            IArrayDescriptorBuilder builder = oldDescriptor.Builder();
            builder.SetParentIndex(parentIndex);
            builder.SetUnknownIndex(parentIndex.CreateUnknownIndex());
            IArrayDescriptor newDescriptor = builder.Build();

            IWriteableSnapshotStructure structure = Structure.Writeable;
            structure.NewIndex(newDescriptor.UnknownIndex);
            structure.SetArray(parentIndex, value);
            structure.SetDescriptor(value, newDescriptor);
            return value;
        }

        /// <summary>
        /// Closes the temporary array which was created on array initialization to prevent orphan arrays.
        /// </summary>
        /// <param name="descriptor">The descriptor of temporary array.</param>
        private void closeTemporaryArray(IArrayDescriptor descriptor)
        {
            TemporaryIndex index = descriptor.ParentIndex as TemporaryIndex;

            if (index != null)
            {
                CurrentData.Writeable.SetMemoryEntry(index, new MemoryEntry());
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
                CurrentData.Writeable.SetMemoryEntry(parentIndex, new MemoryEntry(value));
            }
            else
            {
                HashSet<Value> values;
                MemoryEntry oldEntry;
                if (CurrentData.Readonly.TryGetMemoryEntry(parentIndex, out oldEntry))
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
                CurrentData.Writeable.SetMemoryEntry(parentIndex, this.CreateMemoryEntry(values));
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
            return CreateIndex(indexName, Structure.Readonly.GetDescriptor(arrayValue), isMust, copyFromUnknown);
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
        internal MemoryIndex CreateIndex(string indexName, IArrayDescriptor descriptor, bool isMust, bool copyFromUnknown)
        {
            if (descriptor.ContainsIndex(indexName))
            {
                throw new Exception("Index " + indexName + " is already defined");
            }

            MemoryIndex indexIndex = descriptor.ParentIndex.CreateIndex(indexName);
            Structure.Writeable.NewIndex(indexIndex);

            IArrayDescriptorBuilder builder = descriptor.Builder();
            builder.AddIndex(indexName, indexIndex);
            descriptor = builder.Build();
            
            Structure.Writeable.SetDescriptor(descriptor.ArrayValue, descriptor);

            if (copyFromUnknown)
            {
                CopyMemory(descriptor.UnknownIndex, indexIndex, false);
            }

            return indexIndex;
        }

        /// <summary>
        /// Destroys the array.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        internal void DestroyArray(MemoryIndex parentIndex)
        {
            AssociativeArray arrayValue;
            if (!Structure.Readonly.TryGetArray(parentIndex, out arrayValue))
            {
                return;
            }

            IArrayDescriptor descriptor = Structure.Readonly.GetDescriptor(arrayValue);
            foreach (var index in descriptor.Indexes)
            {
                ReleaseMemory(index.Value);
            }

            ReleaseMemory(descriptor.UnknownIndex);

            Structure.Writeable.RemoveArray(parentIndex, arrayValue);
        }

        #endregion

        #region Memory

        /// <summary>
        /// Determines whether the specified index contains undefined value.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>True whether the specified index contains undefined value.</returns>
        internal bool IsUndefined(MemoryIndex index)
        {
            MemoryEntry entry = CurrentData.Readonly.GetMemoryEntry(index);
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
            IMemoryAlgorithm algorithm = AlgorithmFactories.MemoryAlgorithmFactory.CreateInstance();
            Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.COPY_MEMORY);
            algorithm.CopyMemory(this, sourceIndex, targetIndex, isMust);
            Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.COPY_MEMORY);
        }

        /// <summary>
        /// Destroys the memory of given index and sets the memory of given index to undefined.
        /// </summary>
        /// <param name="index">The index.</param>
        public void DestroyMemory(MemoryIndex index)
        {
            IMemoryAlgorithm algorithm = AlgorithmFactories.MemoryAlgorithmFactory.CreateInstance();
            Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.DELETE_MEMORY);
            algorithm.DestroyMemory(this, index);
            Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.DELETE_MEMORY);
        }

        /// <summary>
        /// Creates the memory entry which contains set of given values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>New memory entry which contains set of given values.</returns>
        public MemoryEntry CreateMemoryEntry(IEnumerable<Value> values)
        {
            IMemoryAlgorithm algorithm = AlgorithmFactories.MemoryAlgorithmFactory.CreateInstance();
            return algorithm.CreateMemoryEntry(this, values);
        }

        /// <summary>
        /// Releases the memory in the given index and removes it from the data model.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void ReleaseMemory(MemoryIndex index)
        {
            DestroyMemory(index);
            DestroyAliases(index);

            Structure.Writeable.RemoveIndex(index);
        }

        /// <summary>
        /// Extends the snapshot.
        /// </summary>
        /// <param name="input">The input.</param>
        private void extendSnapshot(ISnapshotReadonly input)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(input);

            Logger.Log(this, snapshot.getSnapshotIdentification() + " extend");

            IMergeAlgorithm algorithm;
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    algorithm = MemoryAlgorithmFactories.MergeAlgorithmFactory.CreateInstance();
                    Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.EXTEND);
                    algorithm.Extend(this, snapshot);

                    Structure = algorithm.GetMergedStructure();
                    Data = algorithm.GetMergedData();
                    CurrentData = Data;
                    Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.EXTEND);
                    break;

                case SnapshotMode.InfoLevel:
                    algorithm = InfoAlgorithmFactories.MergeAlgorithmFactory.CreateInstance();
                    Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.EXTEND);
                    algorithm.Extend(this, snapshot);

                    Infos = algorithm.GetMergedData();
                    CurrentData = Infos;
                    Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.EXTEND);
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
            var snapshots = inputs.Select (a => SnapshotEntry.ToSnapshot (a)).ToList();

            IMergeAlgorithm algorithm;
            switch (CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    algorithm = MemoryAlgorithmFactories.MergeAlgorithmFactory.CreateInstance();
                    Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.MERGE);
                    algorithm.Merge(this, snapshots);

                    Structure = algorithm.GetMergedStructure();
                    Data = algorithm.GetMergedData();
                    CurrentData = Data;
                    Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.MERGE);
                    break;

                case SnapshotMode.InfoLevel:
                    algorithm = InfoAlgorithmFactories.MergeAlgorithmFactory.CreateInstance();
                    Benchmark.StartAlgorithm(this, algorithm, AlgorithmType.MERGE);
                    algorithm.Merge(this, snapshots);

                    Infos = algorithm.GetMergedData();
                    CurrentData = Infos;
                    Benchmark.FinishAlgorithm(this, algorithm, AlgorithmType.MERGE);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + CurrentMode);
            }
        }

        /// <summary>
        /// Removes the undefined value from memory entry.
        /// </summary>
        /// <param name="memoryIndex">Index of the memory.</param>
        public void RemoveUndefinedFromMemoryEntry(MemoryIndex memoryIndex)
        {
            MemoryEntry oldEntry;
            if (CurrentData.Readonly.TryGetMemoryEntry(memoryIndex, out oldEntry))
            {
                HashSet<Value> values = new HashSet<Value>(oldEntry.PossibleValues);
                if (values.Contains(this.UndefinedValue))
                {
                    values.Remove(this.UndefinedValue);
                    CurrentData.Writeable.SetMemoryEntry(memoryIndex, new MemoryEntry(values));
                }
            }
        }

        #endregion

        #region Aliases

        /// <summary>
        /// Adds the created alias.
        /// </summary>
        /// <param name="aliasData">The alias data.</param>
        public void AddCreatedAlias(IMemoryAlias aliasData)
        {
            createdAliases.Add(aliasData);
        }

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAliases">The must aliases.</param>
        /// <param name="mayAliases">The may aliases.</param>
        public void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            IMemoryAliasBuilder alias;
            IMemoryAlias oldAlias;
            if (Structure.Readonly.TryGetAliases(index, out oldAlias))
            {
                alias = oldAlias.Builder();
            }
            else
            {
                alias = Structure.CreateMemoryAlias(index).Builder();
            }

            if (mustAliases != null)
            {
                alias.MustAliases.AddAll(mustAliases);
            }
            if (mayAliases != null)
            {
                alias.MayAliases.AddAll(mayAliases);
            }

            foreach (MemoryIndex mustIndex in alias.MustAliases)
            {
                if (alias.MayAliases.Contains(mustIndex))
                {
                    alias.MayAliases.Remove(mustIndex);
                }
            }

            IMemoryAlias memoryAlias = alias.Build();
            Structure.Writeable.SetAlias(index, memoryAlias);
            AddCreatedAlias(memoryAlias);
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
            IMemoryAliasBuilder alias;
            IMemoryAlias oldAlias;
            if (Structure.Readonly.TryGetAliases(index, out oldAlias))
            {
                alias = oldAlias.Builder();
            }
            else
            {
                alias = Structure.CreateMemoryAlias(index).Builder();
            }

            if (mustAlias != null)
            {
                alias.MustAliases.Add(mustAlias);

                if (alias.MayAliases.Contains(mustAlias))
                {
                    alias.MayAliases.Remove(mustAlias);
                }
            }

            if (mayAlias != null && !alias.MustAliases.Contains(mayAlias))
            {
                alias.MayAliases.Add(mayAlias);
            }

            IMemoryAlias memoryAlias = alias.Build();
            Structure.Writeable.SetAlias(index, memoryAlias);
            AddCreatedAlias(memoryAlias);
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

            IMemoryAliasBuilder builder = Structure.CreateMemoryAlias(index).Builder();
            builder.MustAliases.AddAll(mustAliases);
            builder.MayAliases.AddAll(mayAliases);

            Structure.Writeable.SetAlias(index, builder.Build());
        }

        /// <summary>
        /// May the set aliases of the given index operation. All must aliases of given index are converted into may.
        /// Alias entry of the given alias indexes are not changed.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mayAliases">The may aliases.</param>
        public void MaySetAliases(MemoryIndex index, HashSet<MemoryIndex> mayAliases)
        {
            IMemoryAliasBuilder builder;
            IMemoryAlias oldAlias;
            if (Structure.Readonly.TryGetAliases(index, out oldAlias))
            {
                builder = oldAlias.Builder();
                convertAliasesToMay(index, builder);
            }
            else
            {
                builder = Structure.CreateMemoryAlias(index).Builder();
            }

            builder.MayAliases.AddAll(mayAliases);
            Structure.Writeable.SetAlias(index, builder.Build());
        }

        /// <summary>
        /// Converts the aliases of given index to may.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="builder">The builder.</param>
        private void convertAliasesToMay(MemoryIndex index, IMemoryAliasBuilder builder)
        {
            foreach (MemoryIndex mustIndex in builder.MustAliases)
            {
                IMemoryAlias alias = Structure.Readonly.GetAliases(mustIndex);

                IMemoryAliasBuilder mustBuilder = Structure.Readonly.GetAliases(mustIndex).Builder();
                mustBuilder.MustAliases.Remove(index);
                mustBuilder.MayAliases.Add(index);
                Structure.Writeable.SetAlias(index, mustBuilder.Build());
            }

            builder.MayAliases.AddAll(builder.MustAliases);
            builder.MustAliases.Clear();
        }

        /// <summary>
        /// Copies the aliases of the source index to target.
        /// </summary>
        /// <param name="sourceIndex">Index of the source.</param>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="isMust">if set to <c>true</c> operation is must otherwise all must aliases are copied as may.</param>
        internal void CopyAliases(MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust)
        {
            IMemoryAlias aliases;
            if (Structure.Readonly.TryGetAliases(sourceIndex, out aliases))
            {
                IMemoryAliasBuilder builder = Structure.CreateMemoryAlias(targetIndex).Builder();
                foreach (MemoryIndex mustAlias in aliases.MustAliases)
                {
                    IMemoryAliasBuilder mustBuilder = Structure.Readonly.GetAliases(mustAlias).Builder();
                    if (isMust)
                    {
                        builder.MustAliases.Add(mustAlias);
                        mustBuilder.MustAliases.Add(targetIndex);
                    }
                    else
                    {
                        builder.MayAliases.Add(mustAlias);
                        mustBuilder.MayAliases.Add(targetIndex);
                    }
                    Structure.Writeable.SetAlias(mustAlias, mustBuilder.Build());
                }

                foreach (MemoryIndex mayAlias in aliases.MayAliases)
                {
                    IMemoryAliasBuilder mayBuilder = Structure.Readonly.GetAliases(mayAlias).Builder();

                    builder.MayAliases.Add(mayAlias);
                    mayBuilder.MayAliases.Add(targetIndex);

                    Structure.Writeable.SetAlias(mayAlias, mayBuilder.Build());
                }

                Structure.Writeable.SetAlias(targetIndex, builder.Build());
            }
        }

        /// <summary>
        /// Destroys the aliases of the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void DestroyAliases(MemoryIndex index)
        {
            IMemoryAlias aliases;
            if (!Structure.Readonly.TryGetAliases(index, out aliases))
            {
                return;
            }

            foreach (MemoryIndex mustIndex in aliases.MustAliases)
            {
                IMemoryAlias alias = Structure.Readonly.GetAliases(mustIndex);
                if (alias.MustAliases.Count == 1 && alias.MayAliases.Count == 0)
                {
                    Structure.Writeable.RemoveAlias(mustIndex);
                }
                else
                {
                    IMemoryAliasBuilder builder = Structure.Readonly.GetAliases(mustIndex).Builder();
                    builder.MustAliases.Remove(index);
                    Structure.Writeable.SetAlias(mustIndex, builder.Build());
                }
            }

            foreach (MemoryIndex mayIndex in aliases.MayAliases)
            {
                IMemoryAlias alias = Structure.Readonly.GetAliases(mayIndex);
                if (alias.MustAliases.Count == 0 && alias.MayAliases.Count == 1)
                {
                    Structure.Writeable.RemoveAlias(mayIndex);
                }
                else
                {
                    IMemoryAliasBuilder builder = Structure.Readonly.GetAliases(mayIndex).Builder();
                    builder.MayAliases.Remove(index);
                    Structure.Writeable.SetAlias(mayIndex, builder.Build());
                }
            }
        }

        #endregion

        #endregion

        #region Private Helpers
        private void extendWithoutComputingCallLevel(ISnapshotReadonly[] inputs)
        {
            if (inputs.Length == 1) {
                extendSnapshot (inputs [0]);
            }
            else
                if (inputs.Length > 1) {
                    mergeSnapshots (inputs);
                }
        }

        private int maxCallLevel(ISnapshotReadonly[] inputs) 
        {
            int callLevel = 0;

            foreach (ISnapshotReadonly input in inputs)
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(input);

                if (snapshot.CallLevel > callLevel)
                {
                    callLevel = snapshot.CallLevel;
                }
            }

            return callLevel;
        }
        #endregion
    }
}