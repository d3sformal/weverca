using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    public interface ISnapshotReadonly
    {
        Value ReadValue(PHP.Core.VariableName sourceVar);
    }
}
