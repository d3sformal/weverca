using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using Weverca.ControlFlowGraph.Analysis;
using Weverca.ControlFlowGraph.Analysis.Memory;

namespace Weverca.ControlFlowGraph.UnitTest
{
    /// <summary>
    /// Here will be tests after snapshot impelmentation - now are here use cases because of interface designing
    /// </summary>
    class MemorySnapshotTests
    {
        public void AssignFromVariable(VariableName targetVar, VariableName sourceVar,ISnapshotReadonly input, ISnapshotReadWrite output)
        {            
            output.Extend(input);
            var value = input.ReadValue(sourceVar);
            output.Assign(targetVar, value);
        }

        public void AssignFromLiteral(VariableName targetVar, string literal, ISnapshotReadonly input, ISnapshotReadWrite output)
        {
            output.Extend(input);
            var value = output.CreateString(literal);
            output.Assign(targetVar, value);
        }

        public void AssignAlias(VariableName targetVar, VariableName sourceVar, ISnapshotReadonly input, ISnapshotReadWrite output)
        {            
            output.Extend(input);
            var sourceAlias=output.CreateAlias(sourceVar);
            output.Assign(targetVar, sourceAlias);
        }

        public void MergeBranches(ISnapshotReadWrite output, params ISnapshotReadonly[] inputs)
        {
            output.Extend(inputs);
        }

        public void MergeWithCall(ISnapshotReadWrite output, ISnapshotReadonly input,CallResult callResult, ISnapshotReadonly callOutput)
        {
            output.Extend(input);
            //process call
            output.MergeWithCall(callResult,callOutput);
            //reverse assign of param aliasses
        }

        public Snapshot ProcessCall(Snapshot caller,CallInfo callInfo)
        {
            return caller.CreateCall(callInfo);
        }

        public void ProcessConcat(ISnapshotReadWrite output, ISnapshotReadonly input,MemoryEntry value1,MemoryEntry value2)
        {
            foreach (var val1 in value1.PossibleValues)
            {
                foreach (var val2 in value2.PossibleValues)
                {
                    //here can be concatenation of all possible values
                }
            }
        }
    }
}
