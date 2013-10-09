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
    }
}
