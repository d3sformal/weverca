using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Represents entry provided by snapshots. Provides accessing to memory based operations that CAN MODIFY
    /// visible state of snapshot (read write operation abstraction)
    /// </summary>
    public abstract class ReadWriteSnapshotEntryBase : ReadSnapshotEntryBase
    {
        /// <summary>
        /// Initialize read write snapshot entry
        /// </summary>
        /// 
        /// <param name="owningSnapshot">
        /// Snapshot creating current entry.
        /// Is used for storing statistics information.
        /// </param>
        protected ReadWriteSnapshotEntryBase(SnapshotBase owningSnapshot)
            : base(owningSnapshot)
        {
        }

        /// <summary>
        /// Write given value at memory represented by snapshot entry
        /// </summary>
        /// <param name="value">Written value</param>
        public abstract void WriteMemory(MemoryEntry value);

        /// <summary>
        /// Set aliases to current snapshot entry. Aliases can be set even to those entries
        /// that doesn't belongs to any variable, field,..
        /// </summary>
        /// <param name="aliases">Aliases that will be set to snapshot entry</param>
        public abstract void SetAliases(IEnumerable<AliasEntry> aliases);
    }
}
