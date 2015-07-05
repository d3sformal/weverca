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
