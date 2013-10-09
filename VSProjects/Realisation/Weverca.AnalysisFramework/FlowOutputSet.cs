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
        /// Determine that output has changed when flow through statement
        /// </summary>
        internal bool HasChanges { get; private set; }

        private int _commitCount = 0;
        
        internal FlowOutputSet(SnapshotBase snapshot) :
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
            Snapshot.CommitTransaction();

            HasChanges = Snapshot.HasChanged || _commitCount==0;
            ++_commitCount;
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

        public AnyStringValue AnyStringValue { get { return Snapshot.AnyStringValue; } }

        public AnyBooleanValue AnyBooleanValue { get { return Snapshot.AnyBooleanValue; } }

        public AnyIntegerValue AnyIntegerValue  { get { return Snapshot.AnyIntegerValue; } }

        public AnyFloatValue AnyFloatValue { get { return Snapshot.AnyFloatValue; } }

        public AnyLongintValue AnyLongintValue { get { return Snapshot.AnyLongintValue; } }

        public AnyObjectValue AnyObjectValue { get { return Snapshot.AnyObjectValue; } }

        public AnyArrayValue AnyArrayValue { get { return Snapshot.AnyArrayValue; } }

        public AnyResourceValue AnyResourceValue { get { return Snapshot.AnyResourceValue; } }

        public AnyValue AnyValue { get { return Snapshot.AnyValue; } }

        public UndefinedValue UndefinedValue { get { return Snapshot.UndefinedValue; } }
        
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
            return Snapshot.CreateFunction(name,analyzer);
        }

        public FunctionValue CreateFunction(LambdaFunctionExpr expression)
        {
            return Snapshot.CreateFunction(expression);
        }

        public TypeValueBase CreateType(ObjectDecl declaration)
        {
            return Snapshot.CreateType(declaration);
        }

        public AssociativeArray CreateArray()
        {
            return Snapshot.CreateArray();
        }

        public ObjectValue CreateObject(TypeValueBase type)
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

        [Obsolete("Use snapshot entry API instead")]
        public void Assign(VariableName targetVar, Value value)
        {
            Snapshot.Assign(targetVar, value);
        }

        [Obsolete("Use snapshot entry API instead")]
        public void Assign(VariableName targetVar, MemoryEntry entry)
        {
            Snapshot.Assign(targetVar, entry);
        }

        [Obsolete("Use snapshot entry API instead")]
        public void AssignAliases(VariableName targetVar, IEnumerable<AliasValue> aliases)
        {
            Snapshot.AssignAliases(targetVar, aliases);
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

        public void DeclareGlobal(TypeValueBase declaration)
        {
            Snapshot.DeclareGlobal(declaration);
        }

        [Obsolete("Use snapshot entry API instead")]
        public void SetField(ObjectValue value, ContainerIndex index, MemoryEntry entry)
        {
            Snapshot.SetField(value, index, entry);
        }

        [Obsolete("Use snapshot entry API instead")]
        public void SetIndex(AssociativeArray value, ContainerIndex index, MemoryEntry entry)
        {
            Snapshot.SetIndex(value, index, entry);
        }

        [Obsolete("Use snapshot entry API instead")]
        public void SetFieldAlias(ObjectValue value, ContainerIndex index, IEnumerable<AliasValue> alias)
        {
            Snapshot.SetFieldAlias(value, index, alias);
        }

        [Obsolete("Use snapshot entry API instead")]
        public void SetIndexAlias(AssociativeArray value, ContainerIndex index, IEnumerable<AliasValue> alias)
        {
            Snapshot.SetIndexAlias(value, index, alias);
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
            throw new NotImplementedException();
        }

        #endregion
    }
}
