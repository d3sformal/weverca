using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    class SnapshotAliasEntry:AliasEntry
    {
        internal readonly SnapshotMemoryEntry SnapshotEntry;

        internal SnapshotAliasEntry(SnapshotMemoryEntry entry)
        {
            SnapshotEntry = entry;
        }
    }
}
