using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    /// <summary>
    /// Hold an information about aliases which hast be created by the merge operation.
    /// 
    /// Tracking merge worker creates this instance if merging will find new alias to some
    /// meory index. When the merge is finished then these instances will contain all possible 
    /// aliases. Aliases will be created at the end of merge operation.
    /// </summary>
    internal class MemoryAliasInfo
    {
        private HashSet<MemoryIndex> removedAliases;

        /// <summary>
        /// Gets or sets the aliases.
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        public IMemoryAliasBuilder Aliases { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is target of merge.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is target of merge; otherwise, <c>false</c>.
        /// </value>
        public bool IsTargetOfMerge { get; set; }

        /// <summary>
        /// Gets the removed aliases.
        /// </summary>
        /// <value>
        /// The removed aliases.
        /// </value>
        public IEnumerable<MemoryIndex> RemovedAliases { get { return removedAliases; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAliasInfo"/> class.
        /// </summary>
        /// <param name="aliases">The aliases.</param>
        /// <param name="isTargetOfMerge">if set to <c>true</c> [is target of merge].</param>
        public MemoryAliasInfo(IMemoryAliasBuilder aliases, bool isTargetOfMerge)
        {
            this.Aliases = aliases;
            this.IsTargetOfMerge = isTargetOfMerge;
            this.removedAliases = new HashSet<MemoryIndex>();
        }

        /// <summary>
        /// Adds the removed alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public void AddRemovedAlias(MemoryIndex alias)
        {
            removedAliases.Add(alias);
        }
    }
}
