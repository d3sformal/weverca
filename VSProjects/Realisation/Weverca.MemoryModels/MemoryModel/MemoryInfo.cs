using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.MemoryModel
{
    public class MemoryInfo
    {
        List<MemoryIndex> mustAliases;
        List<MemoryIndex> mayAliases;

        public IEnumerable<MemoryIndex> MayAliasses { get; set; }

        public IEnumerable<MemoryIndex> MustAliasses { get; set; }
    }
}
