using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel
{
    public class MemoryAlias : AliasValue
    {
        public MemoryIndex Index { get; private set; }

        public MemoryAlias(MemoryIndex index)
        {
            Index = index;
        }
    }
}
