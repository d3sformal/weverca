﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class SnapshotEntry : ReadWriteSnapshotEntryBase
    {
        HashSet<MemoryIndex> memoryIndexes;



        protected override void writeMemory(SnapshotBase context, MemoryEntry value)
        {
            throw new NotImplementedException();
        }

        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            throw new NotImplementedException();
        }

        protected override bool isDefined(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            throw new NotImplementedException();
        }

        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, AnalysisFramework.VariableIdentifier field)
        {
            throw new NotImplementedException();
        }

        protected override AnalysisFramework.VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new NotImplementedException();
        }
    }
}
