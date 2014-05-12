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
        /// Creates the alias to this entry and returnes data which can be used to aliasing the target.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Alias data fro the newly created aliases.</returns>
        AliasData CreateAliasToEntry(Snapshot snapshot);

        /// <summary>
        /// Reads the memory.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Memory represented by current snapshot entry.</returns>
        MemoryEntry ReadMemory(Snapshot snapshot);
    }
}
