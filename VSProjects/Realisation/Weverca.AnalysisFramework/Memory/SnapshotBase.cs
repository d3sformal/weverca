using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework.Memory
{
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
        private static readonly VariableName _returnValue = new VariableName(".return");

        /// <summary>
        /// Statistics object - here are stored statistics
        /// </summary>
        private SnapshotStatistics _statistics = new SnapshotStatistics();

        /// <summary>
        /// Assistant helping memory models resolving memory operations
        /// </summary>
        protected MemoryAssistantBase Assistant { get; private set; }

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

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Iterates over the given object
        /// </summary>
        /// <param name="iteratedObject">Object which iterator will be created</param>
        /// <returns>Iterator for given object</returns>
        protected abstract IEnumerable<ContainerIndex> iterateObject(ObjectValue iteratedObject);

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

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Create iterator for given array
        /// </summary>
        /// <param name="iteratedArray">Array which iterator will be created</param>
        /// <returns>Iterators for given array</returns>
        protected abstract IEnumerable<ContainerIndex> iterateArray(AssociativeArray iteratedArray);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Create alias for given variable
        /// </summary>
        /// <param name="sourceVar">Variable which alias will be created</param>
        /// <returns>Created alias</returns>
        protected abstract AliasValue createAlias(VariableName sourceVar);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Create alias for given index contained in array
        /// </summary>
        /// <param name="array">Array containing index</param>
        /// <param name="index">Aliased index</param>
        /// <returns>Created alias</returns>
        protected abstract AliasValue createIndexAlias(AssociativeArray array, ContainerIndex index);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Create alias for given field of objectValue
        /// </summary>
        /// <param name="objectValue">Value containing aliased field</param>
        /// <param name="field">Aliased field</param>
        /// <returns>Created alias</returns>
        protected abstract AliasValue createFieldAlias(ObjectValue objectValue, ContainerIndex field);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Assign memory entry into <paramref name="targetVar"/>
        /// </summary>
        /// <param name="targetVar">Target of assigning</param>
        /// <param name="entry">Value that will be assigned</param>
        protected abstract void assign(VariableName targetVar, MemoryEntry entry);


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

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Read value stored in snapshot for <paramref name="sourceVar"/>
        /// </summary>
        /// <param name="sourceVar">Variable which value will be read</param>
        /// <returns>Value stored for given variable</returns>
        protected abstract MemoryEntry readValue(VariableName sourceVar);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Tries to read value stored in current snapshot for <paramref name="sourceVar" />
        /// </summary>
        /// <param name="sourceVar">Variable which value will be attempted to read</param>
        /// <param name="entry">Value stored for given variable if exists, otherwise undefined value</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns><c>true</c> if variable exists, <c>false</c> otherwise</returns>
        protected abstract bool tryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Set field specified by index, on object represented by value 
        /// </summary>
        /// <param name="value">Handler for object manipulation</param>
        /// <param name="index">Index of field that will be set</param>
        /// <param name="entry">Data that will be set on specified field</param>
        protected abstract void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Set specified index, in array represented by value 
        /// </summary>
        /// <param name="value">Handler for array manipulation</param>
        /// <param name="index">Array index that will be set</param>
        /// <param name="entry">Data that will be set on specified index</param>
        protected abstract void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry);


        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Get value for field specified by index, in object represented by value 
        /// </summary>
        /// <param name="value">Handler for object manipulation</param>
        /// <param name="index">Index of field that will be set</param>
        /// <returns></returns>
        protected abstract MemoryEntry getField(ObjectValue value, ContainerIndex index);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Tries to get value from object at specified field stored in current snapshot
        /// </summary>
        /// <param name="objectValue">Object which field is resolved</param>
        /// <param name="field">Field where value will be searched</param>
        /// <param name="entry">Value stored at given object field if exists, otherwise undefined value</param>
        /// <returns><c>true</c> if field for given object exists, <c>false</c> otherwise</returns>
        protected abstract bool tryGetField(ObjectValue objectValue, ContainerIndex field, out MemoryEntry entry);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Get value for field specified by index, in array represented by value 
        /// </summary>
        /// <param name="value">Handler for array manipulation</param>
        /// <param name="index">Index that will be set</param>
        /// <returns></returns>
        protected abstract MemoryEntry getIndex(AssociativeArray value, ContainerIndex index);

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// Tries to get value from array at specified index stored in current snapshot
        /// </summary>
        /// <param name="array">Array which index is resolved</param>
        /// <param name="index">Index where value will be searched</param>
        /// <param name="entry">Value stored at given index in array if exists, otherwise undefined value</param>
        /// <returns><c>true</c> if element of index exists in given array, <c>false</c> otherwise</returns>
        protected abstract bool tryGetIndex(AssociativeArray array, ContainerIndex index, out MemoryEntry entry);

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

        [Obsolete("Will be removed from snapshot early - dont need to be implemented")]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectValue"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        protected abstract IEnumerable<FunctionValue> resolveMethod(ObjectValue objectValue, QualifiedName methodName);
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
            IsTransactionStarted = false;

            HasChanged = commitTransaction();
        }

        public void WidenAndCommitTransaction()
        {
            checkFrozenState();
            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot commit and widen because no transaction has been started yet");
            }
            IsTransactionStarted = false;

            HasChanged = widenAndCommitTransaction();
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

        public FunctionValue CreateFunction(FunctionDecl declaration)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.CreatedFunctionValues);
            return new SourceFunctionValue(declaration);
        }

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="declaration">Method declaration</param>
        /// <returns>Created value</returns>
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

        public void SetField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.FieldAssigns);
            setField(value, index, entry);
        }
        public void SetIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.IndexAssigns);
            setIndex(value, index, entry);
        }

        public void SetFieldAlias(ObjectValue value, ContainerIndex index, IEnumerable<AliasValue> alias)
        {
            throw new NotSupportedException("Obsolete API");
        }

        public void SetIndexAlias(AssociativeArray value, ContainerIndex index, IEnumerable<AliasValue> alias)
        {
            throw new NotSupportedException("Obsolete API");
        }

        public MemoryEntry GetField(ObjectValue value, ContainerIndex index)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.FieldReads);
            return getField(value, index);
        }

        public bool TryGetField(ObjectValue objectValue, ContainerIndex field, out MemoryEntry entry)
        {
            // TODO_David: Je tohle treba?
            checkCanUpdate();

            _statistics.Report(Statistic.FieldReadAttempts);
            return tryGetField(objectValue, field, out entry);
        }

        public MemoryEntry GetIndex(AssociativeArray value, ContainerIndex index)
        {
            checkCanUpdate();

            _statistics.Report(Statistic.IndexReads);
            return getIndex(value, index);
        }

        public bool TryGetIndex(AssociativeArray array, ContainerIndex index, out MemoryEntry entry)
        {
            // TODO_David: Je tohle treba?
            checkCanUpdate();

            _statistics.Report(Statistic.IndexReadAttempts);
            return tryGetIndex(array, index, out entry);
        }

        public AliasValue CreateAlias(VariableName sourceVar)
        {
            var result = createAlias(sourceVar);
            _statistics.Report(Statistic.CreatedAliasValues);
            return result;
        }

        public AliasValue CreateIndexAlias(AssociativeArray array, ContainerIndex index)
        {
            var result = createIndexAlias(array, index);

            _statistics.Report(Statistic.CreatedAliasValues);
            return result;
        }

        public AliasValue CreateFieldAlias(ObjectValue objectValue, ContainerIndex field)
        {
            var result = createFieldAlias(objectValue, field);

            _statistics.Report(Statistic.CreatedAliasValues);
            return result;
        }

        public void Assign(VariableName targetVar, Value value)
        {
            checkCanUpdate();

            if (value is AliasValue)
            {
                throw new NotSupportedException("Obsolete API");
            }
            else
            {
                // create implicit memory entry
                Assign(targetVar, new MemoryEntry(value));
            }
        }

        public void Assign(VariableName targetVar, MemoryEntry value)
        {
            checkCanUpdate();
            assign(targetVar, value);
            _statistics.Report(Statistic.MemoryEntryAssigns);
        }

        public void AssignAliases(VariableName targetVar, IEnumerable<AliasValue> aliases)
        {
            throw new NotSupportedException("Obsolete API");
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

        public MemoryEntry ReadValue(VariableName sourceVar)
        {
            var result = readValue(sourceVar);
            _statistics.Report(Statistic.ValueReads);
            return result;
        }

        public bool TryReadValue(VariableName sourceVar, out MemoryEntry entry, bool forceGlobalContext)
        {
            var result = tryReadValue(sourceVar, out entry, forceGlobalContext);
            _statistics.Report(Statistic.ValueReadAttempts);
            return result;
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

        internal IEnumerable<FunctionValue> ResolveMethod(ObjectValue objectValue, QualifiedName methodName)
        {
            _statistics.Report(Statistic.MethodResolvings);
            return resolveMethod(objectValue, methodName);
        }

        public IEnumerable<TypeValue> ResolveType(QualifiedName typeName)
        {
            _statistics.Report(Statistic.TypeResolvings);
            return resolveType(typeName);
        }

        public IEnumerable<ContainerIndex> IterateObject(ObjectValue iteratedObject)
        {
            _statistics.Report(Statistic.ObjectIterations);
            return iterateObject(iteratedObject);
        }

        public IEnumerable<ContainerIndex> IterateArray(AssociativeArray iteratedArray)
        {
            _statistics.Report(Statistic.ArrayIterations);
            return iterateArray(iteratedArray);
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
