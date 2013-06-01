using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
namespace Weverca.ControlFlowGraph.Analysis.Memory
{
    public interface ISnapshotReadonly
    {
        MemoryEntry ReadValue(VariableName sourceVar);
    }
}
