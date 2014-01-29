using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Set of FlowInfo used as output from statement analysis.   
    /// </summary>    
    public class FlowOutputSet : FlowInputSet, ISnapshotReadWrite
    {

        #region Internal methods for transaction handling

        /// <summary>
        /// Determine that the content commited by the last transaction differs from the content commited by the previous transaction
        /// Always return false if the transaction is started and not yet commited
        /// </summary>
        internal bool HasChanges { get; private set; }

        private int _commitCount = 0;

        private readonly int _widenLimit;

        internal FlowOutputSet(SnapshotBase snapshot, int widenLimit = int.MaxValue) :
            base(snapshot)
        {
            //because new snapshot has been initialized
            HasChanges = true;
            _widenLimit = widenLimit;
        }

        /// <summary>
        /// Commit started transaction - sets HasChanged to true if the content is different 
        /// than the content commited by the previous transaction, sets it to false otherwise
        /// NOTE:
        ///     Difference is meant in semantic (two objects with different references but same content doesn't mean difference)
        /// </summary>
        internal void CommitTransaction()
        {
            if (shouldWiden())
            {
                Snapshot.WidenAndCommitTransaction();
            }
            else
            {
                Snapshot.CommitTransaction();
            }

            HasChanges = Snapshot.HasChanged || _commitCount == 0;
            ++_commitCount;
        }

        internal void ResetInitialization()
        {
            _commitCount = 0;
        }

        /// <summary>
        /// Start transaction on contained snapshot
        /// </summary>
        internal void StartTransaction()
        {
            Snapshot.StartTransaction();
        }

        /// <summary>
        /// Reset reported changes
        /// </summary>
        internal void ResetChanges()
        {
            HasChanges = false;
        }

        /// <summary>
        /// Extend output set as call - new entry in call stack is created
        /// </summary>
        /// <param name="callerContext">Flow output of caller</param>
        /// <param name="thisObject">Called object</param>
        /// <param name="arguments">Arguments of call</param>
        internal void ExtendAsCall(FlowOutputSet callerContext, MemoryEntry thisObject, MemoryEntry[] arguments)
        {
            Snapshot.ExtendAsCall(getSnapshot(callerContext), thisObject, arguments);
        }

        #endregion

        #region Snapshot API wrapping



        public StringValue CreateString(string literal)
        {
            return Snapshot.CreateString(literal);
        }

        public IntegerValue CreateInt(int number)
        {
            return Snapshot.CreateInt(number);
        }

        public LongintValue CreateLong(long number)
        {
            return Snapshot.CreateLong(number);
        }

        public BooleanValue CreateBool(bool boolean)
        {
            return Snapshot.CreateBool(boolean);
        }

        public FloatValue CreateDouble(double number)
        {
            return Snapshot.CreateDouble(number);
        }

        public FunctionValue CreateFunction(FunctionDecl declaration)
        {
            return Snapshot.CreateFunction(declaration);
        }

        public FunctionValue CreateFunction(MethodDecl declaration)
        {
            return Snapshot.CreateFunction(declaration);
        }

        public FunctionValue CreateFunction(Name name, NativeAnalyzer analyzer)
        {
            return Snapshot.CreateFunction(name, analyzer);
        }

        public FunctionValue CreateFunction(LambdaFunctionExpr expression)
        {
            return Snapshot.CreateFunction(expression);
        }

        public TypeValue CreateType(ClassDecl declaration)
        {
            return Snapshot.CreateType(declaration);
        }

        public AssociativeArray CreateArray()
        {
            return Snapshot.CreateArray();
        }

        public ObjectValue CreateObject(TypeValue type)
        {
            return Snapshot.CreateObject(type);
        }

        public IntegerIntervalValue CreateIntegerInterval(int start, int end)
        {
            return Snapshot.CreateIntegerInterval(start, end);
        }

        public LongintIntervalValue CreateLongintInterval(long start, long end)
        {
            return Snapshot.CreateLongintInterval(start, end);
        }

        public FloatIntervalValue CreateFloatInterval(double start, double end)
        {
            return Snapshot.CreateFloatInterval(start, end);
        }

        public InfoValue<T> CreateInfo<T>(T data)
        {
            return Snapshot.CreateInfo(data);
        }
        public void SetInfo(Value value, params InfoValue[] info)
        {
            Snapshot.SetInfo(value, info);
        }
        
        public void SetInfo(VariableName variable, params InfoValue[] info)
        {
            Snapshot.SetInfo(variable, info);
        }
        
        public void FetchFromGlobal(params VariableName[] variables)
        {
            Snapshot.FetchFromGlobal(variables);
        }

        public void FetchFromGlobalAll()
        {
            Snapshot.FetchFromGlobalAll();
        }

        public void DeclareGlobal(FunctionDecl declaration)
        {
            Snapshot.DeclareGlobal(declaration);
        }

        public void DeclareGlobal(TypeValue declaration)
        {
            Snapshot.DeclareGlobal(declaration);
        }

        /// <summary>
        /// Expects FlowInput set objects
        /// </summary>
        /// <param name="inputs"></param>
        public void Extend(params ISnapshotReadonly[] inputs)
        {
            var snapshots = getSnapshots(inputs);

            Snapshot.Extend(snapshots);
        }

        public void MergeWithCallLevel(ISnapshotReadonly[] callOutputs)
        {
            var snapshots = getSnapshots(callOutputs);
            Snapshot.MergeWithCallLevel(snapshots);
        }


        #endregion

        #region Private helpers


        private bool shouldWiden()
        {
            return _widenLimit < _commitCount;
        }

        /// <summary>
        /// Convert FlowInput sets info AbstractSnapshots
        /// </summary>
        /// <param name="inputs">FlowInput sets</param>
        /// <returns>Input sets snapshots</returns>
        private SnapshotBase[] getSnapshots(ISnapshotReadonly[] inputs)
        {
            var converted = new SnapshotBase[inputs.Length];

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
        private SnapshotBase getSnapshot(FlowInputSet input)
        {
            return input.Snapshot;
        }
        #endregion

        #region Snapshot output API

        public ReadWriteSnapshotEntryBase GetVariable(VariableIdentifier variable, bool forceGlobalContext = false)
        {
            return Snapshot.GetVariable(variable, forceGlobalContext);
        }

        public ReadSnapshotEntryBase CreateSnapshotEntry(MemoryEntry value)
        {
            return Snapshot.CreateSnapshotEntry(value);
        }

        public ReadWriteSnapshotEntryBase GetControlVariable(VariableName variable)
        {
            return Snapshot.GetControlVariable(variable);
        }

        public ReadWriteSnapshotEntryBase GetLocalControlVariable(VariableName variable)
        {
            return Snapshot.GetLocalControlVariable(variable);
        }

        #endregion




    }
}
