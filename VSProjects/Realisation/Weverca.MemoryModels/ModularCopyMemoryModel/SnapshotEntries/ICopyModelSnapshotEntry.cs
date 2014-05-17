using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries
{
    /// <summary>
    /// Defines basic methods for copy memory model snapshot entries.
    /// </summary>
    public interface ICopyModelSnapshotEntry
    {
        /// <summary>
        /// Gets the path of this snapshot entry.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>The path of this snapshot entry.</returns>
        MemoryPath GetPath(Snapshot snapshot);

        /// <summary>
        /// Reads the memory.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Memory represented by current snapshot entry.</returns>
        MemoryEntry ReadMemory(Snapshot snapshot);
    }
}
