using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    /// <summary>
    /// Represents memory snpashot used for fix point analysis.
    /// NOTE: Snapshot will always work in context of some object.
    /// 
    /// NOTE: Global variables behave in different way than objets variables (cross callstack)
    /// <remarks>
    /// For each ProgramPoint is created few snapshots (global memory, local memory,..) - these snapshots are used through all iterations of fix point algorithm
    /// 
    /// Iteration looks like:
    /// StartTransaction
    /// ..changes in snapshot
    /// CommitTransaction
    /// 
    /// If snapshot has changed against state before start transaction is determined by HasChanged
    /// </remarks>
    /// </summary>
    public abstract class Snapshot:ISnapshotReadWrite
    {
        /// <summary>
        /// Any value singleton
        /// </summary>
        private readonly AnyValue _anyValue = new AnyValue();
        /// <summary>
        /// Undefined value singleton
        /// </summary>
        private readonly UndefinedValue _undefinedValue = new UndefinedValue();

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

        /// <summary>
        /// Start snapshot transaction - changes can be proceeded only when transaction is started
        /// </summary>
        internal void StartTransaction()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Commit snapshot transaction - if snapshot has been changed during transaction HasChanged is set to true
        /// </summary>
        internal void CommitTransaction()
        {
            throw new NotImplementedException();
        }
        


        /// <summary>
        /// Get statistics about snapshot usage
        /// </summary>
        /// <returns>Snapshot statistis</returns>
        public SnapshotStatistics GetStatistics()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Snapshot can be frozen - no other transaction will be allowed to start/commit
        /// </summary>
        internal void Freeze()
        {
            throw new NotImplementedException();
        }

        public void Assign(PHP.Core.VariableName targetVar, object value)
        {
            throw new NotImplementedException();
        }



        public void Extend(params ISnapshotReadonly[] inputs)
        {
            throw new NotImplementedException();
        }

        public void MergeWithCall(ISnapshotReadonly callOutput)
        {
            throw new NotImplementedException();
        }

        public void ExtendFromEntryPoint(ISnapshotReadonly callInput)
        {
            throw new NotImplementedException();
        }

        public Value ReadValue(PHP.Core.VariableName sourceVar)
        {
            throw new NotImplementedException();
        }

        #region Creating value objects from primitive representation

        public AnyValue AnyValue
        {
            get { return _anyValue; }
        }

        public UndefinedValue UndefinedValue
        {
            get { return _undefinedValue; }
        }        

        public StringValue CreateString(string literal)
        {
            throw new NotImplementedException();
        }
        public IntegerValue CreateInt(int number)
        {
            throw new NotImplementedException();
        }
        public BooleanValue CreateBool(bool boolean)
        {
            throw new NotImplementedException();
        }
        public FloatValue CreateFloat(double number)
        {
            throw new NotImplementedException();
        }

        public ObjectValue CreateObject()
        {
            throw new NotImplementedException();
        }

        public AliasValue CreateAlias(PHP.Core.VariableName sourceVar)
        {
            throw new NotImplementedException();
        }
        #endregion


  

    }
}
