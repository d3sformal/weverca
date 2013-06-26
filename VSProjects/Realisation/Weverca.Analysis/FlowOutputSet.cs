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

        internal void CommitTransaction()
        {
            _snapshot.CommitTransaction();
            HasChanges = _snapshot.HasChanged;
        }

        internal void StartTransaction()
        {
            _snapshot.StartTransaction();
        }

        internal void ResetChanges()
        {
            HasChanges = false;
        }

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

        public BooleanValue CreateBool(bool boolean)
        {
            return _snapshot.CreateBool(boolean);
        }

        public FloatValue CreateFloat(double number)
        {
            return _snapshot.CreateFloat(number);
        }

        public FunctionValue CreateFunction(FunctionDecl declaration)
        {
            return _snapshot.CreateFunction(declaration);
        }

        public AssociativeArray CreateArray()
        {
            return _snapshot.CreateArray();
        }

        public ObjectValue CreateObject()
        {
            return _snapshot.CreateObject();
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

        private AbstractSnapshot getSnapshot(FlowInputSet input)
        {
            return input._snapshot;
        }
        #endregion
    }
}
