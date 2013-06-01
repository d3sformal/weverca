using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    public interface ISnapshotReadWrite:ISnapshotReadonly
    {
        #region Creating values
        AnyValue AnyValue { get; }
        UndefinedValue UndefinedValue { get; }

        StringValue CreateString(string literal);
        IntegerValue CreateInt(int number);
        BooleanValue CreateBool(bool boolean);
        FloatValue CreateFloat(double number);
        ObjectValue CreateObject();
        AliasValue CreateAlias(PHP.Core.VariableName sourceVar);
        #endregion

        void Assign(PHP.Core.VariableName targetVar, object value);

        void Extend(params ISnapshotReadonly[] inputs);

        void MergeWithCall(ISnapshotReadonly callOutput);

        void ExtendFromEntryPoint(ISnapshotReadonly callInput);


    }
}
