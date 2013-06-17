using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using Weverca.Analysis.Memory;
using Weverca.Analysis;

namespace Weverca.MemoryModel
{
    class AliasValue : Weverca.Analysis.Memory.AliasValue
    {
        internal VariableName VariableName { get; private set; } 

        internal AliasValue(VariableName variableName)
        {
            VariableName = variableName;
        }
    }
}
