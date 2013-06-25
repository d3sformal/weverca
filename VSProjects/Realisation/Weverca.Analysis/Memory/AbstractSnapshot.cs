﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// Represents memory snpashot used for fix point analysis.
    /// NOTES: 
    ///     * Snapshot will always work in context of some object.
    ///     * Global variables behave in different way than objets variables (cross callstack)
    ///     * Is correct for implementors to cast input Objects, Arrays, Snapshots to concrete implementation - Two snapshot implementations never won't be used together
    /// <remarks>
    /// For each ProgramPoint are created few snapshots (global memory, local memory,..) - these snapshots are used through all iterations of fix point algorithm
    /// TODO: better handling of global memory
    /// 
    /// Iteration looks like:
    /// StartTransaction
    /// ..changes in snapshot
    /// CommitTransaction
    /// 
    /// If snapshot has changed against state before start transaction is determined by HasChanged
    /// </remarks>
    /// </summary>
    public abstract class AbstractSnapshot : ISnapshotReadWrite
    {
        /// <summary>
        /// Any value singleton
        /// </summary>
        private static readonly AnyValue _anyValue = new AnyValue();
        /// <summary>
        /// Undefined value singleton
        /// </summary>
        private static readonly UndefinedValue _undefinedValue = new UndefinedValue();
        /// <summary>
        /// singleton variable where return value is stored
        /// </summary>
        private static readonly VariableName _returnValue = new VariableName(".return");
        /// <summary>
        /// Any value memory entry singleton
        /// </summary>
        private static readonly MemoryEntry _anyValueEntry = new MemoryEntry(_anyValue);
        /// <summary>
        /// Undefined value memory entry singleton
        /// </summary>
        private static readonly MemoryEntry _undefinedValueEntry = new MemoryEntry(_undefinedValue);

        /// <summary>
        /// Statistics object - here are stored statistics
        /// </summary>
        private SnapshotStatistics _statistics = new SnapshotStatistics();

        /// <summary>
        /// Determine that transaction of this snapshot is started - updates can be written
        /// </summary>
        public bool IsTransactionStarted { get; private set; }
        /// <summary>
        /// Determine that snapshot is frozen - cannot be updated
        /// </summary>
        public bool IsFrozen { get; private set; }
        /// <summary>
        /// Determine that memory snapshot has changed on last commit
        /// </summary>
        public bool HasChanged { get; private set; }

        



        #region Template methods - API for implementors

        /// <summary>
        /// Start snapshot transaction - changes can be proceeded only when transaction is started
        /// </summary>
        protected abstract void startTransaction();
        /// <summary>
        /// Commit started transaction - if changes has been detected during transaction must return true, false otherwise
        /// NOTE:
        ///     Change is meant in semantic (two objects with different references but same content doesn't mean change)
        /// </summary>
        /// <returns>True if there is semantic change in transaction, false otherwise</returns>
        protected abstract bool commitTransaction();

        /// <summary>
        /// Create alias for given variable
        /// </summary>
        /// <param name="sourceVar">Variable which alias will be created</param>
        /// <returns>Created alias</returns>
        protected abstract AliasValue createAlias(VariableName sourceVar);
        /// <summary>
        /// Create snapshot that will be used for call invoked from given info
        /// </summary>        
        /// <returns>Snapshot that will be used as entry point of invoked call</returns>
        protected abstract AbstractSnapshot createCall(MemoryEntry ThisObject, MemoryEntry[] arguments);
        /// <summary>
        /// Assign memory entry into targetVar        
        /// </summary>
        /// <param name="targetVar">Target of assigning</param>
        /// <param name="entry">Value that will be assigned</param>
        protected abstract void assign(VariableName targetVar, MemoryEntry entry);
        /// <summary>
        /// Assign alias to given targetVar
        /// </summary>
        /// <param name="targetVar">Target variable</param>
        /// <param name="alias">Assigned alias</param>
        protected abstract void assignAlias(VariableName targetVar, AliasValue alias);
        /// <summary>
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contains till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged</param>
        protected abstract void extend(ISnapshotReadonly[] inputs);
        /// <summary>
        /// Merge given call output with current context.
        /// WARNING: Call can change many objects via references (they don't has to be in global context)
        /// </summary>
        /// <param name="callOutput">Output snapshot of call</param>
        /// <param name="result">Result of merged call</param>
        protected abstract void mergeWithCall(CallResult result, ISnapshotReadonly callOutput);
        /// <summary>
        /// Read value stored in snapshot for sourceVar
        /// </summary>
        /// <param name="sourceVar">Variable which value will be readed</param>
        /// <returns>Value stored for given variable</returns>
        protected abstract MemoryEntry readValue(VariableName sourceVar);

        /// <summary>
        /// Set field specified by index, on object represented by value 
        /// </summary>
        /// <param name="value">Handler for object manipulation</param>
        /// <param name="index">Index of field that will be set</param>
        /// <param name="entry">Data that will be set on specified field</param>
        protected abstract void setField(ObjectValue value, ContainerIndex index, MemoryEntry entry);
        /// <summary>
        /// Set specified index, in array represented by value 
        /// </summary>
        /// <param name="value">Handler for array manipulation</param>
        /// <param name="index">Array index that will be set</param>
        /// <param name="entry">Data that will be set on specified index</param>
        protected abstract void setIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry);
        
        /// <summary>
        /// Set field specified by index, on object represented by value 
        /// </summary>
        /// <param name="value">Handler for object manipulation</param>
        /// <param name="index">Index of field that will be set</param>
        /// <param name="alias">Alias that will be set for field</param>
        protected abstract void setFieldAlias(ObjectValue value, ContainerIndex index, AliasValue alias);
        /// <summary>
        /// Set specified index, in array represented by value 
        /// </summary>
        /// <param name="value">Handler for array manipulation</param>
        /// <param name="index">Index that will be set</param>
        /// <param name="alias">Alias that will be set for field</param>
        protected abstract void setIndexAlias(AssociativeArray value, ContainerIndex index, AliasValue alias);

        /// <summary>
        /// Get value for field specified by index, in object represented by value 
        /// </summary>
        /// <param name="value">Handler for object manipulation</param>
        /// <param name="index">Index of field that will be set</param>        
        protected abstract MemoryEntry getField(ObjectValue value, ContainerIndex index);

        /// <summary>
        /// Get value for field specified by index, in array represented by value 
        /// </summary>
        /// <param name="value">Handler for array manipulation</param>
        /// <param name="index">Index that will be set</param>      
        protected abstract MemoryEntry getIndex(AssociativeArray value, ContainerIndex index);
        
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
        #endregion

        #region Statistic interface for implementors

        /// <summary>
        /// Report hash search based on non-recursinve GetHaschode, Equals routines
        /// </summary>
        protected void ReportSimpleHashSearch()
        {
            ++_statistics.SimpleHashSearches;
        }

        /// <summary>
        /// Report hash assigns based on non-recursinve GetHaschode, Equals routines
        /// </summary>
        protected void ReportSimpleHashAssign()
        {
            ++_statistics.SimpleHashAssigns;
        }

        /// <summary>
        /// Report memory entries merging
        /// </summary>
        protected void ReportMemoryEntryMerge()
        {
            ++_statistics.MemoryEntryMerges;
        }

        /// <summary>
        /// Report explicit memory entry comparison 
        /// </summary>
        protected void ReportMemoryEntryComparison()
        {
            ++_statistics.MemoryEntryComparisons;
        }

        /// <summary>
        /// Report explicit memory entry creation
        /// </summary>
        protected void ReportMemoryEntryCreation()
        {
            ++_statistics.MemoryEntryCreation;
        }

        #endregion

        #region Snapshot controll operations
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
        /// Commit snapshot transaction - if snapshot has been changed during transaction HasChanged is set to true
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

        /// <summary>
        /// Create snapshot that will be used for call invoked from given info
        /// NOTE:
        ///     Returned snapshot is extension of current snapshot with defined arguments
        /// </summary>        
        /// <returns>Snapshot that will be used as entry point of invoked call</returns>
        internal AbstractSnapshot CreateCall(MemoryEntry ThisObject, MemoryEntry[] arguments)
        {
            checkCanUpdate();
            _statistics.CreatedCallSnapshots++;
           var snapshot= createCall(ThisObject, arguments);

           snapshot.StartTransaction();
           for (int i = 0; i < arguments.Length; ++i)
           {
               var argVar = Argument(i);

               snapshot.Assign(argVar, arguments[i]);
           }
           snapshot.CommitTransaction();

           return snapshot;
        }
        #endregion

        #region Implementation of ISnapshotReadWrite interface

        public AnyValue AnyValue { get { return _anyValue; } }
        public UndefinedValue UndefinedValue { get { return _undefinedValue; } }

        public MemoryEntry AnyValueEntry { get { return _anyValueEntry; } }
        public MemoryEntry UndefinedValueEntry { get { return _undefinedValueEntry; } }
        public VariableName ReturnValue { get { return _returnValue; } }

        public VariableName Argument(int index)
        {
            if (index < 0)
            {
                throw new NotSupportedException("Cannot get argument variable for negative index");
            }

            return new VariableName(".arg"+index);
        }

        public StringValue CreateString(string literal)
        {
            checkCanUpdate();

            ++_statistics.CreatedStringValues;
            return new StringValue(literal);
        }

        public IntegerValue CreateInt(int number)
        {
            checkCanUpdate();

            ++_statistics.CreatedIntValues;
            return new IntegerValue(number);
        }

        public BooleanValue CreateBool(bool boolean)
        {
            checkCanUpdate();

            ++_statistics.CreatedBooleanValues;
            return new BooleanValue(boolean);
        }

        public FloatValue CreateFloat(double number)
        {
            checkCanUpdate();

            ++_statistics.CreatedFloatValues;
            return new FloatValue(number);
        }

        public FunctionValue CreateFunction(FunctionDecl declaration)
        {
            checkCanUpdate();

            ++_statistics.CreatedFunctionValues;
            return new FunctionValue(declaration);
        }

        public ObjectValue CreateObject()
        {
            checkCanUpdate();

            throw new NotImplementedException();
        }

        public AssociativeArray CreateArray()
        {
            checkCanUpdate();

            throw new NotImplementedException();
        }

        public void SetField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            checkCanUpdate();

            ++_statistics.FieldAssigns;
            setField(value, index, entry);
        }
        public void SetIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            checkCanUpdate();

            ++_statistics.IndexAssigns;
            setIndex(value, index, entry);
        }

        public void SetFieldAlias(ObjectValue value, ContainerIndex index, AliasValue alias)
        {
            checkCanUpdate();

            ++_statistics.FieldAliasAssigns;
            setFieldAlias(value, index, alias);
        }
        public void SetIndexAlias(AssociativeArray value, ContainerIndex index, AliasValue alias)
        {
            checkCanUpdate();

            ++_statistics.IndexAliasAssigns;
            setIndexAlias(value, index, alias);
        }

        public MemoryEntry GetField(ObjectValue value, ContainerIndex index)
        {
            checkCanUpdate();

            ++_statistics.FieldReads;
            return getField(value, index);
        }
        public MemoryEntry GetIndex(AssociativeArray value, ContainerIndex index)
        {
            checkCanUpdate();

            ++_statistics.IndexReads;
            return getIndex(value, index);
        }

        public AliasValue CreateAlias(VariableName sourceVar)
        {
            checkCanUpdate();

            var result = createAlias(sourceVar);
            ++_statistics.CreatedAliasValues;
            return result;
        }

        public void Assign(VariableName targetVar, Value value)
        {
            checkCanUpdate();

            if (value is AliasValue)
            {
                ++_statistics.AliasAssigns;
                assignAlias(targetVar, value as AliasValue);
            }
            else
            {
                //create implicit memory entry
                Assign(targetVar, new MemoryEntry(value));
            }
        }

        public void Assign(VariableName targetVar, MemoryEntry value)
        {
            checkCanUpdate();
            assign(targetVar, value);
            ++_statistics.MemoryEntryAssigns;
        }

        public void Extend(params ISnapshotReadonly[] inputs)
        {
            checkCanUpdate();
            extend(inputs);
            ++_statistics.SnapshotExtendings;
        }

        public void MergeWithCall(CallResult result, ISnapshotReadonly callOutput)
        {
            checkCanUpdate();
            mergeWithCall(result, callOutput);
            ++_statistics.WithCallMerges;
        }

        public MemoryEntry ReadValue(VariableName sourceVar)
        {
            var result = readValue(sourceVar);
            ++_statistics.ValueReads;
            return result;
        }

        public void FetchFromGlobal(params VariableName[] variables)
        {
            _statistics.GlobalVariableFetches+=variables.Length;
            fetchFromGlobal(variables);
        }

        public void FetchFromGlobalAll()
        {
            var globals = getGlobalVariables();
            FetchFromGlobal(globals.ToArray());
        }
        #endregion

        #region Snapshot private helpers
        void checkCanUpdate()
        {
            checkFrozenState();
            if (!IsTransactionStarted)
            {
                throw new NotSupportedException("Cannot process any updates because snapshot has no started transaction");
            }
        }

        void checkFrozenState()
        {
            if (IsFrozen)
            {
                throw new NotSupportedException("Cannot process action because snapshot has already been frozen");
            }
        }
        #endregion
    }
}