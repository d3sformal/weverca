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
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;
using Weverca.AnalysisFramework.GraphVisualizer;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

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
	/// MemoryIndex for more information.
	/// </summary>
	public class Snapshot : SnapshotBase, IReferenceHolder
	{
		#region Variables and properties

        /// <summary>
        /// Gets the implementation variant with all implementation factories
        /// </summary>
        public ModularMemoryModelFactories Factories { get; private set; }

		/// <summary>
		/// Gets the algorithm factories for the current memory mode.
		/// </summary>
		/// <value>
		/// The algorithm factories.
		/// </value>
		public AlgorithmInstances Algorithms
		{
			get
			{
                return Factories.GetAlgorithms(CurrentMode);
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
        private bool isInitialized;

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
        /// 

		public ISnapshotStructureProxy OldStructure { get; private set; }


        public ISnapshotDataProxy OldData { get; private set; }


        public ISnapshotDataProxy OldInfos { get; private set; }

        public int OldCallLevel { get; private set; }

		public AssignInfo AssignInfo { get; set; }

		public MergeInfo MergeInfo { get; set; }

		/// <summary>
		/// Gets the number of transactions.
		/// </summary>
		/// <value>
		/// The number of transactions.
		/// </value>
		public int NumberOfTransactions { get; private set; }

		public IEnumerable<MemoryIndex> StructureCallChanges { get; set; }

		public IEnumerable<MemoryIndex> DataCallChanges { get; set; }
		
		#endregion


        internal Snapshot(ModularMemoryModelFactories factories)
        {
            SnapshotId = SNAP_ID++;
            Factories = factories;

            Data = Factories.SnapshotDataFactory.CopyInstance(Factories.InitialSnapshotDataInstance);
            Infos = Factories.SnapshotDataFactory.CopyInstance(Factories.InitialSnapshotInfoInstance);
            Structure = Factories.SnapshotStructureFactory.CopyInstance(Factories.InitialSnapshotStructureInstance);

            CallLevel = GLOBAL_CALL_LEVEL;
            OldCallLevel = GLOBAL_CALL_LEVEL;

            NumberOfTransactions = 0;
            CurrentData = Data;

            isInitialized = true;

            Factories.Benchmark.InitializeSnapshot(this);
            Factories.Logger.Log(this, "Constructed snapshot");
        }


		/// <summary>
		/// Gets the snapshot identification which consists of snapshot and data ID separated by colons.
		/// </summary>
		/// <returns>String version snapshot identification which consists of snapshot and data ID separated by colons.</returns>
		public String getSnapshotIdentification()
		{
			return CallLevel + "." + SnapshotId.ToString() + "::s" + Structure.Readonly.StructureId.ToString() + "::d" + Data.Readonly.DataId;
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
            return Algorithms.PrintAlgorithm.SnapshotToString(this);
		}

		#region AbstractSnapshot Implementation

		#region Transaction

		/// <summary>
		/// Start snapshot transaction - changes can be proceeded only when transaction is started
		/// </summary>
		/// <exception cref="System.NotSupportedException">Current mode:  + CurrentMode</exception>
		protected override void startTransaction()
        {
            Factories.Benchmark.StartOperation(this);
			Factories.Benchmark.StartTransaction(this);
            
			Factories.Logger.Log(this, "Start mode: {0}", CurrentMode);

			OldCallLevel = CallLevel;
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

            Factories.Benchmark.FinishOperation(this);
		}

		/// <summary>
		/// Starts the data transaction.
		/// </summary>
		private void startDataTransaction()
		{
			OldStructure = Structure;
			OldData = Data;

            Data = Factories.SnapshotDataFactory.CopyInstance(OldData);
            Structure = Factories.SnapshotStructureFactory.CopyInstance(OldStructure);
			CurrentData = Data;

			Structure.Locked = false;
		}

		/// <summary>
		/// Starts the information transaction.
		/// </summary>
		private void startInfoTransaction()
		{
			OldInfos = Infos;
            Infos = Factories.SnapshotDataFactory.CopyInstance(OldInfos);
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Commit");
            
			Factories.Benchmark.StartAlgorithm(this, AlgorithmType.COMMIT);
            bool differs = Algorithms.CommitAlgorithm.CommitAndSimplify(this, simplifyLimit);
			Factories.Benchmark.FinishAlgorithm(this, AlgorithmType.COMMIT);

			Factories.Logger.LogToSameLine(" " + differs);
			Factories.Logger.Log(this);
			Factories.Logger.Log("\n---------------------------------\n");

            Factories.Benchmark.FinishOperation(this);
			Factories.Benchmark.FinishTransaction(this);

			OldStructure = null;
			OldData = null;
			OldInfos = null;

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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Commit and widen");
            
			Factories.Benchmark.StartAlgorithm(this, AlgorithmType.WIDEN_COMMIT);
            bool differs = Algorithms.CommitAlgorithm.CommitAndWiden(this, simplifyLimit);
			Factories.Benchmark.FinishAlgorithm(this, AlgorithmType.WIDEN_COMMIT);

			Factories.Logger.LogToSameLine(" " + differs);
			Factories.Logger.Log(this);
			Factories.Logger.Log("\n---------------------------------\n");

			Factories.Benchmark.FinishTransaction(this);

			OldStructure = null;
			OldData = null;
			OldInfos = null;

            Factories.Benchmark.FinishOperation(this);
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Init object " + createdObject + " " + type);

            IObjectDescriptor existing;
            if (!Structure.Readonly.TryGetDescriptor(createdObject, out existing))
            {
                IObjectDescriptor descriptor = Factories.StructuralContainersFactories.ObjectDescriptorFactory.CreateObjectDescriptor(Structure.Writeable, createdObject, type, ObjectIndex.CreateUnknown(createdObject));
                Structure.Writeable.NewIndex(descriptor.UnknownIndex);
                Structure.Writeable.SetDescriptor(createdObject, descriptor);
            }

            Factories.Benchmark.FinishOperation(this);
		}

		/// <summary>
		/// Iterates the object.
		/// </summary>
		/// <param name="iteratedObject">The iterated object.</param>
		/// <returns>List of all fields of the given object.</returns>
		/// <exception cref="System.Exception">Unknown object</exception>
		protected IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject)
        {
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Iterate object " + iteratedObject);
			
            IObjectDescriptor descriptor;
            List<ContainerIndex> indexes;
			if (Structure.Readonly.TryGetDescriptor(iteratedObject, out descriptor))
			{
				indexes = new List<ContainerIndex>();
				foreach (var index in descriptor.Indexes)
				{
					indexes.Add(this.CreateIndex(index.Key));
				}
			}
			else
			{
				throw new Exception("Unknown object");
            }

            Factories.Benchmark.FinishOperation(this);
            return indexes;
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Get object type " + objectValue);

			IObjectDescriptor descriptor;
			if (!Structure.Readonly.TryGetDescriptor(objectValue, out descriptor))
			{
				throw new Exception("Unknown object");
            }

            Factories.Benchmark.FinishOperation(this);
            return descriptor.Type;
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
            Factories.Benchmark.StartOperation(this);
			var objectMethods = Weverca.MemoryModels.VirtualReferenceModel.TypeMethodResolver.ResolveMethods(value, this);
            IEnumerable<FunctionValue> methods = Assistant.ResolveMethods(value, methodName, objectMethods);

            Factories.Benchmark.FinishOperation(this);
            return methods;
		}

		/// <summary>
		/// Resolves the method.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="methodName">Name of the method.</param>
		/// <returns>List of methods with the given name for objects within the given memory entry.</returns>
		internal IEnumerable<FunctionValue> resolveMethod(MemoryEntry entry, QualifiedName methodName)
        {
            Factories.Benchmark.StartOperation(this);
			HashSet<FunctionValue> functions = new HashSet<FunctionValue>();
			foreach (Value value in entry.PossibleValues)
			{
				CollectionMemoryUtils.AddAll(functions, resolveMethod(value, methodName));
			}

            Factories.Benchmark.FinishOperation(this);
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
            Factories.Benchmark.StartOperation(this);
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

            Factories.Benchmark.FinishOperation(this);
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Init array " + createdArray);
			TemporaryIndex arrayIndex = CreateTemporary();

			IArrayDescriptor exist;
			if (Structure.Readonly.TryGetDescriptor(createdArray, out exist))
			{
				Factories.Logger.Log(this, "This array has been already initialised: " + createdArray);
			}

            IArrayDescriptor descriptor = Factories.StructuralContainersFactories.ArrayDescriptorFactory.CreateArrayDescriptor(Structure.Writeable, createdArray, arrayIndex);
			Structure.Writeable.NewIndex(descriptor.UnknownIndex);
			Structure.Writeable.SetArray(arrayIndex, createdArray);
			Structure.Writeable.SetDescriptor(createdArray, descriptor);

			Data.Writeable.SetMemoryEntry(arrayIndex, new MemoryEntry(createdArray));
            Data.Writeable.SetMemoryEntry(descriptor.UnknownIndex, new MemoryEntry(UndefinedValue));

            Factories.Benchmark.FinishOperation(this);
		}

		/// <summary>
		/// Iterates the array.
		/// </summary>
		/// <param name="iteratedArray">The iterated array.</param>
		/// <returns>List of indexes of the array.</returns>
		/// <exception cref="System.Exception">Unknown associative array</exception>
		protected IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray)
        {
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Iterate array " + iteratedArray);
			IArrayDescriptor descriptor;

            List<ContainerIndex> indexes;
			if (Structure.Readonly.TryGetDescriptor(iteratedArray, out descriptor))
			{
				indexes = new List<ContainerIndex>();
				foreach (var index in descriptor.Indexes)
				{
					indexes.Add(this.CreateIndex(index.Key));
				}
			}
			else
			{
				throw new Exception("Unknown associative array");
            }

            Factories.Benchmark.FinishOperation(this);
            return indexes;
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "extend");

            if (inputs.Length == 1)
            {
                extendSnapshot(inputs[0]);
            }
            else if (inputs.Length > 1)
            {
                mergeSnapshots(inputs);
            }

            Factories.Benchmark.FinishOperation(this);
		}

		/// <inheritdoc />
		protected override void extendAtSubprogramEntry(ISnapshotReadonly[] inputs, ProgramPointBase[] extendedPoints)
        {
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "merge at subprogram");

			List<Snapshot> snapshots = new List<Snapshot>();
			foreach (var item in inputs)
			{
				Snapshot s = SnapshotEntry.ToSnapshot(item);
				snapshots.Add(s);

				Factories.Logger.Log(this, "merge " + s.getSnapshotIdentification());
			}
                        
            Factories.Benchmark.StartAlgorithm(this, AlgorithmType.MERGE_AT_SUBPROGRAM);
            Algorithms.MergeAlgorithm.MergeAtSubprogram(this, snapshots, extendedPoints);
            Factories.Benchmark.FinishAlgorithm(this, AlgorithmType.MERGE_AT_SUBPROGRAM);

			// Call levels of the caller should be always the same
			Debug.Assert(OldCallLevel == GLOBAL_CALL_LEVEL || OldCallLevel == CallLevel);
			Debug.Assert(Structure.Readonly.CallLevel == CallLevel);


			/*CallLevel = maxCallLevel(inputs);

			extendWithoutComputingCallLevel(inputs);*/

            Factories.Benchmark.FinishOperation(this);
		}

		/// <inheritdoc />
		protected override void extendAtCatchEntry(ISnapshotReadonly[] inputs, CatchBlockDescription catchDescription)
        {
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "extend");

			CallLevel = SnapshotEntry.ToSnapshot (catchDescription.TargetPoint.OutSnapshot).CallLevel;

            if (inputs.Length == 1)
            {
                extendSnapshot(inputs[0]);
            }
            else if (inputs.Length > 1)
            {
                mergeSnapshots(inputs);
            }

            Factories.Benchmark.FinishOperation(this);
		}

		/// <inheritdoc />
		protected override void extendAsCall(SnapshotBase callerContext, ProgramPointGraph callee, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            Factories.Benchmark.StartOperation(this);
			Snapshot callerSnapshot = SnapshotEntry.ToSnapshot(callerContext);
			Factories.Logger.Log(this, "call extend: " + callerSnapshot.getSnapshotIdentification() + " level: " + callerSnapshot.CallLevel + " this: " + thisObject);


			/*CallLevel = callerSnapshot.CallLevel + 1;

			if (oldCallLevel != CallLevel && oldCallLevel != GLOBAL_CALL_LEVEL) 
			{
				// The called function is shared and we are calling it repeatedly
				// Pick the call level from the previous call of extendAsCall
				CallLevel = oldCallLevel;
			}*/
            
            Factories.Benchmark.StartAlgorithm(this, AlgorithmType.EXTEND_AS_CALL);
            Algorithms.MergeAlgorithm.ExtendAsCall(this, callerSnapshot, callee, thisObject);
            Factories.Benchmark.FinishAlgorithm(this, AlgorithmType.EXTEND_AS_CALL);

			if (thisObject != null)
			{
				ReadWriteSnapshotEntryBase snapshotEntry = 
					SnapshotEntry.CreateVariableEntry(new VariableIdentifier(Snapshot.THIS_VARIABLE_IDENTIFIER), GlobalContext.LocalOnly, this.CallLevel);
				snapshotEntry.WriteMemory(this, thisObject);
			}

			// Call levels of the caller should be always the same
			Debug.Assert(OldCallLevel == GLOBAL_CALL_LEVEL || OldCallLevel == CallLevel);
            Debug.Assert(Structure.Readonly.CallLevel == CallLevel);

            Factories.Benchmark.FinishOperation(this);
		}

		/// <summary>
		/// Merge given call output with current context.
		/// WARNING: Call can change many objects via references (they don't has to be in global context)
		/// </summary>
		/// <param name="callOutputs">Output snapshots of call level</param>
		/// <exception cref="System.NotSupportedException">Current mode:  + CurrentMode</exception>
		protected override void mergeWithCallLevel(ProgramPointBase callerPoint, ISnapshotReadonly[] callOutputs)
        {
            Factories.Benchmark.StartOperation(this);
			//throw new NotImplementedException("Merging function call is not implemented");

			Snapshot callSnapshot = (Snapshot)callerPoint.OutSnapshot;

			CallLevel = callSnapshot.CallLevel;
			/*var tempCallLevel = CallLevel;
			// In case of shared functions, the call level of the caller can be bigger than call levels of the callee.
			// Se extendAsCall method.
			if (((Snapshot)callerPoint.OutSnapshot).CallLevel > ((Snapshot)callOutputs[0]).CallLevel ) 
			{
				extend (callerPoint.OutSnapshot);  // Refresh the original content of the call stack
				CallLevel = ((Snapshot)callOutputs[0]).CallLevel; // To be able to do merging, temporary decrease the call level (will be increased after merging)
			}*/

			List<Snapshot> snapshots = new List<Snapshot>(callOutputs.Length);
			foreach (ISnapshotReadonly input in callOutputs)
			{
				Snapshot snapshot = SnapshotEntry.ToSnapshot(input);
				Factories.Logger.Log(this, snapshot.getSnapshotIdentification() + " call merge " + snapshot.CallLevel);
				snapshots.Add(snapshot);
			}
            
            Factories.Benchmark.StartAlgorithm(this, AlgorithmType.MERGE_WITH_CALL);
            Algorithms.MergeAlgorithm.MergeWithCall(this, callSnapshot, snapshots);
            Factories.Benchmark.FinishAlgorithm(this, AlgorithmType.MERGE_WITH_CALL);

            Debug.Assert(Structure.Readonly.CallLevel == CallLevel);
            Factories.Benchmark.FinishOperation(this);
		}

		/// <summary>
		/// Fetch variables from global context into current context
		/// </summary>
		/// <param name="variables">Variables that will be fetched</param>
		/// <example>global x,y;</example>
		protected override void fetchFromGlobal(IEnumerable<VariableName> variables)
        {
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Fetch from global");
			foreach (VariableName name in variables)
			{
				Factories.Logger.Log(this, "Fetch from global " + name);
				ReadWriteSnapshotEntryBase localEntry = getVariable(new VariableIdentifier(name), false);
				ReadWriteSnapshotEntryBase globalEntry = getVariable(new VariableIdentifier(name), true);

				localEntry.SetAliases(this, globalEntry);
            }

            Factories.Benchmark.FinishOperation(this);
		}

		/// <summary>
		/// Get all variables defined in global scope
		/// </summary>
		/// <returns>
		/// Variables defined in global scope
		/// </returns>
		protected override IEnumerable<VariableName> getGlobalVariables()
        {
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Get global ");

			List<VariableName> names = new List<VariableName>();

			foreach (var variable in 
				Structure.Readonly.ReadonlyGlobalContext
				.ReadonlyVariables.Indexes)
			{
				names.Add(new VariableName(variable.Key));
			}

            Factories.Benchmark.FinishOperation(this);
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Resolve function " + functionName);

            IEnumerable<FunctionValue> functions;
			if (Structure.Readonly.IsFunctionDefined(functionName))
			{
				functions = Structure.Readonly.GetFunction(functionName);
			}
			else
			{
				functions = new List<FunctionValue>(0);
			}

            Factories.Benchmark.FinishOperation(this);
            return functions;
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Resolve type " + typeName);

            IEnumerable<TypeValue> types;
			if (Structure.Readonly.IsClassDefined(typeName))
			{
				types = Structure.Readonly.GetClass(typeName);
			}
			else
			{
				types = new List<TypeValue>(0);
            }

            Factories.Benchmark.FinishOperation(this);
            return types;
		}

		/// <summary>
		/// Declare given function into global context
		/// </summary>
		/// <param name="declaration">Declared function</param>
		protected override void declareGlobal(FunctionValue declaration)
        {
            Factories.Benchmark.StartOperation(this);
			QualifiedName name = new QualifiedName(declaration.Name);
			Factories.Logger.Log(this, "Declare function - " + name);

			var structure = Structure.Writeable;

			if (!structure.IsFunctionDefined(name))
			{
				structure.AddFunctiondeclaration(name, declaration);
			}
			else
			{
				structure.AddFunctiondeclaration(name, declaration);
            }

            Factories.Benchmark.FinishOperation(this);
		}

		/// <summary>
		/// Declare given type into global context
		/// </summary>
		/// <param name="declaration">Declared type</param>
		protected override void declareGlobal(TypeValue declaration)
        {
            Factories.Benchmark.StartOperation(this);
			QualifiedName name = declaration.QualifiedName;
			Factories.Logger.Log(this, "Declare class - " + name);

			var structure = Structure.Writeable;

			if (!structure.IsClassDefined(name))
			{
				structure.AddClassDeclaration(name, declaration);
			}
			else
			{
				structure.AddClassDeclaration(name, declaration);
            }

            Factories.Benchmark.FinishOperation(this);
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Get variable - " + variable);

            ReadWriteSnapshotEntryBase snapshotEntry;
			if (forceGlobalContext)
			{
				snapshotEntry = SnapshotEntry.CreateVariableEntry(variable, GlobalContext.GlobalOnly);
			}
			else
			{
				snapshotEntry = SnapshotEntry.CreateVariableEntry(variable, GlobalContext.LocalOnly, CallLevel);
            }

            Factories.Benchmark.FinishOperation(this);
            return snapshotEntry;
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Get control variable - " + name);

			ReadWriteSnapshotEntryBase snapshotEntry = SnapshotEntry.CreateControlEntry(name, GlobalContext.GlobalOnly);

            Factories.Benchmark.FinishOperation(this);
            return snapshotEntry;
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
            Factories.Benchmark.StartOperation(this);
            Factories.Logger.Log(this, "Get entry snap - " + entry);

            ReadWriteSnapshotEntryBase snapshotEntry = null;
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
						MemoryEntry indexEntry = SnapshotDataUtils.GetMemoryEntry(this, Data.Readonly, index);
						if (indexEntry.Count == 1 && indexEntry.PossibleValues.First() == value)
						{
							snapshotEntry = new SnapshotEntry(MemoryPath.MakePathTemporary(index));
						}
					}
				}
			}

            if (snapshotEntry == null)
            {
                snapshotEntry = new SnapshotDataEntry(this, entry);
            }

            Factories.Benchmark.FinishOperation(this);
            return snapshotEntry;
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
            Factories.Benchmark.StartOperation(this);
			Factories.Logger.Log(this, "Get local control variable - " + name.Value);

            ReadWriteSnapshotEntryBase snapshotEntry = SnapshotEntry.CreateControlEntry(name, GlobalContext.LocalOnly, CallLevel);

            Factories.Benchmark.FinishOperation(this);
            return snapshotEntry;
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
            MemoryEntry entry = SnapshotDataUtils.GetMemoryEntry(this, Data.Readonly, index);
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
			var writeableStructure = Structure.Writeable;
			IObjectValueContainerBuilder objects = Structure.Readonly
				.GetObjects(targetIndex).Builder(writeableStructure);
			objects.Add(objectValue);
			writeableStructure.SetObjects(targetIndex, objects.Build(writeableStructure));
		}

		/// <summary>
		/// Makes the may reference object to the given object in the target index.
		/// </summary>
		/// <param name="objects">The objects.</param>
		/// <param name="targetIndex">Index of the target.</param>
		internal void MakeMayReferenceObject(HashSet<ObjectValue> objects, MemoryIndex targetIndex)
		{
			var writeableStructure = Structure.Writeable;
			IObjectValueContainerBuilder objectsContainer = Structure.Readonly
				.GetObjects(targetIndex).Builder(writeableStructure);

			foreach (ObjectValue objectValue in objects)
			{
				objectsContainer.Add(objectValue);
			}

			writeableStructure.SetObjects(targetIndex, objectsContainer.Build(writeableStructure));
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

			var writeableStructure = Structure.Writeable;
			IObjectValueContainerBuilder objectValues = Structure.Readonly.GetObjects(parentIndex).Builder(writeableStructure);
			objectValues.Add(value);

			writeableStructure.SetObjects(parentIndex, objectValues.Build(writeableStructure));
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

			var writeableStructure = Structure.Writeable;
			MemoryIndex fieldIndex = ObjectIndex.Create(descriptor.ObjectValue, fieldName);
			writeableStructure.NewIndex(fieldIndex);

			IObjectDescriptorBuilder builder = descriptor.Builder(writeableStructure);
			builder.AddIndex(fieldName, fieldIndex);
			descriptor = builder.Build(writeableStructure);

			writeableStructure.SetDescriptor(descriptor.ObjectValue, descriptor);

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
			var writeableStructure = Structure.Writeable;
			IObjectValueContainerBuilder objects = Structure.Readonly
				.GetObjects(parentIndex).Builder(writeableStructure);
			objects.Remove(value);
			writeableStructure.SetObjects(parentIndex, objects.Build(writeableStructure));
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

			IWriteableSnapshotStructure structure = Structure.Writeable;
			IArrayDescriptorBuilder builder = oldDescriptor.Builder(structure);
			builder.SetParentIndex(parentIndex);
			builder.SetUnknownIndex(parentIndex.CreateUnknownIndex());
			IArrayDescriptor newDescriptor = builder.Build(structure);

			structure.NewIndex(newDescriptor.UnknownIndex);
			structure.SetArray(parentIndex, value);
			structure.SetDescriptor(value, newDescriptor);

			Data.Writeable.SetMemoryEntry(newDescriptor.UnknownIndex, new MemoryEntry(UndefinedValue));
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

			var structure = Structure.Writeable;

			structure.NewIndex(indexIndex);

			IArrayDescriptorBuilder builder = descriptor.Builder(structure);
			builder.AddIndex(indexName, indexIndex);
			descriptor = builder.Build(structure);

			structure.SetDescriptor(descriptor.ArrayValue, descriptor);

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
            MemoryEntry entry = SnapshotDataUtils.GetMemoryEntry(this, CurrentData.Readonly, index);
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
            Algorithms.MemoryAlgorithm.CopyMemory(this, sourceIndex, targetIndex, isMust);
		}

		/// <summary>
		/// Destroys the memory of given index and sets the memory of given index to undefined.
		/// </summary>
		/// <param name="index">The index.</param>
		public void DestroyMemory(MemoryIndex index)
        {
            Algorithms.MemoryAlgorithm.DestroyMemory(this, index);
		}

		/// <summary>
		/// Creates the memory entry which contains set of given values.
		/// </summary>
		/// <param name="values">The values.</param>
		/// <returns>New memory entry which contains set of given values.</returns>
		public MemoryEntry CreateMemoryEntry(IEnumerable<Value> values)
        {
            IMemoryAlgorithm algorithm = Algorithms.MemoryAlgorithm;
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

			Factories.Logger.Log(this, snapshot.getSnapshotIdentification() + " extend");
            
            Factories.Benchmark.StartAlgorithm(this, AlgorithmType.EXTEND);
            Algorithms.MergeAlgorithm.Extend(this, snapshot);
            Factories.Benchmark.FinishAlgorithm(this, AlgorithmType.EXTEND);
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
			List<Snapshot> snapshots = new List<Snapshot>();
			foreach (var item in inputs)
			{
				Snapshot s = SnapshotEntry.ToSnapshot(item);
				snapshots.Add(s);

				Factories.Logger.Log(this, "merge " + s.getSnapshotIdentification());
			}
            
            Factories.Benchmark.StartAlgorithm(this, AlgorithmType.MERGE);
            Algorithms.MergeAlgorithm.Merge(this, snapshots);
            Factories.Benchmark.FinishAlgorithm(this, AlgorithmType.MERGE);
			
			Debug.Assert(Structure.Readonly.CallLevel == CallLevel);
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

		public void AssignCreatedAliases(Snapshot snapshot, ISnapshotDataProxy data)
		{
			if (snapshot.AssignInfo != null)
			{
				List<Tuple<MemoryIndex, HashSet<Value>>> valuesToAssign = new List<Tuple<MemoryIndex, HashSet<Value>>>();

				foreach (var item in snapshot.AssignInfo.AliasAssignModifications.Modifications)
				{
					MemoryIndex index = item.Key;
					MemoryIndexModification indexModification = item.Value;

					HashSet<Value> values = new HashSet<Value>();
					valuesToAssign.Add(new Tuple<MemoryIndex, HashSet<Value>>(index, values));

					foreach (var datasource in indexModification.Datasources)
					{
						MemoryEntry entry;
						
						ISnapshotDataProxy infos;
						if (snapshot == datasource.SourceSnapshot)
						{
							infos = data;
						}
						else
						{
							infos = datasource.SourceSnapshot.Infos;
						}

						if (infos.Readonly.TryGetMemoryEntry(datasource.SourceIndex, out entry))
						{
							CollectionMemoryUtils.AddAll(values, entry.PossibleValues);
						}
					}
				}

				foreach (var item in valuesToAssign)
				{
					MemoryIndex index = item.Item1;
					HashSet<Value> values = item.Item2;

					MemoryEntry entry = new MemoryEntry(values);
					data.Writeable.SetMemoryEntry(index, entry);
				}
			}
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
			var writeableStructure = Structure.Writeable;
			IMemoryAliasBuilder alias;
			IMemoryAlias oldAlias;
			if (Structure.Readonly.TryGetAliases(index, out oldAlias))
			{
				alias = oldAlias.Builder(writeableStructure);
			}
			else
			{
                alias = Factories.StructuralContainersFactories.MemoryAliasFactory.CreateMemoryAlias(writeableStructure, index).Builder(writeableStructure);
			}

			if (mustAliases != null)
			{
				addAllAliases(index, mustAliases, alias.MustAliases);
			}
			if (mayAliases != null)
			{
				addAllAliases(index, mayAliases, alias.MayAliases);
			}

			foreach (MemoryIndex mustIndex in alias.MustAliases)
			{
				if (alias.MayAliases.Contains(mustIndex))
				{
					alias.MayAliases.Remove(mustIndex);
				}
			}

			IMemoryAlias memoryAlias = alias.Build(writeableStructure);
			writeableStructure.SetAlias(index, memoryAlias);
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
			var writeableStructure = Structure.Writeable;
			IMemoryAliasBuilder alias;
			IMemoryAlias oldAlias;
			if (Structure.Readonly.TryGetAliases(index, out oldAlias))
			{
				alias = oldAlias.Builder(writeableStructure);
			}
			else
			{
                alias = Factories.StructuralContainersFactories.MemoryAliasFactory.CreateMemoryAlias(writeableStructure, index).Builder(writeableStructure);
			}

			if (mustAlias != null && !mustAlias.Equals(index))
			{
				alias.MustAliases.Add(mustAlias);

				if (alias.MayAliases.Contains(mustAlias))
				{
					alias.MayAliases.Remove(mustAlias);
				}
			}

			if (mayAlias != null && !mayAlias.Equals(index) && !alias.MustAliases.Contains(mayAlias))
			{
				alias.MayAliases.Add(mayAlias);
			}

			IMemoryAlias memoryAlias = alias.Build(writeableStructure);
			writeableStructure.SetAlias(index, memoryAlias);
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
			var writeableStructure = Structure.Writeable;
			DestroyAliases(index);

            IMemoryAliasBuilder builder = Factories.StructuralContainersFactories.MemoryAliasFactory.CreateMemoryAlias(writeableStructure, index).Builder(writeableStructure);
			addAllAliases(index, mustAliases, builder.MustAliases);
			addAllAliases(index, mayAliases, builder.MayAliases);

			writeableStructure.SetAlias(index, builder.Build(writeableStructure));
		}

		/// <summary>
		/// Must the set aliases of the given index operation. Clears old alias entry if is set and set new alias to given index.
		/// Alias entry of the given alias indexes are not changed.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="mustAliases">The must aliases.</param>
		/// <param name="mayAliases">The may aliases.</param>
		public void MustSetAliasesWithoutDelete(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
		{
			var writeableStructure = Structure.Writeable;

            IMemoryAliasBuilder builder = Factories.StructuralContainersFactories.MemoryAliasFactory.CreateMemoryAlias(writeableStructure, index).Builder(writeableStructure);
			addAllAliases(index, mustAliases, builder.MustAliases);
			addAllAliases(index, mayAliases, builder.MayAliases);

			writeableStructure.SetAlias(index, builder.Build(writeableStructure));
		}

		/// <summary>
		/// May the set aliases of the given index operation. All must aliases of given index are converted into may.
		/// Alias entry of the given alias indexes are not changed.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="mayAliases">The may aliases.</param>
		public void MaySetAliases(MemoryIndex index, IEnumerable<MemoryIndex> mayAliases)
		{
			var writeableStructure = Structure.Writeable;
			IMemoryAliasBuilder builder;
			IMemoryAlias oldAlias;
			if (Structure.Readonly.TryGetAliases(index, out oldAlias))
			{
				builder = oldAlias.Builder(writeableStructure);
				ConvertAliasesToMay(index, builder);
			}
			else
			{
                builder = Factories.StructuralContainersFactories.MemoryAliasFactory.CreateMemoryAlias(writeableStructure, index).Builder(writeableStructure);
			}

			builder.MayAliases.AddAll(mayAliases);
			writeableStructure.SetAlias(index, builder.Build(writeableStructure));
		}

		/// <summary>
		/// Converts the aliases of given index to may.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="builder">The builder.</param>
		public void ConvertAliasesToMay(MemoryIndex index, IMemoryAliasBuilder builder)
		{
			var writeableStructure = Structure.Writeable;
			foreach (MemoryIndex mustIndex in builder.MustAliases)
			{
				IMemoryAlias alias = Structure.Readonly.GetAliases(mustIndex);

				IMemoryAliasBuilder mustBuilder = Structure.Readonly.GetAliases(mustIndex).Builder(writeableStructure);
				mustBuilder.MustAliases.Remove(index);
				mustBuilder.MayAliases.Add(index);
				writeableStructure.SetAlias(index, mustBuilder.Build(writeableStructure));
			}

			addAllAliases(index, builder.MustAliases, builder.MayAliases);
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
				var writeableStructure = Structure.Writeable;

                IMemoryAliasBuilder builder = Factories.StructuralContainersFactories.MemoryAliasFactory.CreateMemoryAlias(writeableStructure, targetIndex).Builder(writeableStructure);
				foreach (MemoryIndex mustAlias in aliases.MustAliases)
				{
					if (mustAlias.Equals(targetIndex))
					{
						continue;
					}

					IMemoryAliasBuilder mustBuilder = Structure.Readonly.GetAliases(mustAlias).Builder(writeableStructure);
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
					writeableStructure.SetAlias(mustAlias, mustBuilder.Build(writeableStructure));
				}

				foreach (MemoryIndex mayAlias in aliases.MayAliases)
				{
					if (mayAlias.Equals(targetIndex))
					{
						continue;
					}

					IMemoryAliasBuilder mayBuilder = Structure.Readonly.GetAliases(mayAlias).Builder(writeableStructure);

					builder.MayAliases.Add(mayAlias);
					mayBuilder.MayAliases.Add(targetIndex);

					writeableStructure.SetAlias(mayAlias, mayBuilder.Build(writeableStructure));
				}

				writeableStructure.SetAlias(targetIndex, builder.Build(writeableStructure));
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

			var writeableStructure = Structure.Writeable;

			foreach (MemoryIndex mustIndex in aliases.MustAliases)
			{
				IMemoryAlias alias = Structure.Readonly.GetAliases(mustIndex);
				if (alias.MustAliases.Count == 1 && alias.MayAliases.Count == 0)
				{
					writeableStructure.RemoveAlias(mustIndex);
				}
				else
				{
					IMemoryAliasBuilder builder = Structure.Readonly.GetAliases(mustIndex).Builder(writeableStructure);
					builder.MustAliases.Remove(index);
					writeableStructure.SetAlias(mustIndex, builder.Build(writeableStructure));
				}
			}

			foreach (MemoryIndex mayIndex in aliases.MayAliases)
			{
				IMemoryAlias alias = Structure.Readonly.GetAliases(mayIndex);
				if (alias.MustAliases.Count == 0 && alias.MayAliases.Count == 1)
				{
					writeableStructure.RemoveAlias(mayIndex);
				}
				else
				{
					IMemoryAliasBuilder builder = Structure.Readonly.GetAliases(mayIndex).Builder(writeableStructure);
					builder.MayAliases.Remove(index);
					writeableStructure.SetAlias(mayIndex, builder.Build(writeableStructure));
				}
			}
		}

		private void addAllAliases(MemoryIndex targetIndex, IEnumerable<MemoryIndex> sourceAliases, IWriteableSet<MemoryIndex> targetAliases)
		{
			foreach (MemoryIndex alias in sourceAliases)
			{
				if (!targetIndex.Equals(alias))
				{
					targetAliases.Add(alias);
				}
			}
		}

		#endregion

		#endregion

		#region Private Helpers

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

        internal void SetMemoryMergeResult(int callLevel, ISnapshotStructureProxy structure, ISnapshotDataProxy data)
        {
            Structure = structure;
            Data = data;
            CurrentData = data;
            CallLevel = callLevel;
        }

        internal void SetInfoMergeResult(ISnapshotDataProxy data)
        {
            Infos = data;
            CurrentData = Infos;
        }
    }
}