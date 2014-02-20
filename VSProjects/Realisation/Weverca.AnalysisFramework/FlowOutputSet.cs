using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

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

        private readonly int _simplifyLimit;

        internal FlowOutputSet(SnapshotBase snapshot, int widenLimit = int.MaxValue, int simplifyLimit = int.MaxValue) :
            base(snapshot)
        {
            //because new snapshot has been initialized
            HasChanges = true;
            _widenLimit = widenLimit;
            _simplifyLimit = simplifyLimit;
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
                Snapshot.WidenAndCommitTransaction(_simplifyLimit);
            }
            else
            {
                Snapshot.CommitTransaction(_simplifyLimit);
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


        /// <inheritdoc />
        public StringValue CreateString(string literal)
        {
            return Snapshot.CreateString(literal);
        }

        /// <inheritdoc />
        public IntegerValue CreateInt(int number)
        {
            return Snapshot.CreateInt(number);
        }

        /// <inheritdoc />
        public LongintValue CreateLong(long number)
        {
            return Snapshot.CreateLong(number);
        }

        /// <inheritdoc />
        public BooleanValue CreateBool(bool boolean)
        {
            return Snapshot.CreateBool(boolean);
        }

        /// <inheritdoc />
        public FloatValue CreateDouble(double number)
        {
            return Snapshot.CreateDouble(number);
        }

        /// <inheritdoc />
        public FunctionValue CreateFunction(FunctionDecl declaration, FileInfo declaringScript)
        {
            return Snapshot.CreateFunction(declaration, declaringScript);
        }

        /// <inheritdoc />
        public FunctionValue CreateFunction(MethodDecl declaration, FileInfo declaringScript)
        {
            return Snapshot.CreateFunction(declaration, declaringScript);
        }

        /// <inheritdoc />
        public FunctionValue CreateFunction(Name name, NativeAnalyzer analyzer)
        {
            return Snapshot.CreateFunction(name, analyzer);
        }

        /// <inheritdoc />
        public FunctionValue CreateFunction(LambdaFunctionExpr expression, FileInfo declaringScript)
        {
            return Snapshot.CreateFunction(expression, declaringScript);
        }

        /// <inheritdoc />
        public TypeValue CreateType(ClassDecl declaration)
        {
            return Snapshot.CreateType(declaration);
        }

        /// <inheritdoc />
        public AssociativeArray CreateArray()
        {
            return Snapshot.CreateArray();
        }

        /// <inheritdoc />
        public ObjectValue CreateObject(TypeValue type)
        {
            return Snapshot.CreateObject(type);
        }

        /// <inheritdoc />
        public IntegerIntervalValue CreateIntegerInterval(int start, int end)
        {
            return Snapshot.CreateIntegerInterval(start, end);
        }

        /// <inheritdoc />
        public LongintIntervalValue CreateLongintInterval(long start, long end)
        {
            return Snapshot.CreateLongintInterval(start, end);
        }

        /// <inheritdoc />
        public FloatIntervalValue CreateFloatInterval(double start, double end)
        {
            return Snapshot.CreateFloatInterval(start, end);
        }

        /// <inheritdoc />
        public InfoValue<T> CreateInfo<T>(T data)
        {
            return Snapshot.CreateInfo(data);
        }

        /// <inheritdoc />
        public void SetInfo(Value value, params InfoValue[] info)
        {
            Snapshot.SetInfo(value, info);
        }

        /// <inheritdoc />
        public void SetInfo(VariableName variable, params InfoValue[] info)
        {
            Snapshot.SetInfo(variable, info);
        }

        /// <summary>
        /// Fetches given variables from global to local context
        /// </summary>
        /// <param name="variables">fetches variables</param>
        public void FetchFromGlobal(params VariableName[] variables)
        {
            Snapshot.FetchFromGlobal(variables);
        }

        /// <summary>
        /// Fetches all vrables from global to local context
        /// </summary>
        public void FetchFromGlobalAll()
        {
            Snapshot.FetchFromGlobalAll();
        }

        /// <inheritdoc />
        public void DeclareGlobal(FunctionDecl declaration, FileInfo declaringScript)
        {
            Snapshot.DeclareGlobal(declaration, declaringScript);
        }

        /// <inheritdoc />
        public void DeclareGlobal(TypeValue declaration)
        {
            Snapshot.DeclareGlobal(declaration);
        }


        /// <inheritdoc />
        public void Extend(params ISnapshotReadonly[] inputs)
        {
            var snapshots = getSnapshots(inputs);

            Snapshot.Extend(snapshots);
        }


        /// <inheritdoc />
        public void MergeWithCallLevel(ISnapshotReadonly[] callOutputs)
        {
            var snapshots = getSnapshots(callOutputs);
            Snapshot.MergeWithCallLevel(snapshots);
        }


        #endregion

        #region Private helpers

        /// <summary>
        /// Determine that Flow set should be widen based on commit count
        /// </summary>
        /// <returns>True if set should be widen, false otherwise</returns>
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

        /// <inheritdoc />
        public ReadWriteSnapshotEntryBase GetVariable(VariableIdentifier variable, bool forceGlobalContext = false)
        {
            return Snapshot.GetVariable(variable, forceGlobalContext);
        }

        /// <inheritdoc />
        public ReadSnapshotEntryBase CreateSnapshotEntry(MemoryEntry value)
        {
            return Snapshot.CreateSnapshotEntry(value);
        }

        /// <inheritdoc />
        public ReadWriteSnapshotEntryBase GetControlVariable(VariableName variable)
        {
            return Snapshot.GetControlVariable(variable);
        }

        /// <inheritdoc />
        public ReadWriteSnapshotEntryBase GetLocalControlVariable(VariableName variable)
        {
            return Snapshot.GetLocalControlVariable(variable);
        }

        #endregion




    }
}
