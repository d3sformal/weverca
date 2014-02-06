﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Mode enumeration used for switching snapshots operational mode.
    /// </summary>
    public enum SnapshotMode
    {
        /// <summary>
        /// Standard mode used in first phase of analysis. Snapshot
        /// creates abstraction of memory level access. Structure of data structures (arrays, objects,..)
        /// can be changed.
        /// 
        /// Extensions/merges are processed only on memory level
        /// </summary>
        MemoryLevel,

        /// <summary>
        /// Mode used for next phases of analysis. Snapshot creates
        /// abstraction of info level access for data structures. Writings/Readings are targeted  
        /// to info memory, that is organized accordingly data structures in memory level
        /// 
        /// Extensions/merges are processed only on info level
        /// </summary>
        InfoLevel
    }

    /// <summary>
    /// Represents memory snapshot used for fix point analysis.
    /// NOTES:
    ///     * Snapshot will always work in context of some object.
    ///     * Global variables behave in different way than objects variables (cross call stack)
    ///     * Is correct for implementers to cast input Objects, Arrays, Snapshots to concrete implementation - Two snapshot implementations never won't be used together
    /// </summary>
    /// <remarks>
    ///
    /// Iteration looks like:
    /// StartTransaction
    /// ..changes in snapshot
    /// CommitTransaction
    ///
    /// If snapshot has changed against state before start transaction is determined by HasChanged
    /// </remarks>
    public abstract class SnapshotBase : ISnapshotReadWrite
    {
        /// <summary>
        /// Singleton variable where return value is stored
        /// </summary>
        public static readonly VariableName ReturnValue = new VariableName(".return");

        /// <summary>
        /// Returns the number of variables in the snapshot.
        /// </summary>
        /// <returns>the number of variables in the snapshot</returns>
        public virtual int NumVariables()
        {
            return 0;
        }

        /// <summary>
        /// Statistics object - here are stored statistics
        /// </summary>
        private SnapshotStatistics _statistics = new SnapshotStatistics();

        /// <summary>
        /// Assistant helping memory models resolving memory operations
        /// </summary>
        protected MemoryAssistantBase Assistant { get; private set; }

        /// <summary>
        /// Current operational mode of snapshot
        /// </summary>
        public virtual SnapshotMode CurrentMode { get; protected set; }

        /// <summary>
        /// Determine that transaction of this snapshot is started - updates can be written
        /// </summary>
        public bool IsTransactionStarted { get; private set; }

        /// <summary>
        /// Determine that snapshot is frozen - cannot be updated
        /// </summary>
        public bool IsFrozen { get; private set; }

        /// <summary>
        /// Determine that the snapshot commited by the last transaction differs from the snapshot commited by the previous transaction
        /// Always return false if the transaction is started and not yet commited
        /// </summary>
        public bool HasChanged { get; private set; }



        #region Implementation of SnapshotEntry based API

        /// <summary>
        /// Set snapshot into given operational mode. Mode can be switched multiple times during
        /// transaction. Operations are processed according to current mode.
        /// </summary>
        /// <param name="mode">Operational mode of snapshot</param>
        protected virtual void setMode(SnapshotMode mode)
        {
            //by default there is nothing to do - operational mode should be read from CurrentMode member
        }

        /// <summary>
        /// Create snapshot entry providing reading,... services for variable
        /// </summary>
        /// <remarks>
        /// If global context is not forced, searches in local context (there can be 
        /// fetched some variables from global context also),
        /// or in global context in snapshot belonging to global code
        /// </remarks>
        /// <param name="name">Name of variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns>Readable snapshot entry for variable identifier</returns>
        protected abstract ReadWriteSnapshotEntryBase getVariable(VariableIdentifier variable, bool forceGlobalContext);

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        protected abstract ReadWriteSnapshotEntryBase getControlVariable(VariableName name);

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling in local context. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        protected abstract ReadWriteSnapshotEntryBase getLocalControlVariable(VariableName name);

        /// <summary>
        /// Creates snapshot entry containing given value. Created entry doesn't have
        /// explicit memory storage. But it still can be asked for saving indexes, fields, resolving aliases,... !!!
        /// </summary>
        /// <param name="value">Value wrapped in snapshot entry</param>
        /// <returns>Created value entry</returns>
        protected abstract ReadWriteSnapshotEntryBase createSnapshotEntry(MemoryEntry entry);

        #endregion

        #region Template methods - API for implementors

        /// <summary>
        /// Start snapshot transaction - changes can be proceeded only when transaction is started
        /// </summary>
        protected abstract void startTransaction();

        /// <summary>
        /// Commit started transaction - must return true if the content of the snapshot is different 
        /// than the content commited by the previous transaction, false otherwise
        /// NOTE:
        ///     Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <returns><c>true</c> if there is semantic change in transaction, <c>false</c> otherwise</returns>
        protected abstract bool commitTransaction();

        /// <summary>
        /// Widen current transaction and process commit.
        /// Commit started transaction - must return true if the content of the snapshot is different 
        /// than the content commited by the previous transaction, false otherwise
        /// NOTE:
        ///     Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <returns><c>true</c> if there is semantic change in transaction, <c>false</c> otherwise</returns>
        protected abstract bool widenAndCommitTransaction();

        /// <summary>
        /// Initialize object of given type
        /// </summary>
        /// <param name="createdObject">Created object that has to be initialized</param>
        /// <param name="type">Desired type of initialized object</param>
        protected abstract void initializeObject(ObjectValue createdObject, TypeValue type);
        
        /// <summary>
        /// Determine type of given object
        /// </summary>
        /// <param name="objectValue">Object which type is resolved</param>
        /// <returns>Type of given object</returns>
        protected abstract TypeValue objectType(ObjectValue objectValue);

        /// <summary>
        /// Initialize array
        /// </summary>
        /// <param name="createdArray">Created array that has to be initialized</param>
        protected abstract void initializeArray(AssociativeArray createdArray);

        /// <summary>
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contains till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged</param>
        protected abstract void extend(ISnapshotReadonly[] inputs);

        /// <summary>
        /// Extend snapshot as call from given callerContext
        /// </summary>
        /// <param name="callerContext"></param>
        /// <param name="thisObject"></param>
        /// <param name="arguments"></param>
        protected abstract void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments);
        /// <summary>
        /// Merge given call output with current context.
        /// WARNING: Call can change many objects via references (they don't has to be in global context)
        /// </summary>
        /// <param name="callOutputs">Output snapshots of call level</param>        
        protected abstract void mergeWithCallLevel(ISnapshotReadonly[] callOutputs);

        /// <summary>
        /// Set given info for value
        /// </summary>
        /// <param name="value">Value which info is stored</param>
        /// <param name="info">Info stored for value</param>
        protected abstract void setInfo(Value value, params InfoValue[] info);

        /// <summary>
        /// Set given info for variable
        /// </summary>
        /// <param name="variable">Variable which info is stored</param>
        /// <param name="info">Info stored for variable</param>
        protected abstract void setInfo(VariableName variable, params InfoValue[] info);

        /// <summary>
        /// Read info stored for given value
        /// </summary>
        /// <param name="value">Value which info is read</param>
        /// <returns>Stored info</returns>
        protected abstract InfoValue[] readInfo(Value value);

        /// <summary>
        /// Read info stored for given variable
        /// </summary>
        /// <param name="variable">Variable which info is read</param>
        /// <returns>Stored info</returns>
        protected abstract InfoValue[] readInfo(VariableName variable);
               
        /// <summary>
        /// Fetch variables from global context into current context
        /// </summary>
        /// <example>global x,y;</example>
        /// <param name="variables">Variables that will be fetched</param>
        protected abstract void fetchFromGlobal(IEnumerable<VariableName> variables);
        /// <summary>
        /// Get all variables defined in global scope
        /// </summary>
        /// <returns>Variables defined in global scope</returns>
        protected abstract IEnumerable<VariableName> getGlobalVariables();
        /// <summary>
        /// Declare given function into global context
        /// </summary>
        /// <param name="declaration">Declared function</param>
        protected abstract void declareGlobal(FunctionValue declaration);
        /// <summary>
        /// Declare given type into global context
        /// </summary>
        /// <param name="declaration">Declared type</param>
        protected abstract void declareGlobal(TypeValue declaration);

        /// <summary>
        /// Resolves all possible functions for given functionName
        /// NOTE:
        ///     Multiple declarations for single functionName can happen for example because of branch merging
        /// </summary>
        /// <param name="functionName">Name of resolved function</param>
        /// <returns>Resolved functions</returns>
        protected abstract IEnumerable<FunctionValue> resolveFunction(QualifiedName functionName);

        /// <summary>
        /// Resolves all possible functions for given functionName
        /// NOTE:
        ///     Multiple declarations for single functionName can happen for example because of branch merging
        /// </summary>
        /// <param name="functionName">Name of resolved function</param>
        /// <returns>Resolved functions</returns>
        protected abstract IEnumerable<FunctionValue> resolveStaticMethod(TypeValue value, QualifiedName methodName);

        /// <summary>
        /// Resolves all possible types for given typeName
        /// NOTE:
        ///     Multiple declarations for single typeName can happen for example because of branch merging
        /// </summary>
        /// <param name="typeName">Name of resolved type</param>
        /// <returns>Resolved types</returns>
        protected abstract IEnumerable<TypeValue> resolveType(QualifiedName typeName);

        #endregion

        #region Statistic interface for implementors

        /// <summary>
        /// Report statistic operation of given value
        /// </summary>
        /// <param name="statistic">Statistic to be reported</param>
        /// <param name="value">Value that will increase reported statistic</param>
        protected void REPORT(Statistic statistic, int value = 1)
        {
            _statistics.Report(statistic, value);
        }

        /// <summary>
        /// Report hash search based on non-recursive <c>GetHashCode</c> and <c>Equals</c> routines
        /// </summary>
        protected void ReportSimpleHashSearch()
        {
            _statistics.Report(Statistic.SimpleHashSearches);
        }

        /// <summary>
        /// Report hash assigns based on non-recursive <c>GetHashCode</c> and <c>Equals</c> routines
        /// </summary>
        protected void ReportSimpleHashAssign()
        {
            _statistics.Report(Statistic.SimpleHashAssigns);
        }

        /// <summary>
        /// Report memory entries merging
        /// </summary>
        protected void ReportMemoryEntryMerge()
        {
            _statistics.Report(Statistic.MemoryEntryMerges);
        }

        /// <summary>
        /// Report explicit memory entry comparison 
        /// </summary>
        protected void ReportMemoryEntryComparison()
        {
            _statistics.Report(Statistic.MemoryEntryComparisons);
        }

        /// <summary>
        /// Report explicit memory entry creation
        /// </summary>
        protected void ReportMemoryEntryCreation()
        {
            _statistics.Report(Statistic.MemoryEntryCreation);
        }

        #endregion

        #region Snapshot controll operations

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotBase" /> class.
        /// </summary>
        public SnapshotBase()
        {
        }

        /// <summary>
        /// Start snapshot transaction - changes can be proceeded only when transaction is started
        /// </summary>
        public void StartTransaction()
        {
            checkFrozenState();

            if (IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot start transaction multiple times");
            }
            IsTransactionStarted = true;

            HasChanged = false;
            startTransaction();
        }

        /// <summary>
        /// Commit started transaction - sets HasChanged to true if the content of the snapshot is different 
        /// than the content commited by the previous transaction, sets it to false otherwise
        /// NOTE:
        ///     Difference is meant in semantic (two objects with different references but same content doesn't mean difference)
        /// </summary>
        public void CommitTransaction()
        {
            checkFrozenState();

            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot commit transaction because no transaction has been started yet");
            }

            try
            {
                HasChanged = commitTransaction();
            }
            finally
            {
                IsTransactionStarted = false;
            }
        }

        public void WidenAndCommitTransaction()
        {
            checkFrozenState();
            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot commit and widen because no transaction has been started yet");
            }

            try
            {
                HasChanged = widenAndCommitTransaction();
            }
            finally
            {
                IsTransactionStarted = false;
            }
        }

        /// <summary>
        /// Get statistics about snapshot usage - returned statistics remains to current state and is not updated
        /// during next usage
        /// </summary>
        /// <returns>Snapshot statistis</returns>
        public SnapshotStatistics GetStatistics()
        {
            return _statistics.Clone();
        }

        public void SetMode(SnapshotMode mode)
        {
            checkFrozenState();

            if (CurrentMode == mode)
                //there is nothing to change
                return;

            _statistics.Report(Statistic.ModeSwitch);

            CurrentMode = mode;
            setMode(mode);
        }

        /// <summary>
        /// Snapshot can be frozen - no other transaction will be allowed to start/commit
        /// </summary>
        internal void Freeze()
        {
            checkFrozenState();

            if (IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot freeze snapshot when transaction is not commited");
            }

            IsFrozen = true;
        }

        internal void InitAssistant(MemoryAssistantBase assistant)
        {
            if (Assistant != null)
                throw new NotSupportedException("Cannot set memory assistant twice");

            Assistant = assistant;
        }

        internal void ExtendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.AsCallExtendings);
            extendAsCall(callerContext, thisObject, arguments);
        }

        #endregion

        #region Implementation of ISnapshotReadWrite interface

        #region TODO: Implement as singletons!!!
        public AnyValue AnyValue { get { return new AnyValue(); } }
        public AnyStringValue AnyStringValue { get { return new AnyStringValue(); } }
        public AnyBooleanValue AnyBooleanValue { get { return new AnyBooleanValue(); } }
        public AnyIntegerValue AnyIntegerValue { get { return new AnyIntegerValue(); } }
        public AnyFloatValue AnyFloatValue { get { return new AnyFloatValue(); } }
        public AnyLongintValue AnyLongintValue { get { return new AnyLongintValue(); } }
        public AnyObjectValue AnyObjectValue { get { return new AnyObjectValue(); } }
        public AnyArrayValue AnyArrayValue { get { return new AnyArrayValue(); } }
        public AnyResourceValue AnyResourceValue { get { return new AnyResourceValue(); } }
        public UndefinedValue UndefinedValue { get { return new UndefinedValue(); } }
        #endregion

        public InfoValue<T> CreateInfo<T>(T data)
        {
            return new InfoValue<T>(data);
        }

        /// <summary>
        /// Determine type of given object
        /// </summary>
        /// <param name="objectValue">Object which type is resolved</param>
        /// <returns>Type of given object</returns>
        public TypeValue ObjectType(ObjectValue objectValue)
        {
            _statistics.Report(Statistic.ObjectTypeSearches);
            return objectType(objectValue);
        }

        /// <summary>
        /// Creates index for given identifier
        /// </summary>
        /// <param name="identifier">Identifier of index</param>
        /// <returns>Created index</returns>
        public ContainerIndex CreateIndex(string identifier)
        {
            _statistics.Report(Statistic.CreatedIndexes);
            return new ContainerIndex(identifier);
        }

        public StringValue CreateString(string literal)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.CreatedStringValues);
            return new StringValue(literal);
        }

        public IntegerValue CreateInt(int number)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.CreatedIntValues);
            return new IntegerValue(number);
        }

        public LongintValue CreateLong(long number)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.CreatedLongValues);
            return new LongintValue(number);
        }

        public BooleanValue CreateBool(bool boolean)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.CreatedBooleanValues);
            return new BooleanValue(boolean);
        }

        public FloatValue CreateDouble(double number)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.CreatedFloatValues);
            return new FloatValue(number);
        }

        /// <inheritdoc />
        public FunctionValue CreateFunction(FunctionDecl declaration)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.CreatedFunctionValues);
            return new SourceFunctionValue(declaration);
        }

        /// <inheritdoc />
        public FunctionValue CreateFunction(MethodDecl declaration)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.CreatedFunctionValues);
            return new SourceMethodValue(declaration);
        }

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="name">Name of created analyzer</param>
        /// <param name="analyzer">Analyzer declaration</param>
        /// <returns>Created value</returns>
        public FunctionValue CreateFunction(Name name, NativeAnalyzer analyzer)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.CreatedFunctionValues);
            return new NativeAnalyzerValue(name, analyzer);
        }

        /// <summary>
        /// Create function value from given expression
        /// </summary>
        /// <param name="expression">Lambda function declaration</param>
        /// <returns>Created value</returns>
        public FunctionValue CreateFunction(LambdaFunctionExpr expression)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.CreatedFunctionValues);
            return new LambdaFunctionValue(expression);
        }

        public TypeValue CreateType(ClassDecl declaration)
        {
            checkCanUpdate();

            var type = new TypeValue(declaration);
            _statistics.Report(Statistic.CreatedNativeTypeValues);

            return type;
        }

        public ObjectValue CreateObject(TypeValue type)
        {
            checkCanUpdate();

            var createdObject = new ObjectValue();
            _statistics.Report(Statistic.CreatedObjectValues);

            initializeObject(createdObject, type);

            return createdObject;
        }

        public AssociativeArray CreateArray()
        {
            checkCanUpdate();

            var createdArray = new AssociativeArray();
            _statistics.Report(Statistic.CreatedArrayValues);

            initializeArray(createdArray);

            return createdArray;
        }

        public IntegerIntervalValue CreateIntegerInterval(int start, int end)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.CreatedIntIntervalValues);
            return new IntegerIntervalValue(start, end);
        }

        public LongintIntervalValue CreateLongintInterval(long start, long end)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.CreatedLongIntervalValues);
            return new LongintIntervalValue(start, end);
        }

        public FloatIntervalValue CreateFloatInterval(double start, double end)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.CreatedFloatIntervalValues);
            return new FloatIntervalValue(start, end);
        }

        public void SetInfo(Value value, params InfoValue[] info)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.ValueInfoSettings);
            setInfo(value, info);
        }

        public void SetInfo(VariableName variable, params InfoValue[] info)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.VariableInfoSettings);
            setInfo(variable, info);
        }

        public InfoValue[] ReadInfo(Value value)
        {
            _statistics.Report(Statistic.ValueInfoReads);
            return readInfo(value);
        }

        public InfoValue[] ReadInfo(VariableName variable)
        {
            _statistics.Report(Statistic.VariableInfoReads);
            return readInfo(variable);
        }
   
        public void Extend(params ISnapshotReadonly[] inputs)
        {
            checkCanUpdate();
            extend(inputs);
            _statistics.Report(Statistic.SnapshotExtendings);
        }

        public void MergeWithCallLevel(ISnapshotReadonly[] callOutputs)
        {
            checkCanUpdate();
            mergeWithCallLevel(callOutputs);
            _statistics.Report(Statistic.CallLevelMerges);
        }

        public void FetchFromGlobal(params VariableName[] variables)
        {
            _statistics.Report(Statistic.GlobalVariableFetches, variables.Length);
            fetchFromGlobal(variables);
        }

        public void FetchFromGlobalAll()
        {
            var globals = getGlobalVariables();
            FetchFromGlobal(globals.ToArray());
        }

        public void DeclareGlobal(FunctionDecl declaration)
        {
            var function = CreateFunction(declaration);
            _statistics.Report(Statistic.DeclaredFunctions);
            declareGlobal(function);
        }

        public void DeclareGlobal(TypeValue type)
        {
            _statistics.Report(Statistic.DeclaredTypes);
            declareGlobal(type);
        }

        public IEnumerable<FunctionValue> ResolveFunction(QualifiedName functionName)
        {
            _statistics.Report(Statistic.FunctionResolvings);
            return resolveFunction(functionName);
        }

        public IEnumerable<FunctionValue> ResolveStaticMethod(TypeValue value, QualifiedName methodName)
        {
            return resolveStaticMethod(value, methodName);
        }

        public IEnumerable<TypeValue> ResolveType(QualifiedName typeName)
        {
            _statistics.Report(Statistic.TypeResolvings);
            return resolveType(typeName);
        }

        #endregion

        #region Snapshot private helpers

        private void checkCanUpdate()
        {
            checkFrozenState();
            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot process any updates because snapshot has no started transaction");
            }
        }

        private void checkFrozenState()
        {
            if (IsFrozen)
            {
                throw new NotSupportedException("Cannot process action because snapshot has already been frozen");
            }
        }

        #endregion


        #region Snapshot entry API


        /// <summary>
        /// Get snapshot entry providing reading,... services for variable
        /// </summary>
        /// <remarks>
        /// If global context is not forced, searches in local context (there can be 
        /// fetched some variables from global context also),
        /// or in global context in snapshot belonging to global code
        /// </remarks>
        /// <param name="name">Name of variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns>Readable snapshot entry for variable identifier</returns>
        public ReadWriteSnapshotEntryBase GetVariable(VariableIdentifier variable, bool forceGlobalContext = false)
        {
            return getVariable(variable, forceGlobalContext);
        }

        /// <summary>
        /// Get snapshot entry providing reading,... services for variable
        /// </summary>
        /// <remarks>
        /// If global context is not forced, searches in local context (there can be 
        /// fetched some variables from global context also),
        /// or in global context in snapshot belonging to global code
        /// </remarks>
        /// <param name="name">Name of variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns>Readable snapshot entry for variable identifier</returns>
        public ReadSnapshotEntryBase ReadVariable(VariableIdentifier variable, bool forceGlobalContext = false)
        {
            return getVariable(variable, forceGlobalContext);
        }


        /// <summary>
        /// Creates snapshot entry containing given value. Created entry doesn't have
        /// explicit memory storage. But it still can be asked for saving indexes, fields, resolving aliases,... !!!
        /// </summary>
        /// <param name="value">Value wrapped in snapshot entry</param>
        /// <returns>Created value entry</returns>
        public ReadSnapshotEntryBase CreateSnapshotEntry(MemoryEntry value)
        {
            return createSnapshotEntry(value);
        }

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        public ReadWriteSnapshotEntryBase GetControlVariable(VariableName variable)
        {
            return getControlVariable(variable);
        }

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        public ReadSnapshotEntryBase ReadControlVariable(VariableName variable)
        {
            return getControlVariable(variable);
        }

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling in local context. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        public ReadWriteSnapshotEntryBase GetLocalControlVariable(VariableName variable)
        {
            return getLocalControlVariable(variable);
        }

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling in local context. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="variable">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        public ReadSnapshotEntryBase ReadLocalControlVariable(VariableName variable)
        {
            return getLocalControlVariable(variable);
        }

        #endregion

    }
}
