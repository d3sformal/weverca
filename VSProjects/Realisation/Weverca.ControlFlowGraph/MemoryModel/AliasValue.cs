using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using Weverca.ControlFlowGraph.Analysis.Memory;
using Weverca.ControlFlowGraph.Analysis;

namespace Weverca.ControlFlowGraph.MemoryModel
{
    class AliasValue : Weverca.ControlFlowGraph.Analysis.Memory.AliasValue
    {
        internal VariableName VariableName { get; private set; } 

        internal AliasValue(VariableName variableName)
        {
            VariableName = variableName;
        }
    }
}
