using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Set of FlowInfo used as output from statement analysis.   
    /// </summary>
    public class FlowOutputSet : FlowInputSet, ISnapshotReadWrite
    {

        #region Internal methods for transaction handling
        internal bool HasChanges { get; private set; }
        

        internal FlowOutputSet(AbstractSnapshot snapshot) :
            base(snapshot)
        {
            //because new snapshot has been initialized
            HasChanges = true;
        }

        /// <summary>
        /// Commit transaction of contained snapshot
        /// </summary>
        internal void CommitTransaction()
        {
            _snapshot.CommitTransaction();
            HasChanges = _snapshot.HasChanged;
        }

        /// <summary>
        /// Start transaction on contained snapshot
        /// </summary>
        internal void StartTransaction()
        {
            _snapshot.StartTransaction();
        }

        /// <summary>
        /// Reset reported changes
        /// </summary>
        internal void ResetChanges()
        {
            HasChanges = false;
        }

        /// <summary>
        /// Create call output set from contained snapshot
        /// </summary>
        /// <param name="ThisObject">ThisObject of call</param>
        /// <param name="arguments">Arguments for call</param>
        /// <returns>Snapshot with call context</returns>
        internal FlowOutputSet CreateCall(MemoryEntry ThisObject, MemoryEntry[] arguments)
        {
            return new FlowOutputSet(_snapshot.CreateCall(ThisObject, arguments));
        }
        #endregion


        #region Snapshot API wrapping

        public AnyValue AnyValue { get { return _snapshot.AnyValue; } }

        public UndefinedValue UndefinedValue { get { return _snapshot.UndefinedValue; } }

        public MemoryEntry AnyValueEntry { get { return _snapshot.AnyValueEntry; } }

        public MemoryEntry UndefinedValueEntry { get { return _snapshot.UndefinedValueEntry; } }

        public StringValue CreateString(string literal)
        {
            return _snapshot.CreateString(literal);
        }

        public IntegerValue CreateInt(int number)
        {
            return _snapshot.CreateInt(number);
        }

        public LongintValue CreateLong(long number)
        {
            return _snapshot.CreateLong(number);
        }

        public BooleanValue CreateBool(bool boolean)
        {
            return _snapshot.CreateBool(boolean);
        }   

        public FloatValue CreateDouble(double number)
        {
            return _snapshot.CreateDouble(number);
        }

        public FunctionValue CreateFunction(FunctionDecl declaration)
        {
            return _snapshot.CreateFunction(declaration);
        }

        public AssociativeArray CreateArray()
        {
            return _snapshot.CreateArray();
        }

        public ObjectValue CreateObject(TypeDecl type)
        {
            return _snapshot.CreateObject(type);
        }

        public AliasValue CreateAlias(VariableName sourceVar)
        {
            return _snapshot.CreateAlias(sourceVar);
        }

        public void Assign(VariableName targetVar, Value value)
        {
            _snapshot.Assign(targetVar, value);
        }

        public void Assign(VariableName targetVar, MemoryEntry entry)
        {
            _snapshot.Assign(targetVar, entry);
        }

        public void FetchFromGlobal(params VariableName[] variables)
        {
            _snapshot.FetchFromGlobal(variables);
        }

        public void FetchFromGlobalAll()
        {
            _snapshot.FetchFromGlobalAll();
        }

        /// <summary>
        /// Expects FlowInput set objects
        /// </summary>
        /// <param name="inputs"></param>
        public void Extend(params ISnapshotReadonly[] inputs)
        {
            var snapshots = getSnapshots(inputs);

            _snapshot.Extend(snapshots);
        }

        public void MergeWithCallLevel(ISnapshotReadonly[] callOutputs)
        {
            var snapshots=getSnapshots(callOutputs);
            _snapshot.MergeWithCallLevel(snapshots);
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Convert FlowInput sets info AbstractSnapshots
        /// </summary>
        /// <param name="inputs">FlowInput sets</param>
        /// <returns>Input sets snapshots</returns>
        private AbstractSnapshot[] getSnapshots(ISnapshotReadonly[] inputs)
        {
            var converted = new AbstractSnapshot[inputs.Length];

            //we need pass wrapped snapshots into extend call
            for (int i = 0; i < inputs.Length; ++i)
            {
                converted[i] = getSnapshot(inputs[i] as FlowInputSet);
            }
            return converted;
        }

        /// <summary>
        /// Get snapshot from FlowInputSet
        /// </summary>
        /// <param name="input">input set which snapshot will be returned</param>
        /// <returns>Snapshot from FlowInputSet</returns>
        private AbstractSnapshot getSnapshot(FlowInputSet input)
        {
            return input._snapshot;
        }
        #endregion

   
    }
}
