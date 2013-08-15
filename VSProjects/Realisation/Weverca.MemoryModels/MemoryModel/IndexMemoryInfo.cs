using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.MemoryModels.MemoryModel
{
    class IndexMemoryInfo : MemoryInfo
    {
        private Analysis.Memory.AssociativeArray value;

        public IndexMemoryInfo(MemoryIndex index, Analysis.Memory.AssociativeArray value)
            : base(index)
        {
            this.value = value;
        }
    }
}
