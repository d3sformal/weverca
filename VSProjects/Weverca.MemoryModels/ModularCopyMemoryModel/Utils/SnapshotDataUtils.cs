using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Utils
{
    class SnapshotDataUtils
    {
        /// <summary>
        /// Gets the memory entry for given index in given data conatiner.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="data">The data.</param>
        /// <param name="index">The index.</param>
        /// <returns>Associated index if exists; otherwise an empty entry insatnce, created by the snapshot</returns>
        public static MemoryEntry GetMemoryEntry(Snapshot snapshot, IReadOnlySnapshotData data, MemoryIndex index)
        {
            MemoryEntry memoryEntry;
            if (data.TryGetMemoryEntry(index, out memoryEntry))
            {
                return memoryEntry;
            }
            else
            {
                return snapshot.EmptyEntry;
            }
        }
    }
}
