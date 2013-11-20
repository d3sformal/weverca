using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    interface IReferenceHolder
    {
        void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases);

        void AddAlias(MemoryIndex index, MemoryIndex mustAlias, MemoryIndex mayAlias);
    }
}
