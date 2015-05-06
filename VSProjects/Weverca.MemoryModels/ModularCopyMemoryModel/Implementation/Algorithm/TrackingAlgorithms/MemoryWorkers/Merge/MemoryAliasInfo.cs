using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    internal class MemoryAliasInfo
    {
        private HashSet<MemoryIndex> removedAliases;
        public IMemoryAliasBuilder Aliases { get; set; }
        public bool IsTargetOfMerge { get; set; }
        public IEnumerable<MemoryIndex> RemovedAliases { get { return removedAliases; } }

        public MemoryAliasInfo(IMemoryAliasBuilder aliases, bool isTargetOfMerge)
        {
            this.Aliases = aliases;
            this.IsTargetOfMerge = isTargetOfMerge;
            this.removedAliases = new HashSet<MemoryIndex>();
        }

        public void AddRemovedAlias(MemoryIndex alias)
        {
            removedAliases.Add(alias);
        }
    }
}
