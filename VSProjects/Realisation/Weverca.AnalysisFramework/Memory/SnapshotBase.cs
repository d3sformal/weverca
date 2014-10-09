/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.ProgramPoints;

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
        /// Returns the number of memory locations in the snapshot.
        /// Memory locations are top-level variables, all indices of arrays and all properties of objects.
        /// </summary>
        /// <returns>the number of variables in the snapshot</returns>
        public virtual int NumMemoryLocations()
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
        internal protected MemoryAssistantBase Assistant { get; private set; }

        /// <summary>
        /// Current operational mode of snapshot
        /// </summary>
        public virtual SnapshotMode CurrentMode { get; protected set; }

        /// <summary>
        /// Limit number of memory entry possible values count when does simplifying MemoryEntries start
        /// </summary>
        public virtual int SimplifyLimit { get; protected set; }

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

        #region Allocation-site abstraction
        /// <summary>
        /// Represents allocation sites for representing objects and arrays allocated in this program point.
        /// Note that there can be more allocation sites in a single program point: there can be created more
        /// objects or arrays at a single program point: 
        ///     $a = null; 
        ///     $a->b->c->d = 1; // first object is created in position $a->b, second in position $a->b->c
        /// 
        /// Allocation site for given position represents all objects/arrays in this progrma point at this
        /// position.
        /// If this program point is processed more times (in a loop), allocation site represents more objects/arrays.
        /// </summary>
        private class AllocatedSites<T> 
        {
            private LinkedList<T> sites = new LinkedList<T>();
            private LinkedListNode<T> site = null;

            public void StartTransaction() 
            {
                site = sites.First;
            }
            public bool IsAllocated() { return site != null; }
            public T GetSite() { return site.Value; }
            public void NextPosition() { site = site.Next; }
            public void AddSite(T site) { sites.AddLast(site); }
        }

        /// <summary>
        /// Allocation sites for objects.
        /// </summary>
        private AllocatedSites<ObjectValue> _allocatedObjects = new AllocatedSites<ObjectValue>();

        /// <summary>
        /// Allocation sites for arrays.
        /// </summary>
        private AllocatedSites<AssociativeArray> _allocatedArrays = new AllocatedSites<AssociativeArray>();


        #endregion

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
        /// <param name="variable">Name of variable</param>
        /// <param name="forceGlobalContext">Determine that searching in global context has to be forced</param>
        /// <returns>Readable snapshot entry for variable identifier</returns>
        protected abstract ReadWriteSnapshotEntryBase getVariable(VariableIdentifier variable, bool forceGlobalContext);

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="name">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        protected abstract ReadWriteSnapshotEntryBase getControlVariable(VariableName name);

        /// <summary>
        /// Get snapshot entry for variable, used for extra info controlling in local context. Control entries may share names with other variables,
        /// indexes or fields. Control entries are not affected by unknown fields, also they cannot be aliased to non-control
        /// entries.
        /// </summary>
        /// <param name="name">Variable determining control entry</param>
        /// <returns>Created control entry</returns>
        protected abstract ReadWriteSnapshotEntryBase getLocalControlVariable(VariableName name);

        /// <summary>
        /// Creates snapshot entry containing given value. Created entry doesn't have
        /// explicit memory storage. But it still can be asked for saving indexes, fields, resolving aliases,... !!!
        /// </summary>
        /// <param name="entry">Value wrapped in snapshot entry</param>
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
        /// Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <param name="simplifyLimit">Limit number of memory entry possible values count when does simplifying MemoryEntries start</param>
        /// <returns>
        ///   <c>true</c> if there is semantic change in transaction, <c>false</c> otherwise
        /// </returns>
        protected abstract bool commitTransaction(int simplifyLimit);

        /// <summary>
        /// Widen current transaction and process commit.
        /// Commit started transaction - must return true if the content of the snapshot is different 
        /// than the content commited by the previous transaction, false otherwise
        /// NOTE:
        ///     Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <param name="simplifyLimit">Limit number of memory entry possible values count when does simplifying MemoryEntries start</param>s
        /// <returns><c>true</c> if there is semantic change in transaction, <c>false</c> otherwise</returns>
        protected abstract bool widenAndCommitTransaction(int simplifyLimit);

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
        /// Merges information at the entry to the subprogram (function, method, or included file).
        /// 
        /// Note that if inputs.Length > 1 than the subprogram is shared between more extended points (e.g., callers).
        /// 
        /// Note that it holds that inputs.Lenght == extendedPoints.Length and for each inputs[i], extendedPoints[i]
        /// is extended point (e.g., caller) correspoinding to the input.
        /// After the subprogram is processed, method MergeWithCallLevel(extendedPoints[i]) will be called for every
        /// input. Snapshot can, e.g., keep inputs or their parts separated (not merge them) and identify these separated
        /// parts using extendedPoints[i] and than when the subprogram is processed and MergeWithCallLevel(extendedPoints[i])
        /// is called use extendedPoints[i] to identify these parts that belong to the extended point (caller) and drop others.
        /// See <see cref="MergeWithCallLevel"/> for more information.
        /// 
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contain till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged</param>
        /// <param name="extendedPoints">The points that are extended (e.g, callers).</param>
        /// <seealso cref="MergeWithCallLevel"/>
        protected void extendAtSubprogramEntry(ISnapshotReadonly[] inputs, ProgramPointBase[] extendedPoints)
        {
            extend(inputs);
        }

        /// <summary>
        /// Extend snapshot as call from given callerContext
        /// </summary>
        /// <param name="callerContext">The caller context.</param>
        /// <param name="thisObject">The this object.</param>
        /// <param name="arguments">The arguments.</param>
        protected abstract void extendAsCall(SnapshotBase callerContext, MemoryEntry thisObject, MemoryEntry[] arguments);
        /// <summary>
        /// Merges the result of the extension (call or include) performed by extendedPoint into this snapshot.
        /// 
        /// There can be multiple callees for a single call (extension) - callOutputs.Length can be more than 1.
        /// Callees can be shared by multiple callers (extensions) - they can contain parts of the stack and global
        /// state that do not belong to this particular caller.
        /// See <see cref="ExtendAtSubprogramEntry"/> for more information.
        /// 
        /// WARNING: Call can change many objects via references (they don't have to be in global context)
        /// </summary>
        /// <param name="extendedPoint">The program point that was extended (e.g., caller)</param>    
        /// <param name="extensionsOutputs">Output snapshots of callees belonging to the call</param>  
        /// <seealso cref="ExtendAtSubprogramEntry"/>            
        protected abstract void mergeWithCallLevel(ProgramPointBase extendedPoint, ISnapshotReadonly[] extensionsOutputs);

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
        /// Resolves all possible methods for given methodName
        /// NOTE:
        ///     Multiple declarations for single functionName can happen for example because of branch merging
        /// </summary>
        /// <param name="methodName">Name of resolved method</param>
        /// <param name="selfType">Type where static method is resolved</param>
        /// <returns>Resolved methods</returns>
        protected abstract IEnumerable<FunctionValue> resolveStaticMethod(TypeValue selfType, QualifiedName methodName);

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

            _allocatedObjects.StartTransaction();
            _allocatedArrays.StartTransaction();

            HasChanged = false;
            startTransaction();
        }

        /// <summary>
        /// Commit started transaction - sets HasChanged to true if the content of the snapshot is different
        /// than the content commited by the previous transaction, sets it to false otherwise
        /// NOTE:
        /// Difference is meant in semantic (two objects with different references but same content doesn't mean difference)
        /// </summary>
        /// <param name="simplifyLimit">Limit number of memory entry possible values count when does simplifying MemoryEntries start</param>
        /// <exception cref="System.NotSupportedException">Cannot commit transaction because no transaction has been started yet</exception>
        public void CommitTransaction(int simplifyLimit = int.MaxValue)
        {
            checkFrozenState();

            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot commit transaction because no transaction has been started yet");
            }

            try
            {
                HasChanged = commitTransaction(simplifyLimit);
            }
            finally
            {
                IsTransactionStarted = false;
            }
        }

        /// <summary>
        /// Widen current transaction and process commit.
        /// Commit started transaction - must return true if the content of the snapshot is different 
        /// than the content commited by the previous transaction, false otherwise
        /// NOTE:
        ///     Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <param name="simplifyLimit">Limit number of memory entry possible values count when does simplifying MemoryEntries start</param>s
        /// <returns><c>true</c> if there is semantic change in transaction, <c>false</c> otherwise</returns>
        public void WidenAndCommitTransaction(int simplifyLimit)
        {
            checkFrozenState();
            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot commit and widen because no transaction has been started yet");
            }

            try
            {
                HasChanged = widenAndCommitTransaction(simplifyLimit);
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

        /// <summary>
        /// Set operational mode of current snapshot
        /// </summary>
        /// <param name="mode">Operational mode that will be set</param>
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
        /// Set the limit number of memory entry possible values count when does simplifying MemoryEntries start
        /// </summary>
        /// <param name="simplifyLimit">Limit number of memory entry possible values count when does simplifying MemoryEntries start</param>
        public void SetSimplifyLimit(int simplifyLimit)
        {
            checkFrozenState();

            if (SimplifyLimit == simplifyLimit)
                //there is nothing to change
                return;

            SimplifyLimit = simplifyLimit;
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

        /// <summary>
        /// Initializes the assistant.
        /// </summary>
        /// <param name="assistant">The assistant.</param>
        /// <exception cref="System.NotSupportedException">Cannot set memory assistant twice</exception>
        public void InitAssistant(MemoryAssistantBase assistant)
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
        
        /// <inheritdoc />
        public AnyValue AnyValue { get { return new AnyValue(); } }

        /// <inheritdoc />
        public AnyStringValue AnyStringValue { get { return new AnyStringValue(); } }

        /// <inheritdoc />
        public AnyBooleanValue AnyBooleanValue { get { return new AnyBooleanValue(); } }

        /// <inheritdoc />
        public AnyIntegerValue AnyIntegerValue { get { return new AnyIntegerValue(); } }

        /// <inheritdoc />
        public AnyFloatValue AnyFloatValue { get { return new AnyFloatValue(); } }

        /// <inheritdoc />
        public AnyLongintValue AnyLongintValue { get { return new AnyLongintValue(); } }

        /// <inheritdoc />
        public AnyObjectValue AnyObjectValue { get { return new AnyObjectValue(); } }

        /// <inheritdoc />
        public AnyArrayValue AnyArrayValue { get { return new AnyArrayValue(); } }

        /// <inheritdoc />
        public AnyResourceValue AnyResourceValue { get { return new AnyResourceValue(); } }

        /// <inheritdoc />
        public UndefinedValue UndefinedValue { get { return new UndefinedValue(); } }


        /// <inheritdoc />
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
           //TODO remove ContainerIndexes
            return new ContainerIndex(identifier);
        }

        /// <summary>
        /// Creates new string vale from given string
        /// </summary>
        /// <param name="literal">string value</param>
        /// <returns>new String value</returns>
        public StringValue CreateString(string literal)
        {
            _statistics.Report(Statistic.CreatedStringValues);
            return new StringValue(literal);
        }

        /// <summary>
        /// Creates new integer value from given integer
        /// </summary>
        /// <param name="number">interger value</param>
        /// <returns>new interger value</returns>
        public IntegerValue CreateInt(int number)
        {
            _statistics.Report(Statistic.CreatedIntValues);
            return new IntegerValue(number);
        }

        /// <summary>
        /// Creates new integer value from given longint
        /// </summary>
        /// <param name="number">longint value</param>
        /// <returns>new longint value</returns>
        public LongintValue CreateLong(long number)
        {
            _statistics.Report(Statistic.CreatedLongValues);
            return new LongintValue(number);
        }

        /// <summary>
        /// Creates new boolean value from given bool
        /// </summary>
        /// <param name="boolean">bool value</param>
        /// <returns>Creates new boolean value</returns>
        public BooleanValue CreateBool(bool boolean)
        {
            _statistics.Report(Statistic.CreatedBooleanValues);
            return new BooleanValue(boolean);
        }

        /// <summary>
        /// Creates new float value from given double
        /// </summary>
        /// <param name="number">float value</param>
        /// <returns> new float value</returns>
        public FloatValue CreateDouble(double number)
        {
            _statistics.Report(Statistic.CreatedFloatValues);
            return new FloatValue(number);
        }

        /// <inheritdoc />
        public FunctionValue CreateFunction(FunctionDecl declaration, FileInfo declaringScript)
        {
            _statistics.Report(Statistic.CreatedFunctionValues);
            return new SourceFunctionValue(declaration, declaringScript);
        }

        /// <inheritdoc />
        public FunctionValue CreateFunction(MethodDecl declaration, FileInfo declaringScript)
        {
            _statistics.Report(Statistic.CreatedFunctionValues);
            return new SourceMethodValue(declaration, declaringScript);
        }

        /// <summary>
        /// Create function value from given declaration
        /// </summary>
        /// <param name="name">Name of created analyzer</param>
        /// <param name="analyzer">Analyzer declaration</param>
        /// <returns>Created value</returns>
        public FunctionValue CreateFunction(Name name, NativeAnalyzer analyzer)
        {
            _statistics.Report(Statistic.CreatedFunctionValues);
            return new NativeAnalyzerValue(name, analyzer);
        }

        /// <summary>
        /// Create function value from given expression
        /// </summary>
        /// <param name="expression">Lambda function declaration</param>
        /// <param name="declaringScript">Information about owning script</param>
        /// <returns>Created value</returns>
        public FunctionValue CreateFunction(LambdaFunctionExpr expression, FileInfo declaringScript)
        {
            _statistics.Report(Statistic.CreatedFunctionValues);
            return new LambdaFunctionValue(expression, declaringScript);
        }

        /// <summary>
        /// Creates new TypeValue from given ClassDecl
        /// </summary>
        /// <param name="declaration">Class declaration</param>
        /// <returns>new TypeValue</returns>
        public TypeValue CreateType(ClassDecl declaration)
        {
            var type = new TypeValue(declaration);
            _statistics.Report(Statistic.CreatedTypeValues);

            return type;
        }

        /// <summary>
        /// Create new ObjectValue of given type
        /// </summary>
        /// <param name="type">Type of tthe object</param>
        /// <returns>new ObjectValue</returns>
        public ObjectValue CreateObject(TypeValue type)
        {
            checkCanUpdate();

            ObjectValue createdObject;

            if (_allocatedObjects.IsAllocated()) 
            {
                createdObject =_allocatedObjects.GetSite();
                _allocatedObjects.NextPosition();
                // must be called for a case that the object was created in this snapshot but was not propagated there
                // this can happen, e.g., if this program point is processed twice and it is not in the loop
                // this can lead to redundant initialization of objects. For this reason, method initializeObject must
                // detect the case that the object is initialized second time and in this case do not initialize it
                initializeObject(createdObject, type);
                return createdObject;
            }

            createdObject = new ObjectValue ();
            _allocatedObjects.AddSite(createdObject);

            _statistics.Report(Statistic.CreatedObjectValues);

            initializeObject(createdObject, type);

            return createdObject;
        }

        /// <summary>
        /// Creates new AssociativeArray
        /// </summary>
        /// <returns>new AssociativeArray</returns>
        public AssociativeArray CreateArray()
        {
            checkCanUpdate();

            AssociativeArray createdArray;

            if (_allocatedArrays.IsAllocated()) 
            {
                createdArray = _allocatedArrays.GetSite();
                _allocatedArrays.NextPosition();
                initializeArray(createdArray);
                return createdArray;
            }

            createdArray = new AssociativeArray();
            _allocatedArrays.AddSite(createdArray);

            _statistics.Report(Statistic.CreatedArrayValues);

            initializeArray(createdArray);

            return createdArray;
        }

        /// <summary>
        /// Creates new IntegerIntervalValue
        /// </summary>
        /// <param name="start">interval start</param>
        /// <param name="end">interval end</param>
        /// <returns>new IntegerIntervalValue</returns>
        public IntegerIntervalValue CreateIntegerInterval(int start, int end)
        {
            _statistics.Report(Statistic.CreatedIntIntervalValues);
            return new IntegerIntervalValue(start, end);
        }

        /// <summary>
        /// Creates new LongintIntervalValue
        /// </summary>
        /// <param name="start">Interval start</param>
        /// <param name="end">Interval end</param>
        /// <returns>new LongintIntervalValue</returns>
        public LongintIntervalValue CreateLongintInterval(long start, long end)
        {
            _statistics.Report(Statistic.CreatedLongIntervalValues);
            return new LongintIntervalValue(start, end);
        }

        /// <summary>
        /// Creates new FloatIntervalValue
        /// </summary>
        /// <param name="start">interval start</param>
        /// <param name="end">interval end</param>
        /// <returns>new FloatIntervalValue</returns>
        public FloatIntervalValue CreateFloatInterval(double start, double end)
        {
            _statistics.Report(Statistic.CreatedFloatIntervalValues);
            return new FloatIntervalValue(start, end);
        }

        /// <inheritdoc />
        public void SetInfo(Value value, params InfoValue[] info)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.ValueInfoSettings);
            setInfo(value, info);
        }

        /// <inheritdoc />
        public void SetInfo(VariableName variable, params InfoValue[] info)
        {
            checkCanUpdate();
            _statistics.Report(Statistic.VariableInfoSettings);
            setInfo(variable, info);
        }

        /// <inheritdoc />
        public InfoValue[] ReadInfo(Value value)
        {
            _statistics.Report(Statistic.ValueInfoReads);
            return readInfo(value);
        }

        /// <inheritdoc />
        public InfoValue[] ReadInfo(VariableName variable)
        {
            _statistics.Report(Statistic.VariableInfoReads);
            return readInfo(variable);
        }

        /// <inheritdoc />
        public void Extend(params ISnapshotReadonly[] inputs)
        {
            checkCanUpdate();
            extend(inputs);
            _statistics.Report(Statistic.SnapshotExtendings);
        }

        /// <inheritdoc />
        public void ExtendAtSubprogramEntry(ISnapshotReadonly[] inputs, ProgramPointBase[] extendedPoints)
        {
            checkCanUpdate();
            extendAtSubprogramEntry(inputs, extendedPoints);
            _statistics.Report(Statistic.SnapshotExtendings);
        }

        /// <inheritdoc />
        public void MergeWithCallLevel(ProgramPointBase extendedPoint, ISnapshotReadonly[] extensionsOutputs)
        {
            checkCanUpdate();
            mergeWithCallLevel(extendedPoint, extensionsOutputs);
            _statistics.Report(Statistic.CallLevelMerges);
        }

        /// <inheritdoc />
        public void FetchFromGlobal(params VariableName[] variables)
        {
            _statistics.Report(Statistic.GlobalVariableFetches, variables.Length);
            fetchFromGlobal(variables);
        }

        /// <inheritdoc />
        public void FetchFromGlobalAll()
        {
            var globals = getGlobalVariables();
            FetchFromGlobal(globals.ToArray());
        }

        /// <inheritdoc />
        public void DeclareGlobal(FunctionDecl declaration, FileInfo declaringScript)
        {
            var function = CreateFunction(declaration, declaringScript);
            _statistics.Report(Statistic.DeclaredFunctions);
            declareGlobal(function);
        }

        /// <inheritdoc />
        public void DeclareGlobal(TypeValue type)
        {
            _statistics.Report(Statistic.DeclaredTypes);
            declareGlobal(type);
        }
        
        /// <inheritdoc />
        public IEnumerable<FunctionValue> ResolveFunction(QualifiedName functionName)
        {
            _statistics.Report(Statistic.FunctionResolvings);
            return resolveFunction(functionName);
        }


        /// <inheritdoc />
        public IEnumerable<FunctionValue> ResolveStaticMethod(TypeValue type, QualifiedName methodName)
        {
            return resolveStaticMethod(type, methodName);
        }

        /// <inheritdoc />
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
        /// <param name="variable">Name of variable</param>
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
        /// <param name="variable">Name of variable</param>
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