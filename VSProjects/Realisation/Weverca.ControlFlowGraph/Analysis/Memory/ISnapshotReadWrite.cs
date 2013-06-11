using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    public interface ISnapshotReadWrite:ISnapshotReadonly
    {
        #region Creating values
        /// <summary>
        /// Singleton representation of AnyValue
        /// </summary>
        AnyValue AnyValue { get; }
        /// <summary>
        /// Singleton representation of UndefinedValue
        /// </summary>
        UndefinedValue UndefinedValue { get; }
        /// <summary>
        /// Singleton representation of AnyValue inside memory entry
        /// </summary>
        MemoryEntry AnyValueEntry { get; }
        /// <summary>
        /// Singleton representation of UndefinedValue inside memory entry
        /// </summary>
        MemoryEntry UndefinedValueEntry { get; }

        StringValue CreateString(string literal);
        IntegerValue CreateInt(int number);
        BooleanValue CreateBool(bool boolean);
        FloatValue CreateFloat(double number);
        FunctionValue CreateFunction(FunctionDecl declaration);


        /// <summary>
        /// Create array - TODO what kind of info will be neaded for creation?
        /// </summary>
        /// <returns></returns>
        AssociativeArray CreateArray();
        /// <summary>
        /// Create object - TODO what kind of info will be neaded for creation?
        /// </summary>
        /// <returns></returns>
        ObjectValue CreateObject();
        /// <summary>
        /// Create alias for given variable
        /// </summary>
        /// <param name="sourceVar">Variable which alias will be created</param>
        /// <returns>Created alias</returns>
        AliasValue CreateAlias(VariableName sourceVar);
        #endregion

        /// <summary>
        /// Assign value into targetVar
        /// If value is AliasValue - must alias has to be set for variable
        /// </summary>
        /// <param name="targetVar">Target of assigning</param>
        /// <param name="value">Value that will be assigned</param>
        void Assign(VariableName targetVar, Value value);

        /// <summary>
        /// Assign memory entry into targetVar        
        /// </summary>
        /// <param name="targetVar">Target of assigning</param>
        /// <param name="entry">Value that will be assigned</param>
        void Assign(VariableName targetVar, MemoryEntry entry);

        /// <summary>
        /// Snapshot has to contain merged info present in inputs (no matter what snapshots contains till now)
        /// This merged info can be than changed with snapshot updatable operations
        /// NOTE: Further changes of inputs can't change extended snapshot
        /// </summary>
        /// <param name="inputs">Input snapshots that should be merged</param>
        void Extend(params ISnapshotReadonly[] inputs);
        /// <summary>
        /// Merge given call output with current context.
        /// WARNING: Call can change many objects via references (they don't has to be in global context)
        /// </summary>
        /// <param name="callOutput">Output snapshot of call</param>
        /// <param name="result">Result of merged call</param>
        void MergeWithCall(CallResult result, ISnapshotReadonly callOutput);
    }
}
